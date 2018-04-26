using System;
using System.ComponentModel.Design;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.BusinessLogic.Queries.Incomes;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Incomes;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class IncomeItemCommandsTests : QueryTestBase
    {
        [Fact]
        public async void Delete_AccountBalancesDecreased()
        {
            EntityIdSet ids;
            IncomeItem income;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                var balance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var model = new TopupAccountModel
                {
                    Correction = false,
                    AccountId = session.DebitCardAccount.Id,
                    AddIncomeTypeName = session.SalaryIncome.Name,
                    TopupDate = DateTime.Today,
                    Amount = 15000,
                };
                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                income = await commands.Topup(model);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance + model.Amount);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                await commands.Delete(income.Id, false);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(session.DebitCardAccount.Balance - income.Total);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance - income.Total);
            }
        }

        [Fact]
        public async void Delete_Correction_OnlyAvailBalanceDecreased()
        {
            EntityIdSet ids;
            IncomeItem income;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                var balance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var model = new TopupAccountModel
                {
                    Correction = true,
                    AccountId = session.DebitCardAccount.Id,
                    AddIncomeTypeName = session.SalaryIncome.Name,
                    TopupDate = DateTime.Today,
                    Amount = 15000,
                };
                var commands = new AccountCommands(session.UnitOfWork, session.UserSession);
                income = await commands.Topup(model);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                await commands.Delete(income.Id, false);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(session.DebitCardAccount.Balance);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance - income.Total);
            }
        }

        [Fact]
        public async void Update_Creation_AccountBalancesAreIncreased()
        {
            EntityIdSet ids;
            decimal balance, availBalance;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                balance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                var income = new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = new DateTime(2018, 03, 05, 23, 45, 00),
                    IsCorrection = false,
                    Total = 1230.45m,
                    IncomeTypeId = ids.GiftsIncomeId,
                };
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance + income.Total);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }
        }

        [Fact]
        public async void Update_Updating_SameAccount_AccountBalancesAreChanged()
        {
            EntityIdSet ids;
            decimal balance, availBalance;
            IncomeItemModel income;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                balance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                income = new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = new DateTime(2018, 03, 05, 23, 45, 00),
                    IsCorrection = false,
                    Total = 1230.45m,
                    IncomeTypeId = ids.GiftsIncomeId,
                };
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance + income.Total);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                income.Total = 1340;
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance + income.Total);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }
        }

        [Fact]
        public async void Update_Updating_DifferentAccounts_BothAccountsBalancesAreChanged()
        {
            EntityIdSet ids;
            decimal balance, availBalance;
            IncomeItemModel income;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                balance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                income = new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = new DateTime(2018, 03, 05, 23, 45, 00),
                    IsCorrection = false,
                    Total = 1230.45m,
                    IncomeTypeId = ids.GiftsIncomeId,
                };
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance + income.Total);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                income.Total = 1340;
                income.AccountId = ids.CashAccountId;
                await commands.Update(income);
                var account1 = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account1.Balance.ShouldBeEquivalentTo(balance);
                account1.AvailBalance.ShouldBeEquivalentTo(availBalance);
                var account2 = await session.LoadEntity<Account>(ids.CashAccountId);
                account2.Balance.ShouldBeEquivalentTo(session.CashAccount.Balance + income.Total);
                account2.AvailBalance.ShouldBeEquivalentTo(session.CashAccount.AvailBalance + income.Total);
            }
        }

        [Fact]
        public async void Update_CreateCorrection_OnlyAvailBalanceChanged()
        {
            EntityIdSet ids;
            decimal balance, availBalance;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                balance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                var income = new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = new DateTime(2018, 03, 05, 23, 45, 00),
                    IsCorrection = true,
                    Total = 1230.45m,
                    IncomeTypeId = ids.GiftsIncomeId,
                };
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }
        }

        [Fact]
        public async void Update_UpdateCorrection_OnleAvailBalanceCorrected()
        {
            EntityIdSet ids;
            decimal balance, availBalance;
            IncomeItemModel income;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                balance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                income = new IncomeItemModel
                {
                    AccountId = ids.DebitCardAccountId,
                    DateTime = new DateTime(2018, 03, 05, 23, 45, 00),
                    IsCorrection = true,
                    Total = 1230.45m,
                    IncomeTypeId = ids.GiftsIncomeId,
                };
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new IncomeItemCommands(session.UnitOfWork, session.UserSession);
                income.Total = 1340;
                await commands.Update(income);
                var account = await session.LoadEntity<Account>(ids.DebitCardAccountId);
                account.Balance.ShouldBeEquivalentTo(balance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + income.Total);
            }
        }
    }
}