using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Threading;

namespace PixelLab.Common
{
    /// <summary>
    ///     Serves as a base implemetation of <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the item in the list.</typeparam>
    [ContractClass(typeof(ListBaseContract<>))]
    public abstract class ListBase<T> : IList<T>, IList
    {
        protected virtual void RemoveItem(int index)
        {
            throw new NotSupportedException();
        }

        protected virtual void InsertItem(int index, T item)
        {
            throw new NotSupportedException();
        }

        protected virtual void ClearItems()
        {
            throw new NotSupportedException();
        }

        protected virtual void SetItem(int index, T value)
        {
            throw new NotSupportedException();
        }

        protected virtual bool IsReadOnly
        {
            get { return true; }
        }

        protected virtual bool IsFixedSize
        {
            get { return true; }
        }

        protected virtual object SyncRoot
        {
            get
            {
                if (m_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref m_syncRoot, new object(), null);
                }
                return m_syncRoot;
            }
        }

        #region IList<T> Members

        public virtual int IndexOf(T item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(this[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public T this[int index]
        {
            get
            {
                return GetItem(index);
            }
        }

        protected abstract T GetItem(int index);

        void IList<T>.Insert(int index, T item)
        {
            InsertItem(index, item);
        }

        void IList<T>.RemoveAt(int index)
        {
            RemoveItem(index);
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                SetItem(index, value);
            }
        }

        #endregion

        #region ICollection<T> Members

        [Pure]
        public virtual bool Contains(T item)
        {
            if (item == null)
            {
                for (int num1 = 0; num1 < this.Count; num1++)
                {
                    if (this[num1] == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            EqualityComparer<T> comparer1 = EqualityComparer<T>.Default;
            for (int num2 = 0; num2 < this.Count; num2++)
            {
                if (comparer1.Equals(this[num2], item))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo((Array)array, arrayIndex);
        }

        public abstract int Count
        {
            get;
        }

        bool ICollection<T>.IsReadOnly { get { return IsReadOnly; } }

        void ICollection<T>.Add(T item)
        {
            InsertItem(Count, item);
        }

        void ICollection<T>.Clear()
        {
            ClearItems();
        }

        bool ICollection<T>.Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }
            else
            {
                RemoveItem(index);
                return true;
            }
        }

        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            VerifyValueType(value);
            this.InsertItem(Count, (T)value);
            return (this.Count - 1);
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return this.Contains((T)value);
            }
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return this.IndexOf((T)value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            VerifyValueType(value);
            this.InsertItem(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                this.RemoveItem(IndexOf((T)value));
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                VerifyValueType(value);
                SetItem(index, (T)value);
            }
        }

        void IList.RemoveAt(int index)
        {
            RemoveItem(index);
        }

        bool IList.IsReadOnly { get { return IsReadOnly; } }

        bool IList.IsFixedSize { get { return IsFixedSize; } }

        void IList.Clear()
        {
            ClearItems();
        }

        #endregion

        #region ICollection Members

        public virtual void CopyTo(Array array, int index)
        {
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index + i);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return SyncRoot;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        private object m_syncRoot;

        #region Private Static Helpers

        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T) && ((value != null) || typeof(T).IsValueType))
            {
                return false;
            }
            return true;
        }

        [DebuggerStepThrough]
        private static void VerifyValueType(object value)
        {
            if (!IsCompatibleObject(value))
            {
                throw new ArgumentException("value");
            }
        }

        #endregion
    } //*** class ListBase<T>

    [ContractClassFor(typeof(ListBase<>))]
    abstract class ListBaseContract<T> : ListBase<T>
    {
        protected override T GetItem(int index)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(index < Count);
            return default(T);
        }

        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return default(int);
            }
        }
    }
} //*** namespace PixelLab.Common
