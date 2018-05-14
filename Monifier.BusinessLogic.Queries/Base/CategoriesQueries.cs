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
    public class CategoriesQueries : ICategoriesQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public CategoriesQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public Task<List<CategoryModel>> GetAll(bool sortByName = false, bool includeDeleted = false)
        {
            var query = includeDeleted 
                ? _repository.GetQuery<Category>() 
                : _repository.GetQuery<Category>().Where(x => !x.IsDeleted);

            var ownerId = _currentSession.UserId;
            var modelQuery = query
                .Where(x => x.OwnerId == ownerId)
                .Select(x => new CategoryModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ProductCount = x.Products.Count
                    }
                );
            var sortedQuery = sortByName ? modelQuery.OrderBy(x => x.Name) : modelQuery.OrderBy(x => x.Id);
            return sortedQuery.ToListAsync();
        }

        public Task<CategoryModel> GetById(int id)
        {
            return _repository.GetQuery<Category>().Where(x => x.Id == id)
                .Select(x => new CategoryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = x.Products.Count
                }
            ).FirstOrDefaultAsync();
        }

        public async Task<CategoryModel> GetByName(string name, bool includeDeleted = false)
        {
            var category = await _repository.FindByNameAsync<Category>(_currentSession.UserId, name);
            if (category == null || category.IsDeleted && !includeDeleted) return null;
            return new CategoryModel
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<List<ProductModel>> GetProductsByCategoryName(string categoryName, bool includeDeleted = false)
        {
            var category = await _repository.FindByNameAsync<Category>(_currentSession.UserId, categoryName);
            
            if (category == null)
                throw new ArgumentException($"Нет категории с названием \"{categoryName}\"");

            var query = includeDeleted 
                ? _repository.GetQuery<Product>() 
                : _repository.GetQuery<Product>().Where(x => !x.IsDeleted);

            var products = await query
                .Where(x => x.CategoryId == category.Id)
                .Select(x => new ProductModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return products;
        }

        public async Task<CategoryList> GetList(PaginationArgs args)
        {
            var ownerId = _currentSession.UserId;
            var query = args.IncludeDeleted 
                ? _repository.GetQuery<Category>() 
                : _repository.GetQuery<Category>().Where(x => !x.IsDeleted);

            query = query.Where(x => x.OwnerId == ownerId);
            var totalCount = await query.CountAsync();
            var pagination = new PaginationInfo(args, totalCount);
            var models = await query
                .OrderBy(x => x.Id)
                .Skip(pagination.Skipped).Take(pagination.Taken)
                .Select(x => new CategoryModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ProductCount = x.Products.Count
                    }
                )
                .ToListAsync()
                .ConfigureAwait(false);

            return new CategoryList
            {
                List = models,
                Pagination = pagination
            };
        }

        private IQueryable<CategoryModel> CreateFlowCategoriesQuery(int flowId)
        {
            var expenseCategoriesQuery = _repository.GetQuery<ExpensesFlowProductCategory>();
            var catQuery = _repository.GetQuery<Category>();
            var query = from expenseCats in expenseCategoriesQuery
                join cat in catQuery on expenseCats.CategoryId equals cat.Id
                where expenseCats.ExpensesFlowId == flowId && !cat.IsDeleted
                orderby cat.Id
                select new CategoryModel
                {
                    Id = cat.Id,
                    Name = cat.Name
                };
            return query;
        }

        public Task<List<CategoryModel>> GetFlowCategories(int flowId)
        {
            return CreateFlowCategoriesQuery(flowId).ToListAsync();
        }

        public Task<CategoryModel> GetFlowCategoryByName(int flowId, string category)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentNullException(nameof(category));
            var query = CreateFlowCategoriesQuery(flowId);
            return query.Where(x => x.Name.ToLower() == category.ToLower()).FirstOrDefaultAsync();
        }

        public Task<CategoryModel> GetFlowCategoryByProductName(int flowId, string product)
        {
            var categoriesQuery = _repository.GetQuery<Category>();
            var flowCategoriesQuery = _repository.GetQuery<ExpensesFlowProductCategory>();
            var productsQuery = _repository.GetQuery<Product>();
            var query =
                from cat in categoriesQuery
                join catFlow in flowCategoriesQuery on cat.Id equals catFlow.CategoryId
                join prod in productsQuery on catFlow.CategoryId equals prod.CategoryId
                where catFlow.ExpensesFlowId == flowId
                      && string.Equals(prod.Name, product, StringComparison.CurrentCultureIgnoreCase)
                select new CategoryModel
                {
                    Id = cat.Id,
                    Name = cat.Name
                };
            return query.FirstOrDefaultAsync();
        }
    }
}
