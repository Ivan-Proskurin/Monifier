using System;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseFlowExpense
    {
        public string Account { get; set; }
        public int ExpenseFlowId { get; set; }
        public string Category { get; set; }
        public string Product { get; set; }
        public decimal Cost { get; set; }
        public DateTime DateCreated { get; set; }
    }
}