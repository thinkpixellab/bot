using System.ComponentModel;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Collections.Generic;

namespace PixelLab.Common
{
    public abstract class Changeable : INotifyPropertyChanged
    {
#if DEBUG
        public Changeable()
        {
            this.VerifyPropertyNamesOnChange();
        }
#endif

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            Contract.Requires(args != null);
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            Contract.Requires(!propertyName.IsNullOrWhiteSpace());
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper method for raising the PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has changed.</typeparam>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <param name="propertyValue">The original value of the property that has changed.</param>
        /// <param name="value">The new value of the property that has changed.</param>
        /// <returns><c>True</c> if the property was update; otherwise <c>false</c>.</returns>
        protected virtual bool UpdateProperty<T>(
            string propertyName,
            ref T propertyValue,
            T value)
        {
            if (EqualityComparer<T>.Default.Equals(propertyValue, value))
            {
                return false;
            }
            else
            {
                propertyValue = value;
                this.OnPropertyChanged(propertyName);
                return true;
            }
        }
    }
}
