using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Threading;

namespace PixelLab.Common
{
    public static class DispatcherExtensions
    {
        public static IAsyncResult Invoke<TResult>(this SynchronizationContext syncContext, Func<AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            return beginFunction(asyncResult =>
            {
                InvokeHandleCatch(syncContext, asyncResult, endFunction, handler, exceptionHandler);
            }, null);
        }

        public static IAsyncResult Invoke<T, TResult>(this SynchronizationContext syncContext, T param1, Func<T, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            return beginFunction(param1, asyncResult =>
            {
                InvokeHandleCatch(syncContext, asyncResult, endFunction, handler, exceptionHandler);
            }, null);
        }

        public static IAsyncResult Invoke<T1, T2, TResult>(this SynchronizationContext syncContext, T1 param1, T2 param2, Func<T1, T2, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            return beginFunction(param1, param2, asyncResult =>
            {
                InvokeHandleCatch(syncContext, asyncResult, endFunction, handler, exceptionHandler);
            }, null);
        }

        private static void InvokeHandleCatch<TResult>(SynchronizationContext synchronizationContext, IAsyncResult asyncResult, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            Contract.Requires(synchronizationContext != null);
            Contract.Requires(endFunction != null);
            Contract.Requires(handler != null);
            Contract.Requires(exceptionHandler != null);

            synchronizationContext.Send((state) =>
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
                    if (ex.IsCriticalException())
                    {
                        throw;
                    }
                    exceptionHandler(ex);
                }
                // THIS IS IMPORTANT
                // If an error is throw in the handler, it should *not* be bubbled to the exceptionHandler
                // This is why success is used instead of just calling the handler after endFunction()
                if (success)
                {
                    handler(output);
                }
            }, null);
        }
    }
}
