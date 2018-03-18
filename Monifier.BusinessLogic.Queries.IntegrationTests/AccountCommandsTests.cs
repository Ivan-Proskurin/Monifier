using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class AccountCommandsTests : QueryTestBase
    {
        [Fact]
        public async void Topup_BothBalancesIncreased()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var balance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var model = new TopupAccountModel
                {
                    Correcting = false,
                    AccountId = session.DebitCardAccount.Id,
                    AddIncomeTypeName = session.SalaryIncome.Name,
                    TopupDate = DateTime.Today,
                    Amount = 15000,
                };
                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                await commands.Topup(model);
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(balance + model.Amount);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
            }
        }

        [Fact]
        public async void Topup_Correcting_OnlyAvailBalanceIncreased()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var balance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var model = new TopupAccountModel
                {
                    Correcting = true,
                    AccountId = session.DebitCardAccount.Id,
                    AddIncomeTypeName = session.SalaryIncome.Name,
                    TopupDate = DateTime.Today,
                    Amount = 15000,
                };
                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                await commands.Topup(model);
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(balance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
            }
        }

        [Fact]
        public async void Transfer_BalancesChangedAvailsToo()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var balance1 = session.DebitCardAccount.Balance;
                var balance2 = session.CashAccount.Balance;
                var availBalance1 = session.DebitCardAccount.AvailBalance;
                var availBalance2 = session.CashAccount.AvailBalance;
                const decimal transferAmount = 10000m;

                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                await commands.Transfer(session.DebitCardAccount.Id, session.CashAccount.Id, transferAmount);

                var accountQueries = session.UnitOfWork.GetQueryRepository<Account>();
                var debitCard = await accountQueries.GetById(session.DebitCardAccount.Id);
                var cash = await accountQueries.GetById(session.CashAccount.Id);
                debitCard.Balance.ShouldBeEquivalentTo(balance1 - transferAmount);
                debitCard.AvailBalance.ShouldBeEquivalentTo(availBalance1 - transferAmount);
                cash.Balance.ShouldBeEquivalentTo(balance2 + transferAmount);
                cash.AvailBalance.ShouldBeEquivalentTo(availBalance2 + transferAmount);
            }
        }

        [Fact]
        public async void TransferToExpenseFlow_AvailBalanceDecreasedBalanceNot()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                session.DebitCardAccount.Balance = 1000;
                session.UnitOfWork.GetCommandRepository<Account>().Update(session.DebitCardAccount);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var accountBalance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var flowBalance = session.FoodExpenseFlow.Balance;
                const decimal transferAmount = 5000m;

                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                await commands.TransferToExpenseFlow(session.FoodExpenseFlow.Id, session.DebitCardAccount.Id, transferAmount);

                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                var flow = await session.UnitOfWork.GetQueryRepository<ExpenseFlow>().GetById(session.FoodExpenseFlow.Id);

                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance - transferAmount);
                flow.Balance.ShouldBeEquivalentTo(flowBalance + transferAmount);
            }
        }

        [Fact]
        public async void TransferToExpenseFlow_AboveAvailableBalance_ThrowsException()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                session.DebitCardAccount.AvailBalance = 1000;
                session.UnitOfWork.GetCommandRepository<Account>().Update(session.DebitCardAccount);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await commands.TransferToExpenseFlow(session.FoodExpenseFlow.Id, session.DebitCardAccount.Id, 2000);
                });
            }
        }

        [Fact]
        public async void Update_UpdatedAvailNotChanged()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var balance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var newBalance = balance + 500m;

                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                var model = session.DebitCardAccount.ToModel();
                model.Balance = newBalance;
                await commands.Update(model);

                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(newBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            }
        }

        [Fact]
        public async void Update_CreatedBalanceEqualsAvail()
        {
            using (var session = await CreateDefaultSession())
            {
                var model = new AccountModel
                {
                    Id = -1,
                    Balance = 3000m,
                    DateCreated = DateTime.Today,
                    Name = "test",
                    Number = 1,
                };
                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                model = await commands.Update(model);
                model.Id.Should().BeGreaterThan(0);

                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(model.Id);
                account.Should().NotBeNull();
                account.Balance.ShouldBeEquivalentTo(model.Balance);
                account.AvailBalance.ShouldBeEquivalentTo(model.Balance);
            }
        }
    }
}