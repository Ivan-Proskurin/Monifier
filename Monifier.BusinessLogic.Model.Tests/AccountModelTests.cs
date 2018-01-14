using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Base;
using Xunit;

namespace Monifier.BusinessLogic.Model.Tests
{
    public class AccountModelTests
    {
        [Fact]
        public void Topup_WithNegativeAmount_ShouldRaiseException()
        {
            var account = new AccountModel();
            Assert.Throws<ArgumentOutOfRangeException>("amount", () => account.Topup(-10));
        }
        
        [Fact]
        public void Topup_BalanceEqualsAvail_BalancesAreModifiedCorrectly()
        {
            var account = new AccountModel
            {
                Balance = 100,
                AvailBalance = 100
            };
            
            account.Topup(112.45m);
            
            account.Balance.ShouldBeEquivalentTo(212.45m);
            account.AvailBalance.ShouldBeEquivalentTo(212.45m);
        }

        [Fact]
        public void Topup_NegativeBalanceZeroAvail_AvailNotModified()
        {
            var account = new AccountModel
            {
                Balance = -100,
                AvailBalance = 0
            };
            
            account.Topup(50);
            
            account.Balance.ShouldBeEquivalentTo(-50);
            account.AvailBalance.ShouldBeEquivalentTo(0);
        }

        [Fact]
        public void Topup_NegativeBalanceZeroAvail_PositiveBalanceAndAvail()
        {
            var account = new AccountModel
            {
                Balance = -100,
                AvailBalance = 0
            };
            
            account.Topup(200);
            
            account.Balance.ShouldBeEquivalentTo(100);
            account.AvailBalance.ShouldBeEquivalentTo(100);
        }

        [Fact]
        public void Withdraw_WithNegativeAmount_ShouldRaiseException()
        {
            var account = new AccountModel();
            Assert.Throws<ArgumentOutOfRangeException>("amount", () => account.Withdraw(-10));
        }

        [Fact]
        public void Withdraw_BalanceEqualsAvail_WithdrawBelowBalance()
        {
            var account = new AccountModel
            {
                Balance = 100,
                AvailBalance = 100
            };
            
            account.Withdraw(50);
            
            account.Balance.ShouldBeEquivalentTo(50);
            account.AvailBalance.ShouldBeEquivalentTo(50);
        }

        [Fact]
        public void Withdraw_AvailBelowBalance_WithdrawBelowBalance()
        {
            var account = new AccountModel
            {
                Balance = 100,
                AvailBalance = 25
            };
            
            account.Withdraw(20);
            
            account.Balance.ShouldBeEquivalentTo(80);
            account.AvailBalance.ShouldBeEquivalentTo(25);
        }

        [Fact]
        public void Withdraw_AvailBelowBalance_WithdrawAboveBalance()
        {
            var account = new AccountModel
            {
                Balance = 100,
                AvailBalance = 25
            };
            
            account.Withdraw(120);
            
            account.Balance.ShouldBeEquivalentTo(-20);
            account.AvailBalance.ShouldBeEquivalentTo(0);
        }

        [Fact]
        public void Withdraw_AvailBelowBalance_WithdrawBelowBalanceBelowAvail()
        {
            var account = new AccountModel
            {
                Balance = 100,
                AvailBalance = 25
            };
            
            account.Withdraw(80);
            
            account.Balance.ShouldBeEquivalentTo(20);
            account.AvailBalance.ShouldBeEquivalentTo(20);
        }

        [Fact]
        public void Withdraw_PositiveBalanceZeroAvail_AvailNotChanged()
        {
            var account = new AccountModel
            {
                Balance = 50,
                AvailBalance = 0
            };
            
            account.Withdraw(45);
            
            account.Balance.ShouldBeEquivalentTo(5);
            account.AvailBalance.ShouldBeEquivalentTo(0);
        }
    }
}