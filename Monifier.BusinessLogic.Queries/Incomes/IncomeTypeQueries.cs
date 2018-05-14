using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Incomes
{
    public class IncomeTypeQueries : IIncomeTypeQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public IncomeTypeQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public Task<List<IncomeTypeModel>> GetAll(bool sortByName = false, bool includeDeleted = false)
        {
            var ownerId = _currentSession.UserId;
            var query = _repository.GetQuery<IncomeType>()
                .Where(x => x.OwnerId == ownerId)
                .Select(x => new IncomeTypeModel
                {
                    Id = x.Id,
                    Name = x.Name
                });
            var sortedQuery = sortByName ? query.OrderBy(x => x.Name) : query.OrderBy(x => x.Id);
            return sortedQuery.ToListAsync();
        }

        public async Task<IncomeTypeModel> GetById(int id)
        {
            var type = await _repository.LoadAsync<IncomeType>(id).ConfigureAwait(false);
            if (type == null) return null;
            return new IncomeTypeModel
            {
                Id = type.Id,
                Name = type.Name
            };
        }

        public async Task<IncomeTypeModel> GetByName(string name, bool includeDeleted = false)
        {
            var type = await _repository.FindByNameAsync<IncomeType>(_currentSession.UserId, name).ConfigureAwait(false);
            if (type == null) return null;
            return new IncomeTypeModel
            {
                Id = type.Id,
                Name = type.Name
            };
        }

        public Task<List<IncomeTypeModel>> GetFiltered(DateTime dateFrom, DateTime dateTo)
        {
            var typesQuery = _repository.GetQuery<IncomeType>();
            var itemsQuery = _repository.GetQuery<IncomeItem>();
            var ownerId = _currentSession.UserId;
            var query = from t in typesQuery
                        join i in itemsQuery on t.Id equals i.IncomeTypeId into typeItemsJoin
                        from ti in typeItemsJoin.DefaultIfEmpty()
                        where ti.OwnerId == ownerId && ti.DateTime >= dateFrom && ti.DateTime < dateTo
                        group new { ti.IncomeType, ti.Total } by ti.IncomeTypeId into tiGroup
                        select new IncomeTypeModel
                        {
                            Id = tiGroup.Key,
                            Name = tiGroup.FirstOrDefault().IncomeType.Name,
                            SumTotal = tiGroup.Sum(x => x.Total)
                        };
            return query.ToListAsync();
        }
    }
}