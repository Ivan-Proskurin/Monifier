using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Distribution;

namespace Monifier.BusinessLogic.Contract.Distribution
{
    public interface IFlowDistributor
    {
        void Distribute(IEnumerable<IFlowEndPoint> endPoints);
        IList<DistributionFlow> FlowRecords { get; }
    }
}