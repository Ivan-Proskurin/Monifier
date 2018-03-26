using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Model.IntegrationTests
{
    public class ExpenseBillModelTests : QueryTestBase
    {
        private readonly EntityIdSet _ids;
        private readonly ExpenseBillModel _bill;
        
        public ExpenseBillModelTests()
        {
            using (var session = CreateDefaultSession().Result)
            {
                _ids = session.CreateDefaultEntities();
                _bill = new ExpenseBillModel
                {
                    AccountId = session.DebitCardAccount.Id,
                    ExpenseFlowId = session.FoodExpenseFlow.Id,
                    DateTime = DateTime.Now,
                };
                _bill.AddItem(new ExpenseItemModel
                {
                    Category = session.FoodCategory.Name,
                    Cost = 120.45m,
                    Product = session.Meat.Name
                });
            }
        }
        
        [Fact]
        public async void Create_BalancesAreModifiedCorrectly()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                var now = DateTime.Now;
                await _bill.Create(session.UnitOfWork);

                var availBalance = session.DebitCardAccount.AvailBalance;
                var accountBalance = session.DebitCardAccount.Balance - _bill.Cost;
                var flowBalance = session.FoodExpenseFlow.Balance - _bill.Cost;

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
            using (var session = await CreateDefaultSession(_ids))
            {
                session.DebitCardAccount.AvailBalance = 0;
                await session.UpdateEntity(session.DebitCardAccount);
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                await _bill.Create(session.UnitOfWork);

                var accountBalance = session.DebitCardAccount.Balance - _bill.Cost;
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(0);
            }
        }
        
        [Fact]
        public async void Create_LessAvailBalanceNotModified()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                session.DebitCardAccount.Balance = 1;
                session.DebitCardAccount.AvailBalance = _bill.Cost - 1;
                await session.UpdateEntity(session.DebitCardAccount);
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                var availBalance = session.DebitCardAccount.AvailBalance;
                await _bill.Create(session.UnitOfWork);

                var accountBalance = session.DebitCardAccount.Balance - _bill.Cost;
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
            using (var session = await CreateDefaultSession(_ids))
            {
                availBalance = session.DebitCardAccount.AvailBalance;
                await _bill.Create(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                lastWithdraw = account.LastWithdraw;
                oldBalance = account.Balance;
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                var oldCost = _bill.Cost;
                _bill.AddItem(new ExpenseItemModel
                {
                    Category = session.FoodCategory.Name,
                    Product = session.Bread.Name,
                    Cost = 25
                });
                var costDiff = _bill.Cost - oldCost;
                await _bill.Update(session.UnitOfWork);
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
            using (var session = await CreateDefaultSession(_ids))
            {
                oldBalance = session.DebitCardAccount.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
                await _bill.Create(session.UnitOfWork);
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                _bill.AddItem(new ExpenseItemModel
                {
                    Category = session.TechCategory.Name,
                    Product = session.Tv.Name,
                    Cost = 10000m
                });
                _bill.AccountId = session.CashAccount.Id;
                var now = DateTime.Now;
                await _bill.Update(session.UnitOfWork);
                var prevAccount = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                prevAccount.Balance.ShouldBeEquivalentTo(oldBalance);
                prevAccount.AvailBalance.ShouldBeEquivalentTo(availBalance);
                var newAccount = await session.LoadEntity<Account>(session.CashAccount.Id);
                newAccount.Balance.ShouldBeEquivalentTo(session.CashAccount.Balance - _bill.Cost);
                newAccount.LastWithdraw.Should().NotBeNull();
                newAccount.LastWithdraw?.Should().BeOnOrAfter(now);
            }
        }

        [Fact]
        public async void Delete_AccountBalanceTopuped()
        {
            decimal availBalance, balance;
            using (var session = await CreateDefaultSession(_ids))
            {
                await _bill.Save(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(_bill.AccountId ?? 0);
                availBalance = account.AvailBalance;
                balance = account.Balance;
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                await ExpenseBillModel.Delete(_bill.Id, session.UnitOfWork);
                var account = await session.LoadEntity<Account>(_bill.AccountId ?? 0);
                account.Balance.ShouldBeEquivalentTo(balance + _bill.Cost);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            }
        }

        [Fact]
        public async void Create_GreaterThenFlowBalance_CompensatedWithAccountAvail()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                var newCost = session.FoodExpenseFlow.Balance + 200;
                _bill.Items[0].Cost = newCost;
                _bill.Cost = newCost;
                await _bill.Create(session.UnitOfWork);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(0);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance - 200);
            }
        }

        [Fact]
        public async void Create_GreaterThenFlowBalance_AccountAvailNotEnough_FlowBalanceBecomeNegative()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                session.DebitCardAccount.AvailBalance = 100;
                await session.UpdateEntity(session.DebitCardAccount);
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                var newCost = session.FoodExpenseFlow.Balance + 200;
                _bill.Items[0].Cost = newCost;
                _bill.Cost = newCost;
                await _bill.Create(session.UnitOfWork);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(-100);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(0);
            }
        }

        [Fact]
        public async void Create_FlowBalanceIsNegative_AccountAvailIsUsed()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                session.FoodExpenseFlow.Balance = -100;
                await session.UpdateEntity(session.FoodExpenseFlow);
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                await _bill.Create(session.UnitOfWork);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance);

                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance - _bill.Cost);
            }
        }

        [Fact]
        public async void Create_Correcting_AccountAvailNotUsed()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                session.FoodExpenseFlow.Balance = _bill.Cost - 10;
                await session.UpdateEntity(session.FoodExpenseFlow);
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                await _bill.Create(session.UnitOfWork, correcting: true);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(session.FoodExpenseFlow.Balance - _bill.Cost);

                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                account.AvailBalance.ShouldBeEquivalentTo(session.DebitCardAccount.AvailBalance);
            }
        }

        [Fact]
        public async void Update_FlowBalanceNotEnough_AccountAvailIsUsed()
        {
            decimal availBalance, oldBalance, flowBalance;
            DateTime? lastWithdraw;
            using (var session = await CreateDefaultSession(_ids))
            {
                flowBalance = session.FoodExpenseFlow.Balance;
                availBalance = session.DebitCardAccount.AvailBalance;
                await _bill.Create(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                lastWithdraw = account.LastWithdraw;
                oldBalance = account.Balance;
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                var oldCost = _bill.Cost;
                _bill.AddItem(new ExpenseItemModel
                {
                    Category = session.TechCategory.Name,
                    Product = session.Tv.Name,
                    Cost = 10000m
                });
                var costDiff = _bill.Cost - oldCost;
                await _bill.Update(session.UnitOfWork);
                var account = await session.LoadEntity<Account>(session.DebitCardAccount.Id);
                var flow = await session.LoadEntity<ExpenseFlow>(session.FoodExpenseFlow.Id);
                account.Balance.ShouldBeEquivalentTo(oldBalance - costDiff);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance - (_bill.Cost - flowBalance));
                account.LastWithdraw.ShouldBeEquivalentTo(lastWithdraw);
                flow.Balance.ShouldBeEquivalentTo(0);
            }
        }
    }
}