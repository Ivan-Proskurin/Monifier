using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.DataAccess.Contract;

namespace Monifier.BusinessLogic.Queries.Distribution
{
    public class DistributionQueries : IDistributionQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public DistributionQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }
        
        public async Task<DistributionBoard> GetDistributionBoard()
        {
            var board = new DistributionBoard();
            await board.Load(_repository, _currentSession.UserId);
            return board;
        }
    }
}