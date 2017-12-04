using Microsoft.Extensions.DependencyInjection;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.BusinessLogic.Queries.Incomes;
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
        }
    }
}