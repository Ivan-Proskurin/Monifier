using System;
using Monifier.BusinessLogic.Model.Distribution;

namespace Monifier.BusinessLogic.Distribution.Tests.Models
{
    public class FlowEndPointMock : IFlowEndPoint
    {
        public FlowEndPointMock(int id, string name, decimal balance)
        {
            Id = id;
            Name = name;
            Balance = balance;
        }

        public FlowEndPointMock(int id, string name, decimal balance,
            DistributionFlowRule rule) : this(id, name, balance)
        {
            FlowRule = rule ?? throw new ArgumentNullException(nameof(rule));
        }
        
        public int Id { get; }
        public string Name { get; }
        public decimal Balance { get; private set; }
        public void Topup(decimal amount)
        {
            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            Balance -= amount;
        }

        public DistributionFlowRule FlowRule { get; set; }
    }
}