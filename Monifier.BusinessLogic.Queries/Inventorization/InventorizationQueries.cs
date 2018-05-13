using System.Linq;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Inventorization;

namespace Monifier.BusinessLogic.Queries.Inventorization
{
    public class InventorizationQueries : IInventorizationQueries
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IExpenseFlowQueries _flowQueries;

        public InventorizationQueries(
            IAccountQueries accountQueries,
            IExpenseFlowQueries flowQueries)
        {
            _accountQueries = accountQueries;
            _flowQueries = flowQueries;
        }
        
        public async Task<BalanceState> GetBalanceState()
        {
            var accounts = await _accountQueries.GetAll().ConfigureAwait(false);
            var flows = await _flowQueries.GetAll().ConfigureAwait(false);
            var balance = accounts.Sum(x => x.Balance) - accounts.Sum(x => x.AvailBalance) - flows.Sum(x => x.Balance);
            return new BalanceState
            {
                ShowPrompt = balance != 0,
                Balance = balance
            };
        }
    }
}