using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.Web.Pages.Expenses
{
    public class ExpenseFlowsModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;

        public ExpenseFlowsModel(IExpenseFlowQueries expenseFlowQueries)
        {
            _expenseFlowQueries = expenseFlowQueries;
        }
        
        public ExpenseFlowList ExpenseFlows { get; private set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            ExpenseFlows = await _expenseFlowQueries.GetList(new PaginationArgs
            {
                IncludeDeleted = false,
                ItemsPerPage = 7,
                PageNumber = pageNumber
            });
        }
    }
}