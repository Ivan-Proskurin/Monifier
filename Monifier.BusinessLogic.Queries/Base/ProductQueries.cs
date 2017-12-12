using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class ProductQueries : IProductQueries
    {
        private readonly IUnitOfWork _unitOfWork;

        private static ProductModel ToModel(Product product)
        {
            return new ProductModel
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name
            };
        }

        public ProductQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<ProductModel>> GetAll(bool includeDeleted = false)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductModel> GetById(int id)
        {
            var prodRepo = _unitOfWork.GetQueryRepository<Product>();
            var product = await prodRepo.GetById(id);
            if (product == null)
                throw new ArgumentException($"Нет продукта с идентификатором {id}");
            return ToModel(product);
        }

        public async Task<ProductModel> GetByName(string name, bool includeDeleted = false)
        {
            var productRepo = _unitOfWork.GetNamedModelQueryRepository<Product>();
            var product = await productRepo.GetByName(name);
            if (product == null || product.IsDeleted && !includeDeleted) return null;
            return ToModel(product);
        }

        public async Task<List<ProductModel>> GetCategoryProducts(int categoryId, bool includeDeleted = false)
        {
            var productRepo = _unitOfWork.GetQueryRepository<Product>();
            var query = includeDeleted ? productRepo.Query : productRepo.Query.Where(x => !x.IsDeleted);
            return await query
                .Where(x => x.CategoryId == categoryId)
                .Select(x => ToModel(x)).ToListAsync();
        }

        private IQueryable<ProductModel> CreateFlowProductsQuery(int flowId, bool includeDeleted = false)
        {
            var expensesFlowsQuery = _unitOfWork.GetQueryRepository<ExpenseFlow>().Query;
            var expensesCategoriesQuery = _unitOfWork.GetQueryRepository<ExpensesFlowProductCategory>().Query;
            var categoriesQuery = _unitOfWork.GetQueryRepository<Category>().Query;
            var productQuery = _unitOfWork.GetQueryRepository<Product>().Query;
            var query = from expense in expensesFlowsQuery
                join expcat in expensesCategoriesQuery on expense.Id equals expcat.ExpensesFlowId
                join cat in categoriesQuery on expcat.CategoryId equals cat.Id
                join prod in productQuery on cat.Id equals prod.CategoryId
                where expense.Id == flowId && (!expense.IsDeleted && !cat.IsDeleted && !prod.IsDeleted || includeDeleted)
                select ToModel(prod);
            return query;
        }

        public async Task<List<ProductModel>> GetExpensesFlowProducts(int expenseFlowId, bool includeDeleted = false)
        {
            return await CreateFlowProductsQuery(expenseFlowId, includeDeleted).ToListAsync();
        }

        public async Task<ProductModel> GetFlowProductByName(int flowId, string product)
        {
            if (string.IsNullOrEmpty(product))
                throw new ArgumentNullException(nameof(product));
            
            var query = CreateFlowProductsQuery(flowId);
            return await query.Where(x => x.Name.ToLower() == product.ToLower()).FirstOrDefaultAsync();
        }
    }
}
