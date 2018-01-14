using System;
using System.Collections.Generic;
using Monifier.BusinessLogic.Distribution.Model.Contract;

namespace Monifier.BusinessLogic.Distribution.Model
{
    public class FlowDistributionSchemaException : ApplicationException
    {
        public FlowDistributionSchemaException(IEnumerable<IFlowEndPoint> endPoints, string message)
            : base(message)
        {
            EndPoints = endPoints;
        }
        
        public IEnumerable<IFlowEndPoint> EndPoints { get; }
    }
}