using System;
using System.ComponentModel;
using System.Windows.Input;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.SL
{
    [ContractClass(typeof(IAsyncValueContract<>))]
    public interface IAsyncValue<T> : INotifyPropertyChanged
    {
        LoadState State { get; }
        T Value { get; set; }
        void Load();
        event EventHandler ValueLoaded;
        ICommand LoadCommand { get; }
        event EventHandler<UnhandledExceptionEventArgs> LoadError;
    }

    [ContractClassFor(typeof(IAsyncValue<>))]
    internal abstract class IAsyncValueContract<T> : IAsyncValue<T>
    {
        public LoadState State
        {
            get { throw new NotImplementedException(); }
        }

        public T Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                Contract.Requires(State != LoadState.Loading);
                throw new NotImplementedException();
            }
        }

        public void Load()
        {
            Contract.Requires(State != LoadState.Loading);
            throw new NotImplementedException();
        }

        public ICommand LoadCommand
        {
            get
            {
                Contract.Ensures(Contract.Result<ICommand>() != null);
                return default(ICommand);
            }
        }

        event EventHandler IAsyncValue<T>.ValueLoaded
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler<UnhandledExceptionEventArgs> IAsyncValue<T>.LoadError
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
    }
}
