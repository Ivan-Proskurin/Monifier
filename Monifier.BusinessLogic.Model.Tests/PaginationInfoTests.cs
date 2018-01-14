using FluentAssertions;
using Monifier.BusinessLogic.Model.Pagination;
using Xunit;

namespace Monifier.BusinessLogic.Model.Tests
{
    public class PaginationInfoTests
    {
        [Fact]
        public void FromArgs_FirstPage10TotalItems_ReturnsRightTotalPagesCount()
        {
            var args = new PaginationArgs
            {
                PageNumber = 1,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 10);
            
            info.PageNumber.ShouldBeEquivalentTo(1);
            info.ItemsPerPage.ShouldBeEquivalentTo(5);
            info.TotalPageCount.ShouldBeEquivalentTo(2);
            info.Skipped.ShouldBeEquivalentTo(0);
            info.Taken.ShouldBeEquivalentTo(5);
        }

        [Fact]
        public void FromArgs_FirstPageZeroTotalItems_ReturnsRightTotalPagesCount()
        {
            var args = new PaginationArgs
            {
                PageNumber = 1,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 0);
          
            info.PageNumber.ShouldBeEquivalentTo(1);
            info.ItemsPerPage.ShouldBeEquivalentTo(5);
            info.TotalPageCount.ShouldBeEquivalentTo(0);
            info.Skipped.ShouldBeEquivalentTo(0);
            info.Taken.ShouldBeEquivalentTo(5);
        }

        [Fact]
        public void FromArgs_FirstPage9TotalItems_ReturnsRightTotalPagesCount()
        {
            var args = new PaginationArgs
            {
                PageNumber = 2,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 9);

            info.PageNumber.ShouldBeEquivalentTo(2);
            info.ItemsPerPage.ShouldBeEquivalentTo(5);
            info.TotalPageCount.ShouldBeEquivalentTo(2);
            info.Skipped.ShouldBeEquivalentTo(5);
            info.Taken.ShouldBeEquivalentTo(5);
        }

        [Fact]
        public void FromArgs_LastPageTotalItems_ReturnsRightPageNumber()
        {
            var args = new PaginationArgs
            {
                PageNumber = -1,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 14);

            info.PageNumber.ShouldBeEquivalentTo(3);
            info.ItemsPerPage.ShouldBeEquivalentTo(5);
            info.TotalPageCount.ShouldBeEquivalentTo(3);
            info.Skipped.ShouldBeEquivalentTo(10);
            info.Taken.ShouldBeEquivalentTo(5);
        }
    }
}