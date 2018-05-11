using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class CreditCardBalancesUpdater : IBalancesUpdater
    {
        public Task Create(Account account, ExpenseBill bill)
        {
            throw new System.NotImplementedException();
        }

        public Task Delete(ExpenseBill bill)
        {
            throw new System.NotImplementedException();
        }
    }
}