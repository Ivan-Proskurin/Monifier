using System;
using System.Threading.Tasks;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Contract
{
    public interface IUnitOfWork : IDisposable
    {
        IQueryRepository<T> GetQueryRepository<T>() where T : class, IHasId;
        INamedModelQueryRepository<T> GetNamedModelQueryRepository<T>() where T : class, IHasId, IHasName;
        ICommandRepository<T> GetCommandRepository<T>() where T : class, IHasId;
        Task SaveChangesAsync();
        void SaveChanges();
    }
}
