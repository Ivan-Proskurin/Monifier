using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    public class ExpensesListModel : PageModel
    {
        [BindProperty]
        public ExpensesFilter Filter { get; set; }
        
        public ExpensesList Expenses { get; set; }

        public async Task OnGetAsync()
        {
            Filter = new ExpensesFilter
            {
                DateFrom = DateTime.Now.StartOfTheWeek().ToStandardDateStr(),
                DateTo = DateTime.Now.EndOfTheWeek().ToStandardDateStr()
            };
            
            
        }
    }
}