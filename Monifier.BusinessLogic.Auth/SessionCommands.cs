using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Auth
{
    public class SessionCommands : ISessionCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public SessionCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Guid> CreateSession(string login, string password)
        {
            if (login.IsNullOrEmpty())
                throw new ArgumentException("Login must be a non empty string", nameof(login));
            if (password.IsNullOrEmpty())
                throw new ArgumentException("Password must be a non empty sttring", nameof(password));
            
            var userQueries = _unitOfWork.GetQueryRepository<User>();
            var user = await userQueries.GetByLogin(login);
            if (user == null) 
                throw new AuthException($"Неверный логин или пароль");

            var hash = HashHelper.ComputeHash(password, user.Salt);
            if (hash != user.Hash)
                throw new AuthException($"Неверный логин или пароль");
            
            var session = new Session
            {
                Token = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                UserId = user.Id,
                IsAdmin = user.IsAdmin
            };
            
            _unitOfWork.GetCommandRepository<Session>().Create(session);

            await _unitOfWork.SaveChangesAsync();
            
            return session.Token;
        }

        public async Task<bool> Authorize(Guid token, bool isAdmin)
        {
            var sessionQueries = _unitOfWork.GetQueryRepository<Session>();
            var session = await sessionQueries.GetByToken(token);
            return session != null && (isAdmin && session.IsAdmin || !isAdmin);
        }
    }
}