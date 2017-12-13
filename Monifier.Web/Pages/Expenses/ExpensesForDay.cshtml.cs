using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
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
        
        public string Day { get; private set; }
        
        public ExpensesListModel Expenses { get; private set; }
        
        public bool IsDataValid { get; private set; }

        public async Task OnGetAsync(string day)
        {
            if (string.IsNullOrEmpty(day)) day = DateTime.Today.ToStandardString();
            IsDataValid = !string.IsNullOrEmpty(day) && day.ValidateDateTime("day") == null;
            if (IsDataValid)
            {
                var dateTime = day.ParseDtFromStandardString();
                Day = dateTime.Date.ToStandardDateStr();
                Expenses = await _expensesQueries.GetExpensesForDay(dateTime);
            }
        }
    }
}