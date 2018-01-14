using System.Collections.Generic;

namespace Monifier.BusinessLogic.Distribution.Model.Contract
{
    public interface IFlowDistributor
    {
        void Distribute(IEnumerable<IFlowEndPoint> endPoints);
        IList<DistributionFlow> FlowRecords { get; }
    }
}