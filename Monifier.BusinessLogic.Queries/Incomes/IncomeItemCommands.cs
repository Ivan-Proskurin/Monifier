using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeItemCommands : IIncomeItemCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public IncomeItemCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task Delete(int id, bool onlyMark = true)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IncomeItemModel> Update(IncomeItemModel model)
        {
            var itemRepo = _unitOfWork.GetCommandRepository<IncomeItem>();
            var item = new IncomeItem
            {
                Id = model.Id,
                DateTime = model.DateTime,
                IncomeTypeId = model.IncomeTypeId,
                Total = model.Total
            };
            if (item.Id > 0)
            {
                itemRepo.Update(item);
            }
            else
            {
                itemRepo.Create(item);
            }
            await _unitOfWork.SaveChangesAsync();
            model.Id = item.Id;
            return model;
        }
    }
}