using System.Threading.Tasks;

namespace Monifier.BusinessLogic.Contract.Common
{
    public interface ICommonModelCommands<T> where T : class
    {
        Task<T> Update(T model);
        Task Delete(int id, bool onlyMark = true);
    }
}
