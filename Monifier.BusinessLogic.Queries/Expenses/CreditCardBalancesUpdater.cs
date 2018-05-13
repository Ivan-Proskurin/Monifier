using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class CreditCardBalancesUpdater : IBalancesUpdater
    {
        private readonly IEntityRepository _repository;

        public CreditCardBalancesUpdater(IEntityRepository repository)
        {
            _repository = repository;
        }

        public Task Create(Account account, ExpenseBill bill)
        {
            if (!bill.IsCorrection)
                account.Balance -= bill.SumPrice;
            account.AvailBalance -= bill.SumPrice;
            _repository.Update(account);
            return Task.CompletedTask;
        }

        public async Task Delete(ExpenseBill bill)
        {
            if (bill.AccountId == null) return;
            var account = await _repository.LoadAsync<Account>(bill.AccountId.Value);
            if (!bill.IsCorrection)
                account.Balance += bill.SumPrice;
            account.AvailBalance += bill.SumPrice;
            _repository.Update(account);
        }
    }
}