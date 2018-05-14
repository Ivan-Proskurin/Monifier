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
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public AccountQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public Task<List<AccountModel>> GetAll(bool sortByName = false, bool includeDeleted = false)
        {
            var ownerId = _currentSession.UserId;
            var query = _repository.GetQuery<Account>()
                .Where(x => (!x.IsDeleted || includeDeleted) && x.OwnerId == ownerId)
                .Select(x => x.ToModel());
            var sortedQuery = sortByName ? query.OrderBy(x => x.Name) : query.OrderBy(x => x.Number);
            return sortedQuery.ToListAsync();
        }

        public async Task<AccountList> GetList()
        {
            var accounts = await GetAll().ConfigureAwait(false);
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
            return (await _repository.LoadAsync<Account>(id)).ToModel();
        }

        public async Task<AccountModel> GetByName(string name, bool includeDeleted = false)
        {
            var account = await _repository.FindByNameAsync<Account>(_currentSession.UserId, name);
            if (account == null || account.IsDeleted && !includeDeleted) return null;
            return account.ToModel();
        }

        public async Task<int> GetNextNumber()
        {
            var ownerId = _currentSession.UserId;
            var count = await _repository.GetQuery<Account>().CountAsync(x => x.OwnerId == ownerId);
            return count == 0 ? 1 : await _repository.GetQuery<Account>().MaxAsync(x => x.Number) + 1;
        }
    }
}