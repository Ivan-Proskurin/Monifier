using System;
using System.Threading.Tasks;
using Monifier.DataAccess.Model.Auth;

namespace Monifier.IntegrationTests.Infrastructure
{
    public class QueryTestBase
    {
        private readonly string _databaseName;

        public QueryTestBase()
        {
            _databaseName = Guid.NewGuid().ToString();
            using (var session = new UserQuerySession(_databaseName, null))
            {
                UserSveta = session.CreateUser("Света", "svetika", "pass", false);
                UserEvgeny = session.CreateUser("Евгений", "evgen", "password", true);
            }

            DefaultUser = UserSveta;
        }

        public UserQuerySession CreateUnauthorizedSession()
        {
            return new UserQuerySession(_databaseName, null);
        }

        protected async Task<UserQuerySession> CreateSession(User user, EntityIdSet idSet = null)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var session = new UserQuerySession(_databaseName, user);
            if (idSet == null) return session;
            await session.LoadDefaultEntities(idSet);
            return session;
        }

        protected async Task<UserQuerySession> CreateDefaultSession(EntityIdSet idSet = null)
        {
            return await CreateSession(DefaultUser, idSet);
        }

        public User UserSveta { get; }
        public User UserEvgeny { get; }

        public User DefaultUser { get; set; }
    }
}