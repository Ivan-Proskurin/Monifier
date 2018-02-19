using Microsoft.Extensions.DependencyInjection;
using Monifier.BusinessLogic.Auth;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.BusinessLogic.Queries.Distribution;
using Monifier.BusinessLogic.Queries.Expenses;
using Monifier.BusinessLogic.Queries.Incomes;
using Monifier.BusinessLogic.Queries.Inventorization;
using Monifier.BusinessLogic.Queries.Settings;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.EntityFramework;
using Monifier.Web.Auth;

namespace Monifier.Web
{
    public static class BusinessServices
    {
        public static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentSession, CurrentSession>();
            services.AddTransient<IAccountQueries, AccountQueries>();
            services.AddTransient<IAccountCommands, AccountCommands>();
            services.AddTransient<IIncomeTypeQueries, IncomeTypeQueries>();
            services.AddTransient<IIncomeTypeCommands, IncomeTypeCommands>();
            services.AddTransient<IExpenseFlowQueries, ExpenseFlowQueries>();
            services.AddTransient<IExpenseFlowCommands, ExpenseFlowCommands>();
            services.AddTransient<IProductQueries, ProductQueries>();
            services.AddTransient<IProductCommands, ProductCommands>();
            services.AddTransient<ICategoriesQueries, CategoriesQueries>();
            services.AddTransient<ICategoriesCommands, CategoriesCommands>();
            services.AddTransient<IExpensesBillQueries, ExpensesBillQueries>();
            services.AddTransient<IExpensesBillCommands, ExpensesBillCommands>();
            services.AddTransient<IExpensesQueries, ExpensesQueries>();
            services.AddTransient<IUserSettings, UserSettings>();
            services.AddTransient<IIncomesQueries, IncomesQueries>();
            services.AddTransient<IDistributionQueries, DistributionQueries>();
            services.AddTransient<IDistributionCommands, DistributionCommands>();
            services.AddTransient<IFlowDistributor, DefaultFlowDistributor>();
            services.AddTransient<IInventorizationQueries, InventorizationQueries>();
            services.AddTransient<ISessionCommands, SessionCommands>();
        }
    }
}