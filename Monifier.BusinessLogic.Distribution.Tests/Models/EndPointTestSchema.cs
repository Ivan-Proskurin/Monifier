using Monifier.BusinessLogic.Distribution.Model.Contract;

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
            
            RecipientPoint_FixedFromBase500 = new FlowEndPointMock(
                4, "Получатель 500 от базы", 10, rules.RecipientRule_FixedFromBase500);
            
            SourcePoint_5000Balance = new FlowEndPointMock(
                5, "Источник 5000", 5000, rules.SourceRule_SourceDestination);
            
            SourcePoint_ZeroBalance = new FlowEndPointMock(
                6, "Источник с нулевым балансом", 0, rules.SourceRule_SourceDestination);
            
            RecipientPoint_FixedFromBase3000 = new FlowEndPointMock(
                7, "Получатель 3000 от базы", 100, rules.RecipientRule_FixedFromBase3000);
            
            RecipientPoint_PercentFromBase25 = new FlowEndPointMock(
                8, "Получатель 25% от базы", 700, rules.RecipientRule_PercentFromBase25);
            
            SourcePoint_1000Balance = new FlowEndPointMock(
                9, "Источник 1000", 1000, rules.SourceRule_SourceDestination);
            
            RecipientPoint_FixedFromBase2500 = new FlowEndPointMock(
                10, "Получатель 2500 от базы", 900, rules.RecipientRule_FixedFromBase2500);
            
            RecipientPoint_PercentFromBase30 = new FlowEndPointMock(
                12, "Получатель с 30% от базы", 450, rules.RecipientRule_PercentFromBase30);
            
            RecipientPoint_AllRest1 = new FlowEndPointMock(
                13, "Получатель \"Все, что осталось\" 1", 0, rules.RecipientRule_AllRest);
            
            RecipientPoint_AllRest2 = new FlowEndPointMock(
                14, "Получатель \"Все, что осталось\" 2", 100, rules.RecipientRule_AllRest);

            RecipientPoint_AllRest3 = new FlowEndPointMock(
                15, "Получатель \"Все, что осталось\" 3", 120, rules.RecipientRule_AllRest);
            
            RecipientPoint_AllRest4 = new FlowEndPointMock(
                16, "Получатель \"Все, что осталось\" 4", 120, rules.RecipientRule_AllRest);
            
            RecipientPoint_AllRest5 = new FlowEndPointMock(
                17, "Получатель \"Все, что осталось\" 5", 120, rules.RecipientRule_AllRest);

            RecipientPoint_AllRest6 = new FlowEndPointMock(
                18, "Получатель \"Все, что осталось\" 6", 120, rules.RecipientRule_AllRest);

            RecipientPoint_AllRest7 = new FlowEndPointMock(
                19, "Получатель \"Все, что осталось\" 7", 120, rules.RecipientRule_AllRest);

        }
        
        public IFlowEndPoint NotParticipantSourcePoint { get; }
        public IFlowEndPoint NotParticipantRecipientPoint { get; }
        public IFlowEndPoint RecipientPoint_FixedFromBase1000 { get; }
        public IFlowEndPoint RecipientPoint_FixedFromBase500 { get; }
        public IFlowEndPoint RecipientPoint_FixedFromBase3000 { get; }
        public IFlowEndPoint RecipientPoint_FixedFromBase2500 { get; }
        public IFlowEndPoint RecipientPoint_PercentFromBase25 { get; }
        public IFlowEndPoint RecipientPoint_PercentFromBase30 { get; }
        public IFlowEndPoint RecipientPoint_AllRest1 { get; }
        public IFlowEndPoint RecipientPoint_AllRest2 { get; }
        public IFlowEndPoint RecipientPoint_AllRest3 { get; }
        public IFlowEndPoint RecipientPoint_AllRest4 { get; }
        public IFlowEndPoint RecipientPoint_AllRest5 { get; }
        public IFlowEndPoint RecipientPoint_AllRest6 { get; }
        public IFlowEndPoint RecipientPoint_AllRest7 { get; }
        public IFlowEndPoint SourcePoint_5000Balance { get; }
        public IFlowEndPoint SourcePoint_ZeroBalance { get; }
        public IFlowEndPoint SourcePoint_1000Balance { get; }
    }
}