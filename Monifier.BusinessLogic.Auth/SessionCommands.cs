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
        private readonly IEntityRepository _repository;

        public SessionCommands(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<User> CheckUser(string login, string password)
        {
            if (login.IsNullOrEmpty())
                throw new ArgumentException("Login must be a non empty string", nameof(login));
            if (password.IsNullOrEmpty())
                throw new ArgumentException("Password must be a non empty sttring", nameof(password));

            var user = await _repository.GetQuery<User>().GetByLogin(login).ConfigureAwait(false);
            if (user == null)
                throw new AuthException($"Неверный логин или пароль");

            var hash = HashHelper.ComputeHash(password, user.Salt);
            if (hash != user.Hash)
                throw new AuthException($"Неверный логин или пароль");
            return user;
        }

        public async Task<Session> CreateSession(string login, string password)
        {
            var user = await CheckUser(login, password);

            var session = new Session
            {
                Token = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                UserId = user.Id,
                IsAdmin = user.IsAdmin
            };
            
            _repository.Create(session);

            await _repository.SaveChangesAsync().ConfigureAwait(false);

            session.User = user;
            return session;
        }

        public async Task<bool> Authorize(Guid token, bool isAdmin)
        {
            var session = await _repository.GetQuery<Session>().GetByToken(token).ConfigureAwait(false);
            return session != null && (isAdmin && session.IsAdmin || !isAdmin);
        }
    }
}