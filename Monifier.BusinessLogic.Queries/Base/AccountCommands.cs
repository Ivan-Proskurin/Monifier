using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class AccountCommands : IAccountCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;

        public AccountCommands(IUnitOfWork unitOfWork, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
        }

        public async Task<AccountModel> Update(AccountModel model)
        {
            var queries = _unitOfWork.GetNamedModelQueryRepository<Account>();
            var commands = _unitOfWork.GetCommandRepository<Account>();

            var other = await queries.GetByName(_currentSession.UserId, model.Name);
            if (other != null)
                if (other.Id != model.Id)
                    throw new ArgumentException("Счет с таким названием уже есть");
                else
                    commands.Detach(other);

            var account = new Account
            {
                Name = model.Name,
                Balance = model.Balance,
                AvailBalance = model.AvailBalance,
                DateCreated = model.DateCreated,
                Number = model.Number,
                IsDeleted = false,
                OwnerId = _currentSession.UserId
            };
            if (model.Id < 0)
            {
                account.AvailBalance = model.Balance;
                commands.Create(account);
            }
            else
            {
                account.Id = model.Id;
                commands.Update(account);
            }

            await _unitOfWork.SaveChangesAsync();

            model.Id = account.Id;
            return model;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            var accountRepo = _unitOfWork.GetQueryRepository<Account>();
            var account = await accountRepo.GetById(id);
            if (account == null)
                throw new ArgumentException($"Нет счета с Id = {id}");
            
            if (onlyMark)
            {
                account.IsDeleted = true;
            }
            else
            {
                var accountCommands = _unitOfWork.GetCommandRepository<Account>();
                accountCommands.Delete(account);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Topup(TopupAccountModel topup)
        {
            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var incomeCommands = _unitOfWork.GetCommandRepository<IncomeItem>();
            var account = await accountQueries.GetById(topup.AccountId);
            var incomeTypeId = topup.IncomeTypeId;
            if (incomeTypeId == null)
            {
                var incomeTypeCommands = _unitOfWork.GetCommandRepository<IncomeType>();
                var incomeType = new IncomeType {Name = topup.AddIncomeTypeName};
                incomeTypeCommands.Create(incomeType);
                await _unitOfWork.SaveChangesAsync();
                incomeTypeId = incomeType.Id;
            }
            incomeCommands.Create(new IncomeItem
            {
                AccountId = account.Id,
                DateTime = topup.TopupDate,
                IncomeTypeId = incomeTypeId.Value,
                Total = topup.Amount
            });
            if (!topup.Correcting)
            {
                account.Balance += topup.Amount;
            }
            account.AvailBalance += topup.Amount;
            accountCommands.Update(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Transfer(int accountFromId, int accountToId, decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Сумма перевода не должна быть меньше нуля", nameof(amount));
            
            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var accountFrom = await accountQueries.GetById(accountFromId);
            if (accountFrom == null)
                throw new ArgumentException($"Нет счета с Id = {accountFromId}");
            var accountTo = await accountQueries.GetById(accountToId);
            if (accountTo == null)
                throw new ArgumentException($"Нет счета с Id = {accountToId}");
            
            if (accountFrom.Balance < amount)
                throw new InvalidOperationException(
                    $"Невозможно перевести сумму {amount} со счета \"{accountFrom.Name}\", так как на его балансе не хватает средств");

            accountFrom.Balance -= amount;
            accountFrom.AvailBalance -= amount;
            accountTo.Balance += amount;
            accountTo.AvailBalance += amount;
            accountCommands.Update(accountFrom);
            accountCommands.Update(accountTo);
            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task TransferToExpenseFlow(int flowId, int fromAccountId, decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Сумма перевода не должна быть меньше нуля", nameof(amount));
            
            var flowQueries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flow = await flowQueries.GetById(flowId);
            if (flow == null)
                throw new ArgumentException($"Нет категории расходов с Id = {flowId}", nameof(flowId));

            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var account = await accountQueries.GetById(fromAccountId);
            if (account == null)
                throw new ArgumentException($"Нет такого счета с Id = {fromAccountId}", nameof(fromAccountId));
            
            if (account.AvailBalance < amount)
                throw new InvalidOperationException(
                    $"Невозможно перевести сумму {amount} со счета \"{account.Name}\", так как на его доступном балансе не хватает средств");

            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            
            account.AvailBalance -= amount;
            flow.Balance += amount;
            
            accountCommands.Update(account);
            flowCommands.Update(flow);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}