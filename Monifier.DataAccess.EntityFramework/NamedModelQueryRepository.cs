﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.EntityFramework
{
    public class NamedModelQueryRepository<T> : QueryRepository<T>, INamedModelQueryRepository<T> 
        where T : class, IHasId, IHasName, IHasOwnerId
    {
        public NamedModelQueryRepository(DbSet<T> dbSet) : base(dbSet)
        {
        }

        public async Task<T> GetByName(int ownerId, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            var model = await Query.FirstOrDefaultAsync(x => x.OwnerId == ownerId && x.Name.ToLower() == name.ToLower());
            return model;
        }
    }
}
