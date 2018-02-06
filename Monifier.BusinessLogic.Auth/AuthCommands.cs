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
        private readonly IUnitOfWork _unitOfWork;

        public AuthCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<int> CreateUser(string name, string login, string password, bool isAdmin)
        {
            if (name.IsNullOrEmpty())
                throw new ArgumentException("Name should be non empty string", nameof(name));
            
            if (login.IsNullOrEmpty())
                throw new ArgumentException("Login should be non empty string", nameof(login));
            
            if (password.IsNullOrEmpty())
                throw new ArgumentException("Password should be non empty string", nameof(password));

            var other = await _unitOfWork.GetQueryRepository<User>().GetByLogin(login);
            if (other != null)
                throw new AuthException($"Пользователь с логином {login} уже есть");

            var user = new User
            {
                Name = name,
                Login = login,
                IsAdmin = isAdmin,
                IsDeleted = false
            };

            user.Salt = HashHelper.CreateSalt();
            user.Hash = HashHelper.ComputeHash(password, user.Salt);
            
            _unitOfWork.GetCommandRepository<User>().Create(user);

            await _unitOfWork.SaveChangesAsync();
            
            return user.Id;
        }
    }
}