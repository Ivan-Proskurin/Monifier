using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoryModel>> GetAll(bool includeDeleted = false)
        {
            var repo = _unitOfWork.GetQueryRepository<Category>();
            var query = includeDeleted ? repo.Query : repo.Query.Where(x => !x.IsDeleted);
            return await query
                .Select(x => new CategoryModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = x.Products.Count
                }
                ).ToListAsync();
        }

        public async Task<CategoryModel> GetById(int id)
        {
            var repo = _unitOfWork.GetQueryRepository<Category>();
            return await repo.Query.Where(x => x.Id == id)
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
            var category = await _unitOfWork.GetNamedModelQueryRepository<Category>().GetByName(name);
            if (category == null || category.IsDeleted && !includeDeleted) return null;
            return new CategoryModel
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<List<ProductModel>> GetProductsByCategoryName(string categoryName, bool includeDeleted = false)
        {
            var category = await _unitOfWork.GetNamedModelQueryRepository<Category>().GetByName(categoryName);
            if (category == null)
                throw new ArgumentException($"Нет категории с названием \"{categoryName}\"");

            var productsRepo = _unitOfWork.GetQueryRepository<Product>();
            var query = includeDeleted ? productsRepo.Query : productsRepo.Query.Where(x => !x.IsDeleted);
            var products = await query
                .Where(x => x.CategoryId == category.Id)
                .Select(x => new ProductModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
            return products;
        }

        public async Task<CategoryList> GetList(PaginationArgs args)
        {
            var repo = _unitOfWork.GetQueryRepository<Category>();
            var query = args.IncludeDeleted ? repo.Query : repo.Query.Where(x => !x.IsDeleted);
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
                ).ToListAsync();
            return new CategoryList()
            {
                List = models,
                Pagination = pagination
            };
        }

        private IQueryable<CategoryModel> CreateFlowCategoriesQuery(int flowId)
        {
            var expenseCategoriesQuery = _unitOfWork.GetQueryRepository<ExpensesFlowProductCategory>().Query;
            var catQuery = _unitOfWork.GetQueryRepository<Category>().Query;
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

        public async Task<List<CategoryModel>> GetFlowCategories(int flowId)
        {
            return await CreateFlowCategoriesQuery(flowId).ToListAsync();
        }

        public async Task<CategoryModel> GetFlowCategoryByName(int flowId, string category)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentNullException(nameof(category));
            var query = CreateFlowCategoriesQuery(flowId);
            return await query.Where(x => x.Name.ToLower() == category.ToLower()).FirstOrDefaultAsync();
        }
    }
}
