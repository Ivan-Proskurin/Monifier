using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class ProductQueries : IProductQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public ProductQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public Task<List<ProductModel>> GetAll(bool sortByName = false, bool includeDeleted = false)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductModel> GetById(int id)
        {
            var product = await _repository.LoadAsync<Product>(id);
            if (product == null)
                throw new ArgumentException($"Нет продукта с идентификатором {id}");
            return ToModel(product);
        }

        public async Task<ProductModel> GetByName(string name, bool includeDeleted = false)
        {
            var product = await _repository.FindByNameAsync<Product>(_currentSession.UserId, name);
            if (product == null || product.IsDeleted && !includeDeleted) return null;
            return ToModel(product);
        }

        public Task<List<ProductModel>> GetCategoryProducts(int categoryId, bool includeDeleted = false)
        {
            var query = includeDeleted 
                ? _repository.GetQuery<Product>() 
                : _repository.GetQuery<Product>().Where(x => !x.IsDeleted);

            return query
                .Where(x => x.CategoryId == categoryId)
                .Select(x => ToModel(x)).ToListAsync();
        }

        public async Task<ProductList> GetList(int categoryId, PaginationArgs args)
        {
            var query = _repository.GetQuery<Product>().Where(x => !x.IsDeleted)
                .Where(x => x.CategoryId == categoryId);
            var totalCount = await query.CountAsync();
            var pagination = new PaginationInfo(args, totalCount);
            var models = await query
                .OrderBy(x => x.Id)
                .Skip(pagination.Skipped).Take(pagination.Taken)
                .Select(x => ToModel(x))
                .ToListAsync()
                .ConfigureAwait(false);

            return new ProductList
            {
                Products = models,
                Pagination = pagination
            };
        }

        private IQueryable<ProductModel> CreateFlowProductsQuery(int flowId, bool includeDeleted = false)
        {
            var expensesFlowsQuery = _repository.GetQuery<ExpenseFlow>();
            var expensesCategoriesQuery = _repository.GetQuery<ExpensesFlowProductCategory>();
            var categoriesQuery = _repository.GetQuery<Category>();
            var productQuery = _repository.GetQuery<Product>();
            var query = from expense in expensesFlowsQuery
                join expcat in expensesCategoriesQuery on expense.Id equals expcat.ExpensesFlowId
                join cat in categoriesQuery on expcat.CategoryId equals cat.Id
                join prod in productQuery on cat.Id equals prod.CategoryId
                where expense.Id == flowId && (!expense.IsDeleted && !cat.IsDeleted && !prod.IsDeleted || includeDeleted)
                select ToModel(prod);
            return query;
        }

        public Task<List<ProductModel>> GetExpensesFlowProducts(int expenseFlowId, bool includeDeleted = false)
        {
            return CreateFlowProductsQuery(expenseFlowId, includeDeleted).ToListAsync();
        }

        public Task<ProductModel> GetFlowProductByName(int flowId, string product)
        {
            if (string.IsNullOrEmpty(product))
                throw new ArgumentNullException(nameof(product));
            
            var query = CreateFlowProductsQuery(flowId);
            return query.Where(x => x.Name.ToLower() == product.ToLower()).FirstOrDefaultAsync();
        }

        private static ProductModel ToModel(Product product)
        {
            return new ProductModel
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name
            };
        }
    }
}
