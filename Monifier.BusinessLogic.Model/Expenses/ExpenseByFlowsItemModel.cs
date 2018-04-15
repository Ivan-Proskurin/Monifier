using System;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseByFlowsItemModel
    {
        public int FlowId { get; set; }
        public string Flow { get; set; }
        public DateTime LastBill { get; set; }
        public decimal Total { get; set; }
        public bool IsDangerExpense { get; set; }
    }
}