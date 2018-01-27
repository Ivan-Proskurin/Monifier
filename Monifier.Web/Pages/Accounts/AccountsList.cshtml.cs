using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Inventorization;

namespace Monifier.Web.Pages.Accounts
{
    public class AccountsListModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IInventorizationQueries _inventorizationQueries;

        public AccountsListModel(IAccountQueries accountQueries, IInventorizationQueries inventorizationQueries)
        {
            _accountQueries = accountQueries;
            _inventorizationQueries = inventorizationQueries;
        }
        
        public AccountList Accounts { get; private set; }
        public bool CanDistribute { get; private set; }
        
        public BalanceState BalanceState { get; private set; }

        public async Task OnGetAsync()
        {
            Accounts = await _accountQueries.GetList();
            CanDistribute = Accounts.Accounts.Count(x => x.AvailBalance > 0) > 0;
            BalanceState = await _inventorizationQueries.GetBalanceState();
        }
    }
}