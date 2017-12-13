using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Expenses
{
    public class ExpensesFlowProductCategory : IHasId
    {
        public int Id { get; set; }
        
        public int ExpensesFlowId { get; set; }
        [ForeignKey("ExpensesFlowId")]
        public ExpenseFlow ExpenseFlow { get; set; }
        
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}