using System;
using System.Linq;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Queries.Transactions;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class AccountCommandsTests : QueryTestBase
    {
        [Fact]
        public async void Topup_BothBalancesIncreased()
        {
            const string incomeTypeName = "Новый тип дохода";
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var balance = session.DebitCardAccount.Balance;
                var availBalance = session.DebitCardAccount.AvailBalance;
                var model = new TopupAccountModel
                {
                    Correction = false,
                    AccountId = session.DebitCardAccount.Id,
                    AddIncomeTypeName = incomeTypeName,
                    TopupDate = DateTime.Today,
                    Amount = 15000,
                };
                var commands = session.CreateAccountCommands();
                var income = await commands.Topup(model);
                var incomeType = await session.UnitOfWork.GetNamedModelQueryRepository<IncomeType>()
                    .GetByName(session.UserSession.UserId, incomeTypeName);
                income.Total.ShouldBeEquivalentTo(model.Amount);
                income.DateTime.ShouldBeEquivalentTo(model.TopupDate);
                income.AccountId.ShouldBeEquivalentTo(model.AccountId);
                income.IncomeTypeId.ShouldBeEquivalentTo(incomeType.Id);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(balance + model.Amount);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance + model.Amount);
            }
        }

        [Fact]
        public async void Topup_Correction_OnlyAvailBalanceIncreased()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
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
                var commands = session.CreateAccountCommands();
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

                var commands = session.CreateAccountCommands();
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

                var commands = session.CreateAccountCommands();
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
                var commands = session.CreateAccountCommands();
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

                var commands = session.CreateAccountCommands();
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
                var commands = session.CreateAccountCommands();
                model = await commands.Update(model);
                model.Id.Should().BeGreaterThan(0);

                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(model.Id);
                account.Should().NotBeNull();
                account.Balance.ShouldBeEquivalentTo(model.Balance);
                account.AvailBalance.ShouldBeEquivalentTo(model.Balance);
            }
        }

        [Fact]
        public async void Update_DefaultAccountMakeOtherUndefault()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                session.DebitCardAccount.IsDefault = true;
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var model = new AccountModel
                {
                    Id = -1,
                    Balance = 3000m,
                    AvailBalance = 3000m,
                    DateCreated = DateTime.Today,
                    Name = "test",
                    Number = 1,
                    IsDefault = true
                };
                var commands = session.CreateAccountCommands();
                model = await commands.Update(model);

                var queries = session.CreateAccountQueries();
                var accounts = await queries.GetAll();

                var expected = accounts.FirstOrDefault(x => x.Id == model.Id);
                expected.Should().NotBeNull();
                expected.ShouldBeEquivalentTo(model);
                accounts.Where(x => x.Id != model.Id).Select(x => x.IsDefault).ShouldAllBeEquivalentTo(false);
            }
        }

        [Fact]
        public async void Topup_CreatedSingleTransaction()
        {
            const string incomeTypeName = "Новый тип дохода";
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var model = new TopupAccountModel
                {
                    Correction = false,
                    AccountId = session.DebitCardAccount.Id,
                    AddIncomeTypeName = incomeTypeName,
                    TopupDate = DateTime.Today,
                    Amount = 15000,
                };
                var commands = session.CreateAccountCommands();
                var income = await commands.Topup(model);

                var transactoionQueries = session.CreateTransactionQueries();
                var transaction = await transactoionQueries.GetIncomeTransaction(session.DebitCardAccount.Id, income.Id);
                transaction.ShouldBeEquivalentTo(new TransactionModel
                {
                    OwnerId = session.UserSession.UserId,
                    InitiatorId = session.DebitCardAccount.Id,
                    DateTime = income.DateTime,
                    BillId = null,
                    IncomeId = income.Id,
                    ParticipantId = null,
                    Total = income.Total,
                    Balance = session.DebitCardAccount.Balance + income.Total
                }, opt => opt.Excluding(x => x.Id));
            }
        }

        [Fact]
        public async void Transfer_TwoTransactionsCreated()
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                const decimal transferAmount = 10000m;

                var commands = session.CreateAccountCommands();
                var transferTime = await commands.Transfer(session.DebitCardAccount.Id, session.CashAccount.Id, transferAmount);

                var transactionQueries = session.CreateTransactionQueries();
                var transaction = await transactionQueries.GetTransferTransaction(ids.DebitCardAccountId, ids.CashAccountId);
                transaction.ShouldBeEquivalentTo(new TransactionModel
                    {
                        OwnerId = session.UserSession.UserId,
                        DateTime = transferTime,
                        InitiatorId = ids.DebitCardAccountId,
                        BillId = null,
                        IncomeId = null,
                        ParticipantId = ids.CashAccountId,
                        Total = -transferAmount,
                        Balance = session.DebitCardAccount.Balance - transferAmount
                    }, opt => opt.Excluding(x => x.Id)
                );

                transaction = await transactionQueries.GetTransferTransaction(ids.CashAccountId, ids.DebitCardAccountId);
                transaction.ShouldBeEquivalentTo(new TransactionModel
                    {
                        OwnerId = session.UserSession.UserId,
                        DateTime = transferTime,
                        InitiatorId = ids.CashAccountId,
                        BillId = null,
                        IncomeId = null,
                        ParticipantId = ids.DebitCardAccountId,
                        Total = transferAmount,
                        Balance = session.CashAccount.Balance + transferAmount
                    }, opt => opt.Excluding(x => x.Id)
                );
            }
        }

        [Fact]
        public async void Transfer_SameTargetAndDestinaition_ShouldThrowsExcpetion()
        {
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var commands = session.CreateAccountCommands();
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await commands.Transfer(ids.DebitCardAccountId, ids.DebitCardAccountId, 1));
            }
        }
    }
}