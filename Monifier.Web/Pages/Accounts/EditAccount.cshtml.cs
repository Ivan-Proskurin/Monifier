using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;

namespace Monifier.Web.Pages.Accounts
{
    public class EditAccountModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IAccountCommands _accountCommands;

        public EditAccountModel(IAccountQueries accountQueries, IAccountCommands accountCommands)
        {
            _accountQueries = accountQueries;
            _accountCommands = accountCommands;
        }
        
        [BindProperty]
        public EditAccount Account { get; set; }

        public async Task OnGetAsync(int id)
        {
            var account = await _accountQueries.GetById(id);
            Account = account.ToEditAccount();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Account.ProcessAsync(ModelState,
                async () =>
                {
                    await _accountCommands.Update(Account.ToAccountModel());
                    return RedirectToPage("./AccountsList");
                },

                async () => await Task.FromResult(Page())
            );
        }

        public async Task<IActionResult> OnPostDeleteAsync(bool permanent)
        {
            await _accountCommands.Delete(Account.Id, !permanent);
            return RedirectToPage("./AccountsList");
        }
    }
}