using System;
using System.Threading.Tasks;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Contract.Auth
{
    public interface ISessionCommands
    {
        Task<Session> CreateSession(string login, string password);
        Task<bool> Authorize(Guid token, bool isAdmin);
    }
}