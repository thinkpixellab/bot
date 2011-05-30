using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test.Common
{
    [TestClass]
    public class WeakEnumerableTest : SilverlightTest
    {
        [TestMethod, Asynchronous]
        public void TestWeak()
        {
            EnqueueCallback(() =>
            {
                for (int i = 0; i < c_count; i++)
                {
                    m_weirds.Add(new WeirdThing());
                }

                Assert.AreEqual(m_weirds.Count, c_count);
                Assert.AreEqual(m_weirds.Count, WeirdThing.Count);
            });

            EnqueueCallback(GC.Collect);

            EnqueueCallback(() =>
            {
                Assert.AreEqual(c_count, m_weirds.Count);
                Assert.AreEqual(c_count, WeirdThing.Count);
                Assert.AreEqual(m_weirdEnum.Count(), 0);
            });

            EnqueueCallback(() =>
            {
                for (var i = 0; i < m_weirds.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        m_weirdEnum.Add(m_weirds[i]);
                    }
                    else
                    {
                        m_weirdEnum.Insert(m_weirds[i]);
                    }
                }
            });

            EnqueueCallback(GC.Collect);

            EnqueueCallback(() =>
            {
                Assert.AreEqual(c_count, m_weirds.Count);
                Assert.AreEqual(c_count, WeirdThing.Count);
                Assert.AreEqual(c_count, m_weirdEnum.Count());
            });

            EnqueueCallback(() =>
            {
                for (int i = 0; i < half; i++)
                {
                    m_weirds.Remove(m_weirds.Random());
                }
            });

            EnqueueCallback(GC.Collect);

            EnqueueCallback(() =>
            {
                Assert.AreEqual(half, m_weirds.Count);
                Assert.AreEqual(half, WeirdThing.Count);
                Assert.AreEqual(half, m_weirdEnum.Count());
                Assert.AreEqual(half, m_weirds.Intersect(m_weirdEnum).Count());
            });

            EnqueueTestComplete();
        }

        private List<WeirdThing> m_weirds = new List<WeirdThing>();
        private WeakEnumerable<WeirdThing> m_weirdEnum = new WeakEnumerable<WeirdThing>();

        private static int half { get { return c_count / 2; } }

        private const int c_count = 10;

        private class WeirdThing
        {
            public WeirdThing()
            {
                m_id = Interlocked.Increment(ref s_count) - 1;
            }

            ~WeirdThing()
            {
                Interlocked.Decrement(ref s_count);
            }

            private readonly int m_id;

            public static int Count { get { return s_count; } }

            private static int s_count = 0;
        }
    }
}