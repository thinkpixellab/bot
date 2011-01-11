using System;
using System.Diagnostics.Contracts;
using System.Windows.Threading;

namespace PixelLab.Common
{
    public static class DispatcherExtensions
    {
        public static IAsyncResult Invoke<TResult>(this Dispatcher dispatcher, Func<AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler)
        {
            return beginFunction(asyncResult =>
            {
                dispatcher.Invoke(() =>
                {
                    var output = endFunction(asyncResult);
                    handler(output);
                });
            }, null);
        }

        public static IAsyncResult Invoke<T, TResult>(this Dispatcher dispatcher, T param1, Func<T, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler)
        {
            return beginFunction(param1, asyncResult =>
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
