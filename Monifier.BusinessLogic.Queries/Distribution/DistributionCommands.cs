using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.DataAccess.Contract;

namespace Monifier.BusinessLogic.Queries.Distribution
{
    public class DistributionCommands : IDistributionCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFlowDistributor _distributor;

        public DistributionCommands(IUnitOfWork unitOfWork,
            IFlowDistributor distributor)
        {
            _unitOfWork = unitOfWork;
            _distributor = distributor;
        }
        
        public async Task Distribute(DistributionBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            
            await board.Distribute(_unitOfWork, _distributor);
        }

        public async Task Save(DistributionBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            await board.Save(_unitOfWork);
        }
    }
}