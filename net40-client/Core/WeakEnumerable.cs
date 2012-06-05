using System;
using System.Collections.Generic;
using System.Linq;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    /// <summary>
    /// Keeps a collection of items 'weakly'. On enumeration, valid references to items are returned. Invalid references are cleaned up.
    /// </summary>
    /// <typeparam name="T">Any class. Silly to have weak references to value types, no?</typeparam>
    /// <remarks>Very much NOT thread safe.</remarks>
    public class WeakEnumerable<T> : IEnumerable<T> where T : class
    {
        /// <summary>
        /// Adds an item to the end of the enumerable.
        /// </summary>
        /// <remarks>This method requires the entire collection to be enumerated.
        /// If adding to the end is not required, use <see cref="Insert"/>. It's faster.</remarks>
        public void Add(T item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var node = new WeakNode(item);

            var last = FuncEnumerable<WeakNode>.Get(GetNodeEnumerator).LastOrDefault();
            if (last == null)
            {
                m_head = node;
            }
            else
            {
                last.Next = node;
            }
        }

        /// <summary>
        /// Adds an item to the begining (head) of the collection.
        /// </summary>
        /// <remarks>Potentially a lot faster than <see cref="Add"/>.</remarks>
        public void Insert(T item)
        {
            Contract.Requires<ArgumentNullException>(item != null);

            var node = new WeakNode(item);
            node.Next = m_head;
            m_head = node;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var node in FuncEnumerable<WeakNode>.Get(GetNodeEnumerator))
            {
                T current;
                if (node.TryGetValue(out current))
                {
                    yield return current;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerator<WeakNode> GetNodeEnumerator()
        {
            var current = m_head = WeakNode.GetAlive(m_head);
            while (current != null)
            {
                yield return current;
                current = current.Next = WeakNode.GetAlive(current.Next);
            }
        }

        private WeakNode m_head;

        private class WeakNode
        {
            public static WeakNode GetAlive(WeakNode node)
            {
                while (node != null && !node.m_reference.IsAlive)
                {
                    node = node.Next;
                }
                return node;
            }

            public WeakNode(T value, WeakNode next = null)
            {
                Contract.Requires(value != null);
                m_reference = new WeakReference(value);
                Next = next;
            }

            public bool TryGetValue(out T value)
            {
                value = (T)m_reference.Target;
                return value != null;
            }

            public WeakNode Next;
            private readonly WeakReference m_reference;
        }
    }
}
