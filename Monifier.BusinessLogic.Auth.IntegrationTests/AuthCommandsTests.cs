using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.DataAccess.Model.Auth;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Auth.IntegrationTests
{
    public class AuthCommandsTests : QueryTestBase
    {
        [Fact]
        public async void CreateUser_NullUserName_ThrowsArgumentException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(
                    async () => await commands.CreateUser(null, "login", "pass", false));
            }
        }

        [Fact]
        public async void CreateUser_EmptyPassword_ThrowsArgumentException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(
                    async () => await commands.CreateUser("user", "login", string.Empty, false));
            }
        }

        [Fact]
        public async void CreateUser_EmptyLogin_ThrowsArgumentException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(
                    async () => await commands.CreateUser("user", string.Empty, "pass", false));
            }
        }

        [Fact]
        public async void CreateUser_CorrectNameAndPass_CorrectHash()
        {
            const string name = "user";
            const string login = "login";
            const string pass = "mywordismypassword";
            int userId;

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                userId = await commands.CreateUser(name, login, pass, false);
            }
            
            userId.Should().BePositive();

            using (var session = CreateUnauthorizedSession())
            {
                var queries = session.UnitOfWork.GetQueryRepository<User>();
                var user = await queries.GetById(userId);

                user.Should().NotBeNull();
                user.Name.ShouldBeEquivalentTo(name);
                user.Login.ShouldBeEquivalentTo(login);
                user.Salt.Should().NotBeNullOrEmpty();

                var hash = HashHelper.ComputeHash(pass, user.Salt);
                user.Hash.ShouldBeEquivalentTo(hash);
            }
        }

        [Fact]
        public async void CreateUser_DuplicateLogin_ThrowsAuthException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await commands.CreateUser("user", "login", "pass", false);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<AuthException>(
                    async () => await commands.CreateUser("new user", "login", "new pass", true));
            }
        }

        [Fact]
        public async void CreateUser_DuplicateNameDifferentLogin_Ok()
        {
            int userId;
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                userId = await commands.CreateUser("user", "login", "pass", false);
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                var newUserId = await commands.CreateUser("user", "new login", "new pass", true);
                userId.Should().NotBe(newUserId);
            }
        }
    }
}