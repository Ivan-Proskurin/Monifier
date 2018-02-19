using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseBillModel
    {
        public ExpenseBillModel()
        {
            DateTime = DateTime.Now.ToMinutes();
            Items = new List<ExpenseItemModel>();
        }
        
        public int Id { get; set; }
        public int? AccountId { get; set; }
        public int ExpenseFlowId { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Cost { get; set; }
        public int OwnerId { get; set; }
        public List<ExpenseItemModel> Items { get; set; }

        public bool IsNew => Id <= 0;

        public void AddItem(ExpenseItemModel item)
        {
            if (Items == null) Items = new List<ExpenseItemModel>();
            Items.Add(item);
            UpdateState();
        }
        
        public void RemoveLastAddedItem()
        {
            if (Items == null || Items.Count == 0) return;
            var lastItem = Items.LastOrDefault();
            if (lastItem == null) return;
            
            if (IsNew || lastItem.Id <= 0)
            {
                Items.Remove(lastItem);
            }
            else
            {
                lastItem.IsDeleted = true;
            }
            
            UpdateState();
        }

        private void UpdateState()
        {
            var items = GetItems();
            Cost = items.Sum(x => x.Cost);
        }

        public List<ExpenseItemModel> GetItems()
        {
            if (Items == null) Items = new List<ExpenseItemModel>();
            return Items.Where(x => !x.IsDeleted).ToList();
        }

        public ExpenseBillModel Clone()
        {
            var clone = new ExpenseBillModel
            {
                Id = Id,
                DateTime = DateTime,
                Cost = Cost
            };
            if (Items == null) return clone;
            clone.Items = new List<ExpenseItemModel>();
            foreach (var item in Items)
            {
                clone.Items.Add(new ExpenseItemModel
                {
                    Id = item.Id,
                    Category = item.Category,
                    CategoryId = item.CategoryId,
                    ProductId = item.ProductId,
                    Product = item.Product,
                    Cost = item.Cost,
                    Quantity = item.Quantity,
                    Comment = item.Comment,
                    IsDeleted = item.IsDeleted,
                    IsModified = item.IsModified
                });
            }
            return clone;
        }

        public ExpenseItemModel GetItem(int id)
        {
            return Items?.FirstOrDefault(x => x.Id == id);
        }

        public void SetItem(ExpenseItemModel item)
        {
            if (Items == null) return;
            var index = Items.FindIndex(x => x.Id == item.Id);
            if (index < 0) return;
            Items[index] = item;
            UpdateState();
        }

        private void Validate()
        {
            if (Cost != GetItems().Sum(x => x.Cost))
                throw new ValidationException("Сумма чека должна равняться сумме всех входящих в него позиций");
        }

        public async Task Save(IUnitOfWork unitOfWork)
        {
            if (IsNew)
                await Create(unitOfWork);
            else
                await Update(unitOfWork);
        }

        public async Task Create(IUnitOfWork unitOfWork, bool correcting = false)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork));
            
            Validate();
            
            var billCommands = unitOfWork.GetCommandRepository<ExpenseBill>();

            var bill = new ExpenseBill
            {
                ExpenseFlowId = ExpenseFlowId,
                AccountId = AccountId,
                DateTime = DateTime,
                SumPrice = Cost,
                OwnerId = OwnerId
            };

            billCommands.Create(bill);

            var itemsCommands = unitOfWork.GetCommandRepository<ExpenseItem>();

            foreach (var item in Items)
            {
                itemsCommands.Create(new ExpenseItem
                {
                    Bill = bill,
                    CategoryId = item.CategoryId,
                    ProductId = item.ProductId,
                    Price = item.Cost,
                    Quantity = item.Quantity,
                    Comment = item.Comment
                });
            }

            var flowQieries = unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flow = await flowQieries.GetById(bill.ExpenseFlowId);
            flow.Balance -= bill.SumPrice;
            flow.Version++;
            flowCommands.Update(flow);

            if (AccountId != null && !correcting)
            {
                var accountQueries = unitOfWork.GetQueryRepository<Account>();
                var accountCommands = unitOfWork.GetCommandRepository<Account>();
                var account = await accountQueries.GetById(AccountId.Value);
                account.Balance -= bill.SumPrice;
                account.LastWithdraw = DateTime.Now;
                accountCommands.Update(account);
            }

            await unitOfWork.SaveChangesAsync();

            Id = bill.Id;
        }

        public async Task Update(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork));
            
            Validate();
            
            var billQueries = unitOfWork.GetQueryRepository<ExpenseBill>();
            var billCommands = unitOfWork.GetCommandRepository<ExpenseBill>();
            
            var bill = await billQueries.GetById(Id);
            if (bill == null)
                throw new ArgumentException($"Нет счета с Id = {Id}");
            
            var oldsum = bill.SumPrice;
            var oldAccountId = bill.AccountId;
            bill.DateTime = DateTime;
            bill.SumPrice = Cost;
            bill.ExpenseFlowId = ExpenseFlowId;
            bill.AccountId = AccountId;

            var itemQueries = unitOfWork.GetQueryRepository<ExpenseItem>();
            var itemCommands = unitOfWork.GetCommandRepository<ExpenseItem>();
            foreach (var item in Items)
            {
                if (item.Id > 0 && !item.IsModified && !item.IsDeleted) continue;
                if (item.Id <= 0 || item.IsModified)
                {
                    var itemModel = new ExpenseItem
                    {
                        BillId = Id,
                        CategoryId = item.CategoryId,
                        ProductId = item.ProductId,
                        Price = item.Cost,
                        Quantity = item.Quantity,
                        Comment = item.Comment
                    };
                    if (item.Id <= 0)
                        itemCommands.Create(itemModel);
                    else
                    {
                        itemModel.Id = item.Id;
                        itemCommands.Update(itemModel);
                    }
                }
                else if (item.IsDeleted)
                {
                    var itemModel = await itemQueries.GetById(item.Id);
                    if (itemModel == null) continue;
                    itemCommands.Delete(itemModel);
                }
            }
            
            billCommands.Update(bill);

            var flowQueries = unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            flow.Balance = flow.Balance + oldsum - Cost;
            flowCommands.Update(flow);

            var accountQueries = unitOfWork.GetQueryRepository<Account>();
            var accountCommands = unitOfWork.GetCommandRepository<Account>();
            if (oldAccountId != bill.AccountId)
            {
                if (oldAccountId != null)
                {
                    var oldAccount = await accountQueries.GetById(oldAccountId.Value);
                    oldAccount.Balance += oldsum;
                    accountCommands.Update(oldAccount);
                }

                if (bill.AccountId != null)
                {
                    var newAccount = await accountQueries.GetById(bill.AccountId.Value);
                    newAccount.Balance -= bill.SumPrice;
                    newAccount.LastWithdraw = DateTime.Now;
                    accountCommands.Update(newAccount);
                }
            }
            else if (AccountId != null)
            {
                var account = await accountQueries.GetById(AccountId.Value);
                account.Balance += oldsum - Cost;
                accountCommands.Update(account);
            }

            await unitOfWork.SaveChangesAsync();
        }

        public static async Task Delete(int id, IUnitOfWork unitOfWork)
        {
            var billQueries = unitOfWork.GetQueryRepository<ExpenseBill>();
            var bill = await billQueries.GetById(id);
            if (bill == null)
                throw new ArgumentException($"Нет счета с Id = {id}");
            
            var billCommands = unitOfWork.GetCommandRepository<ExpenseBill>();
            var flowQueries = unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = unitOfWork.GetCommandRepository<ExpenseFlow>();

            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            flow.Balance += bill.SumPrice;
            flowCommands.Update(flow);

            if (bill.AccountId != null)
            {
                var accountQueries = unitOfWork.GetQueryRepository<Account>();
                var accountCommands = unitOfWork.GetCommandRepository<Account>();
                var account = await accountQueries.GetById(bill.AccountId.Value);
                account.Balance += bill.SumPrice;
                accountCommands.Update(account);
            }
            
            billCommands.Delete(bill);
            
            await unitOfWork.SaveChangesAsync();
        }
    }
}
