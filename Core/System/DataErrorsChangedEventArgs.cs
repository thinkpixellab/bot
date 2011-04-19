namespace System.ComponentModel
{
    public sealed class DataErrorsChangedEventArgs : EventArgs
    {
        // Methods
        public DataErrorsChangedEventArgs(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        // Properties
        public string PropertyName { get; private set; }
    }
}
