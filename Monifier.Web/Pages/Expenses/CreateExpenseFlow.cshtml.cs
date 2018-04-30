using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class CreateExpenseFlowModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IExpenseFlowCommands _expenseFlowCommands;
        private readonly ITimeService _timeService;

        public CreateExpenseFlowModel(
            IExpenseFlowQueries expenseFlowQueries, 
            IExpenseFlowCommands expenseFlowCommands,
            ITimeService timeService)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _expenseFlowCommands = expenseFlowCommands;
            _timeService = timeService;
        }
        
        [BindProperty]
        public EditExpenseFlow ExpenseFlow { get; set; }

        public async Task OnGetAsync()
        {
            ExpenseFlow = new EditExpenseFlow
            {
                Id = -1,
                Number = await _expenseFlowQueries.GetNextNumber(),
                CreationDate = _timeService.ClientLocalNow.Date.ToStandardString(false),
                Balance = "0"
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await ExpenseFlow.ProcessAsync(ModelState, nameof(ExpenseFlow),
                async () =>
                {
                    var model = new ExpenseFlowModel {Id = -1};
                    ExpenseFlow.ToExpenseFlowModel(model);
                    await _expenseFlowCommands.Update(model);
                    return RedirectToPage("./ExpenseFlows");
                },
                
                async () => await Task.FromResult(Page())
            );
        }
    }
}