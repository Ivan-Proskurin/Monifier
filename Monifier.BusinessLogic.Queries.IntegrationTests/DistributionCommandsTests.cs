using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Model.Distribution;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class DistributionCommandsTests : QueryTestBase
    {
        [Fact]
        public async void Distribute_TotalAboveAvailFund_ThrowsException()
        {
            var ids = await PrepareTestEntities(0, 0);

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                var model = await queries.Load();
                var foodDesiredBalance = session.DebitCardAccount.AvailBalance + 1000;
                var techDesiredBalance = session.CashAccount.AvailBalance + 5000;
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;
                var commands = session.CreateDistributionCommands();
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await commands.Distribute(model));
            }
        }

        [Fact]
        public async void Distribute_DefaultSettings_WithoutFlowFund_Test()
        {
            var ids = await PrepareTestEntities(0, 0);

            decimal foodDesiredBalance, cashAvailBalance;
            const decimal techDesiredBalance = 10000, cashExpectedDistribution = techDesiredBalance + 1000;
            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                foodDesiredBalance = session.DebitCardAccount.AvailBalance + 1000;
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId)
                    .Distributed.ShouldBeEquivalentTo(cashExpectedDistribution);
                model.FundDistributed.ShouldBeEquivalentTo(0);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(0);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance - cashExpectedDistribution);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeTrue();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeTrue();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeTrue();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }
        }

        [Fact]
        public async void Distribute_DefaultSettings_WithFlowFund_Test()
        {
            var ids = await PrepareTestEntities(500, 1000);

            decimal foodDesiredBalance, cashAvailBalance;
            const decimal techDesiredBalance = 10000, cashExpectedDistribution = techDesiredBalance - 500;
            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                foodDesiredBalance = session.DebitCardAccount.AvailBalance + 1000;
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId)
                    .Distributed.ShouldBeEquivalentTo(cashExpectedDistribution);
                model.FundDistributed.ShouldBeEquivalentTo(1500);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(0);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance - cashExpectedDistribution);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeTrue();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeTrue();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeTrue();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }
        }

        [Fact]
        public async void Distribute_DefaultSettings_NegativeFlowBalance_Test()
        {
            var ids = await PrepareTestEntities(-500, 1000);

            decimal foodDesiredBalance, cashAvailBalance;
            const decimal techDesiredBalance = 10000, cashExpectedDistribution = techDesiredBalance + 500;
            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                foodDesiredBalance = session.DebitCardAccount.AvailBalance + 1000;
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId)
                    .Distributed.ShouldBeEquivalentTo(cashExpectedDistribution);
                model.FundDistributed.ShouldBeEquivalentTo(1000);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(0);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance - cashExpectedDistribution);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeTrue();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeTrue();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeTrue();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }
        }

        [Fact]
        public async void Distribute_FlowFundRemains_DefaultAccountIsIncreased()
        {
            var ids = await PrepareTestEntities(500, 1000);

            decimal debitAvailBalance, cashAvailBalance;
            const decimal foodDesiredBalance = 1000;
            const decimal techDesiredBalance = 200;
            const decimal debitExpectedDistribution = -300;
            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                debitAvailBalance = session.DebitCardAccount.AvailBalance;
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(debitExpectedDistribution);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId)
                    .Distributed.ShouldBeEquivalentTo(0);
                model.FundDistributed.ShouldBeEquivalentTo(1500);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(debitAvailBalance - debitExpectedDistribution);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeTrue();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeTrue();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeTrue();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }
        }

        [Fact]
        public async void Distribute_CustomSettings_WithFlowFund_Test()
        {
            const decimal foodDesiredBalance = 1000;
            const decimal techDesiredBalance = 2000;
            const decimal cashExpectedDistribution = 1500;
            decimal debitAvailBalance, cashAvailBalance;

            var ids = await PrepareTestEntities(500, 1000);

            using (var session = await CreateDefaultSession(ids))
            {
                session.CreateAccountFlowSettings(ids.DebitCardAccountId, false);
                session.CreateAccountFlowSettings(ids.CashAccountId, true);
                session.CreateExpenseFlowSettings(ids.FoodExpenseFlowId, true, FlowRule.None, true, foodDesiredBalance);
                session.CreateExpenseFlowSettings(ids.TechExpenseFlowId, true, FlowRule.None, false, techDesiredBalance);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                debitAvailBalance = session.DebitCardAccount.AvailBalance;
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(0);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId)
                    .Distributed.ShouldBeEquivalentTo(cashExpectedDistribution);
                model.FundDistributed.ShouldBeEquivalentTo(500);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(debitAvailBalance);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance - cashExpectedDistribution);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeFalse();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeTrue();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeFalse();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }
        }

        [Fact]
        public async void Distribute_CustomSettings_AccumulativeFund_Test()
        {
            const decimal foodDesiredBalance = 1000;
            const decimal techDesiredBalance = 800;
            const decimal cashExpectedDistribution = 300;
            decimal debitAvailBalance, cashAvailBalance;

            var ids = await PrepareTestEntities(500, 1000);

            using (var session = await CreateDefaultSession(ids))
            {
                session.CreateAccountFlowSettings(ids.DebitCardAccountId, false);
                session.CreateAccountFlowSettings(ids.CashAccountId, true);
                session.CreateExpenseFlowSettings(ids.FoodExpenseFlowId, true, FlowRule.None, true, foodDesiredBalance);
                session.CreateExpenseFlowSettings(ids.TechExpenseFlowId, true, FlowRule.None, false, techDesiredBalance);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                debitAvailBalance = session.DebitCardAccount.AvailBalance;
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(0);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId)
                    .Distributed.ShouldBeEquivalentTo(cashExpectedDistribution);
                model.FundDistributed.ShouldBeEquivalentTo(700);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(debitAvailBalance);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance - cashExpectedDistribution);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeFalse();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeTrue();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeFalse();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }

        }

        [Fact]
        public async void Distribute_RedistributeFlowBalance_Test()
        {
            const decimal foodDesiredBalance = 2300;
            const decimal techDesiredBalance = 2000;
            const decimal debitExpectedDistribution = -200;
            decimal debitAvailBalance, cashAvailBalance;

            var ids = await PrepareTestEntities(1500, 3000);

            using (var session = await CreateDefaultSession(ids))
            {
                session.CreateAccountFlowSettings(ids.DebitCardAccountId, false);
                session.CreateAccountFlowSettings(ids.CashAccountId, false);
                session.CreateExpenseFlowSettings(ids.FoodExpenseFlowId, true, FlowRule.None, true, foodDesiredBalance);
                session.CreateExpenseFlowSettings(ids.TechExpenseFlowId, true, FlowRule.None, true, techDesiredBalance);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();
                var queries = session.CreateDistributionQueries();
                debitAvailBalance = session.DebitCardAccount.AvailBalance;
                cashAvailBalance = session.CashAccount.AvailBalance;
                var model = await queries.Load();
                model.Items.Single(x => x.Flow.Id == ids.FoodExpenseFlowId).Amount = foodDesiredBalance;
                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount = techDesiredBalance;

                var commands = session.CreateDistributionCommands();
                await commands.Distribute(model);

                model.Accounts.Single(x => x.Account.Id == ids.DebitCardAccountId)
                    .Distributed.ShouldBeEquivalentTo(debitExpectedDistribution);
                model.Accounts.Single(x => x.Account.Id == ids.CashAccountId).Distributed.ShouldBeEquivalentTo(0);
                model.FundDistributed.ShouldBeEquivalentTo(4500);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await session.CheckBalanceState();

                session.DebitCardAccount.AvailBalance.ShouldBeEquivalentTo(debitAvailBalance - debitExpectedDistribution);
                session.CashAccount.AvailBalance.ShouldBeEquivalentTo(cashAvailBalance);
                session.FoodExpenseFlow.Balance.ShouldBeEquivalentTo(foodDesiredBalance);
                session.TechExpenseFlow.Balance.ShouldBeEquivalentTo(techDesiredBalance);

                var debitSettings = await session.GetAccountSettings(ids.DebitCardAccountId);
                debitSettings.Should().NotBeNull();
                debitSettings.CanFlow.Should().BeFalse();
                var cashSettings = await session.GetAccountSettings(ids.CashAccountId);
                cashSettings.Should().NotBeNull();
                cashSettings.CanFlow.Should().BeFalse();

                var foodSettings = await session.GetFlowSettings(ids.FoodExpenseFlowId);
                foodSettings.Should().NotBeNull();
                foodSettings.IsRegularExpenses.Should().BeTrue();
                foodSettings.Amount.ShouldBeEquivalentTo(foodDesiredBalance);

                var techSettings = await session.GetFlowSettings(ids.TechExpenseFlowId);
                techSettings.Should().NotBeNull();
                techSettings.IsRegularExpenses.Should().BeTrue();
                techSettings.Amount.ShouldBeEquivalentTo(techDesiredBalance);
            }
        }

        private async Task<EntityIdSet> PrepareTestEntities(decimal foodBalance, decimal techBalance)
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                session.FoodExpenseFlow.Balance = foodBalance;
                session.TechExpenseFlow.Balance = techBalance;
                session.DebitCardAccount.Balance += foodBalance + techBalance;
                await session.UpdateEntity(session.FoodExpenseFlow);
                await session.UpdateEntity(session.TechExpenseFlow);
                await session.UpdateEntity(session.DebitCardAccount);
                return ids;
            }
        }
    }

    public static class UserQueryExtensions
    {
        public static Task<AccountFlowSettings> GetAccountSettings(this UserQuerySession session, int accountId)
        {
            return session.Repository.GetQuery<AccountFlowSettings>()
                .Where(x => x.AccountId == accountId)
                .SingleOrDefaultAsync();
        }

        public static Task<ExpenseFlowSettings> GetFlowSettings(this UserQuerySession session, int flowId)
        {
            return session.Repository.GetQuery<ExpenseFlowSettings>()
                .Where(x => x.ExpenseFlowId == flowId)
                .SingleOrDefaultAsync();
        }

        public static async Task CheckBalanceState(this UserQuerySession session)
        {
            var inventorization = session.CreateInventorizationQueries();
            var state = await inventorization.GetBalanceState();
            state.Balance.ShouldBeEquivalentTo(0);
        }
    }
}