using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    public class DataErrorHelper : INotifyDataErrorInfo
    {
        private readonly Dictionary<string, ReadOnlyCollection<string>> _errors = new Dictionary<string, ReadOnlyCollection<string>>();

        public void SetErrors(string property, params string[] errors)
        {
            Contract.Requires(!property.IsNullOrWhiteSpace());
            Contract.Requires(errors != null);
            if (errors.Length > 0)
            {
                _errors[property] = errors.ToReadOnlyCollection();
                OnErrorsChanged(property);
            }
            else
            {
                ClearErrors(property);
            }
        }

        public void ClearErrors(string property)
        {
            Contract.Requires(!property.IsNullOrWhiteSpace());
            if (_errors.Remove(property))
            {
                OnErrorsChanged(property);
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable<string> GetErrors(string propertyName)
        {
            ReadOnlyCollection<string> errors;
            if (_errors.TryGetValue(propertyName, out errors))
            {
                return errors;
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public bool HasErrors
        {
            get { return this._errors.SelectMany(kvp => kvp.Value).Any(); }
        }

        protected void OnErrorsChanged(string propertyName)
        {
            this.OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
        }

        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs args)
        {
            var handler = ErrorsChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return this.GetErrors(propertyName);
        }
    }
}
