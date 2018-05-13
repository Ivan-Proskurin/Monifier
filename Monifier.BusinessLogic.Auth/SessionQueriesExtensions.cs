using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Auth
{
    public static class SessionQueriesExtensions
    {
        public static Task<Session> GetByToken(this IQueryable<Session> query, Guid token)
        {
            return query.FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}