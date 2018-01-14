using Monifier.BusinessLogic.Distribution.Model.Contract;

namespace Monifier.BusinessLogic.Distribution.Model
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