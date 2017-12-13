using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Monifier.Common.Extensions.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void Capitalize_PassNullOrEmpty_ReturnsNullOrEmpty()
        {
            Assert.IsNull(StringExtensions.Capitalize(null));
            Assert.AreEqual(string.Empty, string.Empty.Capitalize());
        }

        [TestMethod]
        public void Capitalize_PassNormal_ReturnsCapitalized()
        {
            Assert.AreEqual("Капитализированная строка", "капитализированная строка".Capitalize());
        }
    }
}