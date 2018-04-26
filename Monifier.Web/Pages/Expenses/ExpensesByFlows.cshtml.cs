using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Inventorization;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class ExpensesByFlowsPageModel : PageModel
    {
        private readonly IExpensesQueries _expensesQueries;
        private readonly IUserSettings _userSettings;
        private readonly IInventorizationQueries _inventorizationQueries;

        public ExpensesByFlowsPageModel(
            IExpensesQueries expensesQueries, 
            IUserSettings userSettings,
            IInventorizationQueries inventorizationQueries)
        {
            _expensesQueries = expensesQueries;
            _userSettings = userSettings;
            _inventorizationQueries = inventorizationQueries;
        }

        [BindProperty]
        public ReportTableFilter Filter { get; set; }

        public ExpensesByFlowsModel Expenses { get; private set; }

        public BalanceState BalanceState { get; private set; }

        public bool IsDataValid { get; private set; }

        private async Task PrepareModelsAsync()
        {
            BalanceState = await _inventorizationQueries.GetBalanceState();
        }

        private async Task<ExpensesByFlowsModel> LoadExpensesAsync(int pageNumber = 1)
        {
            if (Filter.DateFromAsDateTime != null && Filter.DateToAsDateTime != null)
            {
                var expenses = await _expensesQueries.GetExpensesByFlows(
                    Filter.DateFromAsDateTime.Value,
                    Filter.DateToAsDateTime.Value,
                    new PaginationArgs
                    {
                        IncludeDeleted = false,
                        ItemsPerPage = _userSettings.ItemsPerPage,
                        PageNumber = pageNumber,
                    });
                IsDataValid = true;
                return expenses;
            }
            IsDataValid = false;
            return null;
        }

        public async Task OnGetAsync(string dateFrom, string dateTo, int pageNumber = 1)
        {
            await PrepareModelsAsync();
            if (string.IsNullOrEmpty(dateFrom) || string.IsNullOrEmpty(dateTo))
                Filter = ReportTableFilter.CurrentMonth();
            else
            {
                Filter = new ReportTableFilter(dateFrom, dateTo);
                foreach (var error in Filter.Validate())
                {
                    ModelState.AddModelError(error.PropertyName, error.Message);
                }
            }
            if (ModelState.IsValid)
                Expenses = await LoadExpensesAsync(pageNumber);
            else
                IsDataValid = false;
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            await PrepareModelsAsync();
            return await Filter.ProcessAsync(ModelState, nameof(Filter),
                async () =>
                {
                    Expenses = await LoadExpensesAsync();
                    return Page();
                },
                async () =>
                {
                    IsDataValid = false;
                    return await Task.FromResult(Page());
                });
        }
    }
}