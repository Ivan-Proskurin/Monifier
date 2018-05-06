using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Transactions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Accounts
{
    public class TransactionListModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly ITransactionQueries _transactionQueries;
        private readonly IUserSettings _userSettings;

        public TransactionListModel(
            IAccountQueries accountQueries,
            ITransactionQueries transactionQueries,
            IUserSettings userSettings)
        {
            _accountQueries = accountQueries;
            _transactionQueries = transactionQueries;
            _userSettings = userSettings;
        }

        private async Task PrepareModelsAsync(TransactionFilterViewModel filterViewModel)
        {
            Account = await _accountQueries.GetById(filterViewModel.AccountId);
            Transactions = await _transactionQueries.GetAllTransactions(Filter.ToFilter(), new PaginationArgs
            {
                PageNumber = filterViewModel.PageNumber,
                ItemsPerPage = _userSettings.ItemsPerPage
            });
        }

        public async Task OnGetAsync(int accountId, int? pageNumber = 1)
        {
            Filter = new TransactionFilterViewModel
            {
                Type = string.Empty,
                AccountId = accountId,
                PageNumber = pageNumber ?? 1
            };
            await PrepareModelsAsync(Filter);
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            return await Filter.ProcessAsync(ModelState, nameof(Filter),
                async () =>
                {
                    await PrepareModelsAsync(Filter);
                    return Page();
                },
                async () =>
                {
                    await PrepareModelsAsync(Filter);
                    return Page();
                },
                vrList =>
                {
                    if (!Filter.Type.IsNullOrEmpty() && !FilterTypes.Contains(Filter.Type))
                    {
                        vrList.Add(new ModelValidationResult(nameof(Filter.Type), "Некорректный тип фильтра"));
                    }

                    return Task.CompletedTask;
                });
        }

        public TransactionPaginatedList Transactions { get; private set; }

        public AccountModel Account { get; private set; }

        [BindProperty] public TransactionFilterViewModel Filter { get; set; }

        public List<string> FilterTypes { get; } = new List<string>()
        {
            "Оплата",
            "Перевод",
            "Поступление"
        };
    }
}