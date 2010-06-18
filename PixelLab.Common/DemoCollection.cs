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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace PixelLab.Common {
  /// <remarks>
  ///     The members of this class are only safe to use via one thread (usually the UI thread in a WPF application).
  ///     Any cross-thread changes will cause undesired behavior.
  /// </remarks>
  public class DemoCollection<T> : ReadOnlyObservableCollection<T> {
    public static DemoCollection<T> Create(IList<T> source, int initialCount, int minCount, int maxCount) {
      Contract.Requires(source != null);
      Contract.Requires(source.Count > 0);
      Contract.Requires(initialCount >= 0);
      Contract.Requires(minCount >= 0);
      Contract.Requires(minCount <= initialCount);
      Contract.Requires(initialCount <= maxCount);

      var sourceItems = source.ToReadOnlyCollection();

      var observableCollection = new ObservableCollectionPlus<T>();

      for (int i = 0; i < initialCount; i++) {
        observableCollection.Add(sourceItems[i % sourceItems.Count]);
      }

      return new DemoCollection<T>(sourceItems, observableCollection, minCount, maxCount, initialCount);
    }

    public void Add() {
      if (canAddOrInsert) {
        m_activeItems.Add(m_sourceItems.Random());
      }
    }

    public void Remove() {
      if (canRemove) {
        int targetIndex = m_random.Next(m_activeItems.Count);

        m_activeItems.RemoveAt(targetIndex);
      }
    }

    public void Move() {
      if (canMove) {
        int startIndex = m_random.Next(m_activeItems.Count);
        int endIndex;
        do {
          endIndex = m_random.Next(m_activeItems.Count);
        }
        while (endIndex == startIndex);

        m_activeItems.Move(startIndex, endIndex);
      }
    }

    public void Change() {
      if (canChange) {
        int targetIndex = m_random.Next(m_activeItems.Count);

        m_activeItems[targetIndex] = m_sourceItems.Random();
      }
    }

    public void Insert() {
      if (canAddOrInsert) {
        int targetIndex = m_random.Next(m_activeItems.Count + 1);

        m_activeItems.Insert(targetIndex, m_sourceItems.Random());
      }
    }

    public void Reset() {
      // This is essential. '==' doesn't work against an arbitrary 'T'
      // And item.Equals will blow up if any item is null
      EqualityComparer<T> comparer = EqualityComparer<T>.Default;

      for (int i = 0; i < m_initialCount; i++) {
        int j = i % m_sourceItems.Count;

        if (i < m_activeItems.Count) // just re-shuffle
                {
          if (comparer.Equals(m_activeItems[i], m_sourceItems[j])) {
            // This item is already what it should be
          }
          else // Go look for an active item that matches
                    {
            bool found = false;
            for (int k = i + 1; k < m_activeItems.Count; k++) {
              if (comparer.Equals(m_activeItems[k], m_sourceItems[j])) {
                m_activeItems.Move(k, i);
                found = true;
                break;
              }
            }

            if (!found) {
              m_activeItems[i] = m_sourceItems[j];
            }
          }
        }
        else { // need to add new ones
          m_activeItems.Add(m_sourceItems[j]);
        }
      }

      while (m_activeItems.Count > m_initialCount) { // need to remove extras
        m_activeItems.RemoveAt(m_activeItems.Count - 1);
      }
    }

    public ICommand AddCommand { get { return m_addCommand.Command; } }
    public ICommand RemoveCommand { get { return m_removeCommand.Command; } }
    public ICommand MoveCommand { get { return m_moveCommand.Command; } }
    public ICommand ChangeCommand { get { return m_changeCommand.Command; } }
    public ICommand InsertCommand { get { return m_insertCommand.Command; } }
    public ICommand ResetCommand { get { return m_resetCommand.Command; } }

    #region Implementation

    private DemoCollection(
        ReadOnlyCollection<T> sourceItems,
        ObservableCollectionPlus<T> activeItems,
        int minCount,
        int maxCount,
        int initialCount)
      : base(activeItems) {
      Contract.Requires<ArgumentNullException>(activeItems != null);
      Contract.Requires<ArgumentNullException>(sourceItems != null);
      Contract.Requires(sourceItems.Count > 0);

      m_minCount = minCount;
      m_maxCount = maxCount;
      m_initialCount = initialCount;

      m_activeItems = activeItems;
      m_sourceItems = sourceItems;

      m_addCommand = new CommandWrapper(Add, () => canAddOrInsert);
      m_insertCommand = new CommandWrapper(Insert, () => canAddOrInsert);
      m_removeCommand = new CommandWrapper(Remove, () => canRemove);
      m_moveCommand = new CommandWrapper(Move, () => canMove);
      m_changeCommand = new CommandWrapper(Change, () => canChange);
      m_resetCommand = new CommandWrapper(Reset);

      activeItems.CollectionChanged += (sender, e) => {
        m_removeCommand.UpdateCanExecute();
        m_addCommand.UpdateCanExecute();
        m_moveCommand.UpdateCanExecute();
        m_changeCommand.UpdateCanExecute();
        m_insertCommand.UpdateCanExecute();
      };
    }

    private bool canAddOrInsert { get { return m_activeItems.Count < m_maxCount; } }

    private bool canRemove { get { return m_activeItems.Count > m_minCount; } }

    private bool canMove { get { return m_activeItems.Count > 1; } }

    private bool canChange { get { return m_activeItems.Count > 0; } }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(m_random != null);
      Contract.Invariant(m_sourceItems != null);
      Contract.Invariant(m_sourceItems.Count > 0);
      Contract.Invariant(m_activeItems != null);
      Contract.Invariant(m_addCommand != null);
      Contract.Invariant(m_removeCommand != null);
      Contract.Invariant(m_moveCommand != null);
      Contract.Invariant(m_changeCommand != null);
      Contract.Invariant(m_resetCommand != null);
      Contract.Invariant(m_insertCommand != null);
    }

    private readonly CommandWrapper m_addCommand, m_removeCommand, m_moveCommand, m_changeCommand, m_insertCommand, m_resetCommand;
    private readonly int m_minCount, m_maxCount, m_initialCount;
    private readonly Random m_random = Util.Rnd;
    private readonly ObservableCollectionPlus<T> m_activeItems;
    private readonly ReadOnlyCollection<T> m_sourceItems;

    #endregion

  }

}