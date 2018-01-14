using System.Collections.Generic;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpensesListItemModel
    {
        public List<int> BillIds { get; set; }
        public decimal Sum { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Period { get; set; }
        public string Caption { get; set; }
        public string Goods { get; set; }
        public bool IsDangerExpense { get; set; }
    }
}