using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Auth
{
    public static class UserQueriesExtensions
    {
        public static async Task<User> GetByLogin(this IQueryRepository<User> userQueries, string login)
        {
            return await userQueries.Query.FirstOrDefaultAsync(x => x.Login == login);
        }
    }
}