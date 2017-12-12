using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monifier.DataAccess.Model.Base;

namespace Monifier.DataAccess.EntityFramework.Tests
{
    [TestClass]
    public class MonifiedDbContextTests
    {
        private UnitOfWork _uof;

        [TestInitialize]
        public void SetUpOnce()
        {
            var options =
                new DbContextOptionsBuilder<MonifierDbContext>()
                    .UseSqlServer(
                        "Data Source=.\\SQLEXPRESS;Initial Catalog=Monifier;Integrated Security=True;MultipleActiveResultSets=True")
                    .Options;
            _uof = new UnitOfWork(new MonifierDbContext(options));
        }

        [TestMethod]
        public void QueryProductRepository_ReturnsProductRepository()
        {
            var r = _uof.GetQueryRepository<Product>();
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public void QuerySameRepository_ReturnsSameObject()
        {
            var r1 = _uof.GetQueryRepository<Category>();
            var r2 = _uof.GetQueryRepository<Category>();
            Assert.IsTrue(ReferenceEquals(r1, r2));
        }

        [TestMethod]
        public void QueryCommandRepository_ReturnsCategoryRepository()
        {
            var r = _uof.GetCommandRepository<Category>();
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public void QuerySameCommandRepository_ReturnsSameObject()
        {
            var r1 = _uof.GetCommandRepository<Category>();
            var r2 = _uof.GetCommandRepository<Category>();
            Assert.IsTrue(ReferenceEquals(r1, r2));
        }

        [TestMethod]
        public void QueryQueryAndThenNamedRespository_ReturnsBoth()
        {
            var r1 = _uof.GetQueryRepository<Category>();
            Assert.IsNotNull(r1);
            var r2 = _uof.GetNamedModelQueryRepository<Category>();
            Assert.IsNotNull(r2);
            Assert.AreNotEqual(r1, r2);
        }
    }
}
