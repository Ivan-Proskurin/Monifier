using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.DataAccess.Contract;

namespace Monifier.BusinessLogic.Queries.Distribution
{
    public class DistributionQueries : IDistributionQueries
    {
        private readonly IUnitOfWork _unitOfWork;

        public DistributionQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<DistributionBoard> GetDistributionBoard()
        {
            var board = new DistributionBoard();
            await board.Load(_unitOfWork);
            return board;
        }
    }
}