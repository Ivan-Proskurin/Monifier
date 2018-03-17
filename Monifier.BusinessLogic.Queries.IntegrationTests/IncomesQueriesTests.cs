using System.Linq;
using FluentAssertions;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Queries.Incomes;
using Monifier.Common.Extensions;
using Monifier.IntegrationTests.Infrastructure;
using Xunit;

namespace Monifier.BusinessLogic.Queries.IntegrationTests
{
    public class IncomesQueriesTests : DatabaseRelatedEntitiesTest
    {
        [Fact]
        public async void GetIncomesList_BaseTest()
        {
            var queries = new IncomesQueries(UnitOfWork, CurrentSession);
            var from = Incomes.First().DateTime.AddMilliseconds(-1);
            var to = Incomes.Last().DateTime.AddMilliseconds(1);
            var list = await queries.GetIncomesList(from, to, new PaginationArgs
            {
                ItemsPerPage = 10,
                PageNumber = 1
            });
            list.Totals.Total.ShouldBeEquivalentTo(Incomes.Sum(x => x.Total));
        }

        [Fact]
        public async void GetIncomesByMonth_BaseTest()
        {
            var queries = new IncomesQueries(UnitOfWork, CurrentSession);
            var from = Incomes.First().DateTime.StartOfTheMonth();
            var to = Incomes.Last().DateTime.EndOfTheMonth();
            var list = await queries.GetIncomesByMonth(from, to, new PaginationArgs
            {
                ItemsPerPage = 10,
                PageNumber = 1
            });
            list.Totals.Total.ShouldBeEquivalentTo(Incomes.Sum(x => x.Total));
        }
    }
}