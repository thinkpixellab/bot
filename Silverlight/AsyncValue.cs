using System;
using System.Diagnostics;
using System.Windows;
using PixelLab.Common;
using System.ComponentModel;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.SL
{
    public interface IAsyncValue<T> : INotifyPropertyChanged
    {
        LoadState State { get; }
        T Value { get; set; }
        void Load();
        event EventHandler ValueLoaded;
    }

    public abstract class AsyncValueBase<T> : Changeable, IAsyncValue<T>
    {
        private LoadState _state;
        private IAsyncResult _loadingResult;
        private T _value;

        public AsyncValueBase()
        {
            _state = LoadState.Unloaded;
        }

        public LoadState State
        {
            get
            {
                Deployment.Current.VerifyAccess();
                return _state;
            }
            private set
            {
                Deployment.Current.VerifyAccess();
                _state = value;
                OnPropertyChanged("State");
            }
        }

        public T Value
        {
            get
            {
                Deployment.Current.VerifyAccess();
                return _value;
            }
            set
            {
                Contract.Requires(State != LoadState.Loading);
                internalValueSet(value);
            }
        }

        public event EventHandler ValueLoaded;

        public void Load()
        {
            Contract.Requires(State == LoadState.Unloaded);
            Deployment.Current.VerifyAccess();

            _loadingResult = LoadCore();

            State = LoadState.Loading;
        }

        protected abstract IAsyncResult LoadCore();

        protected void LoadSuccessCallback(T value)
        {
            Contract.Requires(State == LoadState.Loading);
            Deployment.Current.VerifyAccess();
            Debug.Assert(_loadingResult != null);
            _loadingResult = null;
            internalValueSet(value);
        }

        protected void LoadFailCallback(Exception exception)
        {
            Contract.Requires(State == LoadState.Loading);
            Deployment.Current.VerifyAccess();
            Debug.Assert(_loadingResult != null);
            _loadingResult = null;
            _value = default(T);
            State = LoadState.Error;
            OnPropertyChanged("Value");
        }

        private void internalValueSet(T value)
        {
            Deployment.Current.VerifyAccess();
            Debug.Assert(_loadingResult == null);
            _value = value;
            State = LoadState.Loaded;
            OnPropertyChanged("Value");

            var handler = ValueLoaded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    public class AsyncValue<TResult, TParam> : AsyncValueBase<TResult>
    {
        private readonly TParam _param;
        private readonly Func<TParam, Action<TResult>, Action<Exception>, IAsyncResult> _callback;

        public AsyncValue(TParam param, Func<TParam, Action<TResult>, Action<Exception>, IAsyncResult> callBack)
        {
            Contract.Requires(callBack != null);
            _param = param;
            _callback = callBack;
        }

        protected override IAsyncResult LoadCore()
        {
            return _callback(_param, LoadSuccessCallback, LoadFailCallback);
        }
    }

    public class AsyncValue<TResult> : AsyncValueBase<TResult>
    {
        private readonly Func<Action<TResult>, Action<Exception>, IAsyncResult> _callback;

        public AsyncValue(Func<Action<TResult>, Action<Exception>, IAsyncResult> callBack)
        {
            Contract.Requires(callBack != null);
            _callback = callBack;
        }

        protected override IAsyncResult LoadCore()
        {
            return _callback(LoadSuccessCallback, LoadFailCallback);
        }
    }
}
