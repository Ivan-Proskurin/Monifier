using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IProductQueries : ICommonModelQueries<ProductModel>
    {
        Task<List<ProductModel>> GetCategoryProducts(int categoryId, bool includeDeleted = false);
        Task<ProductList> GetList(int categoryId, PaginationArgs args);
        Task<List<ProductModel>> GetExpensesFlowProducts(int expenseFlowId, bool includeDeleted = false);
        Task<ProductModel> GetFlowProductByName(int flowId, string product);
    }
}
