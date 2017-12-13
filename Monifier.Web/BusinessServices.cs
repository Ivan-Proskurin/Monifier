using Microsoft.Extensions.DependencyInjection;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.BusinessLogic.Queries.Expenses;
using Monifier.BusinessLogic.Queries.Incomes;
using Monifier.BusinessLogic.Queries.Settings;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.EntityFramework;

namespace Monifier.Web
{
    public static class BusinessServices
    {
        public static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
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
        }
    }
}