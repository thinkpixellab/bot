using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;
using PixelLab.Test.Helpers;

namespace PixelLab.Test.SL
{
    [TestClass]
    public class ListReorderUtilTests
    {
        [TestMethod]
        public void TestListReorderUtil()
        {
            IList<int> result;

            int[] indicies = new int[] { };

            Assert.IsFalse(ListReorderUtil.CanReorder(indicies, 0, ReorderDirection.Beginning));
            Assert.IsFalse(ListReorderUtil.CanReorder(indicies, 0, ReorderDirection.Beginning));
            Assert.IsFalse(ListReorderUtil.CanReorder(indicies, 10, ReorderDirection.End));
            Assert.IsFalse(ListReorderUtil.CanReorder(indicies, 10, ReorderDirection.Beginning));

            // last item in a list 6 long, move it all the way to the beginning
            result = indicies = new int[] { 5 };
            AssertPlus.ExceptionThrown<Exception>(() => ListReorderUtil.CanReorder(indicies, 5, ReorderDirection.Beginning));
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(ListReorderUtil.CanReorder(result, 6, ReorderDirection.Beginning));
                result = ListReorderUtil.Reorder(result, 6, ReorderDirection.Beginning);
                Assert.AreEqual(4 - i, result[0]);
            }
            Assert.AreEqual(0, result[0]);
            Assert.IsFalse(ListReorderUtil.CanReorder(result, 6, ReorderDirection.Beginning));

            // now move it all the way back to the end
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(ListReorderUtil.CanReorder(result, 6, ReorderDirection.End));
                result = ListReorderUtil.Reorder(result, 6, ReorderDirection.End);
                Assert.AreEqual(1 + i, result[0]);
            }
            Assert.AreEqual(5, result[0]);
            Assert.IsFalse(ListReorderUtil.CanReorder(result, 6, ReorderDirection.End));

            indicies = new int[] { 1, 2 };
            Assert.IsTrue(ListReorderUtil.CanReorder(indicies, 3, ReorderDirection.Beginning));
            Assert.IsFalse(ListReorderUtil.CanReorder(indicies, 3, ReorderDirection.End));

            for (int i = 3; i < 5; i++)
            {
                result = ListReorderUtil.Reorder(indicies, i, ReorderDirection.Beginning);
                Assert.AreEqual(result[0], 0);
                Assert.AreEqual(result[1], 1);
            }

            result = ListReorderUtil.Reorder(indicies, 4, ReorderDirection.End);
            Assert.AreEqual(result[0], 2);
            Assert.AreEqual(result[1], 3);
        }

        [TestMethod]
        public void TestUnpacked()
        {
            int[] indicies = new int[] { 1, 3 };

            Assert.IsTrue(ListReorderUtil.CanReorder(indicies, 4, ReorderDirection.Beginning));
            IList<int> result = ListReorderUtil.Reorder(indicies, 4, ReorderDirection.Beginning);
            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[1], 2);

            indicies = new int[] { 2, 4, 6 };

            Assert.IsTrue(ListReorderUtil.CanReorder(indicies, 7, ReorderDirection.Beginning));
            result = ListReorderUtil.Reorder(indicies, 7, ReorderDirection.Beginning);
            Assert.IsTrue(result.SequenceEqual(new int[] { 2, 4, 5 }));

            Assert.IsTrue(ListReorderUtil.CanReorder(result, 7, ReorderDirection.End));
            result = ListReorderUtil.Reorder(result, 7, ReorderDirection.End);
            Assert.IsTrue(result.SequenceEqual(new int[] { 3, 4, 5 }));

            Assert.IsTrue(ListReorderUtil.CanReorder(result, 7, ReorderDirection.Beginning));
            result = ListReorderUtil.Reorder(result, 7, ReorderDirection.Beginning);
            Assert.IsTrue(result.SequenceEqual(new int[] { 2, 3, 4 }));
        }
    }
}
