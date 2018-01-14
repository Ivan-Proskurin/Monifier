namespace Monifier.BusinessLogic.Distribution.Model
{
    public class DistributionFlowModel
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; }
        public int RecipientId { get; set; }
        public string RecipientName { get; set; }
        public decimal Amount { get; set; }
        public string AmountFormatted { get; set; }
    }
}