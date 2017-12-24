using Monifier.BusinessLogic.Model.Distribution;

namespace Monifier.BusinessLogic.Distribution.Tests.Models
{
    public class EndPointRulesSchema
    {
        public EndPointRulesSchema()
        {
            RecipientRule_FixedFromBase1000 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromBase,
                Amount = 1000
            };

            RecipientRule_FixedFromRest500 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromRest,
                Amount = 500
            };
            
            NotParticipantSourceRule = new DistributionFlowRule
            {
                CanFlow = false,
                Destination = FlowDestination.Source,
                Rule = FlowRule.None,
                Amount = 0
            };
            
            NotParticipantRecipientRule = new DistributionFlowRule
            {
                CanFlow = false,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.None,
                Amount = 0
            };
            
            SourceRule_SourceDestination = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Source,
                Rule = FlowRule.None,
                Amount = 0
            };
            
            SourceRule_BothDestination = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Both,
                Rule = FlowRule.FixedFromBase,
                Amount = 2000
            };

            RecipientRule_PercentFromBase25 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.PercentFromBase,
                Amount = 25
            };

            RecipientRule_FixedFromRest3000 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromRest,
                Amount = 3000
            };

            RecipientRule_FixedFromRest2500 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromRest,
                Amount = 2500
            };
            
            RecipientRule_AllRest = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.AllRest,
                Amount = 0
            };
            
            RecipientRule_PercentFromRest30 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.PercentFromRest,
                Amount = 30
            };
        }

        public DistributionFlowRule NotParticipantSourceRule { get; }
        public DistributionFlowRule NotParticipantRecipientRule { get; }
        public DistributionFlowRule RecipientRule_FixedFromBase1000 { get; }
        public DistributionFlowRule RecipientRule_FixedFromRest500 { get; }
        public DistributionFlowRule RecipientRule_PercentFromBase25 { get; }
        public DistributionFlowRule RecipientRule_PercentFromRest30 { get; }
        public DistributionFlowRule RecipientRule_FixedFromRest3000 { get; }
        public DistributionFlowRule RecipientRule_FixedFromRest2500 { get; }
        public DistributionFlowRule RecipientRule_AllRest { get; }
        public DistributionFlowRule SourceRule_SourceDestination { get; }
        public DistributionFlowRule SourceRule_BothDestination { get; }
    }
}