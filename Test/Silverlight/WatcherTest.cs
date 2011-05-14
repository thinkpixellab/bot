using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;

namespace PixelLab.Test.Common
{
    [TestClass]
    public class WatcherTest
    {
        [TestMethod]
        public void ComplexTest()
        {
            var wtc = new WatchTestClass(2, 0);
            var watcher = new PathWatcher<WatchTestClass, int>(wtc, w => w.Child.Child.Value);

            Assert.AreEqual(0, watcher.Value);
            wtc.Update(42);
            Assert.AreEqual(42, watcher.Value);

            int i = 0;
            int rnd = int.MinValue;
            int changeCount = 10;
            int eventCount = 0;

            watcher.ValueChanged += (sender, args) =>
            {
                eventCount++;
                Assert.AreEqual(watcher.Value, rnd);
            };

            for (i = 0; i < changeCount; i++)
            {
                rnd = Util.Rnd.Next();
                wtc.Update(rnd);
                Assert.AreEqual(rnd, watcher.Value);
            }

            Assert.AreEqual(eventCount, changeCount);

            WatchTestClass.Check();
        }

        public class WatchTestClass : INotifyPropertyChanged
        {
            public WatchTestClass(int depth, int value)
            {
                m_depth = depth;
                if (depth > 0)
                {
                    m_child = new WatchTestClass(depth - 1, value);
                }
                else
                {
                    m_value = value;
                }
                s_instances.Add(this);
            }

            public WatchTestClass Child
            {
                get
                {
                    if (m_child == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return m_child;
                }
                private set
                {
                    Debug.Assert(value != null);
                    m_child = value;
                    OnPropertyChanged("Child");
                }
            }

            public int Value
            {
                get { return m_value.Value; }
                private set
                {
                    if (value != m_value)
                    {
                        m_value = value;
                        OnPropertyChanged("Value");
                    }
                }
            }

            public void Update(int value)
            {
                Debug.Assert(!m_dead);
                if (m_depth > 0)
                {
                    if (Util.Rnd.Next() % 2 == 0)
                    {
                        Child.Kill();
                        Child = new WatchTestClass(m_depth - 1, value);
                    }
                    else
                    {
                        Child.Update(value);
                    }
                }
                else
                {
                    Value = value;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add
                {
                    Debug.Assert(!m_dead);
                    Debug.Assert(!m_locked);
                    m_handlers.Add(value);
                }
                remove
                {
                    var success = m_handlers.Remove(value);
                    Debug.Assert(success);
                }
            }

            private void OnPropertyChanged(string propertyName)
            {
                try
                {
                    m_locked = true;
                    m_handlers.ForEach(pceh => pceh(this, new PropertyChangedEventArgs(propertyName)));
                }
                finally
                {
                    m_locked = false;
                }
            }

            private void Kill()
            {
                Debug.Assert(!m_dead);
                if (m_depth == 0)
                {
                    Debug.Assert(m_child == null);
                    m_value = null;
                }
                else
                {
                    Debug.Assert(!m_value.HasValue);
                    m_child.Kill();
                    m_child = null;
                }
                m_dead = true;
            }

            private WatchTestClass m_child;
            private int? m_value;
            private bool m_dead;
            private bool m_locked;

            private readonly List<PropertyChangedEventHandler> m_handlers = new List<PropertyChangedEventHandler>();
            private readonly int m_depth;

            public static void Check()
            {
                s_instances.Where(wtc => wtc.m_dead).ForEach(wtc =>
                {
                    Debug.Assert(wtc.m_handlers.IsEmpty());
                });
            }

            private static List<WatchTestClass> s_instances = new List<WatchTestClass>();
        }
    }
}
