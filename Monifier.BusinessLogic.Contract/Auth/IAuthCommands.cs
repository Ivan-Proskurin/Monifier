using System.Threading.Tasks;

namespace Monifier.BusinessLogic.Contract.Auth
{
    public interface IAuthCommands
    {
        Task<int> CreateUser(string name, string login, string password, bool isAdmin);
        Task UpdateUser(int userId, string newName, string newPassword);
    }
}