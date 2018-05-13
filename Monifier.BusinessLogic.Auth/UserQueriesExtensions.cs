using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Auth
{
    public static class UserQueriesExtensions
    {
        public static Task<User> GetByLogin(this IQueryable<User> query, string login)
        {
            return query.FirstOrDefaultAsync(x => x.Login == login);
        }
    }
}