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

        [Fact]
        public async void UpdateUser_OnlyNewPassProvided_PassAndSaltUpdatedNameNotChanged()
        {
            const string name = "user";
            const string login = "login";
            const string pass = "mywordismypassword";
            const string newpass = "newpassword";
            int userId;
            string salt;

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                userId = await commands.CreateUser(name, login, pass, false);
                var user = await session.LoadEntity<User>(userId);
                salt = user.Salt;
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await commands.UpdateUser(userId, null, newpass);
                var user = await session.LoadEntity<User>(userId);
                user.Name.ShouldBeEquivalentTo(name);
                user.Salt.Should().NotBe(salt);
                var hash = HashHelper.ComputeHash(newpass, user.Salt);
                user.Hash.ShouldBeEquivalentTo(hash);
            }
        }

        [Fact]
        public async void UpdateUser_OnlyNameProvided_NameUpdatedSaltAndPassNotChanged()
        {
            const string name = "user";
            const string newName = "vovka";
            const string login = "login";
            const string pass = "mywordismypassword";
            int userId;
            string salt, hash;

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                userId = await commands.CreateUser(name, login, pass, false);
                var user = await session.LoadEntity<User>(userId);
                salt = user.Salt;
                hash = user.Hash;
            }

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await commands.UpdateUser(userId, newName, null);
                var user = await session.LoadEntity<User>(userId);
                user.Name.ShouldBeEquivalentTo(newName);
                user.Hash.ShouldBeEquivalentTo(hash);
                user.Salt.ShouldBeEquivalentTo(salt);
            }
        }

        [Fact]
        public async void UpdateUser_UserIsNotExist_ThrowsException()
        {
            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(async () => await commands.UpdateUser(3, "new name", null));
            }
        }

        [Fact]
        public async void UpdateUser_UserAndPassAreNull_ThrowsException()
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

            using (var session = CreateUnauthorizedSession())
            {
                var commands = new AuthCommands(session.UnitOfWork);
                await Assert.ThrowsAsync<ArgumentException>(async () => await commands.UpdateUser(userId, null, null));
            }
        }
    }
}