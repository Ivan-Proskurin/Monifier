using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Accounts;
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
        private readonly ITransactionCommands _transactionCommands;
        private readonly ITransactionQueries _transactionQueries;

        public IncomeItemCommands(IUnitOfWork unitOfWork,
            ICurrentSession currentSession,
            ITransactionCommands transactionCommands,
            ITransactionQueries transactionQueries)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
            _transactionCommands = transactionCommands;
            _transactionQueries = transactionQueries;
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
            int accountId;
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
                accountId = oldItem.AccountId;
                if (oldItem.AccountId == item.AccountId)
                {
                    var account = await _unitOfWork.LoadEntity<Account>(item.AccountId).ConfigureAwait(false);
                    if (!item.IsCorrection)
                        account.Balance += item.Total - oldItem.Total;
                    account.AvailBalance += item.Total - oldItem.Total;
                    accountCommands.Update(account);
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
                }
                incomeCommands.Update(item);
            }
            else
            {
                accountId = item.AccountId;
                var account = await _unitOfWork.LoadEntity<Account>(item.AccountId).ConfigureAwait(false);
                if (!item.IsCorrection)
                    account.Balance += item.Total;
                account.AvailBalance += item.Total;
                accountCommands.Update(account);
                incomeCommands.Create(item);
            }
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            model.Id = item.Id;

            var transaction = await _transactionQueries.GetIncomeTransaction(accountId, model.Id).ConfigureAwait(false);
            if (transaction == null)
            {
                transaction = new TransactionModel
                {
                    InitiatorId = model.AccountId,
                    DateTime = model.DateTime,
                    IncomeId = model.Id,
                    Total = model.Total
                };
            }
            else
            {
                transaction.InitiatorId = model.AccountId;
                transaction.Total = model.Total;
            }

            await _transactionCommands.Update(transaction).ConfigureAwait(false);

            return model;
        }
    }
}