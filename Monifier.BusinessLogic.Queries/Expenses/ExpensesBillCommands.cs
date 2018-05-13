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
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;
        private readonly IBalancesUpdaterFactory _balancesUpdaterFactory;
        private readonly ITransitionBalanceUpdater _transitionBalanceUpdater;
        private readonly ITransactionBuilder _transactionBuilder;

        public ExpensesBillCommands(
            IEntityRepository repository, 
            ICurrentSession currentSession,
            IBalancesUpdaterFactory balancesUpdaterFactory,
            ITransitionBalanceUpdater transitionBalanceUpdater,
            ITransactionBuilder transactionBuilder)
        {
            _repository = repository;
            _currentSession = currentSession;
            _balancesUpdaterFactory = balancesUpdaterFactory;
            _transitionBalanceUpdater = transitionBalanceUpdater;
            _transactionBuilder = transactionBuilder;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new NotSupportedException("Операция не поддерживается");

            var bill = await _repository.LoadAsync<ExpenseBill>(id);
            if (bill == null)
                throw new ArgumentException($"Нет чека с Id = {id}");

            var account = bill.AccountId != null ? await _repository.LoadAsync<Account>(bill.AccountId.Value) : null;
            if (account != null)
            {
                var balanceUpdater = _balancesUpdaterFactory.Create(account.AccountType);
                await balanceUpdater.Delete(bill);
            }

            await _transactionBuilder.DeleteExpense(bill).ConfigureAwait(false);

            _repository.Delete(bill);

            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<int> Create(ExpenseBillModel model, bool correction = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Validate();

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

            _repository.Create(bill);

            foreach (var item in model.Items)
            {
                _repository.Create(new ExpenseItem
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
                account = await _repository.LoadAsync<Account>(model.AccountId.Value);
            }

            if (account != null)
            {
                var balancesUpdater = _balancesUpdaterFactory.Create(account.AccountType);
                await balancesUpdater.Create(account, bill).ConfigureAwait(false);
                _transactionBuilder.CreateExpense(bill, account.Balance);
            }

            await _repository.SaveChangesAsync().ConfigureAwait(false);

            model.Id = bill.Id;
            return model.Id;
        }

        public async Task<ExpenseBillModel> Update(ExpenseBillModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Validate();

            var bill = await _repository.LoadAsync<ExpenseBill>(model.Id);
            if (bill == null)
                throw new ArgumentException($"Нет счета с Id = {model.Id}");

            var oldsum = bill.SumPrice;
            var oldAccountId = bill.AccountId;
            bill.DateTime = model.DateTime;
            bill.SumPrice = model.Cost;
            bill.ExpenseFlowId = model.ExpenseFlowId;
            bill.AccountId = model.AccountId;

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
                        _repository.Create(itemModel);
                    else
                    {
                        itemModel.Id = item.Id;
                        _repository.Update(itemModel);
                    }
                }
                else if (item.IsDeleted)
                {
                    var itemModel = await _repository.LoadAsync<ExpenseItem>(item.Id);
                    if (itemModel == null) continue;
                    _repository.Delete(itemModel);
                }
            }

            _repository.Update(bill);

            var balance = await _transitionBalanceUpdater.Update(bill, oldsum, oldAccountId);

            await _transactionBuilder.UpdateExpense(bill, oldAccountId, balance);

            await _repository.SaveChangesAsync().ConfigureAwait(false);
            return model;
        }

        public Task Save(ExpenseBillModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.IsNew)
                return Create(model);
            else
                return Update(model);
        }
    }
}
