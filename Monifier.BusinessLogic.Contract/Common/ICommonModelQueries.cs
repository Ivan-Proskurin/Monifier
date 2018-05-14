using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monifier.BusinessLogic.Contract.Common
{
    public interface ICommonModelQueries<T> where T : class
    {
        Task<List<T>> GetAll(bool sortByName = false, bool includeDeleted = false);
        Task<T> GetById(int id);
        Task<T> GetByName(string name, bool includeDeleted = false);
    }
}
