using System;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test
{
    [TestClass]
    public class SortHelperTests : SilverlightTest
    {
        private const int _testCount = 100;

        [TestMethod]
        public void TestComparerFunc()
        {
            var numbers = getTestNumbers();

            SortHelper.QuickSort(numbers, new Func<int, int, int>(reverseSort));

            Assert.IsTrue(numbers.TrueForAllAdjacentPairs((a, b) => a >= b));
        }

        [TestMethod]
        public void TestComparison()
        {
            var numbers = getTestNumbers();

            SortHelper.QuickSort(numbers, new Comparison<int>(reverseSort));

            Assert.IsTrue(numbers.TrueForAllAdjacentPairs((a, b) => a >= b));
        }

        [TestMethod]
        public void TestComparer()
        {
            var numbers = getTestNumbers();

            SortHelper.QuickSort(numbers, Comparer<int>.Default);

            Assert.IsTrue(numbers.TrueForAllAdjacentPairs((a, b) => a <= b));
        }

        [TestMethod]
        public void TestDefault()
        {
            var numbers = getTestNumbers();

            SortHelper.QuickSort(numbers);

            Assert.IsTrue(numbers.TrueForAllAdjacentPairs((a, b) => a <= b));
        }

        private static int reverseSort(int a, int b)
        {
            return b.CompareTo(a);
        }

        private static int[] getTestNumbers()
        {
            var numbers = new int[_testCount];

            for (var i = 0; i < _testCount; i++)
            {
                numbers[i] = Util.Rnd.Next();
            }

            return numbers;
        }
    }
}
