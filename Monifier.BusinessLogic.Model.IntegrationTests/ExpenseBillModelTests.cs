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

                var accountQueries = session.UnitOfWork.GetQueryRepository<Account>();
                var account = await accountQueries.GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
                account.LastWithdraw.Should().NotBeNull();
                account.LastWithdraw?.Should().BeOnOrAfter(now);

                var flowQueries = session.UnitOfWork.GetQueryRepository<ExpenseFlow>();
                var flow = await flowQueries.GetById(session.FoodExpenseFlow.Id);
                flow.Balance.ShouldBeEquivalentTo(flowBalance);
            }
        }

        [Fact]
        public async void Create_ZeroAvailBalanceNotModified()
        {
            using (var session = await CreateDefaultSession(_ids))
            {
                session.DebitCardAccount.AvailBalance = 0;
                var accountCommands = session.UnitOfWork.GetCommandRepository<Account>();
                accountCommands.Update(session.DebitCardAccount);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                await _bill.Create(session.UnitOfWork);

                var accountBalance = session.DebitCardAccount.Balance - _bill.Cost;
                var accountQueries = session.UnitOfWork.GetQueryRepository<Account>();
                var account = await accountQueries.GetById(session.DebitCardAccount.Id);
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
                var accountCommands = session.UnitOfWork.GetCommandRepository<Account>();
                accountCommands.Update(session.DebitCardAccount);
                await session.UnitOfWork.SaveChangesAsync();
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                var availBalance = session.DebitCardAccount.AvailBalance;
                await _bill.Create(session.UnitOfWork);

                var accountBalance = session.DebitCardAccount.Balance - _bill.Cost;
                var accountQueries = session.UnitOfWork.GetQueryRepository<Account>();
                var account = await accountQueries.GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(accountBalance);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            }
        }

        [Fact]
        public async void Update_SameAccountBalanceCorrected()
        {
            decimal availBalance, oldBalance;
            DateTime? lastWithdraw;
            using (var session = await CreateDefaultSession(_ids))
            {
                availBalance = session.DebitCardAccount.AvailBalance;
                await _bill.Create(session.UnitOfWork);
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
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
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(oldBalance - costDiff);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
                account.LastWithdraw.ShouldBeEquivalentTo(lastWithdraw);
            }
        }

        [Fact]
        public async void Update_AccountChangedBalancesChangedCorrectly()
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
                var prevAccount = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.DebitCardAccount.Id);
                prevAccount.Balance.ShouldBeEquivalentTo(oldBalance);
                prevAccount.AvailBalance.ShouldBeEquivalentTo(availBalance);
                var newAccount = await session.UnitOfWork.GetQueryRepository<Account>().GetById(session.CashAccount.Id);
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
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(_bill.AccountId ?? 0);
                availBalance = account.AvailBalance;
                balance = account.Balance;
            }

            using (var session = await CreateDefaultSession(_ids))
            {
                await ExpenseBillModel.Delete(_bill.Id, session.UnitOfWork);
                var account = await session.UnitOfWork.GetQueryRepository<Account>().GetById(_bill.AccountId ?? 0);
                account.Balance.ShouldBeEquivalentTo(balance + _bill.Cost);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            }
        }
    }
}