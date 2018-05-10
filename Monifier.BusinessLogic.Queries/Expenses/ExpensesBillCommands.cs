using System;
using System.Linq;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Accounts;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesBillCommands : IExpensesBillCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;

        public ExpensesBillCommands(IUnitOfWork unitOfWork, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new NotSupportedException("Операция не поддерживается");

            var billQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var bill = await billQueries.GetById(id);
            if (bill == null)
                throw new ArgumentException($"Нет чека с Id = {id}");

            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();
            var flowQueries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();

            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            flow.Balance += bill.SumPrice;
            flowCommands.Update(flow);

            if (bill.AccountId != null && !bill.IsCorrection)
            {
                var accountQueries = _unitOfWork.GetQueryRepository<Account>();
                var accountCommands = _unitOfWork.GetCommandRepository<Account>();
                var account = await accountQueries.GetById(bill.AccountId.Value);
                account.Balance += bill.SumPrice;
                accountCommands.Update(account);
            }

            billCommands.Delete(bill);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> Create(ExpenseBillModel model, bool correction = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Validate();

            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();

            var bill = new ExpenseBill
            {
                ExpenseFlowId = model.ExpenseFlowId,
                AccountId = model.AccountId,
                DateTime = model.DateTime,
                SumPrice = model.Cost,
                OwnerId = _currentSession.UserId,
                IsCorrection = correction,
            };
            model.IsCorection = correction;

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

            Account account = null;
            if (model.AccountId != null)
            {
                var accountQueries = _unitOfWork.GetQueryRepository<Account>();
                account = await accountQueries.GetById(model.AccountId.Value);
            }

            var flowQieries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flow = await flowQieries.GetById(bill.ExpenseFlowId);
            if (flow == null)
                throw new InvalidOperationException($"Can't find expense flow with id {bill.ExpenseFlowId}");
            var lack = bill.SumPrice - Math.Max(flow.Balance, 0);
            var withdrawTotal = bill.SumPrice;
            if (lack > 0 && account != null && !correction)
            {
                var compensation = Math.Min(Math.Max(account.AvailBalance, 0), lack);
                withdrawTotal -= compensation;
                account.AvailBalance -= compensation;
            }
            flow.Balance -= withdrawTotal;
            flow.Version++;
            flowCommands.Update(flow);

            if (account != null && !correction)
            {
                var accountCommands = _unitOfWork.GetCommandRepository<Account>();
                account.Balance -= bill.SumPrice;
                account.LastWithdraw = DateTime.Now;
                accountCommands.Update(account);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            model.Id = bill.Id;

            if (model.AccountId != null)
            {
                var transation = new Transaction
                {
                    OwnerId = model.OwnerId,
                    DateTime = model.DateTime,
                    InitiatorId = model.AccountId.Value,
                    BillId = model.Id,
                    Total = model.Cost,
                };
                var transactionCommands = _unitOfWork.GetCommandRepository<Transaction>();
                transactionCommands.Create(transation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return model.Id;
        }

        public async Task<ExpenseBillModel> Update(ExpenseBillModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Validate();

            var billQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();

            var bill = await billQueries.GetById(model.Id);
            if (bill == null)
                throw new ArgumentException($"Нет счета с Id = {model.Id}");

            var oldsum = bill.SumPrice;
            var oldAccountId = bill.AccountId;
            bill.DateTime = model.DateTime;
            bill.SumPrice = model.Cost;
            bill.ExpenseFlowId = model.ExpenseFlowId;
            bill.AccountId = model.AccountId;

            var itemQueries = _unitOfWork.GetQueryRepository<ExpenseItem>();
            var itemCommands = _unitOfWork.GetCommandRepository<ExpenseItem>();
            foreach (var item in model.Items)
            {
                if (item.Id > 0 && !item.IsModified && !item.IsDeleted) continue;
                if (item.Id <= 0 || item.IsModified)
                {
                    var itemModel = new ExpenseItem
                    {
                        BillId = model.Id,
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

            var flowQueries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();

            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            var newAccount = bill.AccountId != null ? await accountQueries.GetById(bill.AccountId.Value) : null;
            flow.Balance = flow.Balance + oldsum;
            var lack = bill.SumPrice - Math.Max(flow.Balance, 0);
            var withdrawTotal = bill.SumPrice;
            if (lack > 0 && newAccount != null)
            {
                var compensation = Math.Min(Math.Max(newAccount.AvailBalance, 0), lack);
                withdrawTotal -= compensation;
                newAccount.AvailBalance -= compensation;
            }
            flow.Balance -= withdrawTotal;
            flow.Version++;
            flowCommands.Update(flow);

            if (!bill.IsCorrection)
            {
                if (oldAccountId != bill.AccountId)
                {
                    if (oldAccountId != null)
                    {
                        var oldAccount = await accountQueries.GetById(oldAccountId.Value);
                        oldAccount.Balance += oldsum;
                        accountCommands.Update(oldAccount);
                    }

                    if (newAccount != null)
                    {
                        newAccount.Balance -= bill.SumPrice;
                        newAccount.LastWithdraw = DateTime.Now;
                        accountCommands.Update(newAccount);
                    }
                }
                else if (newAccount != null)
                {
                    newAccount.Balance += oldsum - model.Cost;
                    accountCommands.Update(newAccount);
                }
            }
            else if (newAccount != null)
            {
                accountCommands.Update(newAccount);
            }

            var transactionCommands = _unitOfWork.GetCommandRepository<Transaction>();
            var transactionQueries = _unitOfWork.GetQueryRepository<Transaction>();
            if (oldAccountId == model.AccountId)
            {
                var transaction = transactionQueries.Query
                    .SingleOrDefault(x => x.OwnerId == model.OwnerId && x.InitiatorId == model.AccountId && x.BillId == model.Id);
                if (transaction != null)
                {
                    transaction.DateTime = model.DateTime;
                    transaction.Total = model.Cost;
                    transactionCommands.Update(transaction);
                }
            }
            else if (model.AccountId != null)
            {
                var transaction = transactionQueries.Query
                    .SingleOrDefault(x => x.OwnerId == model.OwnerId && x.InitiatorId == oldAccountId && x.BillId == model.Id);
                if (transaction == null)
                {
                    transaction = new Transaction
                    {
                        OwnerId = model.OwnerId,
                        DateTime = model.DateTime,
                        BillId = model.Id,
                        InitiatorId = model.AccountId.Value,
                        Total = model.Cost
                    };
                    transactionCommands.Create(transaction);
                }
                else
                {
                    transaction.InitiatorId = model.AccountId.Value;
                    transaction.Total = model.Cost;
                    transactionCommands.Update(transaction);
                }
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return model;
        }

        public async Task Save(ExpenseBillModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.IsNew)
                await Create(model);
            else
                await Update(model);
        }
    }
}
