namespace Monifier.BusinessLogic.Distribution.Model.Contract
{
    public interface IFlowEndPoint
    {
        int Id { get; }
        string Name { get; }
        decimal Balance { get; }
        void Topup(decimal amount);
        void Withdraw(decimal amount);
        DistributionFlowRule FlowRule { get; set; }
    }
}