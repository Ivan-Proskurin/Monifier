using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Contract.Model;
using Monifier.DataAccess.Model.Base;

namespace Monifier.DataAccess.Model.Expenses
{
    public class ExpenseBill : IHasId
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public virtual ICollection<ExpenseItem> Items { get; set; }

        public decimal SumPrice { get; set; }

        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
        
        public int ExpenseFlowId { get; set; }
        
        [ForeignKey("ExpenseFlowId")]
        public ExpenseFlow ExpenseFlow { get; set; }
    }
}
