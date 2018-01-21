using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Accounts;
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
        public async void Topup_AvailBalanceIncreased()
        {
            var balance = DebitCardAccount.Balance;
            var availBalance = DebitCardAccount.AvailBalance;
            var model = new TopupAccountModel
            {
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
        public async void Transfer_BalancesChangedAvailsNot()
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
            debitCard.AvailBalance.ShouldBeEquivalentTo(availBalance1);
            cash.Balance.ShouldBeEquivalentTo(balance2 + transferAmount);
            cash.AvailBalance.ShouldBeEquivalentTo(availBalance2);
        }

        [Fact]
        public async void TransferToExpenseFlow_AvailBalanceDecreasedBalanceNot()
        {
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
    }
}