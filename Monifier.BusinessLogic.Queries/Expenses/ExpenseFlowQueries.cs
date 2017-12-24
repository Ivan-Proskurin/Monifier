using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpenseFlowQueries : IExpenseFlowQueries
    {
        private readonly IUnitOfWork _unitOfWork;

        private static ExpenseFlowModel ToModel(ExpenseFlow entity)
        {
            if (entity == null) return null;
            return new ExpenseFlowModel
            {
                Id = entity.Id,
                Number = entity.Number,
                Name = entity.Name,
                DateCreated = entity.DateCreated,
                Balance = entity.Balance,
            };
        }

        public ExpenseFlowQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<List<ExpenseFlowModel>> GetAll(bool includeDeleted = false)
        {
            var queries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            return await queries.Query
                .Where(x => !x.IsDeleted || includeDeleted)
                .Select(x => ToModel(x))
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public async Task<ExpenseFlowModel> GetById(int id)
        {
            return ToModel(await _unitOfWork.GetQueryRepository<ExpenseFlow>().GetById(id));
        }

        public async Task<ExpenseFlowModel> GetWithCategories(int id)
        {
            var flow = ToModel(await _unitOfWork.GetQueryRepository<ExpenseFlow>().GetById(id));
            if (flow == null) return null;
            
            var expenseCategoriesQuery = _unitOfWork.GetQueryRepository<ExpensesFlowProductCategory>().Query;
            var catQuery = _unitOfWork.GetQueryRepository<Category>().Query;
            var query = from expenseCats in expenseCategoriesQuery
                join cat in catQuery on expenseCats.CategoryId equals cat.Id
                where expenseCats.ExpensesFlowId == id && !cat.IsDeleted
                orderby cat.Id
                select cat.Id;
            
            flow.Categories = await query.ToListAsync();
            
            return flow;
        }

        public Task<ExpenseFlowModel> GetByName(string name, bool includeDeleted = false)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> GetNextNumber()
        {
            var flowQuery = _unitOfWork.GetQueryRepository<ExpenseFlow>().Query;
            var count = await flowQuery.CountAsync();
            return count == 0 ? 1 : await flowQuery.MaxAsync(x => x.Number) + 1;
        }
    }
}