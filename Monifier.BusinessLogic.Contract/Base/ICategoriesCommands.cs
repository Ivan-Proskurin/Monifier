using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface ICategoriesCommands : ICommonModelCommands<CategoryModel>
    {
        Task<CategoryModel> CreateNewOrBind(int flowId, string categoryName);
    }
}
