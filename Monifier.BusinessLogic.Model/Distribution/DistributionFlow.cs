namespace Monifier.BusinessLogic.Model.Distribution
{
    public class DistributionFlow
    {
        public DistributionFlow(IFlowEndPoint source, IFlowEndPoint recipient, decimal amount)
        {
            Source = source;
            Recipient = recipient;
            Amount = amount;
        }
        
        public IFlowEndPoint Source { get; }
        public IFlowEndPoint Recipient { get; }
        public decimal Amount { get; }
    }
}