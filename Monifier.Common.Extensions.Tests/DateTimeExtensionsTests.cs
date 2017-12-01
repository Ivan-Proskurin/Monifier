using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Monifier.Common.Extensions.Tests
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        [TestMethod]
        public void StartOfTheMonth_MiddleOfMonth_ReturnsStart()
        {
            Assert.AreEqual(new DateTime(2017, 3, 1), new DateTime(2017, 3, 25).StartOfTheMonth());
        }

        [TestMethod]
        public void StartOfTheMonth_EndOfMonth_ReturnsStart()
        {
            Assert.AreEqual(new DateTime(2017, 3, 1), new DateTime(2017, 3, 31).StartOfTheMonth());
        }

        [TestMethod]
        public void StartOfTheMonth_StartOfMonth_ReturnsStart()
        {
            Assert.AreEqual(new DateTime(2017, 3, 1), new DateTime(2017, 3, 1).StartOfTheMonth());
        }

        [TestMethod]
        public void EndOfTheMonth_MiddleOfMonth_ReturnsEnd()
        {
            Assert.AreEqual(new DateTime(2017, 12, 31, 23, 59, 59), new DateTime(2017, 12, 22).EndOfTheMonth());
        }

        [TestMethod]
        public void EndOfTheMonth_StartOfMonth_ReturnsEnd()
        {
            Assert.AreEqual(new DateTime(2017, 12, 31, 23, 59, 59), new DateTime(2017, 12, 1).EndOfTheMonth());
        }

        [TestMethod]
        public void EndOfTheMonth_EndOfMonth_ReturnsEnd()
        {
            Assert.AreEqual(new DateTime(2017, 12, 31, 23, 59, 59),
                new DateTime(2017, 12, 31, 23, 59, 59).EndOfTheMonth());
        }

        [TestMethod]
        public void ParseStandard_OkString_ReturnsRightDt()
        {
            Assert.AreEqual(new DateTime(2017, 5, 23, 12, 34, 00), "2017.05.23 12:34".ParseDtFromStandardString());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseStandard_LongString_ReturnsException()
        {
            Assert.AreEqual(new DateTime(2017, 5, 23, 12, 34, 00),
                "2017.05.23 12:34:20.23".ParseDtFromStandardString());
        }

        [TestMethod]
        public void StartOfTheWeek_MiddleOfTheWeek_ReturnsStart()
        {
            Assert.AreEqual(new DateTime(2017, 10, 16), new DateTime(2017, 10, 18).StartOfTheWeek());
        }

        [TestMethod]
        public void StartOfTheWeek_StartOfTheWeek_ReturnsSame()
        {
            Assert.AreEqual(new DateTime(2017, 10, 16), new DateTime(2017, 10, 16).StartOfTheWeek());
        }

        [TestMethod]
        public void EndOfTheWeek_MiddleOfTheWeek_ReturnsEnd()
        {
            Assert.AreEqual(new DateTime(2017, 10, 15, 23, 59, 59), new DateTime(2017, 10, 13).EndOfTheWeek());
        }

        [TestMethod]
        public void EndOfTheWeek_EndOfTheWeek_ReturnsEnd()
        {
            Assert.AreEqual(new DateTime(2017, 10, 15, 23, 59, 59),
                new DateTime(2017, 10, 15, 23, 59, 59).EndOfTheWeek());
        }
    }
}