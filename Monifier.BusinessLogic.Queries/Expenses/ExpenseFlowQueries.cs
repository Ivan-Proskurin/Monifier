using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpenseFlowQueries : IExpenseFlowQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public ExpenseFlowQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public Task<List<ExpenseFlowModel>> GetAll(bool includeDeleted = false)
        {
            var queries = _repository.GetQuery<ExpenseFlow>();
            var ownerId = _currentSession.UserId;
            return queries
                .Where(x => (!x.IsDeleted || includeDeleted) && x.OwnerId == ownerId)
                .Select(x => x.ToModel())
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public async Task<ExpenseFlowList> GetList(PaginationArgs args)
        {
            var flowQueries = _repository.GetQuery<ExpenseFlow>();
            var ownerId = _currentSession.UserId;
            var query = flowQueries
                .Where(x => (!x.IsDeleted || args.IncludeDeleted) && x.OwnerId == ownerId)
                .OrderByDescending(x => x.Version)
                .ThenBy(x => x.Id);
            
            var totalCount = await query.CountAsync();
            var pagination = new PaginationInfo(args, totalCount);
            var flows = await query
                .Skip(pagination.Skipped)
                .Take(pagination.Taken)
                .Select(x => x.ToModel())
                .ToListAsync()
                .ConfigureAwait(false);

            var total = await query.Select(x => x.Balance).SumAsync().ConfigureAwait(false);
            
            return new ExpenseFlowList
            {
                ExpenseFlows = flows,
                Totals = new TotalsInfoModel
                {
                    Caption = "Всего на балансе:",
                    Total = total
                },
                Pagination = pagination
            };
        }

        public async Task<ExpenseFlowModel> GetById(int id)
        {
            return (await _repository.LoadAsync<ExpenseFlow>(id)).ToModel();
        }

        public async Task<ExpenseFlowModel> GetWithCategories(int id)
        {
            var flow = (await _repository.LoadAsync<ExpenseFlow>(id)).ToModel();
            if (flow == null) return null;
            
            var expenseCategoriesQuery = _repository.GetQuery<ExpensesFlowProductCategory>();
            var catQuery = _repository.GetQuery<Category>();
            var query = from expenseCats in expenseCategoriesQuery
                join cat in catQuery on expenseCats.CategoryId equals cat.Id
                where expenseCats.ExpensesFlowId == id && !cat.IsDeleted
                orderby cat.Id
                select cat.Id;
            
            flow.Categories = await query.ToListAsync().ConfigureAwait(false);
            
            return flow;
        }

        public async Task<ExpenseFlowModel> GetByName(string name, bool includeDeleted = false)
        {
            return (await _repository.FindByNameAsync<ExpenseFlow>(_currentSession.UserId, name)).ToModel();
        }

        public async Task<int?> GetIdByName(string name)
        {
            return name.IsNullOrEmpty() ? null : (await GetByName(name))?.Id;
        }

        public async Task<string> GetNameById(int? id)
        {
            return id == null ? null : (await GetById(id.Value))?.Name;
        }

        public async Task<int> GetNextNumber()
        {
            var flowQuery = _repository.GetQuery<ExpenseFlow>();
            var ownerId = _currentSession.UserId;
            var count = await flowQuery.CountAsync(x => x.OwnerId == ownerId);
            return count == 0 ? 1 : await flowQuery.MaxAsync(x => x.Number) + 1;
        }
    }
}