using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeItemCommands : IIncomeItemCommands
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;
        private readonly ITransactionBuilder _transactionBuilder;

        public IncomeItemCommands(IEntityRepository repository,
            ICurrentSession currentSession,
            ITransactionBuilder transactionBuilder)
        {
            _repository = repository;
            _currentSession = currentSession;
            _transactionBuilder = transactionBuilder;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new NotSupportedException("Операция не поддерживается");
            var income = await _repository.LoadAsync<IncomeItem>(id).ConfigureAwait(false);
            var account = await _repository.LoadAsync<Account>(income.AccountId).ConfigureAwait(false);
            account.AvailBalance -= income.Total;
            if (!income.IsCorrection)
                account.Balance -= income.Total;
            _repository.Update(account);
            _repository.Delete(income);
            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IncomeItemModel> Update(IncomeItemModel model)
        {
            var item = new IncomeItem
            {
                Id = model.Id,
                AccountId = model.AccountId,
                DateTime = model.DateTime,
                IncomeTypeId = model.IncomeTypeId,
                Total = model.Total,
                OwnerId = _currentSession.UserId,
                IsCorrection = model.IsCorrection,
            };
            if (item.Id > 0)
            {
                var oldItem = await _repository.LoadAsync<IncomeItem>(item.Id).ConfigureAwait(false);
                var accountId = oldItem.AccountId;
                decimal balance;
                if (oldItem.AccountId == item.AccountId)
                {
                    var account = await _repository.LoadAsync<Account>(item.AccountId).ConfigureAwait(false);
                    if (!item.IsCorrection)
                        account.Balance += item.Total - oldItem.Total;
                    account.AvailBalance += item.Total - oldItem.Total;
                    _repository.Update(account);
                    balance = account.Balance;
                }
                else
                {
                    var account1 = await _repository.LoadAsync<Account>(oldItem.AccountId).ConfigureAwait(false);
                    var account2 = await _repository.LoadAsync<Account>(item.AccountId).ConfigureAwait(false);
                    if (!oldItem.IsCorrection)
                        account1.Balance -= oldItem.Total;
                    account1.AvailBalance -= oldItem.Total;
                    if (!item.IsCorrection)
                        account2.Balance += item.Total;
                    account2.AvailBalance += item.Total;
                    _repository.Update(account1);
                    _repository.Update(account2);
                    balance = account2.Balance;
                }
                _repository.Update(item);
                await _transactionBuilder.UpdateIncome(accountId, item, balance).ConfigureAwait(false);
                await _repository.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                var account = await _repository.LoadAsync<Account>(item.AccountId).ConfigureAwait(false);
                if (!item.IsCorrection)
                    account.Balance += item.Total;
                account.AvailBalance += item.Total;
                _repository.Update(account);
                _repository.Create(item);
                await _repository.SaveChangesAsync().ConfigureAwait(false);
                _transactionBuilder.CreateIncome(item, account.Balance);
                await _repository.SaveChangesAsync().ConfigureAwait(false);
            }
            model.Id = item.Id;
            return model;
        }
    }
}