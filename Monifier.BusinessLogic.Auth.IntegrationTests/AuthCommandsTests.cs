using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Auth;
using Monifier.DataAccess.Model.Auth;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Auth.IntegrationTests
{
    public class AuthCommandsTests : DatabaseRelatedBlankTest
    {
        [Fact]
        public async void CreateUser_NullUserName_ThrowsArgumentException()
        {
            var commands = new AuthCommands(UnitOfWork);
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await commands.CreateUser(null, "login", "pass", false));
        }

        [Fact]
        public async void CreateUser_EmptyPassword_ThrowsArgumentException()
        {
            var commands = new AuthCommands(UnitOfWork);
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await commands.CreateUser("user", "login", string.Empty, false));
        }

        [Fact]
        public async void CreateUser_EmptyLogin_ThrowsArgumentException()
        {
            var commands = new AuthCommands(UnitOfWork);
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await commands.CreateUser("user", string.Empty, "pass", false));
        }

        [Fact]
        public async void CreateUser_CorrectNameAndPass_CorrectHash()
        {
            var name = "user";
            var login = "login";
            var pass = "mywordismypassword";
            var userId = await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new AuthCommands(uof);
                return await commands.CreateUser(name, login, pass, false);
            });
            
            userId.Should().BePositive();

            await UseUnitOfWorkAsync(async uof =>
            {
                var queries = uof.GetQueryRepository<User>();
                var user = await queries.GetById(userId);
                
                user.Should().NotBeNull();
                user.Name.ShouldBeEquivalentTo(name);
                user.Login.ShouldBeEquivalentTo(login);
                user.Salt.Should().NotBeNullOrEmpty();

                var hash = HashHelper.ComputeHash(pass, user.Salt);
                user.Hash.ShouldBeEquivalentTo(hash);
            });
        }

        [Fact]
        public async void CreateUser_DuplicateLogin_ThrowsAuthException()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new AuthCommands(uof);
                await commands.CreateUser("user", "login", "pass", false);
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new AuthCommands(uof);
                await Assert.ThrowsAsync<AuthException>(
                    async () => await commands.CreateUser("new user", "login", "new pass", true));
            });
        }

        [Fact]
        public async void CreateUser_DuplicateNameDifferentLogin_Ok()
        {
            var userId = await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new AuthCommands(uof);
                return await commands.CreateUser("user", "login", "pass", false);
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                var commands = new AuthCommands(uof);
                var newUserId = await commands.CreateUser("user", "new login", "new pass", true);
                userId.Should().NotBe(newUserId);
            });
        }
    }
}