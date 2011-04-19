using System.Collections;

namespace System.ComponentModel
{
    public interface INotifyDataErrorInfo
    {
        // Events
        event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        // Methods
        IEnumerable GetErrors(string propertyName);

        // Properties
        bool HasErrors { get; }
    }
}
