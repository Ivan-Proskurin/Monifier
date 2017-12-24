using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Extensions;
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
        
        public List<AccountModel> Accounts { get; private set; }
        
        public string AvailableBalance { get; private set; }

        public async Task OnGetAsync(int? fromId = null, int? toId = null)
        {
            Accounts = await _accountQueries.GetAll();
            Transfer = new Transfer();
            if (fromId != null)
            {
                var account = Accounts.SingleOrDefault(x => x.Id == fromId.Value);
                Transfer.AccountFrom = account?.Name;
                AvailableBalance = account?.Balance.ToMoney();
            }
            else
            {
                AvailableBalance = "-";
            }
            
            if (toId != null)
            {
                Transfer.AccountTo = Accounts.SingleOrDefault(x => x.Id == toId.Value)?.Name;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Accounts = await _accountQueries.GetAll();
            
            return await Transfer.ProcessAsync(ModelState,
                async () =>
                {
                    var accountFrom = await _accountQueries.GetByName(Transfer.AccountFrom);
                    var accountTo = await _accountQueries.GetByName(Transfer.AccountTo);
                    await _accountCommands.Transfer(accountFrom.Id, accountTo.Id, Transfer.Amount.ParseMoneyInvariant());
                    return RedirectToPage("./AccountsList");
                },
                
                async () =>
                {
                    var account = await _accountQueries.GetByName(Transfer.AccountFrom);
                    AvailableBalance = account != null ? account?.Balance.ToMoney() : "-";
                    return Page();
                },
                
                async vrList =>
                {
                    var accountFrom = await _accountQueries.GetByName(Transfer.AccountFrom);
                    var accountTo = await _accountQueries.GetByName(Transfer.AccountTo);
                    if (accountFrom == null) vrList.Add(new ModelValidationResult("Transfer.AccountFrom", "Такого счета нет"));
                    if (accountTo == null) vrList.Add(new ModelValidationResult("Transfer.AccountTo", "Такого счета нет"));
                });
        }

        public async Task<JsonResult> OnPostAccountBalanceAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async name =>
            {
                var account = await _accountQueries.GetByName(name);
                return account == null ? "-" : account.Balance.ToMoney();
            });
        }
    }
}