using System.Threading.Tasks;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Contract
{
    public interface INamedModelQueryRepository<T> : IQueryRepository<T> where T : class, IHasId, IHasName, IHasOwnerId
    {
        Task<T> GetByName(int ownerId, string name);
    }
}
