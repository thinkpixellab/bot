using System;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.Common;
using PixelLab.Test.Helpers;

namespace PixelLab.Test
{
    [TestClass]
    public class PropertyWatcherTest : SilverlightTest
    {
        [TestMethod]
        public void TestWatch()
        {
            var changed = false;

            var testInstance = new TestChangeable();

            var watcher = testInstance.AddWatcher("Foo", () => changed = true);

            Assert.IsTrue(watcher.IsWatching("Foo"));

            Assert.IsFalse(changed);
            testInstance.Foo = 1;
            Assert.IsTrue(changed);

            // test dupe watcher not allowed...
            AssertPlus.ExceptionThrown<ArgumentException>(() =>
            {
                watcher.AddWatcher("Foo", () => { });
            });

            // Clear out the foo watch and try again
            watcher.StopWatching("Foo");

            changed = false;
            watcher.AddWatcher("Foo", () => changed = true);
            testInstance.Foo = 5;

            Assert.IsTrue(changed);

            watcher.StopWatchingAll();

            Assert.IsFalse(watcher.IsWatching("Foo"));

            int changeCount = 0;
            watcher.AddWatcher(new[] { "Foo", "Bar", "Baz" }, () => { changeCount++; });
            Assert.IsTrue(watcher.IsWatching("Foo"));
            Assert.IsTrue(watcher.IsWatching("Bar"));
            Assert.IsTrue(watcher.IsWatching("Baz"));

            testInstance.Foo = 10;
            testInstance.Bar = 11;
            testInstance.Baz = 12;

            Assert.AreEqual(3, changeCount);

            watcher.StopWatchingAll();

            Assert.IsFalse(watcher.IsWatching("Foo"));
            Assert.IsFalse(watcher.IsWatching("Bar"));
            Assert.IsFalse(watcher.IsWatching("Baz"));
        }
    }
}
