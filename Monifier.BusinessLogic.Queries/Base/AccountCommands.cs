using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class AccountCommands : IAccountCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AccountModel> Update(AccountModel model)
        {
            var queries = _unitOfWork.GetNamedModelQueryRepository<Account>();
            var commands = _unitOfWork.GetCommandRepository<Account>();

            var other = await queries.GetByName(model.Name);
            if (other != null && other.Id != model.Id)
                throw new ArgumentException("Счет с таким названием уже есть");

            var account = new Account
            {
                Id = model.Id,
                Name = model.Name,
                Balance = model.Balance
            };
            if (model.Id < 0)
                commands.Create(account);
            else
                commands.Update(account);

            await _unitOfWork.SaveChangesAsync();

            model.Id = account.Id;
            return model;
        }

        public Task Delete(int id, bool onlyMark = true)
        {
            throw new System.NotImplementedException();
        }
    }
}