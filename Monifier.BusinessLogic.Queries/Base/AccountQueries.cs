using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
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
        private readonly ICurrentSession _currentSession;

        public AccountQueries(IUnitOfWork unitOfWork, ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _currentSession = currentSession;
        }

        public async Task<List<AccountModel>> GetAll(bool includeDeleted = false)
        {
            var queryRep = _unitOfWork.GetQueryRepository<Account>();
            var ownerId = _currentSession.UserId;
            return await queryRep.Query
                .Where(x => (!x.IsDeleted || includeDeleted) && x.OwnerId == ownerId)
                .Select(x => x.ToModel())
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public async Task<AccountList> GetList()
        {
            var accounts = await GetAll();
            return new AccountList
            {
                Accounts = accounts,
                Totals = new AccountsTotals
                {
                    Caption = "Суммарный баланс:",
                    Total = accounts.Sum(x => x.Balance),
                    AvailBalanceTotal = accounts.Sum(x => x.AvailBalance)
                }
            };
        }

        public async Task<AccountModel> GetById(int id)
        {
            return (await _unitOfWork.GetQueryRepository<Account>().GetById(id)).ToModel();
        }

        public async Task<AccountModel> GetByName(string name, bool includeDeleted = false)
        {
            var account = await _unitOfWork.GetNamedModelQueryRepository<Account>().GetByName(_currentSession.UserId, name);
            if (account == null || account.IsDeleted && !includeDeleted) return null;
            return account.ToModel();
        }

        public async Task<int> GetNextNumber()
        {
            var accountQuery = _unitOfWork.GetQueryRepository<Account>().Query;
            var ownerId = _currentSession.UserId;
            var count = await accountQuery.CountAsync(x => x.OwnerId == ownerId);
            return count == 0 ? 1 : await accountQuery.MaxAsync(x => x.Number) + 1;
        }
    }
}