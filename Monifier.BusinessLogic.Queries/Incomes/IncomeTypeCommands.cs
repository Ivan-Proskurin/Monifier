using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeTypeCommands : IIncomeTypeCommands
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public IncomeTypeCommands(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public async Task<IncomeTypeModel> Update(IncomeTypeModel model)
        {
            var type = new IncomeType
            {
                Id = model.Id,
                Name = model.Name,
                OwnerId = _currentSession.UserId
            };
            if (type.Id > 0)
            {
                _repository.Update(type);
            }
            else
            {
                _repository.Create(type);
            }
            await _repository.SaveChangesAsync().ConfigureAwait(false);
            model.Id = type.Id;
            return model;
        }

        public Task Delete(int id, bool onlyMark = true)
        {
            throw new System.NotImplementedException();
        }
    }
}