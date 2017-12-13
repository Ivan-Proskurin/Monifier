﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;

namespace Monifier.Web.Pages.Expenses
{
    public class ViewExpenseBillModel : PageModel
    {
        private readonly IExpensesBillQueries _expensesBillQueries;
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IAccountQueries _accountQueries;
        private readonly IExpensesBillCommands _expensesBillCommands;

        public ViewExpenseBillModel(
            IExpensesBillQueries expensesBillQueries,
            IExpenseFlowQueries expenseFlowQueries,
            IAccountQueries accountQueries,
            IExpensesBillCommands expensesBillCommands)
        {
            _expensesBillQueries = expensesBillQueries;
            _expenseFlowQueries = expenseFlowQueries;
            _accountQueries = accountQueries;
            _expensesBillCommands = expensesBillCommands;
        }
        
        public ExpenseBillModel Bill { get; private set; }
        
        public ExpenseFlowModel ExpenseFlow { get; private set; }
        
        public AccountModel Account { get; private set; }
        
        [BindProperty]
        public string Day { get; set; }
        
        [BindProperty]
        public int BillId { get; set; }
        
        public async Task OnGetAsync(int billId)
        {
            Bill = await _expensesBillQueries.GetById(billId);
            ExpenseFlow = await _expenseFlowQueries.GetById(Bill.ExpenseFlowId);
            Account = await _accountQueries.GetById(Bill.AccountId);
            Day = Bill.DateTime.Date.ToStandardString();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            await _expensesBillCommands.Delete(BillId, false);
            return RedirectToPage("./ExpensesForDay", new { day = Day });
        }
    }
}