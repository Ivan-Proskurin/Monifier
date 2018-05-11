using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class CreditCardBalancesUpdater : IBalancesUpdater
    {
        private readonly IUnitOfWork _untOfWork;

        public CreditCardBalancesUpdater(IUnitOfWork untOfWork)
        {
            _untOfWork = untOfWork;
        }

        public Task Create(Account account, ExpenseBill bill)
        {
            if (!bill.IsCorrection)
                account.Balance -= bill.SumPrice;
            account.AvailBalance -= bill.SumPrice;
            var accountCommands = _untOfWork.GetCommandRepository<Account>();
            accountCommands.Update(account);
            return Task.CompletedTask;
        }

        public async Task Delete(ExpenseBill bill)
        {
            if (bill.AccountId == null) return;
            var accountQueries = _untOfWork.GetQueryRepository<Account>();
            var account = await accountQueries.GetById(bill.AccountId.Value);
            if (!bill.IsCorrection)
                account.Balance += bill.SumPrice;
            account.AvailBalance += bill.SumPrice;
            var accountCommands = _untOfWork.GetCommandRepository<Account>();
            accountCommands.Update(account);
        }
    }
}