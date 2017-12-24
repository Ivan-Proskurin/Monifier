namespace Monifier.BusinessLogic.Model.Distribution
{
    public enum FlowRule
    {
        None,
        FixedFromBase,
        PercentFromBase,
        FixedFromRest,
        PercentFromRest,
        AllRest
    }

    public enum FlowDestination
    {
        Source,
        Recipient,
        Both
    }
    
    public class DistributionFlowRule
    {
        public bool CanFlow { get; set; }
        public FlowDestination Destination { get; set; }
        public FlowRule Rule { get; set; }
        public decimal Amount { get; set; }
    }
}