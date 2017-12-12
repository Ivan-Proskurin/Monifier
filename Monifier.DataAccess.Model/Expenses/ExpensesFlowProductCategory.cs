using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Contract.Model;
using Monifier.DataAccess.Model.Base;

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