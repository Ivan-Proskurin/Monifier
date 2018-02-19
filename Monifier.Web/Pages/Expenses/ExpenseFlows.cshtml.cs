using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Inventorization;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class ExpenseFlowsModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IInventorizationQueries _inventorizationQueries;

        public ExpenseFlowsModel(IExpenseFlowQueries expenseFlowQueries, IInventorizationQueries inventorizationQueries)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _inventorizationQueries = inventorizationQueries;
        }
        
        public ExpenseFlowList ExpenseFlows { get; private set; }
        public BalanceState BalanceState { get; private set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            ExpenseFlows = await _expenseFlowQueries.GetList(new PaginationArgs
            {
                IncludeDeleted = false,
                ItemsPerPage = 7,
                PageNumber = pageNumber
            });
            BalanceState = await _inventorizationQueries.GetBalanceState();
        }
    }
}