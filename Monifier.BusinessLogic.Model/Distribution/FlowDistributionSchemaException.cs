using System;
using System.Collections.Generic;

namespace Monifier.BusinessLogic.Model.Distribution
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