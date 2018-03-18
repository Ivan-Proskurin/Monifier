using System;
using System.Threading.Tasks;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.DataAccess.Model.Auth;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Auth.IntegrationTests
{
    public class SessionCommandsTests : QueryTestBase
    {
        private const string Login = "exosyphen";
        private const string Pass = "mywordismypassword";
        
        private async Task<int> CreateUser(UserQuerySession session, string login, string password, bool isAdmin = false)
        {
            var commands = new AuthCommands(session.UnitOfWork);
            return await commands.CreateUser(login, login, password, isAdmin);
        }

        private async Task<int> CreateUser(UserQuerySession session)
        {
            return await CreateUser(session, Login, Pass);
        }

        private async Task<Session> CreateSession(UserQuerySession session, bool isAdmin)
        {
            await CreateUser(session, Login, Pass, isAdmin);
            var sessionCommands = new SessionCommands(session.UnitOfWork);
            return await sessionCommands.CreateSession(Login, Pass);
        }
        
        private async Task<Session> CreateSession(UserQuerySession session, string login, bool isAdmin)
        {
            await CreateUser(session, login, login, isAdmin);
            var sessionCommands = new SessionCommands(session.UnitOfWork);
            return await sessionCommands.CreateSession(login, login);
        }

        [Fact]
        public async void CreateSession_EmptyLogin_ThrowsArgumentException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(async () => await commands.CreateSession(string.Empty, "pass"));
            }
        }

        [Fact]
        public async void CreateSession_EmptyPass_ThrowsArgumentExcpetion()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(async () => await commands.CreateSession("login", string.Empty));
            }
        }

        [Fact]
        public async void CreateSession_CorrectLoginPass_Ok()
        {
            int userId;
            using (var session = CreateUnauthorizedSession())
            {
                userId = await CreateUser(session);
            }

            using (var s = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(s.UnitOfWork);
                var session = await commands.CreateSession(Login, Pass);
                session.Should().NotBeNull();
                session.Token.Should().NotBeEmpty();
                session.UserId.ShouldBeEquivalentTo(userId);
                session.User.Should().NotBeNull();
                session.User.Login.ShouldBeEquivalentTo(Login);
            }
        }

        [Fact]
        public async void CreateSession_WrongLoginPass_ThrowsAuthException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                await CreateUser(session);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<AuthException>(async () => await commands.CreateSession("user", "pass"));
            }
        }

        [Fact]
        public async void CreateSession_UnknownUser_ThrowsAuthException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                await CreateUser(session);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<AuthException>(async () => await commands.CreateSession("exosyphen", "pass"));
            }
        }

        [Fact]
        public async void Authorize_UnknownToken_ReturnsFalse()
        {
            using (var session = CreateUnauthorizedSession())
            {
                await CreateUser(session);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                var result = await commands.Authorize(Guid.NewGuid(), false);
                result.Should().BeFalse();
            }
        }

        [Fact]
        public async void Authorize_InvalidRole_ReturnsFalse()
        {
            Session s;
            using (var scope = CreateUnauthorizedSession())
            {
                s = await CreateSession(scope, false);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                var result = await commands.Authorize(s.Token, true);
                result.Should().BeFalse();
            }
        }
        
        [Fact]
        public async void Authorize_CorrectRole_ReturnsTrue()
        {
            Session session1;
            using (var session = CreateUnauthorizedSession())
            {
                session1 = await CreateSession(session, "user1", false);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                var result = await commands.Authorize(session1.Token, false);
                result.Should().BeTrue();
            }

            Session session2;
            using (var session = CreateUnauthorizedSession())
            {
                session2 = await CreateSession(session, "user2", true);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new SessionCommands(session.UnitOfWork);
                var result = await commands.Authorize(session2.Token, true);
                result.Should().BeTrue();
                result = await commands.Authorize(session2.Token, false);
                result.Should().BeTrue();
            }
        }

    }
}