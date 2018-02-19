using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;

namespace Monifier.Web.Pages.Incomes
{
    [Authorize]
    public class IncomesTableModel : PageModel
    {
        private readonly IIncomesQueries _incomesQueries;
        private readonly IUserSettings _userSettings;

        public IncomesTableModel(IIncomesQueries incomesQueries, IUserSettings userSettings)
        {
            _incomesQueries = incomesQueries;
            _userSettings = userSettings;
        }

        [BindProperty]
        public ReportTableFilter Filter { get; set; }

        public bool IsDataValid { get; private set; }

        public IncomesListModel Incomes { get; private set; }

        private async Task<IncomesListModel> LoadIncomesAsync(int pageNumber = 1)
        {
            if (Filter.DateFromAsDateTime != null && Filter.DateToAsDateTime != null)
            {
                var incomes = await _incomesQueries.GetIncomesList(
                    Filter.DateFromAsDateTime.Value, Filter.DateToAsDateTime.Value, new PaginationArgs
                    {
                        IncludeDeleted = false,
                        ItemsPerPage = _userSettings.ItemsPerPage,
                        PageNumber = pageNumber
                    });
                IsDataValid = true;
                return incomes;
            }
            else
            {
                IsDataValid = false;
            }
            return null;
        }

        public async Task OnGetAsync(string dateFrom, string dateTo, int pageNumber = 1)
        {
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
                Incomes = await LoadIncomesAsync(pageNumber);
            else
                IsDataValid = false;
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            return await Filter.ProcessAsync(ModelState,
                async () =>
                {
                    Incomes = await LoadIncomesAsync();
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