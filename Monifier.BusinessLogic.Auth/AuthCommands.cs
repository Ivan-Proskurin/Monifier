using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Auth
{
    public class AuthCommands : IAuthCommands
    {
        private readonly IEntityRepository _repository;

        public AuthCommands(IEntityRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<int> CreateUser(string name, string login, string password, bool isAdmin)
        {
            if (name.IsNullOrEmpty())
                throw new ArgumentException("Name should be non empty string", nameof(name));
            
            if (login.IsNullOrEmpty())
                throw new ArgumentException("Login should be non empty string", nameof(login));
            
            if (password.IsNullOrEmpty())
                throw new ArgumentException("Password should be non empty string", nameof(password));

            var other = await _repository.GetQuery<User>().GetByLogin(login).ConfigureAwait(false);
            if (other != null)
                throw new AuthException($"Пользователь с логином {login} уже есть");

            var user = new User
            {
                Name = name,
                Login = login,
                IsAdmin = isAdmin,
                IsDeleted = false,
                Salt = HashHelper.CreateSalt()
            };

            user.Hash = HashHelper.ComputeHash(password, user.Salt);
            
            _repository.Create(user);

            await _repository.SaveChangesAsync().ConfigureAwait(false);
            
            return user.Id;
        }

        public async Task UpdateUser(int userId, string newName, string newPassword)
        {
            if (newName == null && newPassword == null)
                throw new ArgumentException("Provide either newName or newPassword");
            var user = await _repository.LoadAsync<User>(userId).ConfigureAwait(false);
            if (user == null)
                throw new ArgumentException($"Threre is no user with id {userId}", nameof(userId));

            if (newName != null)
                user.Name = newName;
            if (newPassword != null)
            {
                user.Salt = HashHelper.CreateSalt();
                user.Hash = HashHelper.ComputeHash(newPassword, user.Salt);
            }

            _repository.Update(user);
            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}