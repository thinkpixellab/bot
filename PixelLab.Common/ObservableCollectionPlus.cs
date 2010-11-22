using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace PixelLab.Common
{
    public class ObservableCollectionPlus<T> : ObservableCollection<T>
    {
        public ObservableCollectionPlus() : this(Enumerable.Empty<T>()) { }
#if WP7
    public ObservableCollectionPlus(IEnumerable<T> collection) {
      m_roCollection = new ReadOnlyObservableCollection<T>(this);
      m_lock = new WrappedLock(BeforeMultiUpdate, unlock);

      // yes, crazy events are fired here. Who cares. No one can be listening. :-)
      collection.ForEach(item => base.Add(item));
    }
#else
        public ObservableCollectionPlus(IEnumerable<T> collection)
            : base(collection)
        {
            m_roCollection = new ReadOnlyObservableCollection<T>(this);
            m_lock = new WrappedLock(BeforeMultiUpdate, unlock);
        }
#endif
        public IDisposable BeginMultiUpdate()
        {
            Contract.Ensures(Contract.Result<IDisposable>() != null);
            return m_lock.GetLock();
        }

        public ReadOnlyObservableCollection<T> ReadOnly
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyObservableCollection<T>>() != null);
                return m_roCollection;
            }
        }

#if SILVERLIGHT
        public void Move(int oldIndex, int newIndex)
        {
            Contract.Requires(oldIndex >= 0 && oldIndex < base.Count);
            MoveItem(oldIndex, newIndex);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            Contract.Requires(oldIndex >= 0 && oldIndex < base.Count);
            if (oldIndex != newIndex)
            {
                T item = base[oldIndex];
                using (BeginMultiUpdate())
                {
                    base.RemoveItem(oldIndex);
                    base.InsertItem(newIndex, item);
                }
            }
        }
#endif

        public void Sort(Func<T, T, int> comparer)
        {
            Contract.Requires(comparer != null);
            var changed = Items.QuickSort(comparer);
            if (changed)
            {
                raiseReset();
            }
        }

        protected virtual void BeforeMultiUpdate() { }

        protected virtual void AfterMultiUpdate() { }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (m_lock.IsLocked)
            {
                m_isChanged = true;
            }
            else
            {
                base.OnPropertyChanged(e);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (m_lock.IsLocked)
            {
                m_isChanged = true;
            }
            else
            {
                base.OnCollectionChanged(e);
            }
        }

        private void unlock()
        {
            if (m_isChanged)
            {
                raiseReset();
                m_isChanged = false;
            }
            AfterMultiUpdate();
        }

        private void raiseReset()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(m_lock != null);
            Contract.Invariant(m_roCollection != null);
        }

        private bool m_isChanged;

        private readonly WrappedLock m_lock;
        private readonly ReadOnlyObservableCollection<T> m_roCollection;
    }
}
