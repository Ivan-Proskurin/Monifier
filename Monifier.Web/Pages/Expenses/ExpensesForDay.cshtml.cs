using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Expenses
{
    public class BillExpensesForDayModel : PageModel
    {
        private readonly IExpensesQueries _expensesQueries;

        public BillExpensesForDayModel(IExpensesQueries expensesQueries)
        {
            _expensesQueries = expensesQueries;
        }
        
        [BindProperty]
        public string Day { get; set; }
        
        public ExpensesListModel Expenses { get; private set; }
        
        public bool IsDataValid { get; private set; }

        public async Task OnGetAsync(string day)
        {
            if (string.IsNullOrEmpty(day)) day = DateTime.Today.ToStandardString();
            IsDataValid = !string.IsNullOrEmpty(day) && day.ValidateDateTime("day") == null;
            if (IsDataValid)
            {
                var dateTime = day.ParseDtFromStandardString();
                Day = dateTime.Date.ToStandardString();
                Expenses = await _expensesQueries.GetExpensesForDay(dateTime);
            }
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            if (string.IsNullOrEmpty(Day))
            {
                ModelState.AddModelError(string.Empty, "Не введена дата!");
            }
            var dayResult = Day.ValidateDateTime("Day");
            if (dayResult != null)
                ModelState.AddModelError(string.Empty, dayResult.Message);
            
            IsDataValid = ModelState.IsValid;

            if (IsDataValid)
            {
                var dateTime = Day.ParseDtFromStandardString();
                Expenses = await _expensesQueries.GetExpensesForDay(dateTime);
            }

            return Page();
        }
    }
}