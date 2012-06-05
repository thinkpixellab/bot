using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test
{
    [TestClass]
    public class CollectionExtensionsTests : SilverlightTest
    {
        private IDictionary<Type, IList<Type>> _typeMap;

        [TestMethod]
        public void TestTrueForAllAdjacentPairs()
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

        [TestMethod]
        public void TestSelectRecursive()
        {
            printType(typeof(object));

            // there should be at least 

            var depthFirstTraversal = GetTypeChildren().SelectRecursive((type) => GetTypeChildren(type)).ToList();
            Debug.WriteLine(depthFirstTraversal.Count);
        }

        private void printType(Type type, int depth = 0)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                builder.Append("  ");
            }
            builder.Append(type.Name);
            Debug.WriteLine(builder.ToString());
            foreach (var child in GetTypeChildren(type))
            {
                printType(child, depth + 1);
            }
        }

        private IEnumerable<Type> GetTypeChildren(Type type = null)
        {
            if (type == null)
            {
                yield return typeof(object);
            }
            else
            {
                IList<Type> types;
                if (TypeMap.TryGetValue(type, out types))
                {
                    foreach (var item in types.OrderBy(t => t.Name))
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        private IDictionary<Type, IList<Type>> TypeMap
        {
            get
            {
                Deployment.Current.VerifyAccess();
                if (_typeMap == null)
                {
                    _typeMap = new Dictionary<Type, IList<Type>>();
                    foreach (var type in typeof(CollectionExtensionsTests).Assembly.GetTypes())
                    {
                        PopulateTypeMap(_typeMap, type);
                    }
                }

                return _typeMap;
            }
        }

        private static void PopulateTypeMap(IDictionary<Type, IList<Type>> map, Type type, Type child = null)
        {
            Util.ThrowUnless(map != null);

            if (type != null)
            {
                if (!map.ContainsKey(type))
                {
                    map[type] = new List<Type>();
                    var parent = type.BaseType;
                    PopulateTypeMap(map, parent, type);
                }
                Debug.Assert(!map[type].Contains(child));
                if (child != null)
                {
                    map[type].Add(child);
                }
            }
        }
    }
}
