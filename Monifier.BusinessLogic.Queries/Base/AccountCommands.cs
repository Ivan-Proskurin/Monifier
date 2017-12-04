using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class AccountCommands : IAccountCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountModel> Update(AccountModel model)
        {
            var queries = _unitOfWork.GetNamedModelQueryRepository<Account>();
            var commands = _unitOfWork.GetCommandRepository<Account>();

            var other = await queries.GetByName(model.Name);
            if (other != null)
                if (other.Id != model.Id)
                    throw new ArgumentException("Счет с таким названием уже есть");
                else
                    commands.Detach(other);

            var account = new Account
            {
                Name = model.Name,
                Balance = model.Balance,
                DateCreated = model.DateCreated,
                Number = model.Number,
                IsDeleted = false
            };
            if (model.Id < 0)
                commands.Create(account);
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
            var accountRepo = _unitOfWork.GetQueryRepository<Account>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var incomeCommands = _unitOfWork.GetCommandRepository<IncomeItem>();
            var account = await accountRepo.GetById(topup.AccountId);
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
            account.Balance += topup.Amount;
            accountCommands.Update(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Transfer(int accountFromId, int accountToId, decimal amount)
        {
            var accountRepo = _unitOfWork.GetQueryRepository<Account>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var accountFrom = await accountRepo.GetById(accountFromId);
            if (accountFrom == null)
                throw new ArgumentException($"Нет счета с Id = {accountFromId}");
            var accountTo = await accountRepo.GetById(accountToId);
            if (accountTo == null)
                throw new ArgumentException($"Нет счета с Id = {accountToId}");
            
            accountFrom.Balance -= amount;
            accountTo.Balance += amount;
            accountCommands.Update(accountFrom);
            accountCommands.Update(accountTo);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}