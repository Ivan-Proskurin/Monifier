using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.Common.Extensions;

namespace Monifier.BusinessLogic.Distribution.Model
{
    public class FlowDistributionException : ApplicationException
    {
        public FlowDistributionException(IEnumerable<IFlowEndPoint> sources, 
            IFlowEndPoint recipient, decimal amount)
            : base($"Невозможно пополнить \"{recipient.Name}\" на указанную сумму ({amount.ToMoney()})")
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