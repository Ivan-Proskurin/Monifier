using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IProductQueries : ICommonModelQueries<ProductModel>
    {
        Task<List<ProductModel>> GetCategoryProducts(int categoryId, bool includeDeleted = false);
    }
}
