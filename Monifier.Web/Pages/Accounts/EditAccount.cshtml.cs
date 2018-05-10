using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.DataAccess.Model.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;

namespace Monifier.Web.Pages.Accounts
{
    [Authorize]
    public class EditAccountModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IAccountCommands _accountCommands;
        private readonly ITransactionQueries _transactionQueries;

        public EditAccountModel(
            IAccountQueries accountQueries, 
            IAccountCommands accountCommands,
            ITransactionQueries transactionQueries)
        {
            _accountQueries = accountQueries;
            _accountCommands = accountCommands;
            _transactionQueries = transactionQueries;
        }
        
        [BindProperty]
        public EditAccount Account { get; set; }

        public List<string> AccountTypes { get; private set; }

        public List<TransactionViewModel> Transactions { get; private set; }

        public async Task OnGetAsync(int id)
        {
            var account = await _accountQueries.GetById(id);
            AccountTypes = AccountTypeHelper.GetAllHumanNames();
            Transactions = await _transactionQueries.GetLastTransactions(account.Id, 3);
            Account = account.ToEditAccount();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Account.ProcessAsync(ModelState, nameof(Account),
                async () =>
                {
                    var model = await _accountQueries.GetById(Account.Id);
                    Account.ToAccountModel(model);
                    await _accountCommands.Update(model);
                    return RedirectToPage("./AccountsList");
                },

                async () =>
                {
                    Transactions = await _transactionQueries.GetLastTransactions(Account.Id, 3);
                    AccountTypes = AccountTypeHelper.GetAllHumanNames();
                    return await Task.FromResult(Page());
                });
        }

        public async Task<IActionResult> OnPostDeleteAsync(bool permanent)
        {
            await _accountCommands.Delete(Account.Id, !permanent);
            return RedirectToPage("./AccountsList");
        }
    }
}