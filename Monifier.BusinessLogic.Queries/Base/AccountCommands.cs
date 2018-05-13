using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class AccountCommands : IAccountCommands
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;
        private readonly IIncomeItemCommands _incomeItemCommands;
        private readonly ITimeService _timeService;
        private readonly ITransactionBuilder _transactionBuilder;

        public AccountCommands(IEntityRepository repository, 
            ICurrentSession currentSession,
            IIncomeItemCommands incomeItemCommands,
            ITimeService timeService,
            ITransactionBuilder transactionBuilder)
        {
            _repository = repository;
            _currentSession = currentSession;
            _incomeItemCommands = incomeItemCommands;
            _timeService = timeService;
            _transactionBuilder = transactionBuilder;
        }

        public async Task<AccountModel> Update(AccountModel model)
        {
            var other = await _repository.FindByNameAsync<Account>(_currentSession.UserId, model.Name).ConfigureAwait(false);
            if (other != null)
                if (other.Id != model.Id)
                    throw new ArgumentException("Счет с таким названием уже есть");
                else
                    _repository.Detach(other);

            var account = new Account
            {
                Name = model.Name,
                Balance = model.Balance,
                AvailBalance = model.AvailBalance,
                DateCreated = model.DateCreated,
                Number = model.Number,
                IsDeleted = false,
                OwnerId = _currentSession.UserId,
                IsDefault = model.IsDefault,
                AccountType = model.AccountType
            };

            if (account.IsDefault)
            {
                var all = await _repository.GetQuery<Account>().ToListAsync().ConfigureAwait(false);
                all.ForEach(x =>
                {
                    x.IsDefault = false;
                    _repository.Update(x);
                });
            }

            if (model.Id < 0)
            {
                account.AvailBalance = model.Balance;
                _repository.Create(account);
            }
            else
            {
                account.Id = model.Id;
                _repository.Update(account);
            }

            await _repository.SaveChangesAsync().ConfigureAwait(false);

            model.Id = account.Id;
            return model;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            var account = await _repository.LoadAsync<Account>(id).ConfigureAwait(false);
            if (account == null)
                throw new ArgumentException($"Нет счета с Id = {id}");
            
            if (onlyMark)
            {
                account.IsDeleted = true;
            }
            else
            {
                _repository.Delete(account);
            }
            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IncomeItemModel> Topup(TopupAccountModel topup)
        {
            var account = await _repository.LoadAsync<Account>(topup.AccountId).ConfigureAwait(false);
            var incomeTypeId = topup.IncomeTypeId;
            if (incomeTypeId == null)
            {
                var incomeType = new IncomeType
                {
                    Name = topup.AddIncomeTypeName,
                    OwnerId = _currentSession.UserId
                };
                _repository.Create(incomeType);
                await _repository.SaveChangesAsync().ConfigureAwait(false);
                incomeTypeId = incomeType.Id;
            }

            var income = new IncomeItemModel
            {
                AccountId = account.Id,
                DateTime = topup.TopupDate,
                IncomeTypeId = incomeTypeId.Value,
                Total = topup.Amount,
                IsCorrection = topup.Correction,
            };

            await _incomeItemCommands.Update(income).ConfigureAwait(false);

            await _repository.SaveChangesAsync().ConfigureAwait(false);
            return income;
        }

        public async Task<DateTime> Transfer(int accountFromId, int accountToId, decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Сумма перевода не должна быть меньше нуля", nameof(amount));
            
            var accountFrom = await _repository.LoadAsync<Account>(accountFromId);
            if (accountFrom == null)
                throw new ArgumentException($"Нет счета с Id = {accountFromId}");
            var accountTo = await _repository.LoadAsync<Account>(accountToId);
            if (accountTo == null)
                throw new ArgumentException($"Нет счета с Id = {accountToId}");
            
            if (accountToId == accountFromId)
                throw new InvalidOperationException("Нельзя вполнить перевод между одинковыми счетами");
            if (accountFrom.Balance < amount)
                throw new InvalidOperationException(
                    $"Невозможно перевести сумму {amount} со счета \"{accountFrom.Name}\", так как на его балансе не хватает средств");

            var transferTime = _timeService.ClientLocalNow;

            accountFrom.Balance -= amount;
            accountFrom.AvailBalance -= amount;
            accountTo.Balance += amount;
            accountTo.AvailBalance += amount;
            _repository.Update(accountFrom);
            _repository.Update(accountTo);
            _transactionBuilder.CreateTransfer(accountFrom, accountTo, transferTime, amount);
            await _repository.SaveChangesAsync().ConfigureAwait(false);

            return transferTime;
        }

        public async Task TransferToExpenseFlow(int flowId, int fromAccountId, decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Сумма перевода не должна быть меньше нуля", nameof(amount));
            
            var flow = await _repository.LoadAsync<ExpenseFlow>(flowId);
            if (flow == null)
                throw new ArgumentException($"Нет категории расходов с Id = {flowId}", nameof(flowId));

            var account = await _repository.LoadAsync<Account>(fromAccountId);
            if (account == null)
                throw new ArgumentException($"Нет такого счета с Id = {fromAccountId}", nameof(fromAccountId));
            
            if (account.AvailBalance < amount)
                throw new InvalidOperationException(
                    $"Невозможно перевести сумму {amount} со счета \"{account.Name}\", так как на его доступном балансе не хватает средств");

            account.AvailBalance -= amount;
            flow.Balance += amount;
            
            _repository.Update(account);
            _repository.Update(flow);

            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}