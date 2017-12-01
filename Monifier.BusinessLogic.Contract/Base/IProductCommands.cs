using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IProductCommands : ICommonModelCommands<ProductModel>
    {
        Task<ProductModel> AddProductToCategory(int categoryId, string productName);
        Task<List<int>> GroupDeletion(int[] ids, bool onlyMark = true);
    }
}