using FluentAssertions;
using Monifier.BusinessLogic.Model.Base;
using System;
using System.Linq;
using Xunit;

namespace Monifier.BusinessLogic.Model.Tests
{
    public class AccountModelTests
    {
        [Fact]
        public void GetDefaultAccount_EmptyList_ReturnsNull()
        {
            new AccountModel[0].GetDefaultAccount().Should().BeNull();
        }

        [Fact]
        public void GetDefaultAccount_SingleAccountIsNotDefault_ReturnsThisItem()
        {
            var account = new AccountModel
            {
                Id = 1,
                LastWithdraw = DateTime.Now,
                IsDefault = false
            };
            new[] { account }.GetDefaultAccount().ShouldBeEquivalentTo(account);
        }

        [Fact]
        public void GetDefaultAccount_SingleAccountIsDefault_ReturnsThisItem()
        {
            var account = new AccountModel
            {
                Id = 1,
                LastWithdraw = DateTime.Now,
                IsDefault = true
            };
            new[] { account }.GetDefaultAccount().ShouldBeEquivalentTo(account);
        }

        [Fact]
        public void GetDefaultAccount_TwoAccountsOnIsDefault_ReturnsThatIsDefault()
        {
            var accounts = new[]
            {
                new AccountModel
                {
                    Id = 1,
                    LastWithdraw = DateTime.Now,
                    IsDefault = true
                },
                new AccountModel
                {
                    Id = 2,
                    LastWithdraw = DateTime.Now,
                    IsDefault = false
                },
            };
            var defaultAccount = accounts.Single(x => x.IsDefault);
            accounts.GetDefaultAccount().ShouldBeEquivalentTo(defaultAccount);
        }

        [Fact]
        public void GetDefaultAccount_TwoAccountsBothAreDefault_ReturnsThatHasMaxLastWithdraw()
        {
            var accounts = new[]
            {
                new AccountModel
                {
                    Id = 1,
                    LastWithdraw = DateTime.Now,
                    IsDefault = true
                },
                new AccountModel
                {
                    Id = 2,
                    LastWithdraw = DateTime.Now,
                    IsDefault = true
                },
            };
            var defaultAccount = accounts.OrderByDescending(x => x.LastWithdraw).First();
            accounts.GetDefaultAccount().ShouldBeEquivalentTo(defaultAccount);
        }

        [Fact]
        public void GetDefaultAccount_TwoAccountsBothAreNotDefault_ReturnsThatHasMaxLastWithdraw()
        {
            var accounts = new[]
            {
                new AccountModel
                {
                    Id = 1,
                    LastWithdraw = DateTime.Now,
                    IsDefault = false
                },
                new AccountModel
                {
                    Id = 2,
                    LastWithdraw = DateTime.Now,
                    IsDefault = false
                },
            };
            var defaultAccount = accounts.OrderByDescending(x => x.LastWithdraw).First();
            accounts.GetDefaultAccount().ShouldBeEquivalentTo(defaultAccount);
        }

        [Fact]
        public void GetDefaultAccount_DifferentAccounts_ReturnsDefault()
        {
            var today = DateTime.Today;
            var date1 = today.AddDays(-5);
            var date2 = today.AddDays(-4);
            var date3 = today.AddDays(-3);
            var date4 = today.AddDays(-2);
            var date5 = today.AddDays(-1);

            var expected = new AccountModel
            {
                Id = 4,
                LastWithdraw = date2,
                IsDefault = true
            };

            var accounts = new[]
{
                new AccountModel
                {
                    Id = 1,
                    LastWithdraw = date5,
                    IsDefault = false
                },
                new AccountModel
                {
                    Id = 2,
                    LastWithdraw = date4,
                    IsDefault = false
                },
                new AccountModel
                {
                    Id = 3,
                    LastWithdraw = date3,
                    IsDefault = false
                },
                expected,
                new AccountModel
                {
                    Id = 5,
                    LastWithdraw = date1,
                    IsDefault = true
                },
            };
            accounts.GetDefaultAccount().ShouldBeEquivalentTo(expected);
        }
    }
}
