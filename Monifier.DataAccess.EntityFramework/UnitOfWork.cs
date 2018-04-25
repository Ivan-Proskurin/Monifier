using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.EntityFramework
{
    public class UnitOfWork : IUnitOfWork
    {
        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        #endregion

        private readonly MonifierDbContext _context;
        private readonly Dictionary<Type, object> _queryRepositories;
        private readonly Dictionary<Type, object> _namedQueryRepositories;
        private readonly Dictionary<Type, object> _commandRepositories;

        public UnitOfWork(MonifierDbContext context)
        {
            _context = context;
            _queryRepositories = new Dictionary<Type, object>();
            _namedQueryRepositories = new Dictionary<Type, object>();
            _commandRepositories = new Dictionary<Type, object>();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public ICommandRepository<T> GetCommandRepository<T>() where T : class, IHasId
        {
            _commandRepositories.TryGetValue(typeof(T), out object repo);
            if (repo != null) return repo as ICommandRepository<T>;
            var props = _context.GetType().GetProperties();
            foreach (var p in props)
            {
                var pType = p.PropertyType;
                var gArgs = pType.GenericTypeArguments;
                if (gArgs.Length > 0 && gArgs[0] == typeof(T))
                {
                    var pValue = p.GetValue(_context, new object[0]);
                    var qr = new CommandRepository<T>(_context, pValue as DbSet<T>);
                    _commandRepositories.Add(typeof(T), qr);
                    return qr;
                }
            }
            return null;
        }

        public IQueryRepository<T> GetQueryRepository<T>() where T : class, IHasId
        {
            _queryRepositories.TryGetValue(typeof(T), out object repo);
            if (repo != null) return repo as IQueryRepository<T>;
            var props = _context.GetType().GetProperties();
            foreach (var p in props)
            {
                var pType = p.PropertyType;
                var gArgs = pType.GenericTypeArguments;
                if (gArgs.Length > 0 && gArgs[0] == typeof(T))
                {
                    var pValue = p.GetValue(_context, new object[0]);
                    var qr = new QueryRepository<T>(pValue as DbSet<T>);
                    _queryRepositories.Add(typeof(T), qr);
                    return qr;
                }
            }
            return null;
        }

        public INamedModelQueryRepository<T> GetNamedModelQueryRepository<T>() where T : class, IHasId, IHasName, IHasOwnerId
        {
            _namedQueryRepositories.TryGetValue(typeof(T), out object repo);
            if (repo != null) return repo as INamedModelQueryRepository<T>;
            var props = _context.GetType().GetProperties();
            foreach (var p in props)
            {
                var pType = p.PropertyType;
                var gArgs = pType.GenericTypeArguments;
                if (gArgs.Length > 0 && gArgs[0] == typeof(T))
                {
                    var pValue = p.GetValue(_context, new object[0]);
                    var qr = new NamedModelQueryRepository<T>(pValue as DbSet<T>);
                    _namedQueryRepositories.Add(typeof(T), qr);
                    return qr;
                }
            }
            return null;
        }

        public async Task<T> LoadEntity<T>(int id) where T : class, IHasId
        {
            var queries = GetQueryRepository<T>();
            return await queries.GetById(id);
        }
    }
}
