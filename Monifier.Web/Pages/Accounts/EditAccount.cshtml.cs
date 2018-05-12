using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Processing;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;

namespace Monifier.Web.Pages.Accounts
{
    [Authorize]
    public class EditAccountModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountQueries _accountQueries;
        private readonly IAccountCommands _accountCommands;
        private readonly ITransactionQueries _transactionQueries;
        private readonly ICreditCardProcessing _creditCardProcessing;

        public EditAccountModel(
            IUnitOfWork unitOfWork,
            IAccountQueries accountQueries, 
            IAccountCommands accountCommands,
            ITransactionQueries transactionQueries,
            ICreditCardProcessing creditCardProcessing)
        {
            _unitOfWork = unitOfWork;
            _accountQueries = accountQueries;
            _accountCommands = accountCommands;
            _transactionQueries = transactionQueries;
            _creditCardProcessing = creditCardProcessing;
        }
        
        [BindProperty]
        public EditAccount Account { get; set; }

        public List<string> AccountTypes { get; private set; }

        public List<TransactionViewModel> Transactions { get; private set; }

        private async Task PrepareModelsAsync(int accountId)
        {
            AccountTypes = AccountTypeHelper.GetAllHumanNames();
            Transactions = await _transactionQueries.GetLastTransactions(accountId, 3);
        }

        public async Task OnGetAsync(int id)
        {
            var account = await _accountQueries.GetById(id);
            await PrepareModelsAsync(account.Id);
            Account = account.ToEditAccount();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Account.ProcessAsync(ModelState, nameof(Account),
                async () =>
                {
                    var model = await _accountQueries.GetById(Account.Id);
                    var oldBalance = model.Balance;
                    Account.ToAccountModel(model);
                    await _accountCommands.Update(model);
                    if (model.Balance < oldBalance && model.AccountType == AccountType.CreditCard)
                    {
                        var account = await _unitOfWork.GetQueryRepository<Account>().GetById(model.Id);
                        await _creditCardProcessing.ProcessReducingBalanceAsCreditFees(account, oldBalance - model.Balance);
                    }
                    return RedirectToPage("./AccountsList");
                },

                async () =>
                {
                    await PrepareModelsAsync(Account.Id);
                    return Page();
                });
        }

        public async Task<IActionResult> OnPostDeleteAsync(bool permanent)
        {
            await _accountCommands.Delete(Account.Id, !permanent);
            return RedirectToPage("./AccountsList");
        }
    }
}