using System;
using System.ComponentModel;
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
    }

    [ContractClassFor(typeof(IAsyncValue<>))]
    internal abstract class IAsyncValueContract<T> : IAsyncValue<T>
    {
        #region IAsyncValue<T> Members

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

        #endregion

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
    }
}
