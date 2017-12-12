using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    public class ExpensesTableModel : PageModel
    {
        private readonly IExpensesQueries _expensesQueries;

        public ExpensesTableModel(IExpensesQueries expensesQueries)
        {
            _expensesQueries = expensesQueries;
        }
        
        [BindProperty]
        public ExpensesTableFilter Filter { get; set; }
        
        public ExpensesListModel Expenses { get; private set; }

        public async Task OnGetAsync()
        {
            Filter = ExpensesTableFilter.CurrentWeek();
            Expenses = await _expensesQueries.GetExpensesByDay(
                Filter.DateFromAsDateTime, Filter.DateToAsDateTime, new PaginationArgs
                {
                    IncludeDeleted = false,
                    ItemsPerPage = 1000,
                    PageNumber = 1
                });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Filter.ProcessAsync(ModelState,
                async () =>
                {
                    return Page();
                },
                async () => Page()
            );
        }
    }
}