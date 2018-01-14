using Monifier.BusinessLogic.Distribution.Model;
using Monifier.DataAccess.Model.Distribution;

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

            RecipientRule_FixedFromBase500 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromBase,
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

            RecipientRule_PercentFromBase25 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.PercentFromBase,
                Amount = 25
            };

            RecipientRule_FixedFromBase3000 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromBase,
                Amount = 3000
            };

            RecipientRule_FixedFromBase2500 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.FixedFromBase,
                Amount = 2500
            };
            
            RecipientRule_AllRest = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.AllRest,
                Amount = 0
            };
            
            RecipientRule_PercentFromBase30 = new DistributionFlowRule
            {
                CanFlow = true,
                Destination = FlowDestination.Recipient,
                Rule = FlowRule.PercentFromBase,
                Amount = 30
            };
        }

        public DistributionFlowRule NotParticipantSourceRule { get; }
        public DistributionFlowRule NotParticipantRecipientRule { get; }
        public DistributionFlowRule RecipientRule_FixedFromBase1000 { get; }
        public DistributionFlowRule RecipientRule_FixedFromBase500 { get; }
        public DistributionFlowRule RecipientRule_PercentFromBase25 { get; }
        public DistributionFlowRule RecipientRule_PercentFromBase30 { get; }
        public DistributionFlowRule RecipientRule_FixedFromBase3000 { get; }
        public DistributionFlowRule RecipientRule_FixedFromBase2500 { get; }
        public DistributionFlowRule RecipientRule_AllRest { get; }
        public DistributionFlowRule SourceRule_SourceDestination { get; }
    }
}