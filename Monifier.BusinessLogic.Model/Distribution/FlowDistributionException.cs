using System;
using System.Collections.Generic;
using System.Linq;

namespace Monifier.BusinessLogic.Model.Distribution
{
    public class FlowDistributionException : ApplicationException
    {
        public FlowDistributionException(IEnumerable<IFlowEndPoint> sources, 
            IFlowEndPoint recipient, decimal amount)
            : base("Невозможно выполнить перевод от источников получателю на указанную сумму")
        {
            Sources = sources.ToList();
            Recipient = recipient;
            Amount = amount;
        }
        
        public IList<IFlowEndPoint> Sources { get; }
        public IFlowEndPoint Recipient { get; }
        public decimal Amount { get; }
    }
}