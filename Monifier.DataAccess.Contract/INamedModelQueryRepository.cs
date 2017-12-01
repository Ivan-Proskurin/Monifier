using System.Threading.Tasks;
using Monifier.DataAccess.Contract.Model;

namespace Monifier.DataAccess.Contract
{
    public interface INamedModelQueryRepository<T> : IQueryRepository<T> where T : class, IHasId, IHasName
    {
        Task<T> GetByName(string name);
    }
}
