using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Inventorization;
using Monifier.Common.Extensions;
using Monifier.Web.Models;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class BillExpensesForDayModel : PageModel
    {
        private readonly IExpensesQueries _expensesQueries;
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IInventorizationQueries _inventorizationQueries;

        public BillExpensesForDayModel(
            IExpensesQueries expensesQueries, 
            IExpenseFlowQueries expenseFlowQueries,
            IInventorizationQueries inventorizationQueries)
        {
            _expensesQueries = expensesQueries;
            _expenseFlowQueries = expenseFlowQueries;
            _inventorizationQueries = inventorizationQueries;
        }
        
        [BindProperty]
        public ReportTableFilter Filter { get; set; }

        public List<ExpenseFlowModel> Flows { get; private set; }
        
        public ExpensesListModel Expenses { get; private set; }

        public BalanceState BalanceState { get; private set; }
        
        public bool IsDataValid { get; private set; }

        private async Task PrepareModelsAsync()
        {
            BalanceState = await _inventorizationQueries.GetBalanceState();
            Flows = await _expenseFlowQueries.GetAll();
        }

        private async Task<ExpensesListModel> LoadExpensesAsync()
        {
            if (Filter.DateFromAsDateTime != null && Filter.DateToAsDateTime != null)
            {
                var expenses = await _expensesQueries.GetExpensesForDay(
                    new ExpensesFilter
                    {
                        FlowId = Filter.FlowId,
                        DateFrom = Filter.DateFromAsDateTime.Value,
                        DateTo = Filter.DateToAsDateTime.Value,
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

        public async Task OnGetAsync(string day, int? flowId = null)
        {
            await PrepareModelsAsync();
            if (day.IsNullOrEmpty())
                Filter = ReportTableFilter.CurrentDay();
            else
            {
                Filter = new ReportTableFilter(day, day, flowId)
                {
                    Flow = await _expenseFlowQueries.GetNameById(flowId)
                };
                foreach (var error in Filter.Validate())
                {
                    ModelState.AddModelError(error.PropertyName, error.Message);
                }
            }
            if (ModelState.IsValid)
                Expenses = await LoadExpensesAsync();
            else
                IsDataValid = false;
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            await PrepareModelsAsync();
            Filter.DateTo = Filter.DateFrom;
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