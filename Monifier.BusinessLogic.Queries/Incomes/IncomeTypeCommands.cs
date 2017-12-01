using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeTypeCommands : IIncomeTypeCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public IncomeTypeCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IncomeTypeModel> Update(IncomeTypeModel model)
        {
            var typeRepo = _unitOfWork.GetCommandRepository<IncomeType>();
            var type = new IncomeType
            {
                Id = model.Id,
                Name = model.Name
            };
            if (type.Id > 0)
            {
                typeRepo.Update(type);
            }
            else
            {
                typeRepo.Create(type);
            }
            await _unitOfWork.SaveChangesAsync();
            model.Id = type.Id;
            return model;
        }

        public Task Delete(int id, bool onlyMark = true)
        {
            throw new System.NotImplementedException();
        }
    }
}