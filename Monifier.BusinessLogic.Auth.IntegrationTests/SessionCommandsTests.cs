using System;
using System.Threading.Tasks;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Auth;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Auth.IntegrationTests
{
    public class SessionCommandsTests : DatabaseRelatedBlankTest
    {
        private const string Login = "exosyphen";
        private const string Pass = "mywordismypassword";
        
        private async Task<int> CreateUser(IUnitOfWork uof, string login, string password, bool isAdmin = false)
        {
            var commands = new AuthCommands(uof);
            return await commands.CreateUser(login, login, password, isAdmin);
        }

        private async Task<int> CreateUser(IUnitOfWork uof)
        {
            return await CreateUser(uof, Login, Pass);
        }

        private async Task<Session> CreateSession(IUnitOfWork uof, bool isAdmin)
        {
            await CreateUser(uof, Login, Pass, isAdmin);
            var sessionCommands = new SessionCommands(uof);
            return await sessionCommands.CreateSession(Login, Pass);
        }
        
        private async Task<Session> CreateSession(IUnitOfWork uof, string login, bool isAdmin)
        {
            await CreateUser(uof, login, login, isAdmin);
            var sessionCommands = new SessionCommands(uof);
            return await sessionCommands.CreateSession(login, login);
        }

        [Fact]
        public async void CreateSession_EmptyLogin_ThrowsArgumentException()
        {
            var commands = new SessionCommands(UnitOfWork);
            await Assert.ThrowsAsync<ArgumentException>(async () => await commands.CreateSession(string.Empty, "pass"));
        }

        [Fact]
        public async void CreateSession_EmptyPass_ThrowsArgumentExcpetion()
        {
            var commands = new SessionCommands(UnitOfWork);
            await Assert.ThrowsAsync<ArgumentException>(async () => await commands.CreateSession("login", string.Empty));
        }

        [Fact]
        public async void CreateSession_CorrectLoginPass_Ok()
        {
            var userId = await UseUnitOfWorkAsync(async uof => await CreateUser(uof));
            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                var session = await commands.CreateSession(Login, Pass);
                session.Should().NotBeNull();
                session.Token.Should().NotBeEmpty();
                session.UserId.ShouldBeEquivalentTo(userId);
                session.User.Should().NotBeNull();
                session.User.Login.ShouldBeEquivalentTo(Login);
            });
        }

        [Fact]
        public async void CreateSession_WrongLoginPass_ThrowsAuthException()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                await CreateUser(uof);
            });
            
            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                await Assert.ThrowsAsync<AuthException>(async () => await commands.CreateSession("user", "pass"));
            });
        }

        [Fact]
        public async void CreateSession_UnknownUser_ThrowsAuthException()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                await CreateUser(uof);
            });
            
            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                await Assert.ThrowsAsync<AuthException>(async () => await commands.CreateSession("exosyphen", "pass"));
            });
        }

        [Fact]
        public async void Authorize_UnknownToken_ReturnsFalse()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                await CreateSession(uof, false);
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                var result = await commands.Authorize(Guid.NewGuid(), false);
                result.Should().BeFalse();
            });
        }

        [Fact]
        public async void Authorize_InvalidRole_ReturnsFalse()
        {
            var session = await UseUnitOfWorkAsync(async uof => await CreateSession(uof, false));

            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                var result = await commands.Authorize(session.Token, true);
                result.Should().BeFalse();
            });
        }
        
        [Fact]
        public async void Authorize_CorrectRole_ReturnsTrue()
        {
            var session1 = await UseUnitOfWorkAsync(async uof => await CreateSession(uof, "user1", false));

            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                var result = await commands.Authorize(session1.Token, false);
                result.Should().BeTrue();
            });
            
            var session2 = await UseUnitOfWorkAsync(async uof => await CreateSession(uof, "user2", true));

            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new SessionCommands(uof);
                var result = await commands.Authorize(session2.Token, true);
                result.Should().BeTrue();
                result = await commands.Authorize(session2.Token, false);
                result.Should().BeTrue();
            });
        }

    }
}