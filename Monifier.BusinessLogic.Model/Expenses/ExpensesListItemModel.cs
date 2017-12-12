using System;
using System.Collections.Generic;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpensesListItemModel
    {
        public List<int> BillIds { get; set; }
        public decimal Sum { get; set; }
        public DateTime DateTime { get; set; }
    }
}