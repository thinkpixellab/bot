using System;
using System.Windows.Threading;
using System.Diagnostics.Contracts;

namespace PixelLab.Common
{
    public static class DispatcherExtensions
    {
        public static void Invoke<TResult>(this Dispatcher dispatcher, Func<AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler)
        {
            beginFunction(asyncResult =>
            {
                dispatcher.Invoke(() =>
                {
                    var output = endFunction(asyncResult);
                    handler(output);
                });
            }, null);
        }

        public static void Invoke<T, TResult>(this Dispatcher dispatcher, T param1, Func<T, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler)
        {
            beginFunction(param1, asyncResult =>
            {
                dispatcher.Invoke(() =>
                {
                    var output = endFunction(asyncResult);
                    handler(output);
                });
            }, null);
        }

        public static void Invoke(this Dispatcher dispatcher, Action action)
        {
            Contract.Requires(dispatcher != null);
            Contract.Requires(action != null);

            var context = new DispatcherSynchronizationContext(dispatcher);
            context.Send(state =>
            {
                action();
            }, null);
        }
    }
}
