using System.ComponentModel;
using System.Diagnostics.Contracts;

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
    }
}
