/*
The MIT License

Copyright (c) 2010 Pixel Lab

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;

namespace PixelLab.Common {
  //////////////////////////////////////////////////////////////////////////
  /// <summary>
  ///     Serves as a base implemetation of <see cref="IList{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the item in the list.</typeparam>
  //////////////////////////////////////////////////////////////////////////
  public abstract class ListBase<T> : IList<T>, IList {

    //--------------------------------------------------------------------
    /// <exception cref="NotSupportedException">
    ///     RemoveAt is not directly supported. To add support,
    ///     override in a subclass.
    /// </exception>
    //--------------------------------------------------------------------
    protected virtual bool RemoveItem(int index) {
      throw new NotSupportedException();
    }

    //--------------------------------------------------------------------
    /// <exception cref="NotSupportedException">
    ///     Add is not directly supported. To add support,
    ///     override in a subclass.
    /// </exception>
    //--------------------------------------------------------------------
    protected virtual void InsertItem(int index, T item) {
      throw new NotSupportedException();
    }

    //--------------------------------------------------------------------
    /// <exception cref="NotSupportedException">
    ///     Clear is not directly supported. To add support,
    ///     override in a subclass.
    /// </exception>
    //--------------------------------------------------------------------
    protected virtual void ClearItems() {
      throw new NotSupportedException();
    }

    //--------------------------------------------------------------------
    /// <exception cref="NotSupportedException">
    ///     SetItem is not directly supported. To add support,
    ///     override in a subclass.
    /// </exception>
    //--------------------------------------------------------------------
    protected virtual void SetItem(int index, T value) {
      throw new NotSupportedException();
    }

    protected virtual bool IsReadOnly {
      get { return true; }
    }

    protected virtual bool IsFixedSize {
      get { return true; }
    }

    #region IList<T> Members

    //--------------------------------------------------------------------
    /// <summary>
    ///     Searches for the specified object and returns the zero-based
    ///     index of the first occurrence within the entire <see cref="ListBase{T}"/>.
    /// </summary>
    /// <param name="item">
    ///     The object to locate in the <see cref="ListBase{T}"/>. 
    ///     The value can be null for reference types.</param>
    /// <returns>
    ///     The zero-based index of the first occurrence of item within the 
    ///     entire <see cref="ListBase{T}"/>, if found; otherwise, –1.
    /// </returns>
    //--------------------------------------------------------------------
    public virtual int IndexOf(T item) {
      for (int i = 0; i < this.Count; i++) {
        if (EqualityComparer<T>.Default.Equals(this[i], item)) {
          return i;
        }
      }
      return -1;
    }

    //--------------------------------------------------------------------
    /// <summary>
    ///     Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    //--------------------------------------------------------------------
    public T this[int index] {
      get {
        return GetItem(index);
      }
    }

    //--------------------------------------------------------------------
    /// <summary>
    ///     When overridden in a derived class, provides the item at the 
    ///     specified index;
    /// </summary>
    /// <param name="index">The index of the desired item.</param>
    /// <returns>The item at the provided index.</returns>
    //--------------------------------------------------------------------
    protected abstract T GetItem(int index);

    void IList<T>.Insert(int index, T item) {
      InsertItem(index, item);
    }

    void IList<T>.RemoveAt(int index) {
      RemoveItem(index);
    }

    T IList<T>.this[int index] {
      get {
        return this[index];
      }
      set {
        SetItem(index, value);
      }
    }

    #endregion

    #region ICollection<T> Members

    //--------------------------------------------------------------------
    /// <summary>
    ///     Determines whether an element is in the list.
    /// </summary>
    /// <param name="item">
    ///     The object to locate in the list.
    ///     The value can be null for reference types.
    /// </param>
    /// <returns>
    ///     true if item is found in the list; otherwise, false.
    /// </returns>
    //--------------------------------------------------------------------
    [Pure]
    public virtual bool Contains(T item) {
      if (item == null) {
        for (int num1 = 0; num1 < this.Count; num1++) {
          if (this[num1] == null) {
            return true;
          }
        }
        return false;
      }
      EqualityComparer<T> comparer1 = EqualityComparer<T>.Default;
      for (int num2 = 0; num2 < this.Count; num2++) {
        if (comparer1.Equals(this[num2], item)) {
          return true;
        }
      }
      return false;
    }

    //--------------------------------------------------------------------
    /// <summary>
    ///     Copies the entire list to a compatible one-dimensional array,
    ///     starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional Array that is the destination of the elements copied from list.
    ///     The Array must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    //--------------------------------------------------------------------
    public virtual void CopyTo(T[] array, int arrayIndex) {
      CopyTo((Array)array, arrayIndex);
    }

    //--------------------------------------------------------------------
    /// <summary>
    ///     When overridden in a derived class, returns the number of items
    ///     in the list.
    /// </summary>
    /// <remarks>The number of items in the list.</remarks>
    //--------------------------------------------------------------------
    public abstract int Count {
      get;
    }

    bool ICollection<T>.IsReadOnly { get { return IsReadOnly; } }

    void ICollection<T>.Add(T item) {
      InsertItem(Count, item);
    }

    void ICollection<T>.Clear() {
      ClearItems();
    }

    bool ICollection<T>.Remove(T item) {
      return RemoveItem(IndexOf(item));
    }

    #endregion

    #region IEnumerable<T> Members

    public virtual IEnumerator<T> GetEnumerator() {
      for (int i = 0; i < this.Count; i++) {
        yield return this[i];
      }
    }

    #endregion

    #region IList Members

    int IList.Add(object value) {
      VerifyValueType(value);
      this.InsertItem(Count, (T)value);
      return (this.Count - 1);
    }

    bool IList.Contains(object value) {
      if (IsCompatibleObject(value)) {
        return this.Contains((T)value);
      }
      return false;
    }

    int IList.IndexOf(object value) {
      if (IsCompatibleObject(value)) {
        return this.IndexOf((T)value);
      }
      return -1;
    }

    void IList.Insert(int index, object value) {
      VerifyValueType(value);
      this.InsertItem(index, (T)value);
    }

    void IList.Remove(object value) {
      if (IsCompatibleObject(value)) {
        this.RemoveItem(IndexOf((T)value));
      }
    }

    object IList.this[int index] {
      get {
        return this[index];
      }
      set {
        VerifyValueType(value);
        SetItem(index, (T)value);
      }
    }

    void IList.RemoveAt(int index) {
      RemoveItem(index);
    }

    bool IList.IsReadOnly { get { return IsReadOnly; } }

    bool IList.IsFixedSize { get { return IsFixedSize; } }

    void IList.Clear() {
      ClearItems();
    }

    #endregion

    #region ICollection Members

    //--------------------------------------------------------------------
    /// <summary>
    ///     Copies the entire <see cref="ListBase{T}"/> to a compatible one-dimensional 
    ///     array, starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional Array that is the destination of the elements copied from <see cref="ListBase{T}"/>.
    ///     The Array must have zero-based indexing.
    /// </param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    //--------------------------------------------------------------------
    public virtual void CopyTo(Array array, int index) {
      for (int i = 0; i < this.Count; i++) {
        array.SetValue(this[i], index + i);
      }
    }

    bool ICollection.IsSynchronized {
      get { return false; }
    }

    object ICollection.SyncRoot {
      get {
        if (m_syncRoot == null) {
          Interlocked.CompareExchange(ref m_syncRoot, new object(), null);
        }
        return m_syncRoot;
      }
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    #endregion

    private object m_syncRoot;

    #region Private Static Helpers

    private static bool IsCompatibleObject(object value) {
      if (!(value is T) && ((value != null) || typeof(T).IsValueType)) {
        return false;
      }
      return true;
    }

    [DebuggerStepThrough]
    private static void VerifyValueType(object value) {
      if (!IsCompatibleObject(value)) {
        throw new ArgumentException("value");
      }
    }

    #endregion

  } //*** class ListBase<T>

} //*** namespace PixelLab.Common
