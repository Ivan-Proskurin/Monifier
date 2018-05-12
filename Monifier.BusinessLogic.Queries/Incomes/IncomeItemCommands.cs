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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;
        private readonly ITransactionBuilder _transactionBuilder;

        public IncomeItemCommands(IUnitOfWork unitOfWork,
            ICurrentSession currentSession,
            ITransactionBuilder transactionBuilder)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
            _transactionBuilder = transactionBuilder;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new NotSupportedException("Операция не поддерживается");
            var incomeCommands = _unitOfWork.GetCommandRepository<IncomeItem>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var income = await _unitOfWork.LoadEntity<IncomeItem>(id).ConfigureAwait(false);
            var account = await _unitOfWork.LoadEntity<Account>(income.AccountId).ConfigureAwait(false);
            account.AvailBalance -= income.Total;
            if (!income.IsCorrection)
                account.Balance -= income.Total;
            accountCommands.Update(account);
            incomeCommands.Delete(income);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IncomeItemModel> Update(IncomeItemModel model)
        {
            var incomeCommands = _unitOfWork.GetCommandRepository<IncomeItem>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
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
                var oldItem = await _unitOfWork.LoadEntity<IncomeItem>(item.Id).ConfigureAwait(false);
                var accountId = oldItem.AccountId;
                decimal balance;
                if (oldItem.AccountId == item.AccountId)
                {
                    var account = await _unitOfWork.LoadEntity<Account>(item.AccountId).ConfigureAwait(false);
                    if (!item.IsCorrection)
                        account.Balance += item.Total - oldItem.Total;
                    account.AvailBalance += item.Total - oldItem.Total;
                    accountCommands.Update(account);
                    balance = account.Balance;
                }
                else
                {
                    var account1 = await _unitOfWork.LoadEntity<Account>(oldItem.AccountId).ConfigureAwait(false);
                    var account2 = await _unitOfWork.LoadEntity<Account>(item.AccountId).ConfigureAwait(false);
                    if (!oldItem.IsCorrection)
                        account1.Balance -= oldItem.Total;
                    account1.AvailBalance -= oldItem.Total;
                    if (!item.IsCorrection)
                        account2.Balance += item.Total;
                    account2.AvailBalance += item.Total;
                    accountCommands.Update(account1);
                    accountCommands.Update(account2);
                    balance = account2.Balance;
                }
                incomeCommands.Update(item);
                await _transactionBuilder.UpdateIncome(accountId, item, balance).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                var account = await _unitOfWork.LoadEntity<Account>(item.AccountId).ConfigureAwait(false);
                if (!item.IsCorrection)
                    account.Balance += item.Total;
                account.AvailBalance += item.Total;
                accountCommands.Update(account);
                incomeCommands.Create(item);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                _transactionBuilder.CreateIncome(item, account.Balance);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            model.Id = item.Id;
            return model;
        }
    }
}