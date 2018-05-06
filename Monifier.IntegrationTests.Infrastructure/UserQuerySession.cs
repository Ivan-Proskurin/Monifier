using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Auth;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.BusinessLogic.Queries.Incomes;
using Monifier.BusinessLogic.Queries.Transactions;
using Monifier.BusinessLogic.Support;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.EntityFramework;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;
using Monifier.DataAccess.Model.Distribution;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class UserQuerySession : IDisposable
    {
        #region Fields and constructor

        private readonly IUnitOfWork _unitOfWork;

        public UserQuerySession(string databaseName, User user)
        {
            if (databaseName.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(databaseName));
            _unitOfWork = CreateUnitOfWork(databaseName);
            if (user == null) return;
            User = user;
            UserSession = new CurrentSessionImpl(user);
        }

        #endregion

        #region UnitOfWork, current user and session

        private static IUnitOfWork CreateUnitOfWork(string databaseName)
        {
            var options = new DbContextOptionsBuilder<MonifierDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var context = new MonifierDbContext(options);
            return new UnitOfWork(context);
        }

        public IUnitOfWork UnitOfWork => _unitOfWork;

        public User User { get; }

        public ICurrentSession UserSession { get; }

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
                _unitOfWork?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UserQuerySession()
        {
            Dispose(false);
        }

        #endregion

        #region Entity factories

        public User CreateUser(string name, string login, string password, bool isAdmin)
        {
            var authCommands = new AuthCommands(_unitOfWork);
            var userId = authCommands.CreateUser(name, login, password, isAdmin).Result;
            var userQueries = _unitOfWork.GetQueryRepository<User>();
            return userQueries.GetById(userId).Result;
        }

        public Category CreateCategory(string name)
        {
            var commands = _unitOfWork.GetCommandRepository<Category>();
            var entity = new Category
            {
                Name = name,
                OwnerId = UserSession.UserId
            };
            commands.Create(entity);
            return entity;
        }

        public Product CreateProduct(int categoryId, string name)
        {
            var commands = _unitOfWork.GetCommandRepository<Product>();
            var entity = new Product
            {
                CategoryId = categoryId,
                Name = name,
                OwnerId = UserSession.UserId
            };
            commands.Create(entity);
            return entity;
        }

        public ExpenseFlow CreateExpenseFlow(string name, decimal balance, DateTime date, int number)
        {
            var commands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var entity = new ExpenseFlow
            {
                Name = name,
                Balance = balance,
                DateCreated = date,
                Number = number,
                Version = 1,
                OwnerId = UserSession.UserId
            };
            commands.Create(entity);
            return entity;
        }

        public Account CreateAccount(string name, decimal balance, DateTime date, bool isDefault = false)
        {
            var commands = _unitOfWork.GetCommandRepository<Account>();
            var entity = new Account
            {
                Name = name,
                DateCreated = date,
                Balance = balance,
                AvailBalance = balance,
                Number = 1,
                OwnerId = UserSession.UserId,
                IsDefault = isDefault
            };
            commands.Create(entity);
            return entity;
        }

        public IncomeType CreateIncomeType(string name)
        {
            var commands = _unitOfWork.GetCommandRepository<IncomeType>();
            var entity = new IncomeType
            {
                Name = name,
                OwnerId = UserSession.UserId
            };
            commands.Create(entity);
            return entity;
        }

        public IncomeItem CreateIncome(int typeId, DateTime dateTime, decimal total, int accountId)
        {
            var commands = _unitOfWork.GetCommandRepository<IncomeItem>();
            var entity = new IncomeItem
            {
                AccountId = accountId,
                DateTime = dateTime,
                IncomeTypeId = typeId,
                Total = total,
                OwnerId = UserSession.UserId
            };
            commands.Create(entity);
            return entity;
        }

        public async Task<ExpenseBillModel> CreateExpenseBill(int accountId, int flowId, DateTime dateTime, 
            Product product, decimal cost)
        {
            var bill = new ExpenseBillModel
            {
                AccountId = accountId,
                OwnerId = UserSession.UserId,
                ExpenseFlowId = flowId,
                DateTime = dateTime
            };
            bill.AddItem(new ExpenseItemModel
            {
                CategoryId = product.CategoryId,
                ProductId = product.Id,
                Cost = cost
            });
            await bill.Save(_unitOfWork);
            return bill;
        }

        public AccountFlowSettings CreateAccountFlowSettinsg(int accountId, bool canFlow)
        {
            var commands = _unitOfWork.GetCommandRepository<AccountFlowSettings>();
            var entity = new AccountFlowSettings
            {
                AccountId = accountId,
                CanFlow = canFlow
            };
            commands.Create(entity);
            return entity;
        }

        public ExpenseFlowSettings CreateExpenseFlowSettings(int flowId, bool canFlow, FlowRule rule, decimal amount)
        {
            var commands = _unitOfWork.GetCommandRepository<ExpenseFlowSettings>();
            var entity = new ExpenseFlowSettings
            {
                ExpenseFlowId = flowId,
                CanFlow = canFlow,
                Rule = rule,
                Amount = amount
            };
            commands.Create(entity);
            return entity;
        }

        #endregion

        #region Entities

        public EntityIdSet CreateDefaultEntities()
        {
            SalaryIncome = CreateIncomeType("Зарплата");
            GiftsIncome = CreateIncomeType("Подарки");

            FoodCategory = CreateCategory("Продукты");
            TechCategory = CreateCategory("Техника");

            Bread = CreateProduct(FoodCategory.Id, "Хлеб");
            Meat = CreateProduct(FoodCategory.Id, "Мясо");
            Tv = CreateProduct(TechCategory.Id, "Телевизор");

            FoodExpenseFlow = CreateExpenseFlow("Продукты питания", 1000, DateTime.Today, 1);
            TechExpenseFlow = CreateExpenseFlow("Техника", 30000, DateTime.Today, 2);

            DebitCardAccount = CreateAccount("Дебетовая карта", 15000, DateTime.Today);
            CashAccount = CreateAccount("Наличные", 30000, DateTime.Today);

            Incomes = new[]
                {
                    CreateIncome(SalaryIncome.Id, new DateTime(2018, 01, 31), 100000, DebitCardAccount.Id),
                    CreateIncome(GiftsIncome.Id, new DateTime(2018, 03, 08), 8000, CashAccount.Id)
                }
                .OrderBy(x => x.DateTime).ToList();

            _unitOfWork.SaveChanges();

            return new EntityIdSet
            {
                SalaryIncomeId = SalaryIncome.Id,
                GiftsIncomeId = GiftsIncome.Id,
                FoodCategoryId = FoodCategory.Id,
                TechCategoryId = TechCategory.Id,
                BreadId = Bread.Id,
                MeatId = Meat.Id,
                TvId = Tv.Id,
                FoodExpenseFlowId = FoodExpenseFlow.Id,
                TechExpenseFlowId = TechExpenseFlow.Id,
                DebitCardAccountId = DebitCardAccount.Id,
                CashAccountId = CashAccount.Id,
                IncomeIds = Incomes.Select(x => x.Id).ToList(),
            };
        }

        public async Task<T> LoadEntity<T>(int id) where T : class, IHasId
        {
            var queries = _unitOfWork.GetQueryRepository<T>();
            return await queries.GetById(id);
        }

        public async Task<List<T>> LoadEntities<T>() where T : class, IHasId
        {
            var queries = _unitOfWork.GetQueryRepository<T>();
            return await queries.Query.ToListAsync();
        }

        public async Task LoadDefaultEntities(EntityIdSet idSet)
        {
            SalaryIncome = await LoadEntity<IncomeType>(idSet.SalaryIncomeId);
            GiftsIncome = await LoadEntity<IncomeType>(idSet.GiftsIncomeId);
            FoodCategory = await LoadEntity<Category>(idSet.FoodCategoryId);
            TechCategory = await LoadEntity<Category>(idSet.TechCategoryId);
            Bread = await LoadEntity<Product>(idSet.BreadId);
            Meat = await LoadEntity<Product>(idSet.MeatId);
            Tv = await LoadEntity<Product>(idSet.TvId);
            FoodExpenseFlow = await LoadEntity<ExpenseFlow>(idSet.FoodExpenseFlowId);
            TechExpenseFlow = await LoadEntity<ExpenseFlow>(idSet.TechExpenseFlowId);
            DebitCardAccount = await LoadEntity<Account>(idSet.DebitCardAccountId);
            CashAccount = await LoadEntity<Account>(idSet.CashAccountId);

            Incomes = (await LoadEntities<IncomeItem>()).OrderBy(x => x.DateTime).ToList();
        }

        public async Task UpdateEntity<T>(T entity) where T : class, IHasId
        {
            var commands = _unitOfWork.GetCommandRepository<T>();
            commands.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public IncomeType SalaryIncome { get; private set; }
        public IncomeType GiftsIncome { get; private set; }

        public Category FoodCategory { get; private set; }
        public Category TechCategory { get; private set; }

        public Product Bread { get; private set; }
        public Product Meat { get; private set; }
        public Product Tv { get; private set; }

        public ExpenseFlow FoodExpenseFlow { get; private set; }
        public ExpenseFlow TechExpenseFlow { get; private set; }

        public Account DebitCardAccount { get; private set; }
        public Account CashAccount { get; private set; }

        public List<IncomeItem> Incomes { get; private set; }

        #endregion

        #region Commands & Queries factories

        public IncomeItemCommands CreateIncomeCommands()
        {
            return new IncomeItemCommands(UnitOfWork, UserSession,
                new TransactionCommands(UnitOfWork, UserSession),
                new TransactionQueries(UnitOfWork, UserSession));
        }

        public AccountCommands CreateAccountCommands()
        {
            return new AccountCommands(UnitOfWork, UserSession, 
                new IncomeItemCommands(UnitOfWork, UserSession, new TransactionCommands(UnitOfWork, UserSession), 
                    new TransactionQueries(UnitOfWork, UserSession)),
                new TransactionCommands(UnitOfWork, UserSession),
                new TimeService(UserSession));
        }

        public TransactionQueries CreateTransactionQueries()
        {
            return new TransactionQueries(UnitOfWork, UserSession);
        }

        #endregion
    }
}