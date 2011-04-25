using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.SL
{
    public abstract class AsyncValueBase<T> : Changeable, IAsyncValue<T>
    {
        private readonly DelegateCommand _loadCommand;
        private LoadState _state;
        private IAsyncResult _loadingResult;
        private T _value;
        private bool _doingLoad;

        public AsyncValueBase()
        {
            _state = LoadState.Unloaded;
            _doingLoad = false;
            _loadCommand = new DelegateCommand(Load, () => State != LoadState.Loading);
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
                _loadCommand.RaiseCanExecuteChanged();
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
                internalValueSet(value);
            }
        }

        public ICommand LoadCommand { get { return _loadCommand; } }

        public event EventHandler ValueLoaded;

        public event EventHandler<UnhandledExceptionEventArgs> LoadError;

        public void Load()
        {
            Deployment.Current.VerifyAccess();
            Debug.Assert(!DoingLoad);

            try
            {
                DoingLoad = true;
                State = LoadState.Loading;
                var asyncResult = LoadCore();
                Debug.Assert(asyncResult != null, "The provided delegate should always return a non-null IAsnycResult");
                if (State == LoadState.Loading)
                {
                    // load did not do anything syncronously. Cool. Let's store the result
                    Debug.Assert(!asyncResult.IsCompleted);
                    _loadingResult = asyncResult;
                }
                else
                {
                    Debug.Assert(asyncResult.IsCompleted);
                    Debug.Assert(asyncResult.CompletedSynchronously);
                }
            }
            finally
            {
                DoingLoad = false;
            }
        }

        protected bool DoingLoad
        {
            get
            {
                Deployment.Current.VerifyAccess();
                return _doingLoad;
            }
            private set
            {
                Deployment.Current.VerifyAccess();
                _doingLoad = value;
            }
        }

        protected abstract IAsyncResult LoadCore();

        protected void LoadSuccessCallback(T value)
        {
            Contract.Requires(State == LoadState.Loading);
            Deployment.Current.VerifyAccess();
            if (DoingLoad)
            {
                Debug.Assert(_loadingResult == null);
            }
            else
            {
                Debug.Assert(_loadingResult != null);
                Debug.Assert(!_loadingResult.CompletedSynchronously);
                Debug.Assert(_loadingResult.IsCompleted);
                _loadingResult = null;
            }
            internalValueSet(value);
        }

        protected void LoadFailCallback(Exception exception)
        {
            Contract.Requires(State == LoadState.Loading);
            Deployment.Current.VerifyAccess();

            if (DoingLoad) // result came back within the Load call...
            {
                Debug.Assert(_loadingResult == null);
            }
            else
            {
                Debug.Assert(_loadingResult != null);
                Debug.Assert(!_loadingResult.CompletedSynchronously);
                Debug.Assert(_loadingResult.IsCompleted);
                _loadingResult = null;
            }
            _value = default(T);
            State = LoadState.Error;
            OnPropertyChanged("Value");

            var handler = LoadError;
            if (handler != null)
            {
                handler(this, new UnhandledExceptionEventArgs(exception, false));
            }
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
