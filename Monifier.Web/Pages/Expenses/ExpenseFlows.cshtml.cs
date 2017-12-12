using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    public class ExpenseFlowsModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;

        public ExpenseFlowsModel(IExpenseFlowQueries expenseFlowQueries)
        {
            _expenseFlowQueries = expenseFlowQueries;
        }
        
        public List<ExpenseFlowModel> ExpenseFlows { get; private set; }

        public async Task OnGetAsync()
        {
            ExpenseFlows = await _expenseFlowQueries.GetAll();
        }
    }
}