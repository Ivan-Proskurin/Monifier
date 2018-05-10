using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Model.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;

namespace Monifier.Web.Pages.Accounts
{
    [Authorize]
    public class CreateAccountModel : PageModel
    {
        private readonly IAccountCommands _accountCommands;
        private readonly IAccountQueries _accountQueries;
        private readonly ITimeService _timeService;

        public CreateAccountModel(
            IAccountCommands accountCommands, 
            IAccountQueries accountQueries,
            ITimeService timeService)
        {
            _accountCommands = accountCommands;
            _accountQueries = accountQueries;
            _timeService = timeService;
        }
        
        [BindProperty]
        public EditAccount Account { get; set; }

        public List<string> AccountTypes { get; private set; }

        public async Task OnGetAsync()
        {
            Account = new EditAccount
            {
                Id = -1,
                Number = await _accountQueries.GetNextNumber(),
                CreationDate = _timeService.ClientLocalNow.ToStandardString(false)
            };
            AccountTypes = AccountTypeHelper.GetAllHumanNames();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Account.ProcessAsync(ModelState, nameof(Account),
                async () =>
                {
                    var model = new AccountModel {Id = -1};
                    Account.ToAccountModel(model);
                    await _accountCommands.Update(model);
                    return RedirectToPage("./AccountsList");
                },

                async () =>
                {
                    AccountTypes = AccountTypeHelper.GetAllHumanNames();
                    return await Task.FromResult(Page());
                });
        }
    }
}