using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class AccountQueries : IAccountQueries
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static AccountModel ToModel(Account account)
        {
            if (account == null) return null;
            return new AccountModel
            {
                Id = account.Id,
                Number = account.Number,
                DateCreated = account.DateCreated,
                Name = account.Name,
                Balance = account.Balance
            };
        }

        public async Task<List<AccountModel>> GetAll(bool includeDeleted = false)
        {
            var queryRep = _unitOfWork.GetQueryRepository<Account>();
            return await queryRep.Query
                .Where(x => !x.IsDeleted || includeDeleted)
                .Select(x => ToModel(x))
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public async Task<AccountList> GetList()
        {
            var accounts = await GetAll();
            return new AccountList
            {
                Accounts = accounts,
                Totals = new TotalsInfoModel
                {
                    Caption = "Суммарный баланс:",
                    Total = accounts.Sum(x => x.Balance)
                }
            };
        }

        public async Task<AccountModel> GetById(int id)
        {
            return ToModel(await _unitOfWork.GetQueryRepository<Account>().GetById(id));
        }

        public async Task<AccountModel> GetByName(string name, bool includeDeleted = false)
        {
            var account = await _unitOfWork.GetNamedModelQueryRepository<Account>().GetByName(name);
            if (account == null || account.IsDeleted && !includeDeleted) return null;
            return ToModel(account);
        }

        public async Task<int> GetNextNumber()
        {
            return await _unitOfWork.GetQueryRepository<Account>().Query.MaxAsync(x => x.Number) + 1;
        }
    }
}