using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
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
                var queries = session.CreateDistributionQueries();
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

                session.CreateAccountFlowSettings(ids.DebitCardAccountId, true);
                session.CreateExpenseFlowSettings(ids.FoodExpenseFlowId, true, FlowRule.FixedFromBase, false, 15000);
                await session.UnitOfWork.SaveChangesAsync();

                var queries = session.CreateDistributionQueries();
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

        [Fact]
        public async void Load_NoSettings_ReturnsDefault()
        {
            using (var session = await CreateSession(UserEvgeny))
            {
                session.CreateDefaultEntities();
            }

            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var queries = session.CreateDistributionQueries();
                var model = await queries.Load();
                model.ShouldBeEquivalentTo(new DistributionModel
                {
                    Accounts = new List<DistributionAccount>
                    {
                        new DistributionAccount
                        {
                            Account = session.DebitCardAccount.ToModel(),
                            UseInDistribution = true,
                            StartBalance = session.DebitCardAccount.AvailBalance,
                            Distributed = 0
                        },
                        new DistributionAccount
                        {
                            Account = session.CashAccount.ToModel(),
                            UseInDistribution = true,
                            StartBalance = session.CashAccount.AvailBalance,
                            Distributed = 0
                        }
                    },
                    Items = new List<DistributionItem>
                    {
                        new DistributionItem
                        {
                            Flow = session.FoodExpenseFlow.ToModel(),
                            Mode = DistributionMode.RegularExpenses,
                            StartBalance = session.FoodExpenseFlow.Balance,
                            Amount = 0,
                        },
                        new DistributionItem
                        {
                            Flow = session.TechExpenseFlow.ToModel(),
                            Mode = DistributionMode.RegularExpenses,
                            StartBalance = session.TechExpenseFlow.Balance,
                            Amount = 0,
                        }
                    },
                    FundDistributed = 0,
                });
                model.FlowFund.ShouldBeEquivalentTo(31000);
                model.TotalFund.ShouldBeEquivalentTo(76000);
            }
        }

        [Fact]
        public async void Load_CustomSettings_ReturnsAccordingToSettings()
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                session.CreateAccountFlowSettings(ids.CashAccountId, false);
                session.CreateExpenseFlowSettings(ids.TechExpenseFlowId, true, FlowRule.FixedFromBase, false, 12300);
                session.CreateExpenseFlowSettings(ids.FoodExpenseFlowId, true, FlowRule.FixedFromBase, true, 15000);
                await session.UnitOfWork.SaveChangesAsync();
                var queries = session.CreateDistributionQueries();
                var model = await queries.Load();
                model.ShouldBeEquivalentTo(new DistributionModel
                {
                    Accounts = new List<DistributionAccount>
                    {
                        new DistributionAccount
                        {
                            Account = session.CashAccount.ToModel(),
                            UseInDistribution = false,
                            StartBalance = session.CashAccount.AvailBalance,
                            Distributed = 0
                        },
                        new DistributionAccount
                        {
                            Account = session.DebitCardAccount.ToModel(),
                            UseInDistribution = true,
                            StartBalance = session.DebitCardAccount.AvailBalance,
                            Distributed = 0
                        }
                    },
                    Items = new List<DistributionItem>
                    {
                        new DistributionItem
                        {
                            Flow = session.FoodExpenseFlow.ToModel(),
                            Mode = DistributionMode.RegularExpenses,
                            StartBalance = session.FoodExpenseFlow.Balance,
                            Amount = 15000,
                        },
                        new DistributionItem
                        {
                            Flow = session.TechExpenseFlow.ToModel(),
                            Mode = DistributionMode.Accumulation,
                            StartBalance = session.TechExpenseFlow.Balance,
                            Amount = session.TechExpenseFlow.Balance,
                        }
                    },
                    FundDistributed = 0,
                });
                model.FlowFund.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance);
                model.TotalFund.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance +
                                                     session.DebitCardAccount.AvailBalance);

                model.Items.Single(x => x.Flow.Id == ids.TechExpenseFlowId).Amount =
                    session.TechExpenseFlow.Balance - 100;
                model.TotalFund.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance +
                                                     session.DebitCardAccount.AvailBalance + 100);
            }
        }
    }
}
