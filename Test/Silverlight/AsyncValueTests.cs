using System;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixelLab.SL;

namespace PixelLab.Test.SL
{
    [TestClass]
    public class AsyncValueTests : SilverlightTest
    {
        private static readonly int MagicValue = Environment.TickCount;

        [TestMethod]
        [Asynchronous]
        public void BasicAsync()
        {
            var asyncValue = new AsyncValue<int>(AsyncTestInput);
            Assert.AreEqual(LoadState.Unloaded, asyncValue.State);
            Assert.AreEqual(default(int), asyncValue.Value);

            asyncValue.ValueLoaded += (sender, args) => asyncValue_handleLoad(asyncValue);
            asyncValue.Load();
            Assert.AreEqual(LoadState.Loading, asyncValue.State);
        }

        [TestMethod]
        public void BasicSynchronous()
        {
            var asyncValue = new AsyncValue<int>(SyncTestInput);
            Assert.AreEqual(LoadState.Unloaded, asyncValue.State);
            Assert.AreEqual(default(int), asyncValue.Value);

            var loadHappened = false;
            asyncValue.ValueLoaded += (sender, args) =>
            {
                loadHappened = true;
            };
            asyncValue.Load();
            Assert.IsTrue(loadHappened);
        }

        [TestMethod]
        public void SyncException()
        {
            var asyncValue = new AsyncValue<int>(SyncErrorInput);
            Assert.AreEqual(LoadState.Unloaded, asyncValue.State);
            Assert.AreEqual(default(int), asyncValue.Value);

            var loadHappened = false;
            asyncValue.ValueLoaded += (sender, args) =>
            {
                loadHappened = true;
            };

            var errorHappened = false;
            asyncValue.LoadError += (sender, args) =>
            {
                errorHappened = true;
                args.Handled = true;
            };

            asyncValue.Load();
            Assert.IsFalse(loadHappened);
            Assert.IsTrue(errorHappened);
        }

        // TODO: test error async

        private void asyncValue_handleLoad(IAsyncValue<int> asyncValue)
        {
            EnqueueTestComplete();
            Assert.AreEqual(LoadState.Loaded, asyncValue.State);
            Assert.AreEqual(MagicValue, asyncValue.Value);
        }

        private IDisposable AsyncTestInput(Action<int> resultHandle, Action<Exception> exceptionHandler)
        {
            var result = new DummyAsyncResult();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                resultHandle(MagicValue);
            });
            return result;
        }

        private IDisposable SyncTestInput(Action<int> resultHandler, Action<Exception> exceptionHandler)
        {
            resultHandler(MagicValue);
            var result = new DummyAsyncResult();
            return result;
        }

        private IDisposable SyncErrorInput(Action<int> resultHandler, Action<Exception> exceptionHandler)
        {
            exceptionHandler(new NotSupportedException());
            var result = new DummyAsyncResult();
            return result;
        }
    }

    internal class DummyAsyncResult : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}
