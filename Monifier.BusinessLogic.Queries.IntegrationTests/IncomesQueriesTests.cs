using System.Linq;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Common.Extensions;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class IncomesQueriesTests : QueryTestBase
    {
        [Fact]
        public async void GetIncomesList_BaseTest()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var queries = session.CreateIncomesQueries();
                var from = session.Incomes.First().DateTime.AddMilliseconds(-1);
                var to = session.Incomes.Last().DateTime.AddMilliseconds(1);
                var list = await queries.GetIncomesList(from, to, new PaginationArgs
                {
                    ItemsPerPage = 10,
                    PageNumber = 1
                });
                list.Totals.Total.ShouldBeEquivalentTo(session.Incomes.Sum(x => x.Total));
            }
        }

        [Fact]
        public async void GetIncomesByMonth_BaseTest()
        {
            using (var session = await CreateDefaultSession())
            {
                session.CreateDefaultEntities();
                var queries = session.CreateIncomesQueries();
                var from = session.Incomes.First().DateTime.StartOfTheMonth();
                var to = session.Incomes.Last().DateTime.EndOfTheMonth();
                var list = await queries.GetIncomesByMonth(from, to, new PaginationArgs
                {
                    ItemsPerPage = 10,
                    PageNumber = 1
                });
                list.Totals.Total.ShouldBeEquivalentTo(session.Incomes.Sum(x => x.Total));
            }
        }
    }
}