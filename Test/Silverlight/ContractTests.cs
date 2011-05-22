using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Contracts;
using PixelLab.Test.Helpers;

namespace PixelLab.Test.SL
{
    [TestClass]
    public class ContractTests
    {
        [TestMethod]
        public void TestRequireFalse()
        {
            AssertPlus.ExceptionThrown<Exception>(() => Contract.Requires(false));
            var ex = AssertPlus.ExceptionThrown<Exception>(() => Contract.Requires(false, "foo"));
            Assert.AreEqual(ex.Message, "foo");

            AssertPlus.ExceptionThrown<ArgumentException>(() => Contract.Requires<ArgumentException>(false));
            var ax = AssertPlus.ExceptionThrown<ArgumentException>(() => Contract.Requires<ArgumentException>(false, "bar"));
            Assert.AreEqual(ax.Message, "bar");
        }
    }
}