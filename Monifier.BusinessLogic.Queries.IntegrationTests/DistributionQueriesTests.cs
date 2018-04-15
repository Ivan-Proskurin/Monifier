using FluentAssertions;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Queries.Distribution;
using Monifier.DataAccess.Model.Distribution;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class DistributionQueriesTests : QueryTestBase
    {
        [Fact]
        public async void GetDistributionBoard_DefaultSettings_Test()
        {
            using (var session = await CreateSession(UserEvgeny))
            {
                session.CreateDefaultEntities();
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var queries = new DistributionQueries(session.UnitOfWork, session.UserSession);
                var board = await queries.GetDistributionBoard();
                board.Accounts.ShouldBeEquivalentTo(new[] 
                {
                    new DistributionSource
                    {
                        Id = ids.DebitCardAccountId,
                        CanFlow = false,
                        Name = session.DebitCardAccount.Name,
                        Balance = session.DebitCardAccount.Balance
                    },
                    new DistributionSource
                    {
                        Id = ids.CashAccountId,
                        CanFlow = false,
                        Name = session.CashAccount.Name,
                        Balance = session.CashAccount.Balance
                    },
                });
                board.ExpenseFlows.ShouldBeEquivalentTo(new[] 
                {
                    new DistributionRecipient
                    {
                        Id = ids.FoodExpenseFlowId,
                        Name = session.FoodExpenseFlow.Name,
                        Balance = session.FoodExpenseFlow.Balance,
                        CanFlow = false,
                        Rule = FlowRule.None
                    },
                    new DistributionRecipient
                    {
                        Id = ids.TechExpenseFlowId,
                        Name = session.TechExpenseFlow.Name,
                        Balance = session.TechExpenseFlow.Balance,
                        CanFlow = false,
                        Rule = FlowRule.None
                    },
                });
                board.DistributionFlows.Should().BeNull();
                board.BaseAmount.ShouldBeEquivalentTo(session.DebitCardAccount.Balance + session.CashAccount.Balance);
            }
        }

        [Fact]
        public async void GetDistributionBoard_CustomSettings_Test()
        {
            using (var session = await CreateSession(UserEvgeny))
            {
                session.CreateDefaultEntities();
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();

                session.CreateAccountFlowSettinsg(ids.DebitCardAccountId, true);
                session.CreateExpenseFlowSettings(ids.FoodExpenseFlowId, true, FlowRule.FixedFromBase, 15000);
                await session.UnitOfWork.SaveChangesAsync();

                var queries = new DistributionQueries(session.UnitOfWork, session.UserSession);
                var board = await queries.GetDistributionBoard();
                board.Accounts.ShouldBeEquivalentTo(new[]
                {
                    new DistributionSource
                    {
                        Id = ids.DebitCardAccountId,
                        CanFlow = true,
                        Name = session.DebitCardAccount.Name,
                        Balance = session.DebitCardAccount.Balance
                    },
                    new DistributionSource
                    {
                        Id = ids.CashAccountId,
                        CanFlow = false,
                        Name = session.CashAccount.Name,
                        Balance = session.CashAccount.Balance
                    },
                });
                board.ExpenseFlows.ShouldBeEquivalentTo(new[]
                {
                    new DistributionRecipient
                    {
                        Id = ids.FoodExpenseFlowId,
                        Name = session.FoodExpenseFlow.Name,
                        Balance = session.FoodExpenseFlow.Balance,
                        CanFlow = true,
                        Rule = FlowRule.FixedFromBase,
                        Amount = 15000
                    },
                    new DistributionRecipient
                    {
                        Id = ids.TechExpenseFlowId,
                        Name = session.TechExpenseFlow.Name,
                        Balance = session.TechExpenseFlow.Balance,
                        CanFlow = false,
                        Rule = FlowRule.None
                    },
                });
                board.DistributionFlows.Should().BeNull();
                board.BaseAmount.ShouldBeEquivalentTo(session.DebitCardAccount.Balance + session.CashAccount.Balance);
            }
        }
    }
}
