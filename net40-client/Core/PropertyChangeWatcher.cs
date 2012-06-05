#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace PixelLab.Common
{
    /// <remarks>In order to be 'safe' this method uses reflection on every 'watch' to ensure the property exists.
    /// In order to be thread safe, this class uses lock. (Sadly, read/write locks don't exist in Silverlight.)
    /// So be careful using this in preformance-critical contexts.
    /// </remarks>
    public sealed class PropertyChangeWatcher
    {
        private readonly INotifyPropertyChanged _owner;
        private readonly Dictionary<string, Action> _handlers = new Dictionary<string, Action>();

        public Type OwnerType { get; private set; }

        private PropertyChangeWatcher(INotifyPropertyChanged owner)
        {
            Contract.Requires(owner != null);
            _owner = owner;
            OwnerType = _owner.GetType();
        }

        public PropertyChangeWatcher AddWatcher(string propertyName, Action handler)
        {
            return AddWatcher(new[] { propertyName }, handler);
        }

        public PropertyChangeWatcher AddWatcher(string propertyName1, string propertyName2, Action handler)
        {
            return AddWatcher(new[] { propertyName1, propertyName2 }, handler);
        }

        public PropertyChangeWatcher AddWatcher(IList<string> propertyNames, Action handler)
        {
            Contract.Requires(handler != null);
            Contract.Requires(propertyNames != null);
            Contract.Requires(propertyNames.Count > 0);
            Contract.Requires(propertyNames.AllUnique());
            Util.ThrowUnless(propertyNames.All(name => OwnerType.HasPublicInstanceProperty(name)), "The target object does not contain one or more of the properties provided");

            lock (_handlers)
            {
                foreach (var key in propertyNames)
                {
                    Util.ThrowUnless<ArgumentException>(!IsWatching(key), "Must not already be watching property '{0}'".DoFormat(key));
                }

                if (_handlers.Count == 0)
                {
                    _owner.PropertyChanged += _owner_PropertyChanged;
                }

                foreach (var propertyName in propertyNames)
                {
                    _handlers[propertyName] = handler;
                }
                return this;
            }
        }

        public bool IsWatching(string property)
        {
            lock (_handlers)
            {
                return _handlers.ContainsKey(property);
            }
        }

        public void StopWatching(string property)
        {
            lock (_handlers)
            {
                Util.ThrowUnless(IsWatching(property));
                _handlers.Remove(property);
                if (_handlers.Count == 0)
                {
                    _owner.PropertyChanged -= _owner_PropertyChanged;
                }
            }
        }

        public void StopWatchingAll()
        {
            lock (_handlers)
            {
                _handlers.Clear();
                _owner.PropertyChanged -= _owner_PropertyChanged;
            }
        }

        public static PropertyChangeWatcher AddWatcher(INotifyPropertyChanged source, IList<string> propertyNames, Action handler)
        {
            return (new PropertyChangeWatcher(source)).AddWatcher(propertyNames, handler);
        }

        private void _owner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Action handler;
            lock (_handlers)
            {
                if (_handlers.TryGetValue(e.PropertyName, out handler))
                {
                    handler();
                }
            }
        }
    }
}
