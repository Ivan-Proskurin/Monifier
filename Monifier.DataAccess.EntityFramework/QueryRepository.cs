using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.EntityFramework
{
    public class QueryRepository<T> : IQueryRepository<T> where T : class, IHasId
    {
        private readonly DbSet<T> _dbSet;

        public QueryRepository(DbSet<T> dbSet)
        {
            _dbSet = dbSet;
        }

        private IQueryable<T> _query;
        public IQueryable<T> Query => _query ?? (_query = _dbSet.AsNoTracking());

        public async Task<T> GetById(int id)
        {
            return await Query.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
