using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.BusinessLogic.Distribution
{
    public class DistributionAccount : IFlowEndPoint
    {
        public AccountModel Account { get; set; }
        public bool UseInDistribution { get; set; }
        public decimal StartBalance { get; set; }
        public decimal Distributed { get; set; }

        #region IFlowEndPoint

        public int Id => Account.Id;
        public string Name => Account.Name;
        public decimal Balance => Account.AvailBalance;

        public void Topup(decimal amount)
        {
            (Account as IFlowEndPoint).Topup(amount);
            Distributed -= amount;
        }

        public void Withdraw(decimal amount)
        {
            (Account as IFlowEndPoint).Withdraw(amount);
            Distributed += amount;
        }

        public DistributionFlowRule FlowRule
        {
            get => (Account as IFlowEndPoint).FlowRule;
            set => (Account as IFlowEndPoint).FlowRule = value;
        }

        #endregion
    }
}