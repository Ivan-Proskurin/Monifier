using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesBillCommands : IExpensesBillCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;
        private readonly IBalancesUpdaterFactory _balancesUpdaterFactory;
        private readonly ITransitionBalanceUpdater _transitionBalanceUpdater;
        private readonly ITransactionBuilder _transactionBuilder;

        public ExpensesBillCommands(
            IUnitOfWork unitOfWork, 
            ICurrentSession currentSession,
            IBalancesUpdaterFactory balancesUpdaterFactory,
            ITransitionBalanceUpdater transitionBalanceUpdater,
            ITransactionBuilder transactionBuilder)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
            _balancesUpdaterFactory = balancesUpdaterFactory;
            _transitionBalanceUpdater = transitionBalanceUpdater;
            _transactionBuilder = transactionBuilder;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new NotSupportedException("Операция не поддерживается");

            var billQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var bill = await billQueries.GetById(id);
            if (bill == null)
                throw new ArgumentException($"Нет чека с Id = {id}");

            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var account = bill.AccountId != null ? await accountQueries.GetById(bill.AccountId.Value) : null;
            if (account != null)
            {
                var balanceUpdater = _balancesUpdaterFactory.Create(account.AccountType);
                await balanceUpdater.Delete(bill);
            }

            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();
            billCommands.Delete(bill);

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
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

            if (account != null)
            {
                var balancesUpdater = _balancesUpdaterFactory.Create(account.AccountType);
                await balancesUpdater.Create(account, bill).ConfigureAwait(false);
            }

            _transactionBuilder.Create(bill);

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            model.Id = bill.Id;
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

            await _transitionBalanceUpdater.Update(bill, oldsum, oldAccountId);

            await _transactionBuilder.Update(bill, oldAccountId);

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
