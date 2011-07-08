using System;
using System.Reactive.Linq;
using System.Threading;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    public static class DispatcherExtensions
    {
        public static IDisposable Invoke<TResult>(this SynchronizationContext syncContext, Func<AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            Contract.Requires(syncContext != null);
            Contract.Requires(beginFunction != null);
            Contract.Requires(handler != null);
            Contract.Requires(exceptionHandler != null);

            var func = Observable.FromAsyncPattern<TResult>(beginFunction, endFunction);
            var observable = func();
            return observable.ObserveOn(syncContext).Subscribe(handler, exceptionHandler);
        }

        public static IDisposable Invoke<T, TResult>(this SynchronizationContext syncContext, T param1, Func<T, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            Contract.Requires(syncContext != null);
            Contract.Requires(beginFunction != null);
            Contract.Requires(handler != null);
            Contract.Requires(exceptionHandler != null);

            var func = Observable.FromAsyncPattern<T, TResult>(beginFunction, endFunction);
            var observable = func(param1);
            return observable.ObserveOn(syncContext).Subscribe(handler, exceptionHandler);
        }

        public static IDisposable Invoke<T1, T2, TResult>(this SynchronizationContext syncContext, T1 param1, T2 param2, Func<T1, T2, AsyncCallback, object, IAsyncResult> beginFunction, Func<IAsyncResult, TResult> endFunction, Action<TResult> handler, Action<Exception> exceptionHandler)
        {
            Contract.Requires(syncContext != null);
            Contract.Requires(beginFunction != null);
            Contract.Requires(handler != null);
            Contract.Requires(exceptionHandler != null);

            var func = Observable.FromAsyncPattern<T1, T2, TResult>(beginFunction, endFunction);
            var observable = func(param1, param2);
            return observable.ObserveOn(syncContext).Subscribe(handler, exceptionHandler);
        }
    }
}
