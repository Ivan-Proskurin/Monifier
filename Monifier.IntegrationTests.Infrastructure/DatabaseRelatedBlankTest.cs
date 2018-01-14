﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.EntityFramework;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class DatabaseRelatedBlankTest : IDisposable
    {
        #region Fields and constructor

        private readonly string _databaseName;
        protected readonly IUnitOfWork UnitOfWork;

        public DatabaseRelatedBlankTest()
        {
            _databaseName = Guid.NewGuid().ToString();
            UnitOfWork = CreateUnitOfWork();
        }
        
        #endregion
        
        #region UnitOfWork

        private IUnitOfWork CreateUnitOfWork()
        {
            var options = new DbContextOptionsBuilder<MonifierDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;
            var context = new MonifierDbContext(options);
            return new UnitOfWork(context);
        }

        protected void UseUnitOfWork(Action<IUnitOfWork> action)
        {
            using (var uof = CreateUnitOfWork())
            {
                action(uof);
            }
        }
        
        protected async Task UseUnitOfWorkAsync(Func<IUnitOfWork, Task> action)
        {
            using (var uof = CreateUnitOfWork())
            {
                await action(uof);
            }
        }

        protected async Task<T> UseUnitOfWorkAsync<T>(Func<IUnitOfWork, Task<T>> action)
        {
            using (var uof = CreateUnitOfWork())
            {
                return await action(uof);
            }
        }
        
        #endregion
        
        #region Disposable
        
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                UnitOfWork?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DatabaseRelatedBlankTest()
        {
            Dispose(false);
        }
        
        #endregion
        
        #region Entities

        protected Category CreateCategory(string name, IUnitOfWork uof = null)
        {
            if (uof == null) uof = UnitOfWork;
            var commands = uof.GetCommandRepository<Category>();
            var entity = new Category
            {
                Name = name,
            };
            commands.Create(entity);
            return entity;
        }

        protected Product CreateProduct(int categoryId, string name, IUnitOfWork uof = null)
        {
            if (uof == null) uof = UnitOfWork;
            var commands = uof.GetCommandRepository<Product>();
            var entity = new Product
            {
                CategoryId = categoryId,
                Name = name
            };
            commands.Create(entity);
            return entity;
        }

        protected ExpenseFlow CreateExpenseFlow(string name, decimal balance, DateTime date, int number, 
            IUnitOfWork uof = null)
        {
            if (uof == null) uof = UnitOfWork;
            var commands = uof.GetCommandRepository<ExpenseFlow>();
            var entity = new ExpenseFlow
            {
                Name = name,
                Balance = balance,
                DateCreated = date,
                Number = number,
                Version = 1,
            };
            commands.Create(entity);
            return entity;
        }

        protected Account CreateAccount(string name, decimal balance, DateTime date, IUnitOfWork uof = null)
        {
            if (uof == null) uof = UnitOfWork;
            var commands = uof.GetCommandRepository<Account>();
            var entity = new Account
            {
                Name = name,
                DateCreated = date,
                Balance = balance,
                AvailBalance = balance,
                Number = 1,
            };
            commands.Create(entity);
            return entity;
        }

        #endregion
    }
}