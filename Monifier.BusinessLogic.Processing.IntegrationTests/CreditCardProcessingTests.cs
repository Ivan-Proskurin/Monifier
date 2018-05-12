using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Processing.IntegrationTests
{
    public class CreditCardProcessingTests : QueryTestBase
    {
        [Fact]
        public async void ProcessReducingBalanceAsCreditFees_NotCreditCard_ThrowsException()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var processing = session.CreateCreditCardProcessing();
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await processing.ProcessReducingBalanceAsCreditFees(session.DebitCardAccount, 1000));
            }
        }

        [Fact]
        public async void ProcessReducingBalanceAsCreditFees_CreatedBillWithCategoryAndProduct()
        {
            const decimal reduceAmount = 2500;
            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                var processing = session.CreateCreditCardProcessing();
                var billId = await processing.ProcessReducingBalanceAsCreditFees(session.CreditCardAccount, reduceAmount);

                var bill = await LoadBill(session, billId);

                bill.Should().NotBeNull();
                bill.IsCorrection.Should().BeTrue();
                bill.AccountId.Should().Be(ids.CreditCardAccountId);
                bill.SumPrice.Should().Be(reduceAmount);
                bill.OwnerId.Should().Be(session.UserSession.UserId);

                var flow = await session.LoadEntity<ExpenseFlow>(bill.ExpenseFlowId);
                flow.Should().NotBeNull();
                flow.Name.Should().Be("Кредиты");
                flow.OwnerId.Should().Be(session.UserSession.UserId);

                bill.Items.Count.Should().Be(1);
                var item = bill.Items.First();
                item.CategoryId.Should().NotBeNull();
                item.Price.Should().Be(reduceAmount);
                var category = await session.LoadEntity<Category>(item.CategoryId ?? 0);
                category.Should().NotBeNull();
                category.Name.Should().Be("Кредиты");
                category.OwnerId.Should().Be(session.UserSession.UserId);
                item.ProductId.Should().NotBeNull();

                var product = await session.LoadEntity<Product>(item.ProductId ?? 0);
                product.Should().NotBeNull();
                product.OwnerId.Should().Be(session.UserSession.UserId);
                product.Name.Should().Be(session.CreditCardAccount.Name);

                var account = await session.LoadEntity<Account>(bill.AccountId ?? 0);
                account.AvailBalance.Should().Be(session.CreditCardAccount.AvailBalance - reduceAmount);
            }
        }

        [Fact]
        public async void ProcessReducingBalanceAsCreditFees_ProductExists_MustUseExisting()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var processing = session.CreateCreditCardProcessing();
                var product = session.CreateProduct(session.TechCategory.Id, session.CreditCardAccount.Name);
                await session.UnitOfWork.SaveChangesAsync();

                var billId = await processing.ProcessReducingBalanceAsCreditFees(session.CreditCardAccount, 1000);
                var bill = await LoadBill(session, billId);
                var item = bill.Items.First();
                item.ProductId.Should().Be(product.Id);
            }
        }

        [Fact]
        public async void ProcessReducingBalanceAsCreditFees_CategoryExists_MustUseExisting()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var processing = session.CreateCreditCardProcessing();
                var category = session.CreateCategory("Кредиты");
                await session.UnitOfWork.SaveChangesAsync();

                var billId = await processing.ProcessReducingBalanceAsCreditFees(session.CreditCardAccount, 1000);
                var bill = await LoadBill(session, billId);
                var item = bill.Items.First();
                item.CategoryId.Should().Be(category.Id);
            }
        }

        [Fact]
        public async void ProcessReducingBalanceAsCreditFees_FlowExists_MustUseExisting()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var processing = session.CreateCreditCardProcessing();
                var flow = session.CreateExpenseFlow("Кредиты", 1000, DateTime.Today, 10);
                await session.UnitOfWork.SaveChangesAsync();

                var billId = await processing.ProcessReducingBalanceAsCreditFees(session.CreditCardAccount, 1000);
                var bill = await LoadBill(session, billId);
                bill.ExpenseFlowId.Should().Be(flow.Id);
            }
        }

        [Fact]
        public async void ProcessReducingBalanceAsCreditFees_FlowCategoryProductExist_MustUseExisting()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var processing = session.CreateCreditCardProcessing();
                var flow = session.CreateExpenseFlow("Кредиты", 1000, DateTime.Today, 10);
                var category = session.CreateCategory("Кредиты");
                await session.UnitOfWork.SaveChangesAsync();
                var product = session.CreateProduct(category.Id, session.CreditCardAccount.Name);
                await session.UnitOfWork.SaveChangesAsync();

                var billId = await processing.ProcessReducingBalanceAsCreditFees(session.CreditCardAccount, 1000);
                var bill = await LoadBill(session, billId);
                bill.ExpenseFlowId.Should().Be(flow.Id);
                var item = bill.Items.First();
                item.CategoryId.Should().Be(category.Id);
                item.ProductId.Should().Be(product.Id);
            }
        }

        private async Task<ExpenseBill> LoadBill(UserQuerySession session, int id)
        {
            var billsQueries = session.UnitOfWork.GetQueryRepository<ExpenseBill>();
            return await billsQueries.Query.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}