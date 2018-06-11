using System.Collections.Generic;
using FluentAssertions;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Model.Base;
using Xunit;

namespace Monifier.BusinessLogic.Model.Tests
{
    public class DistributionModelTests
    {
        public DistributionModelTests()
        {
            DebitCardAccount = new AccountModel
            {
                Id = 1,
                AccountType = AccountType.DebitCard,
                AvailBalance = 15000
            };
            CashAccount = new AccountModel
            {
                Id = 2,
                AccountType = AccountType.Cash,
                AvailBalance = 1200
            };
            FoodFlow = new ExpenseFlowModel
            {
                Id = 1,
                Balance = 100,
            };
            TechFlow = new ExpenseFlowModel
            {
                Id = 2,
                Balance = 0
            };

            Debit = new DistributionAccount
            {
                Account = DebitCardAccount,
                UseInDistribution = true
            };
            Cash = new DistributionAccount
            {
                Account = CashAccount,
                UseInDistribution = true
            };

            Food = new DistributionItem
            {
                Flow = FoodFlow,
                Mode = DistributionMode.RegularExpenses,
                Amount = 10000
            };
            Tech = new DistributionItem
            {
                Flow = TechFlow,
                Mode = DistributionMode.Accumulation,
                Amount = 1000
            };

            Model = new DistributionModel
            {
                Accounts = new List<DistributionAccount> { Debit, Cash },
                Items = new List<DistributionItem> { Food, Tech }
            };
        }

        [Fact]
        public void DefaultCase_Test()
        {
            Model.FlowFund.ShouldBeEquivalentTo(100);
            Model.TotalFund.ShouldBeEquivalentTo(16300);
            Model.Total.ShouldBeEquivalentTo(11000);
            Model.CanDistribute.Should().BeTrue();
        }

        [Fact]
        public void NegativeAccountBalance_Test()
        {
            CashAccount.AvailBalance = -1000;
            Model.FlowFund.ShouldBeEquivalentTo(100);
            Model.TotalFund.ShouldBeEquivalentTo(15100);
            Model.Total.ShouldBeEquivalentTo(11000);
            Model.CanDistribute.Should().BeTrue();
        }

        [Fact]
        public void NegativeFlowBalance_Test()
        {
            FoodFlow.Balance = -100;
            Model.FlowFund.ShouldBeEquivalentTo(0);
            Model.TotalFund.ShouldBeEquivalentTo(16200);
            Model.Total.ShouldBeEquivalentTo(11100);
            Model.CanDistribute.Should().BeTrue();
        }

        [Fact]
        public void CompensateWithAccumulativeFlow_Test()
        {
            TechFlow.Balance = 10000;
            Tech.Amount = 8000;
            Model.FlowFund.ShouldBeEquivalentTo(2100);
            Model.TotalFund.ShouldBeEquivalentTo(18300);
            Model.Total.ShouldBeEquivalentTo(10000);
            Model.CanDistribute.Should().BeTrue();
        }

        [Fact]
        public void TotalAboveFund_CantDistribute_Test()
        {
            Tech.Amount = 15000;
            Model.FlowFund.ShouldBeEquivalentTo(100);
            Model.TotalFund.ShouldBeEquivalentTo(16300);
            Model.Total.ShouldBeEquivalentTo(25000);
            Model.CanDistribute.Should().BeFalse();
        }

        [Fact]
        public void ExludeAccount_Test()
        {
            Cash.UseInDistribution = false;
            Model.FlowFund.ShouldBeEquivalentTo(100);
            Model.TotalFund.ShouldBeEquivalentTo(15100);
            Model.Total.ShouldBeEquivalentTo(11000);
            Model.CanDistribute.Should().BeTrue();
        }

        [Fact]
        public void NonZeroAccumulativeFlow_Test()
        {
            TechFlow.Balance = 1000;
            Tech.Amount = 3000;
            Model.FlowFund.ShouldBeEquivalentTo(100);
            Model.TotalFund.ShouldBeEquivalentTo(16300);
            Model.Total.ShouldBeEquivalentTo(12000);
            Model.CanDistribute.Should().BeTrue();
        }

        [Fact]
        public void NegativeAccumulatuveFlowBalance_Test()
        {
            TechFlow.Balance = -1500;
            Model.FlowFund.ShouldBeEquivalentTo(100);
            Model.TotalFund.ShouldBeEquivalentTo(16300);
            Model.Total.ShouldBeEquivalentTo(12500);
            Model.CanDistribute.Should().BeTrue();
        }

        public AccountModel DebitCardAccount { get; }
        public AccountModel CashAccount { get; }
        public ExpenseFlowModel FoodFlow { get; }
        public ExpenseFlowModel TechFlow { get; }

        public DistributionAccount Debit { get; }
        public DistributionAccount Cash { get; }
        public DistributionItem Food { get; }
        public DistributionItem Tech { get; }

        public DistributionModel Model { get; }
    }
}