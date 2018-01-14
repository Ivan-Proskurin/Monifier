using System.Threading.Tasks;
using Monifier.BusinessLogic.Distribution;

namespace Monifier.BusinessLogic.Contract.Distribution
{
    public interface IDistributionQueries
    {
        Task<DistributionBoard> GetDistributionBoard();
    }
}