using Monifier.DataAccess.Model.Distribution;

namespace Monifier.BusinessLogic.Distribution.Model
{
    public enum FlowDestination
    {
        Source,
        Recipient
    }
    
    public class DistributionFlowRule
    {
        public bool CanFlow { get; set; }
        public FlowDestination Destination { get; set; }
        public FlowRule Rule { get; set; }
        public decimal Amount { get; set; }
    }
}