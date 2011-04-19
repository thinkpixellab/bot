using System;
using System.Collections.Generic;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    public static class SortHelper
    {
        /*
        public static int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
          int num;
          try {
            if (comparer == null) {
              comparer = Comparer<T>.Default;
            }
            num = InternalBinarySearch(array, index, length, value, comparer);
          }
          catch (Exception exception) {
            throw new InvalidOperationException("InvalidOperation_IComparerFailed", exception);
          }
          return num;
        }

        internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
          int num = index;
          int num2 = (index + length) - 1;
          while (num <= num2) {
            int num3 = num + ((num2 - num) >> 1);
            int num4 = comparer.Compare(array[num3], value);
            if (num4 == 0) {
              return num3;
            }
            if (num4 < 0) {
              num = num3 + 1;
            }
            else {
              num2 = num3 - 1;
            }
          }
          return ~num;
        }*/

        public static bool QuickSort<T>(this IList<T> list, Func<T, T, int> comparer)
        {
            Contract.Requires(list != null);
            Contract.Requires(comparer != null);
            return Sort(list, 0, list.Count, comparer.ToComparer());
        }

        public static bool QuickSort<T>(this IList<T> list, IComparer<T> comparer)
        {
            Contract.Requires(list != null);
            Contract.Requires(comparer != null);
            return Sort(list, 0, list.Count, comparer);
        }

        public static bool QuickSort<T>(this IList<T> list, Comparison<T> comparison)
        {
            Contract.Requires(list != null);
            Contract.Requires(comparison != null);
            return Sort(list, 0, list.Count, comparison.ToComparer());
        }

        public static bool QuickSort<T>(this IList<T> list)
        {
            Contract.Requires(list != null);
            return Sort(list, 0, list.Count, null);
        }

        private static bool Sort<T>(IList<T> keys, int index, int length, IComparer<T> comparer)
        {
            Contract.Requires(comparer != null);
            Contract.Requires(keys != null);

            if (length > 1)
            {
                try
                {
                    if (comparer == null)
                    {
                        comparer = Comparer<T>.Default;
                    }
                    return quickSort(keys, index, index + (length - 1), comparer);
                }
                catch (IndexOutOfRangeException ioore)
                {
                    throw new ArgumentException("BogusIComparer", ioore);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("IComparerFailed", exception);
                }
            }
            return false;
        }

        private static bool quickSort<T>(IList<T> keys, int left, int right, IComparer<T> comparer)
        {
            Contract.Requires(comparer != null);
            Contract.Requires(keys != null);
            Contract.Requires(left >= 0);
            Contract.Requires(left < keys.Count);
            Contract.Requires(right >= 0);
            Contract.Requires(right < keys.Count);

            bool change = false;
            do
            {
                int a = left;
                int b = right;
                int num3 = a + ((b - a) >> 1);
                change = swapIfGreaterWithItems(keys, comparer, a, num3) || change;
                change = swapIfGreaterWithItems(keys, comparer, a, b) || change;
                change = swapIfGreaterWithItems(keys, comparer, num3, b) || change;
                T y = keys[num3];
                do
                {
                    while (comparer.Compare(keys[a], y) < 0)
                    {
                        a++;
                    }
                    while (comparer.Compare(y, keys[b]) < 0)
                    {
                        b--;
                    }
                    if (a > b)
                    {
                        break;
                    }
                    if (a < b)
                    {
                        T local2 = keys[a];
                        keys[a] = keys[b];
                        keys[b] = local2;
                        change = true;
                    }
                    a++;
                    b--;
                }
                while (a <= b);
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        change = quickSort(keys, left, b, comparer) || change;
                    }
                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        change = quickSort(keys, a, right, comparer) || change;
                    }
                    right = b;
                }
            }
            while (left < right);

            return change;
        }

        private static bool swapIfGreaterWithItems<T>(IList<T> keys, IComparer<T> comparer, int a, int b)
        {
            Contract.Requires(comparer != null);
            Contract.Requires(keys != null);
            Contract.Requires(a >= 0);
            Contract.Requires(a < keys.Count);
            Contract.Requires(a >= 0);
            Contract.Requires(b < keys.Count);

            if ((a != b) && (comparer.Compare(keys[a], keys[b]) > 0))
            {
                T local = keys[a];
                keys[a] = keys[b];
                keys[b] = local;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
