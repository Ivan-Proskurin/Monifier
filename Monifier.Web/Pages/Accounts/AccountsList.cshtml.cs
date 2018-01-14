using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.Web.Pages.Accounts
{
    public class AccountsListModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;

        public AccountsListModel(IAccountQueries accountQueries)
        {
            _accountQueries = accountQueries;
        }
        
        public AccountList Accounts { get; private set; }
        public bool CanDistribute { get; private set; }

        public async Task OnGetAsync()
        {
            Accounts = await _accountQueries.GetList();
            CanDistribute = Accounts.Accounts.Count(x => x.AvailBalance > 0) > 0;
        }
    }
}