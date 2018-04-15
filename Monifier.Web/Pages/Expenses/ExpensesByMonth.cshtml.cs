using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class ExpensesByMonthModel : PageModel
    {
        private readonly IExpensesQueries _expensesQueries;
        private readonly IUserSettings _userSettings;
        private readonly IExpenseFlowQueries _expenseFlowQueries;

        public ExpensesByMonthModel(IExpensesQueries expensesQueries, IUserSettings userSettings,
            IExpenseFlowQueries expenseFlowQueries)
        {
            _expensesQueries = expensesQueries;
            _userSettings = userSettings;
            _expenseFlowQueries = expenseFlowQueries;
        }

        [BindProperty]
        public ReportTableFilter Filter { get; set; }

        public List<ExpenseFlowModel> Flows { get; private set; }

        public ExpensesListModel Expenses { get; private set; }

        public bool IsDataValid { get; private set; }

        private async Task<ExpensesListModel> LoadExpensesAsync(int pageNumber = 1)
        {
            if (Filter.DateFromAsDateTime != null && Filter.DateToAsDateTime != null)
            {
                var expenses = await _expensesQueries.GetExpensesByMonth(
                    new ExpensesFilter
                    {
                        FlowId = Filter.FlowId,
                        DateFrom = Filter.DateFromAsDateTime.Value,
                        DateTo = Filter.DateToAsDateTime.Value
                    },
                    new PaginationArgs
                    {
                        IncludeDeleted = false,
                        ItemsPerPage = _userSettings.ItemsPerPage,
                        PageNumber = pageNumber
                    });
                IsDataValid = true;
                return expenses;
            }
            else
            {
                IsDataValid = false;
            }
            return null;
        }

        public async Task OnGetAsync(string dateFrom, string dateTo, int pageNumber = 1, int? flowId = null)
        {
            Flows = await _expenseFlowQueries.GetAll();
            if (string.IsNullOrEmpty(dateFrom) || string.IsNullOrEmpty(dateTo))
                Filter = ReportTableFilter.CurrentYear();
            else
            {
                Filter = new ReportTableFilter(dateFrom, dateTo, flowId)
                {
                    Flow = await _expenseFlowQueries.GetNameById(flowId)
                };
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
            Flows = await _expenseFlowQueries.GetAll();
            return await Filter.ProcessAsync(ModelState, nameof(Filter),
                async () =>
                {
                    Filter.FlowId = await _expenseFlowQueries.GetIdByName(Filter.Flow);
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