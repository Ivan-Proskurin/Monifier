using System;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Model.IntegrationTests
{
    public class ExpenseBillModelTests : DatabaseRelatedEntitiesTest
    {
        private readonly ExpenseBillModel _bill;
        
        public ExpenseBillModelTests()
        {
            _bill = new ExpenseBillModel
            {
                AccountId = DebitCardAccount.Id,
                ExpenseFlowId = FoodExpenseFlow.Id,
                DateTime = DateTime.Now,
            };
            _bill.AddItem(new ExpenseItemModel
            {
                Category = FoodCategory.Name,
                Cost = 120.45m,
                Product = Meat.Name
            });
        }
        
        [Fact]
        public async void Create_BalancesAreModifiedCorrectly()
        {
            var now = DateTime.Now;
            await _bill.Create(UnitOfWork);

            var availBalance = DebitCardAccount.AvailBalance;
            var accountBalance = DebitCardAccount.Balance - _bill.Cost;
            var flowBalance = FoodExpenseFlow.Balance - _bill.Cost;
            
            var accountQueries = UnitOfWork.GetQueryRepository<Account>();
            var account = await accountQueries.GetById(DebitCardAccount.Id);
            account.Balance.ShouldBeEquivalentTo(accountBalance);
            account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            account.LastWithdraw.Should().NotBeNull();
            account.LastWithdraw?.Should().BeOnOrAfter(now);
            
            var flowQueries = UnitOfWork.GetQueryRepository<ExpenseFlow>();
            var flow = await flowQueries.GetById(FoodExpenseFlow.Id);
            flow.Balance.ShouldBeEquivalentTo(flowBalance);
        }

        [Fact]
        public async void Create_ZeroAvailBalanceNotModified()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                DebitCardAccount.AvailBalance = 0;
                var accountCommands = uof.GetCommandRepository<Account>();
                accountCommands.Update(DebitCardAccount);
                await uof.SaveChangesAsync();
            });
            
            await _bill.Create(UnitOfWork);

            var accountBalance = DebitCardAccount.Balance - _bill.Cost;
            var accountQueries = UnitOfWork.GetQueryRepository<Account>();
            var account = await accountQueries.GetById(DebitCardAccount.Id);
            account.Balance.ShouldBeEquivalentTo(accountBalance);
            account.AvailBalance.ShouldBeEquivalentTo(0);
        }
        
        [Fact]
        public async void Create_LessAvailBalanceNotModified()
        {
            await UseUnitOfWorkAsync(async uof =>
            {
                DebitCardAccount.Balance = 1;
                DebitCardAccount.AvailBalance = _bill.Cost - 1;
                var accountCommands = uof.GetCommandRepository<Account>();
                accountCommands.Update(DebitCardAccount);
                await uof.SaveChangesAsync();
            });

            var availBalance = DebitCardAccount.AvailBalance;
            await _bill.Create(UnitOfWork);

            var accountBalance = DebitCardAccount.Balance - _bill.Cost;
            var accountQueries = UnitOfWork.GetQueryRepository<Account>();
            var account = await accountQueries.GetById(DebitCardAccount.Id);
            account.Balance.ShouldBeEquivalentTo(accountBalance);
            account.AvailBalance.ShouldBeEquivalentTo(availBalance);
        }

        [Fact]
        public async void Update_SameAccountBalanceCorrected()
        {
            var availBalance = DebitCardAccount.AvailBalance;
            var lastWithdraw = DebitCardAccount.LastWithdraw;
            var oldBalance = await UseUnitOfWorkAsync(async uof =>
            {
                await _bill.Create(uof);
                var account = await uof.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
                lastWithdraw = account.LastWithdraw;
                return account.Balance;
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                var oldCost = _bill.Cost;
                _bill.AddItem(new ExpenseItemModel
                {
                    Category = TechCategory.Name,
                    Product = Tv.Name,
                    Cost = 10000m
                });
                var costDiff = _bill.Cost - oldCost;
                await _bill.Update(uof);
                var account = await uof.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
                account.Balance.ShouldBeEquivalentTo(oldBalance - costDiff);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
                account.LastWithdraw.ShouldBeEquivalentTo(lastWithdraw);
            });
        }

        [Fact]
        public async void Update_AccountChangedBalancesChangedCorrectly()
        {
            var oldBalance = DebitCardAccount.Balance;
            var availBalance = DebitCardAccount.AvailBalance;
            await _bill.Create(UnitOfWork);

            await UseUnitOfWorkAsync(async uof =>
            {
                _bill.AddItem(new ExpenseItemModel
                {
                    Category = TechCategory.Name,
                    Product = Tv.Name,
                    Cost = 10000m
                });
                _bill.AccountId = CashAccount.Id;
                var now = DateTime.Now;
                await _bill.Update(uof);
                var prevAccount = await uof.GetQueryRepository<Account>().GetById(DebitCardAccount.Id);
                prevAccount.Balance.ShouldBeEquivalentTo(oldBalance);
                prevAccount.AvailBalance.ShouldBeEquivalentTo(availBalance);
                var newAccount = await uof.GetQueryRepository<Account>().GetById(CashAccount.Id);
                newAccount.Balance.ShouldBeEquivalentTo(CashAccount.Balance - _bill.Cost);
                newAccount.LastWithdraw.Should().NotBeNull();
                newAccount.LastWithdraw?.Should().BeOnOrAfter(now);
            });
        }

        [Fact]
        public async void Delete_AccountBalanceTopuped()
        {
            decimal availBalance = 0;
            var balance = await UseUnitOfWorkAsync(async uof =>
            {
                await _bill.Save(uof);
                var account = await uof.GetQueryRepository<Account>().GetById(_bill.AccountId ?? 0);
                availBalance = account.AvailBalance;
                return account.Balance;
            });

            await UseUnitOfWorkAsync(async uof =>
            {
                await ExpenseBillModel.Delete(_bill.Id, UnitOfWork);
                var account = await UnitOfWork.GetQueryRepository<Account>().GetById(_bill.AccountId ?? 0);
                account.Balance.ShouldBeEquivalentTo(balance + _bill.Cost);
                account.AvailBalance.ShouldBeEquivalentTo(availBalance);
            });
        }
    }
}