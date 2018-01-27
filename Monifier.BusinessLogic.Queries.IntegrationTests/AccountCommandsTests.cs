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
    public class AccountCommandsTests : DatabaseRelatedEntitiesTest
    {
        [Fact]
        public async void Topup_BothBalancesIncreased()
        {
            var balance = DebitCardAccount.Balance;
            var availBalance = DebitCardAccount.AvailBalance;
            var model = new TopupAccountModel
            {
                Correcting = false,
                AccountId = DebitCardAccount.Id,
                AddIncomeTypeName = SalaryIncome.Name,
                TopupDate = DateTime.Today,
                Amount = 15000,
            };
            var commands = new AccountCommands(UnitOfWork);
            await commands.Topup(model);
            var account = await UnitOfWork.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
            account.Balance.ShouldBeEquivalentTo(balance + model.Amount);
            account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
        }

        [Fact]
        public async void Topup_Correcting_OnlyAvailBalanceIncreased()
        {
            var balance = DebitCardAccount.Balance;
            var availBalance = DebitCardAccount.AvailBalance;
            var model = new TopupAccountModel
            {
                Correcting = true,
                AccountId = DebitCardAccount.Id,
                AddIncomeTypeName = SalaryIncome.Name,
                TopupDate = DateTime.Today,
                Amount = 15000,
            };
            var commands = new AccountCommands(UnitOfWork);
            await commands.Topup(model);
            var account = await UnitOfWork.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
            account.Balance.ShouldBeEquivalentTo(balance);
            account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
        }

        [Fact]
        public async void Transfer_BalancesChangedAvailsToo()
        {
            var balance1 = DebitCardAccount.Balance;
            var balance2 = CashAccount.Balance;
            var availBalance1 = DebitCardAccount.AvailBalance;
            var availBalance2 = CashAccount.AvailBalance;
            const decimal transferAmount = 10000m;
            
            var commands = new AccountCommands(UnitOfWork);
            await commands.Transfer(DebitCardAccount.Id, CashAccount.Id, transferAmount);

            var accountQueries = UnitOfWork.GetQueryRepository<Account>();
            var debitCard = await accountQueries.GetById(DebitCardAccount.Id);
            var cash = await accountQueries.GetById(CashAccount.Id);
            debitCard.Balance.ShouldBeEquivalentTo(balance1 - transferAmount);
            debitCard.AvailBalance.ShouldBeEquivalentTo(availBalance1 - transferAmount);
            cash.Balance.ShouldBeEquivalentTo(balance2 + transferAmount);
            cash.AvailBalance.ShouldBeEquivalentTo(availBalance2 + transferAmount);
        }

        [Fact]
        public async void TransferToExpenseFlow_AvailBalanceDecreasedBalanceNot()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                DebitCardAccount.Balance = 1000;
                uof.GetCommandRepository<Account>().Update(DebitCardAccount);
                await uof.SaveChangesAsync();
            });
            
            var accountBalance = DebitCardAccount.Balance;
            var availBalance = DebitCardAccount.AvailBalance;
            var flowBalance = FoodExpenseFlow.Balance;
            const decimal transferAmount = 5000m;
            
            var commands = new AccountCommands(UnitOfWork);
            await commands.TransferToExpenseFlow(FoodExpenseFlow.Id, DebitCardAccount.Id, transferAmount);

            var account = await UnitOfWork.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
            var flow = await UnitOfWork.GetQueryRepository<ExpenseFlow>().GetById(FoodExpenseFlow.Id);
            
            account.Balance.ShouldBeEquivalentTo(accountBalance);
            account.AvailBalance.ShouldBeEquivalentTo(availBalance - transferAmount);
            flow.Balance.ShouldBeEquivalentTo(flowBalance + transferAmount);
        }

        [Fact]
        public async void TransferToExpenseFlow_AboveAvailableBalance_ThrowsException()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                DebitCardAccount.AvailBalance = 1000;
                uof.GetCommandRepository<Account>().Update(DebitCardAccount);
                await uof.SaveChangesAsync();
            });
            
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var commands = new AccountCommands(UnitOfWork);
                await commands.TransferToExpenseFlow(FoodExpenseFlow.Id, DebitCardAccount.Id, 2000);
            });
        }

        [Fact]
        public async void Update_UpdatedAvailNotChanged()
        {
            var balance = DebitCardAccount.Balance;
            var availBalance = DebitCardAccount.AvailBalance;
            var newBalance = balance + 500m;
            
            var commands = new AccountCommands(UnitOfWork);
            var model = DebitCardAccount.ToModel();
            model.Balance = newBalance;
            await commands.Update(model);

            var account = await UnitOfWork.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
            account.Balance.ShouldBeEquivalentTo(newBalance);
            account.AvailBalance.ShouldBeEquivalentTo(availBalance);
        }

        [Fact]
        public async void Update_CreatedBalanceEqualsAvail()
        {
            var model = new AccountModel
            {
                Id = -1,
                Balance = 3000m,
                DateCreated = DateTime.Today,
                Name = "test",
                Number = 1,
            };
            var commands = new AccountCommands(UnitOfWork);
            model = await commands.Update(model);
            model.Id.Should().BeGreaterThan(0);

            var account = await UnitOfWork.GetQueryRepository<Account>().GetById(model.Id);
            account.Should().NotBeNull();
            account.Balance.ShouldBeEquivalentTo(model.Balance);
            account.AvailBalance.ShouldBeEquivalentTo(model.Balance);
        }
    }
}