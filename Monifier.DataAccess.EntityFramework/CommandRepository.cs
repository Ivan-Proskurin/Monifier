using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Contract.Model;

namespace Monifier.DataAccess.EntityFramework
{
    public class CommandRepository<T> : ICommandRepository<T> where T : class, IHasId
    {
        private readonly MonifierDbContext _context;
        private readonly DbSet<T> _dbSet;

        public CommandRepository(MonifierDbContext context, DbSet<T> dbSet)
        {
            _context = context;
            _dbSet = dbSet;
        }

        public void Create(T model)
        {
            _dbSet.Add(model);
        }

        public void Delete(T model)
        {
            _dbSet.Remove(model);
        }

        public void Update(T model)
        {
            _dbSet.Attach(model);
            _context.Entry(model).State = EntityState.Modified;
        }

        public void Detach(T model)
        {
            _context.Entry(model).State = EntityState.Detached;
        }
    }
}
