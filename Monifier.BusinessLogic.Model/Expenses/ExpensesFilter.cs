using System;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpensesFilter
    {
        public int? FlowId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}