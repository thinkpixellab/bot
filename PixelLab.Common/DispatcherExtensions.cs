using System;
using System.Diagnostics.Contracts;
using System.Windows.Threading;

namespace PixelLab.Common
{
    public static class DispatcherExtensions
    {
        public static IAsyncResult Invoke<TResult>(this Dispatcher dispatcher, Func<AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            return beginFunction(asyncResult =>
            {
                dispatcher.InvokeHandleCatch(asyncResult, endFunction, handler, exceptionHandler);
            }, null);
        }

        public static IAsyncResult Invoke<T, TResult>(this Dispatcher dispatcher, T param1, Func<T, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            return beginFunction(param1, asyncResult =>
            {
                dispatcher.InvokeHandleCatch(asyncResult, endFunction, handler, exceptionHandler);
            }, null);
        }

        // TODO: add logic for 'critical' exception - out of memory, stack overflow, etc
        private static void InvokeHandleCatch<TResult>(this Dispatcher dispatcher, IAsyncResult asyncResult, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            dispatcher.Invoke(() =>
            {
                var success = false;
                TResult output = default(TResult);
                try
                {
                    output = endFunction(asyncResult);
                    success = true;
                }
                catch (Exception ex)
                {
                    exceptionHandler(ex);
                }
                // THIS IS IMPORTANT
                // If an error is throw in the handler, it should *not* be bubbled to the exceptionHandler
                // This is why success is used instead of just calling the handler after endFunction()
                if (success)
                {
                    handler(output);
                }
            });
        }

        private static void Invoke(this Dispatcher dispatcher, Action action)
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
