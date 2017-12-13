using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesBillCommands : IExpensesBillCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpensesBillCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new ArgumentException("Операция не поддерживается", nameof(onlyMark));
            
            var bill = await _unitOfWork.GetQueryRepository<ExpenseBill>().GetById(id);
            if (bill == null)
                throw new ArgumentException($"Нет счета с Id = {id}");
            
            _unitOfWork.GetCommandRepository<ExpenseBill>().Delete(bill);
            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExpenseBillModel> Update(ExpenseBillModel model)
        {
            var commands = _unitOfWork.GetCommandRepository<ExpenseItem>();
            foreach (var item in model.Items)
            {
                if (item.Id > 0 && !item.IsModified && !item.IsDeleted) continue;
                if (item.Id < 0 || item.IsModified)
                {
                    var itemModel = new ExpenseItem
                    {
                        Id = item.Id,
                        BillId = model.Id,
                        CategoryId = item.CategoryId,
                        ProductId = item.ProductId,
                        Price = item.Cost,
                        Quantity = item.Quantity,
                        Comment = item.Comment
                    };
                    if (item.Id < 0)
                        commands.Create(itemModel);
                    else
                        commands.Update(itemModel);
                }
                else if (item.IsDeleted)
                {
                    var itemModel = await _unitOfWork.GetQueryRepository<ExpenseItem>().GetById(item.Id);
                    if (itemModel == null) continue;
                    commands.Delete(itemModel);
                }
            }

            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();
            billCommands.Update(new ExpenseBill
            {
                Id = model.Id,
                DateTime = model.DateTime,
                SumPrice = model.Cost
            });

            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task Create(ExpenseBillModel model)
        {
            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();

            var bill = new ExpenseBill
            {
                ExpenseFlowId = model.ExpenseFlowId,
                AccountId = model.AccountId,
                DateTime = model.DateTime,
                SumPrice = model.Cost
            };

            billCommands.Create(bill);

            var itemsCommands = _unitOfWork.GetCommandRepository<ExpenseItem>();

            foreach (var item in model.Items)
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

            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var account = await accountQueries.GetById(bill.AccountId);
            account.Balance -= bill.SumPrice;
            accountCommands.Update(account);

            var flowQieries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flow = await flowQieries.GetById(bill.ExpenseFlowId);
            flow.Balance -= bill.SumPrice;
            flowCommands.Update(flow);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
