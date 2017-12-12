using System.Collections.Generic;

namespace Monifier.Web.Models.Expenses
{
    public class ExpenseBillViewModel
    {
        public List<ExpenseItemViewModel> Subtotals { get; set; }
        public string DateTime { get; set; }
        public string Subtotal { get; set; }
    }
}