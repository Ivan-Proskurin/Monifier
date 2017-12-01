using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Contract.Model;

namespace Monifier.DataAccess.EntityFramework
{
    public class NamedModelQueryRepository<T> : QueryRepository<T>, INamedModelQueryRepository<T> 
        where T : class, IHasId, IHasName
    {
        public NamedModelQueryRepository(DbSet<T> dbSet) : base(dbSet)
        {
        }

        public async Task<T> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            var model = await Query.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
            return model;
        }
    }
}
