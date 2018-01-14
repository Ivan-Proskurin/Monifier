using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Contracts;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.DataAccess.Model.Distribution
{
    public enum FlowRule
    {
        None,
        FixedFromBase,
        PercentFromBase,
        AllRest
    }
    
    public class ExpenseFlowSettings : IHasId
    {
        public int Id { get; set; }
        public int ExpenseFlowId { get; set; }
        [ForeignKey("ExpenseFlowId")]
        public ExpenseFlow ExpenseFlow { get; set; }
        public bool CanFlow { get; set; }
        public FlowRule Rule { get; set; }
        public decimal Amount { get; set; }
    }
}