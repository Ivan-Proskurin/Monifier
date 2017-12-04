using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IAccountQueries : ICommonModelQueries<AccountModel>
    {
        Task<AccountList> GetList();
        Task<int> GetNextNumber();
    }
}