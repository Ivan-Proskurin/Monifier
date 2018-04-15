using FluentAssertions;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Queries.Expenses;
using Monifier.BusinessLogic.Queries.Settings;
using Monifier.IntegrationTests.Infrastructure;
using System;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class ExpensesQueriesTests : QueryTestBase
    {
        [Fact]
        public async void GetExpensesByFlowsTests()
        {
            var date1 = new DateTime(2018, 01, 15);
            var date2 = new DateTime(2018, 01, 31);
            var date3 = new DateTime(2018, 02, 01);

            using (var session = await CreateSession(UserEvgeny))
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
            }

            using (var session = await CreateDefaultSession())
            {
                var ids = session.CreateDefaultEntities();
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date1, session.Meat, 345.45m);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.FoodExpenseFlowId, date2, session.Bread, 46);
                await session.CreateExpenseBill(ids.CashAccountId, ids.TechExpenseFlowId, date2, session.Tv, 20000);
                await session.CreateExpenseBill(ids.DebitCardAccountId, ids.TechExpenseFlowId, date3, session.Tv, 10000);

                var queries = new ExpensesQueries(session.UnitOfWork, new UserSettings(), session.UserSession);
                var report = await queries.GetExpensesByFlows(new DateTime(2017, 12, 30), date3, new PaginationArgs
                {
                    PageNumber = 1,
                    ItemsPerPage = 10,
                });
                report.Items.Count.ShouldBeEquivalentTo(2);
                report.Items[0].ShouldBeEquivalentTo(new ExpenseByFlowsItemModel
                {
                    FlowId = ids.TechExpenseFlowId,
                    Flow = "Техника",
                    LastBill = date2,
                    Total = 20000
                });
                report.Items[1].ShouldBeEquivalentTo(new ExpenseByFlowsItemModel
                {
                    FlowId = ids.FoodExpenseFlowId,
                    Flow = "Продукты питания",
                    LastBill = date2,
                    Total = 391.45m
                });
                report.Totals.Total.ShouldBeEquivalentTo(20391.45m);
            }
        }
    }
}
