﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    public class CreateExpenseFlowModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IExpenseFlowCommands _expenseFlowCommands;

        public CreateExpenseFlowModel(IExpenseFlowQueries expenseFlowQueries, IExpenseFlowCommands expenseFlowCommands)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _expenseFlowCommands = expenseFlowCommands;
        }
        
        [BindProperty]
        public EditExpenseFlow ExpenseFlow { get; set; }

        public async Task OnGetAsync()
        {
            ExpenseFlow = new EditExpenseFlow
            {
                Id = -1,
                Number = await _expenseFlowQueries.GetNextNumber(),
                CreationDate = DateTime.Today.ToStandardString(false),
                Balance = "0"
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await ExpenseFlow.ProcessAsync(ModelState,
                async () =>
                {
                    var flow = await _expenseFlowCommands.Update(ExpenseFlow.ToExpenseFlowModel());
                    return RedirectToPage("./EditExpenseFlow", new { id = flow.Id });
                },
                
                async () => await Task.FromResult(Page())
            );
        }
    }
}