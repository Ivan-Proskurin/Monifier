using System.Linq;
using System.Threading.Tasks;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.EntityFramework
{
    public class EntityRepository : IEntityRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public EntityRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<T> LoadAsync<T>(int id) where T : class, IHasId
        {
            var queries = _unitOfWork.GetQueryRepository<T>();
            return queries.GetById(id);
        }

        public Task<T> FindByNameAsync<T>(int ownerId, string name) where T : class, IHasId, IHasName, IHasOwnerId
        {
            var queries = _unitOfWork.GetNamedModelQueryRepository<T>();
            return queries.GetByName(ownerId, name);
        }

        public IQueryable<T> GetQuery<T>() where T : class, IHasId
        {
            var queries = _unitOfWork.GetQueryRepository<T>();
            return queries.Query;
        }

        public void Create<T>(T entity) where T : class, IHasId
        {
            var commands = _unitOfWork.GetCommandRepository<T>();
            commands.Create(entity);
        }

        public void Update<T>(T entity) where T : class, IHasId
        {
            var commands = _unitOfWork.GetCommandRepository<T>();
            commands.Update(entity);
        }

        public void Delete<T>(T entity) where T : class, IHasId
        {
            var commands = _unitOfWork.GetCommandRepository<T>();
            commands.Delete(entity);
        }

        public void Detach<T>(T entity) where T : class, IHasId
        {
            var commands = _unitOfWork.GetCommandRepository<T>();
            commands.Detach(entity);
        }

        public void SaveChanges()
        {
            _unitOfWork.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return _unitOfWork.SaveChangesAsync();
        }
    }
}