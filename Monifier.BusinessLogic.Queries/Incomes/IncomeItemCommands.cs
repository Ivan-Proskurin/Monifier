using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeItemCommands : IIncomeItemCommands
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;

        public IncomeItemCommands(IUnitOfWork unitOfWork, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new NotSupportedException("Операция не поддерживается");
            var incomeCommands = _unitOfWork.GetCommandRepository<IncomeItem>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();
            var income = await _unitOfWork.LoadEntity<IncomeItem>(id).ConfigureAwait(false);
            var account = await _unitOfWork.LoadEntity<Account>(income.AccountId).ConfigureAwait(false);
            account.AvailBalance -= income.Total;
            if (!income.IsCorrection)
                account.Balance -= income.Total;
            accountCommands.Update(account);
            incomeCommands.Delete(income);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IncomeItemModel> Update(IncomeItemModel model)
        {
            var itemRepo = _unitOfWork.GetCommandRepository<IncomeItem>();
            var item = new IncomeItem
            {
                Id = model.Id,
                DateTime = model.DateTime,
                IncomeTypeId = model.IncomeTypeId,
                Total = model.Total,
                OwnerId = _currentSession.UserId,
                IsCorrection = model.IsCorrection,
            };
            if (item.Id > 0)
            {
                itemRepo.Update(item);
            }
            else
            {
                itemRepo.Create(item);
            }
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            model.Id = item.Id;
            return model;
        }
    }
}