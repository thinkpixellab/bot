using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PixelLab.Contracts;

namespace PixelLab.Common
{
    public class SortedObservableEnumerable<TItem, TSource> : Changeable, IEnumerable<TItem>, INotifyCollectionChanged where TSource : class, IEnumerable<TItem>, INotifyCollectionChanged
    {
        private readonly TSource _source;
        private IComparer<TItem> _comparer;

        public SortedObservableEnumerable(TSource source, IComparer<TItem> comparer = null)
        {
            Contract.Requires(source != null);
            _source = source;
            _comparer = comparer;
            _source.CollectionChanged += (sender, args) =>
            {
                OnCollectionChanged();
            };
        }

        public IComparer<TItem> Comparer
        {
            get { return _comparer; }
            set
            {
                if (UpdateProperty("Comparer", ref _comparer, value))
                {
                    OnCollectionChanged();
                }
            }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            if (_comparer == null)
            {
                return _source.GetEnumerator();
            }
            else
            {
                return _source.OrderBy(a => a, _comparer).GetEnumerator();
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
