using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monifier.DataAccess.Model.Base;

namespace Monifier.DataAccess.EntityFramework.Tests
{
    [TestClass]
    public class CashedDbContextTests
    {
        [TestMethod]
        public void QueryProductRepository_ReturnsProductRepository()
        {
            var uof = new UnitOfWork(new MonifierDbContext());
            var r = uof.GetQueryRepository<Product>();
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public void QuerySameRepository_ReturnsSameObject()
        {
            var uof = new UnitOfWork(new MonifierDbContext());
            var r1 = uof.GetQueryRepository<Category>();
            var r2 = uof.GetQueryRepository<Category>();
            Assert.IsTrue(ReferenceEquals(r1, r2));
        }

        [TestMethod]
        public void QueryCommandRepository_ReturnsCategoryRepository()
        {
            var uof = new UnitOfWork(new MonifierDbContext());
            var r = uof.GetCommandRepository<Category>();
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public void QuerySameCommandRepository_ReturnsSameObject()
        {
            var uof = new UnitOfWork(new MonifierDbContext());
            var r1 = uof.GetCommandRepository<Category>();
            var r2 = uof.GetCommandRepository<Category>();
            Assert.IsTrue(ReferenceEquals(r1, r2));
        }
    }
}
