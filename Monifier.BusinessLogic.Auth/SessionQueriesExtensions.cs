using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Auth
{
    public static class SessionQueriesExtensions
    {
        public static async Task<Session> GetByToken(this IQueryRepository<Session> queries, Guid token)
        {
            return await queries.Query.FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}