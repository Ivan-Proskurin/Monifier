using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;

namespace Monifier.Web.Pages.Accounts
{
    public class CreateAccountModel : PageModel
    {
        private readonly IAccountCommands _accountCommands;
        private readonly IAccountQueries _accountQueries;

        public CreateAccountModel(IAccountCommands accountCommands, IAccountQueries accountQueries)
        {
            _accountCommands = accountCommands;
            _accountQueries = accountQueries;
        }
        
        [BindProperty]
        public EditAccount Account { get; set; }

        public async Task OnGetAsync()
        {
            Account = new EditAccount
            {
                Id = -1,
                Number = await _accountQueries.GetNextNumber(),
                CreationDate = DateTime.Now.ToStandardString(false)
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Account.ProcessAsync(ModelState,
                async () =>
                {
                    var model = new AccountModel {Id = -1};
                    Account.ToAccountModel(model);
                    await _accountCommands.Update(model);
                    return RedirectToPage("./AccountsList");
                },

                async () => await Task.FromResult(Page())
            );
        }
    }
}