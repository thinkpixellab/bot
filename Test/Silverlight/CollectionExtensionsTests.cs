using System;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test
{
    [TestClass]
    public class CollectionExtensionsTests : SilverlightTest
    {
        [TestMethod]
        public void TrueForAllAdjacentPairs()
        {
            var sortComparer = new Func<int, int, bool>((a, b) => a <= b);

            Assert.IsTrue(Enumerable.Range(0, 0).ToList().TrueForAllAdjacentPairs(sortComparer), "Always true for a list of zero items");
            Assert.IsTrue(Enumerable.Range(0, 1).ToList().TrueForAllAdjacentPairs(sortComparer), "Always true for a list of one item");
            Assert.IsTrue(Enumerable.Range(0, 2).ToList().TrueForAllAdjacentPairs(sortComparer), "True for a trivial list");
            Assert.IsTrue(Enumerable.Range(0, 100).ToList().TrueForAllAdjacentPairs(sortComparer), "True for a big list");

            for (int i = 0; i < 10; i++)
            {
                var list = Enumerable.Range(0, 10).ToList();
                var startIndex = Util.Rnd.Next(1, list.Count);
                var endIndex = Util.Rnd.Next(0, startIndex);

                var item = list[startIndex];
                list.RemoveAt(startIndex);
                list.Insert(endIndex, item);

                Assert.IsFalse(list.TrueForAllAdjacentPairs(sortComparer), "False for an out-of-order list");
            }
        }
    }
}
