using System;
using System.Collections.Generic;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseFlowModel
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal Balance { get; set; }
        public List<int> Categories { get; set; }
    }
}