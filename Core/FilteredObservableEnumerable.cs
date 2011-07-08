using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PixelLab.Contracts;

namespace PixelLab.Common
{
    public class FilteredObservableEnumerable<TItem, TSource> : Changeable, IEnumerable<TItem>, INotifyCollectionChanged where TSource : class, IEnumerable<TItem>, INotifyCollectionChanged
    {
        private readonly TSource _source;
        private Func<TItem, bool> _filter;

        public FilteredObservableEnumerable(TSource source, Func<TItem, bool> filter = null)
        {
            Contract.Requires(source != null);
            _source = source;
            _filter = filter;
            _source.CollectionChanged += (sender, args) =>
            {
                OnCollectionChanged();
            };
        }

        public Func<TItem, bool> Filter
        {
            get { return _filter; }
            set
            {
                if (UpdateProperty("Filter", ref _filter, value))
                {
                    OnCollectionChanged();
                }
            }
        }

        /// <summary>
        /// If a Filter is active, raise a collection reset event.
        /// </summary>
        public void ResetFilter()
        {
            if (_filter != null)
            {
                OnCollectionChanged();
            }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            if (_filter == null)
            {
                return _source.GetEnumerator();
            }
            else
            {
                return _source.Where(_filter).GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args = null)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, args ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
