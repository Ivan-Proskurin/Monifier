using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Monifier.Common.Extensions.Tests
{
    [TestClass]
    public class DecimalExtensionsTests
    {
        [TestMethod]
        public void ParseMoneyInvariant_Integer_ReturnsInteger()
        {
            Assert.AreEqual(123m, "123".ParseMoneyInvariant());
        }

        [TestMethod]
        public void ParseMoneyInvariant_NumberWithComma_ReturnsDecimal()
        {
            Assert.AreEqual(123.45m, "123,45".ParseMoneyInvariant());
        }

        [TestMethod]
        public void ParseMoneyInvariant_NumberWithPoint_ReturnsDecimal()
        {
            Assert.AreEqual(123.45m, "123.45".ParseMoneyInvariant());
        }
    }
}