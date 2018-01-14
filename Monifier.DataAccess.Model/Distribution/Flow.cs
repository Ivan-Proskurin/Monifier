using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.DataAccess.Model.Distribution
{
    public class Flow : IHasId
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        [ForeignKey("SourceId")]
        public Account Source { get; set; }
        public int RecipientId { get; set; }
        [ForeignKey("RecipientId")]
        public ExpenseFlow Recipient { get; set; }
        public decimal Amount { get; set; }
        public int DistributionId { get; set; }
        [ForeignKey("DistributionId")]
        public Distribution Distribution { get; set; }
    }
}