using System;
using System.Threading.Tasks;

namespace Monifier.BusinessLogic.Contract.Auth
{
    public interface ISessionCommands
    {
        Task<Guid> CreateSession(string login, string password);
        Task<bool> Authorize(Guid token, bool isAdmin);
    }
}