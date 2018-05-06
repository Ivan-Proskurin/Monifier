using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Accounts;

namespace Monifier.BusinessLogic.Queries.Transactions
{
    public class TransactionCommands : ITransactionCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;

        public TransactionCommands(IUnitOfWork unitOfWork, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
        }

        public async Task<TransactionModel> Update(TransactionModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.ParticipantId == null && model.BillId == null && model.IncomeId == null)
                throw new InvalidOperationException("Invalid transation");
            var particiapntCount = (model.ParticipantId != null ? 1 : 0) + (model.BillId != null ? 1 : 0) +
                (model.IncomeId != null ? 1 : 0);
            if (particiapntCount > 1)
                throw new InvalidOperationException("Transactions can be completed between only two participants");
            if (model.InitiatorId == model.ParticipantId)
                throw new InvalidOperationException("Cannot process transfer between same participants");

            var entity = new Transaction
            {
                Id = model.Id,
                OwnerId = _currentSession.UserId,
                DateTime = model.DateTime,
                InitiatorId = model.InitiatorId,
                ParticipantId = model.ParticipantId,
                BillId = model.BillId,
                IncomeId = model.IncomeId,
                Total = model.Total,
            };

            var commands = _unitOfWork.GetCommandRepository<Transaction>();

            if (model.Id > 0)
                commands.Update(entity);
            else
                commands.Create(entity);

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            model.Id = entity.Id;
            return model;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            var commands = _unitOfWork.GetCommandRepository<Transaction>();
            var entity = await _unitOfWork.LoadEntity<Transaction>(id).ConfigureAwait(false);
            if (entity == null)
                throw new InvalidOperationException($"There is no transation with id {id}");
            if (onlyMark)
            {
                entity.IsDeleted = true;
            }
            else
            {
                commands.Delete(entity);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}