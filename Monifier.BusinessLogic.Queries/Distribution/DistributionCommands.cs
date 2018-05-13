using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.DataAccess.Contract;

namespace Monifier.BusinessLogic.Queries.Distribution
{
    public class DistributionCommands : IDistributionCommands
    {
        private readonly IEntityRepository _repository;
        private readonly IFlowDistributor _distributor;
        private readonly ITimeService _timeService;

        public DistributionCommands(
            IEntityRepository repository,
            IFlowDistributor distributor,
            ITimeService timeService)
        {
            _repository = repository;
            _distributor = distributor;
            _timeService = timeService;
        }
        
        public Task Distribute(DistributionBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            
            return board.Distribute(_repository, _distributor);
        }

        public Task Save(DistributionBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            return board.Save(_repository, _timeService.ClientLocalNow);
        }
    }
}