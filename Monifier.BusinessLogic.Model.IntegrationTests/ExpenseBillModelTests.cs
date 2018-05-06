using System;
using System.Linq;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Queries.Expenses;
using Monifier.BusinessLogic.Queries.Transactions;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Model.IntegrationTests
{
    public class ExpenseBillModelTests : QueryTestBase
    {
        [Fact]
        public async void Create_BalancesAreModifiedCorrectly()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var now = DateTime.Now;
                var bill = CreateBill(session);
                await bill.Create(session.UnitOfWork);

                var availBalance = session.DebitCardAccount.AvailBalance;
                var accountBalance = session.DebitCardAccount.Balance - bill.Cost;
                var flowBalance = session.FoodExpenseFlow.Balance - bill.Cost;

                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
                account.LastWithdraw.Should().NotBeNull();
                account.LastWithdraw?.Should().BeOnOrAfter(now);

                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(flowBalance);
            }
        }

        [Fact]
        public async void Create_ZeroAvailBalanceNotModified()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                session.DebitCardAccount.AvailBalance = 0;
                await session.UpdateEntity(session.DebitCardAccount);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var bill = CreateBill(session);
                await bill.Create(session.UnitOfWork);

                var accountBalance = session.DebitCardAccount.Balance - bill.Cost;
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(0);
            }
        }

        [Fact]
        public async void Create_LessAvailBalanceNotModified()
        {
            EntityIdSet ids;
            ExpenseBillModel bill;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                session.DebitCardAccount.Balance = 1;
                session.DebitCardAccount.AvailBalance = bill.Cost - 1;
                await session.UpdateEntity(session.DebitCardAccount);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var availBalance = session.DebitCardAccount.AvailBalance;
                await bill.Create(session.UnitOfWork);

                var accountBalance = session.DebitCardAccount.Balance - bill.Cost;
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            }
        }

        [Fact]
        public async void Update_SameAccount_BalanceCorrected()
        {
            decimal availBalance, oldBalance;
            DateTime? lastWithdraw;
            ExpenseBillModel bill;
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                availBalance = session.DebitCardAccount.AvailBalance;
                await bill.Create(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                lastWithdraw = account.LastWithdraw;
                oldBalance = account.Balance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var oldCost = bill.Cost;
                bill.AddItem(new ExpenseItemModel
                {
                    Category = session.FoodCategory.Name,
                    Product = session.Bread.Name,
                    Cost = 25
                });
                var costDiff = bill.Cost - oldCost;
                await bill.Update(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(oldBalance - costDiff);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
                account.LastWithdraw.ShouldBeEquivalentTo(lastWithdraw);
            }
        }

        [Fact]
        public async void Update_AccountChanged_BalancesChangedCorrectly()
        {
            decimal oldBalance, availBalance;
            ExpenseBillModel bill;
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                oldBalance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
                await bill.Create(session.UnitOfWork);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                bill.AddItem(new ExpenseItemModel
                {
                    Category = session.TechCategory.Name,
                    Product = session.Tv.Name,
                    Cost = 10000m
                });
                bill.AccountId = session.CashAccount.Id;
                var now = DateTime.Now;
                await bill.Update(session.UnitOfWork);
                var prevAccount = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                prevAccount.Balance.ShouldBeEquivalentTo(oldBalance);
                prevAccount.AvailBalance.ShouldBeEquivalentTo(availBalance);
                var newAccount = await session.LoadEntity<Account>(session.CashAccount.Id);
                newAccount.Balance.ShouldBeEquivalentTo(session.CashAccount.Balance - bill.Cost);
                newAccount.LastWithdraw.Should().NotBeNull();
                newAccount.LastWithdraw?.Should().BeOnOrAfter(now);
            }
        }

        [Fact]
        public async void Delete_AccountBalanceTopuped()
        {
            decimal availBalance, balance;
            EntityIdSet ids;
            ExpenseBillModel bill;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(bill.AccountId ?? 0);
                availBalance = account.AvailBalance;
                balance = account.Balance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                await ExpenseBillModel.Delete(bill.Id, session.UnitOfWork);
                var account = await session.LoadEntity<Account>(bill.AccountId ?? 0);
                account.Balance.ShouldBeEquivalentTo(balance + bill.Cost);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            }
        }

        [Fact]
        public async void Create_GreaterThenFlowBalance_CompensatedWithAccountAvail()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var bill = CreateBill(session);
                var newCost = session.FoodExpenseFlow.Balance + 200;
                bill.Items[0].Cost = newCost;
                bill.Cost = newCost;
                await bill.Create(session.UnitOfWork);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(0);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance - 200);
            }
        }

        [Fact]
        public async void Create_GreaterThenFlowBalance_AccountAvailNotEnough_FlowBalanceBecomeNegative()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                session.DebitCardAccount.AvailBalance = 100;
                await session.UpdateEntity(session.DebitCardAccount);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var bill = CreateBill(session);
                var newCost = session.FoodExpenseFlow.Balance + 200;
                bill.Items[0].Cost = newCost;
                bill.Cost = newCost;
                await bill.Create(session.UnitOfWork);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(-100);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(0);
            }
        }

        [Fact]
        public async void Create_FlowBalanceIsNegative_AccountAvailIsUsed()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                session.FoodExpenseFlow.Balance = -100;
                await session.UpdateEntity(session.FoodExpenseFlow);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var bill = CreateBill(session);
                await bill.Create(session.UnitOfWork);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance);

                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance - bill.Cost);
            }
        }

        [Fact]
        public async void Create_Correction_AccountBalancesNotChanged()
        {
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                var bill = CreateBill(session);
                session.FoodExpenseFlow.Balance = bill.Cost - 10;
                await session.UpdateEntity(session.FoodExpenseFlow);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var bill = CreateBill(session);
                await bill.Create(session.UnitOfWork, correction: true);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance - bill.Cost);

                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(session.DebitCardAccount.Balance);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
            }
        }

        [Fact]
        public async void Update_FlowBalanceNotEnough_AccountAvailIsUsed()
        {
            decimal availBalance, oldBalance, flowBalance;
            DateTime? lastWithdraw;
            ExpenseBillModel bill;
            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                flowBalance = session.FoodExpenseFlow.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
                await bill.Create(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                lastWithdraw = account.LastWithdraw;
                oldBalance = account.Balance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var oldCost = bill.Cost;
                bill.AddItem(new ExpenseItemModel
                {
                    Category = session.TechCategory.Name,
                    Product = session.Tv.Name,
                    Cost = 10000m
                });
                var costDiff = bill.Cost - oldCost;
                await bill.Update(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                account.Balance.ShouldBeEquivalentTo(oldBalance - costDiff);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance - (bill.Cost - flowBalance));
                account.LastWithdraw.ShouldBeEquivalentTo(lastWithdraw);
                flow.Balance.ShouldBeEquivalentTo(0);
            }
        }

        [Fact]
        public async void Delete_Correction_AccountBalancesAreNotChanged()
        {
            decimal flowBalance;
            EntityIdSet ids;
            ExpenseBillModel bill;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                session.FoodExpenseFlow.Balance = bill.Cost - 10;
                await session.UpdateEntity(session.FoodExpenseFlow);
                flowBalance = session.FoodExpenseFlow.Balance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                bill = CreateBill(session);
                await bill.Create(session.UnitOfWork, correction: true);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance - bill.Cost);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var commands = new ExpensesBillCommands(session.UnitOfWork, session.UserSession);
                await commands.Delete(bill.Id, false);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(flowBalance);

                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(session.DebitCardAccount.Balance);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
            }
        }

        [Fact]
        public async void Update_Correction_AccountBalancesAreNotChanged()
        {
            decimal availBalance, oldBalance, flowBalance;
            DateTime? lastWithdraw;
            EntityIdSet ids;
            ExpenseBillModel bill;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                flowBalance = session.FoodExpenseFlow.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
                await bill.Create(session.UnitOfWork, correction: true);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                lastWithdraw = account.LastWithdraw;
                oldBalance = account.Balance;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                bill.AddItem(new ExpenseItemModel
                {
                    Category = session.TechCategory.Name,
                    Product = session.Tv.Name,
                    Cost = 10000m
                });
                await bill.Update(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(0);
                account.Balance.ShouldBeEquivalentTo(oldBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance - (bill.Cost - flowBalance));
                account.LastWithdraw.ShouldBeEquivalentTo(lastWithdraw);
            }
        }

        [Fact]
        public async void Create_SingleTransactionIsCreated()
        {
            ExpenseBillModel bill;
            using (var session = await CreateSession(UserEvgeny))
            {
                session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);
            }

            EntityIdSet ids;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);
            }

            using (var session = await CreateDefaultSession(ids))
            {
                var transactionsQueries = new TransactionQueries(session.UnitOfWork, session.UserSession);
                var transaction = await transactionsQueries.GetBillTransaction(ids.DebitCardAccountId, bill.Id);
                transaction.ShouldBeEquivalentTo(
                    new TransactionModel
                    {
                        InitiatorId = ids.DebitCardAccountId,
                        OwnerId = bill.OwnerId,
                        ParticipantId = null,
                        BillId = bill.Id,
                        DateTime = bill.DateTime,
                        IsDeleted = false,
                        Total = bill.Cost
                    }, opt => opt.Excluding(x => x.Id));
            }
        }

        [Fact]
        public async void Update_SameAccount_TransactionUpdated()
        {
            ExpenseBillModel bill;
            using (var session = await CreateSession(UserEvgeny))
            {
                session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);
            }

            EntityIdSet ids;
            int transactionId;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);

                var transactionsQueries = new TransactionQueries(session.UnitOfWork, session.UserSession);
                var transaction = await transactionsQueries.GetBillTransaction(ids.DebitCardAccountId, bill.Id);
                transaction.ShouldBeEquivalentTo(
                    new TransactionModel
                    {
                        InitiatorId = ids.DebitCardAccountId,
                        OwnerId = bill.OwnerId,
                        ParticipantId = null,
                        BillId = bill.Id,
                        IncomeId = null,
                        DateTime = bill.DateTime,
                        IsDeleted = false,
                        Total = bill.Cost
                    }, opt => opt.Excluding(x => x.Id));
                transactionId = transaction.Id;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                bill.ExpenseFlowId = ids.TechExpenseFlowId;
                bill.AddItem(new ExpenseItemModel
                {
                    CategoryId = ids.TechCategoryId,
                    ProductId = ids.TvId,
                    Cost = 89000,
                });
                await bill.Update(session.UnitOfWork);

                var transactionsQueries = new TransactionQueries(session.UnitOfWork, session.UserSession);
                var transaction = await transactionsQueries.GetBillTransaction(ids.DebitCardAccountId, bill.Id);
                transaction.ShouldBeEquivalentTo(
                    new TransactionModel
                    {
                        Id = transactionId,
                        OwnerId = bill.OwnerId,
                        InitiatorId = ids.DebitCardAccountId,
                        ParticipantId = null,
                        BillId = bill.Id,
                        IncomeId = null,
                        DateTime = bill.DateTime,
                        IsDeleted = false,
                        Total = bill.Cost
                    });
            }
        }

        [Fact]
        public async void Update_DifferentAccount_TransactionIsUpdated()
        {
            ExpenseBillModel bill;
            using (var session = await CreateSession(UserEvgeny))
            {
                session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);
            }

            EntityIdSet ids;
            int transactionId;
            using (var session = await CreateDefaultSession())
            {
                ids = session.CreateDefaultEntities();
                bill = CreateBill(session);
                await bill.Save(session.UnitOfWork);

                var transactionsQueries = new TransactionQueries(session.UnitOfWork, session.UserSession);
                var transactions = await transactionsQueries.GetInitiatorTransactions(ids.DebitCardAccountId);
                transactions.Count.ShouldBeEquivalentTo(1);
                var transaction = transactions.Single();
                transaction.ShouldBeEquivalentTo(
                    new TransactionModel
                    {
                        InitiatorId = ids.DebitCardAccountId,
                        OwnerId = bill.OwnerId,
                        ParticipantId = null,
                        BillId = bill.Id,
                        IncomeId = null,
                        DateTime = bill.DateTime,
                        IsDeleted = false,
                        Total = bill.Cost
                    }, opt => opt.Excluding(x => x.Id));
                transactionId = transaction.Id;
            }

            using (var session = await CreateDefaultSession(ids))
            {
                bill.ExpenseFlowId = ids.TechExpenseFlowId;
                bill.AccountId = ids.CashAccountId;
                bill.AddItem(new ExpenseItemModel
                {
                    CategoryId = ids.TechCategoryId,
                    ProductId = ids.TvId,
                    Cost = 89000,
                });
                await bill.Update(session.UnitOfWork);

                var transactionsQueries = new TransactionQueries(session.UnitOfWork, session.UserSession);
                var transaction = await transactionsQueries.GetBillTransaction(ids.CashAccountId, bill.Id);
                transaction.ShouldBeEquivalentTo(
                    new TransactionModel
                    {
                        Id = transactionId,
                        OwnerId = bill.OwnerId,
                        InitiatorId = ids.CashAccountId,
                        ParticipantId = null,
                        BillId = bill.Id,
                        IncomeId = null,
                        DateTime = bill.DateTime,
                        IsDeleted = false,
                        Total = bill.Cost
                    });
            }
        }

        private static ExpenseBillModel CreateBill(UserQuerySession session)
        {
            var bill = new ExpenseBillModel
            {
                OwnerId = session.UserSession.UserId,
                AccountId = session.DebitCardAccount.Id,
                ExpenseFlowId = session.FoodExpenseFlow.Id,
                DateTime = DateTime.Now,
            };
            bill.AddItem(new ExpenseItemModel
            {
                Category = session.FoodCategory.Name,
                Cost = 120.45m,
                Product = session.Meat.Name
            });
            return bill;
        }

    }
}