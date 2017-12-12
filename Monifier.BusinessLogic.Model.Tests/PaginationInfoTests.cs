using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Tests
{
    [TestClass]
    public class PaginationInfoTests
    {
        [TestMethod]
        public void FromArgs_FirstPage10TotalItems_ReturnsRightTotalPagesCount()
        {
            var args = new PaginationArgs
            {
                PageNumber = 1,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 10);
            
            Assert.AreEqual(1, info.PageNumber);
            Assert.AreEqual(5, info.ItemsPerPage);
            Assert.AreEqual(2, info.TotalPageCount);
            Assert.AreEqual(0, info.Skipped);
            Assert.AreEqual(5, info.Taken);
        }

        [TestMethod]
        public void FromArgs_FirstPageZeroTotalItems_ReturnsRightTotalPagesCount()
        {
            var args = new PaginationArgs
            {
                PageNumber = 1,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 0);

            Assert.AreEqual(1, info.PageNumber);
            Assert.AreEqual(5, info.ItemsPerPage);
            Assert.AreEqual(0, info.TotalPageCount);
            Assert.AreEqual(0, info.Skipped);
            Assert.AreEqual(5, info.Taken);
        }

        [TestMethod]
        public void FromArgs_FirstPage9TotalItems_ReturnsRightTotalPagesCount()
        {
            var args = new PaginationArgs
            {
                PageNumber = 2,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 9);

            Assert.AreEqual(2, info.PageNumber);
            Assert.AreEqual(5, info.ItemsPerPage);
            Assert.AreEqual(2, info.TotalPageCount);
            Assert.AreEqual(5, info.Skipped);
            Assert.AreEqual(5, info.Taken);
        }

        [TestMethod]
        public void FromArgs_LastPageTotalItems_ReturnsRightPageNumber()
        {
            var args = new PaginationArgs
            {
                PageNumber = -1,
                ItemsPerPage = 5,
                IncludeDeleted = false
            };
            var info = new PaginationInfo(args, 14);

            Assert.AreEqual(3, info.PageNumber);
            Assert.AreEqual(5, info.ItemsPerPage);
            Assert.AreEqual(3, info.TotalPageCount);
            Assert.AreEqual(10, info.Skipped);
            Assert.AreEqual(5, info.Taken);
        }
    }
}