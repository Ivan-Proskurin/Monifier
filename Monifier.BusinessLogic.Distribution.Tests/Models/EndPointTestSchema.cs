using System.Net.Http.Headers;
using Monifier.BusinessLogic.Model.Distribution;

namespace Monifier.BusinessLogic.Distribution.Tests.Models
{
    public class EndPointTestSchema
    {
        public EndPointTestSchema()
        {
            var rules = new EndPointRulesSchema();
            
            NotParticipantSourcePoint = new FlowEndPointMock(
                1, "Неучаствующий источник", 100, rules.NotParticipantSourceRule);

            NotParticipantRecipientPoint = new FlowEndPointMock(
                2, "Неучаствующий получатель", 50, rules.NotParticipantRecipientRule);
            
            RecipientPoint_FixedFromBase1000 = new FlowEndPointMock(
                3, "Получатель 1000 от базы", 300, rules.RecipientRule_FixedFromBase1000);
            
            RecipientPoint_FixedFromRest500 = new FlowEndPointMock(
                4, "Получатель 500 от базы", 10, rules.RecipientRule_FixedFromRest500);
            
            SourcePoint_5000Balance = new FlowEndPointMock(
                5, "Источник 5000", 5000, rules.SourceRule_SourceDestination);
            
            SourcePoint_ZeroBalance = new FlowEndPointMock(
                6, "Источник с нулевым балансом", 0, rules.SourceRule_SourceDestination);
            
            RecipientPoint_FixedFromRest3000 = new FlowEndPointMock(
                7, "Получатель 3000 от остатка", 100, rules.RecipientRule_FixedFromRest3000);
            
            RecipientPoint_PercentFromBase25 = new FlowEndPointMock(
                8, "Получатель 25% от базы", 700, rules.RecipientRule_PercentFromBase25);
            
            SourcePoint_1000Balance = new FlowEndPointMock(
                9, "Источник 1000", 1000, rules.SourceRule_SourceDestination);
            
            RecipientPoint_FixedFromRest2500 = new FlowEndPointMock(
                10, "Получатель 2500 от остатка", 900, rules.RecipientRule_FixedFromRest2500);
            
            SourcePoint_Minus1000BothDestination = new FlowEndPointMock(
                11, "Получатель с минус 1000, может выступать как источник", -1000, rules.SourceRule_BothDestination);
            
            RecipientPoint_PercentFromRest30 = new FlowEndPointMock(
                12, "Получатель с 30% от остатка", 450, rules.RecipientRule_PercentFromRest30);
            
            RecipientPoint_AllRest1 = new FlowEndPointMock(
                13, "Получатель \"Все, что осталось\" 1", 0, rules.RecipientRule_AllRest);
            
            RecipientPoint_AllRest2 = new FlowEndPointMock(
                14, "Получатель \"Все, что осталось\" 2", 100, rules.RecipientRule_AllRest);

        }
        
        public IFlowEndPoint NotParticipantSourcePoint { get; }
        public IFlowEndPoint NotParticipantRecipientPoint { get; }
        public IFlowEndPoint RecipientPoint_FixedFromBase1000 { get; }
        public IFlowEndPoint RecipientPoint_FixedFromRest500 { get; }
        public IFlowEndPoint RecipientPoint_FixedFromRest3000 { get; }
        public IFlowEndPoint RecipientPoint_FixedFromRest2500 { get; }
        public IFlowEndPoint RecipientPoint_PercentFromBase25 { get; }
        public IFlowEndPoint RecipientPoint_PercentFromRest30 { get; }
        public IFlowEndPoint RecipientPoint_AllRest1 { get; }
        public IFlowEndPoint RecipientPoint_AllRest2 { get; }
        public IFlowEndPoint SourcePoint_5000Balance { get; }
        public IFlowEndPoint SourcePoint_ZeroBalance { get; }
        public IFlowEndPoint SourcePoint_1000Balance { get; }
        public IFlowEndPoint SourcePoint_Minus1000BothDestination { get; }
    }
}