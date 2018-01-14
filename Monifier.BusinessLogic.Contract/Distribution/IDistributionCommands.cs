using System.Threading.Tasks;
using Monifier.BusinessLogic.Distribution;

namespace Monifier.BusinessLogic.Contract.Distribution
{
    public interface IDistributionCommands
    {
        Task Distribute(DistributionBoard board);
        Task Save(DistributionBoard board);
    }
}