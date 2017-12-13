using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Accounts
{
    public class TransferModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IAccountCommands _accountCommands;

        public TransferModel(IAccountQueries accountQueries, IAccountCommands accountCommands)
        {
            _accountQueries = accountQueries;
            _accountCommands = accountCommands;
        }
        
        [BindProperty]
        public Transfer Transfer { get; set; }
        
        public List<AccountModel> Accounts { get; set; }

        public async Task OnGetAsync()
        {
            Transfer = new Transfer();
            Accounts = await _accountQueries.GetAll();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Accounts = await _accountQueries.GetAll();
            var accountFrom = Accounts.SingleOrDefault(x => x.Name == Transfer.AccountFrom);
            var accountTo = Accounts.SingleOrDefault(x => x.Name == Transfer.AccountTo);
            
            return await Transfer.ProcessAsync(ModelState,
                async () =>
                {
                    await _accountCommands.Transfer(accountFrom.Id, accountTo.Id, Transfer.Amount.ParseMoneyInvariant());
                    return RedirectToPage("./AccountsList");
                },
                
                async () => await Task.FromResult(Page()),
                
                async vrList =>
                {
                    if (accountFrom == null) vrList.Add(new ModelValidationResult("Transfer.AccountFrom", "Такого счета нет"));
                    if (accountTo == null) vrList.Add(new ModelValidationResult("Transfer.AccountTo", "Такого счета нет"));
                    await Task.CompletedTask;
                });
        }
    }
}