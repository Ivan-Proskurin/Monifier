using System.Linq;
using System.Threading.Tasks;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Contract
{
    public interface IEntityRepository
    {
        Task<T> LoadAsync<T>(int id) where T : class, IHasId;
        Task<T> FindByNameAsync<T>(int ownerId, string name) where T : class, IHasId, IHasName, IHasOwnerId;
        IQueryable<T> GetQuery<T>() where T : class, IHasId;
        void Create<T>(T entity) where T : class, IHasId;
        void Update<T>(T entity) where T : class, IHasId;
        void Delete<T>(T entity) where T : class, IHasId;
        void Detach<T>(T entity) where T : class, IHasId;
        void SaveChanges();
        Task SaveChangesAsync();
    }
}