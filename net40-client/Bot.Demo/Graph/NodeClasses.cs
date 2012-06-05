using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo
{
    public class Node<T> where T : IEquatable<T>
    {
        internal Node(T item, NodeCollection<T> parent)
        {
            Debug.Assert(item != null && parent != null);

            m_item = item;
            m_parent = parent;
        }

        public ReadOnlyObservableCollection<Node<T>> ChildNodes
        {
            get
            {
                if (m_children == null)
                {
                    m_parent.NodeChildrenChanged += m_parent_NodeChildrenChanged;

                    m_children = new ObservableCollection<Node<T>>(m_parent.GetChildren(this.m_item));
                    m_childrenReadOnly = new ReadOnlyObservableCollection<Node<T>>(m_children);
                }
                return m_childrenReadOnly;
            }
        }

        public T Item { get { return m_item; } }

        public override string ToString()
        {
            return "Node - '" + m_item.ToString() + "'";
        }

        private void m_parent_NodeChildrenChanged(object sender, NodeChildrenChangedArgs<T> args)
        {
            if (args.Parent.Equals(this.m_item))
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    Debug.Assert(!m_children.Contains(m_parent[args.Child]));
                    m_children.Add(m_parent[args.Child]);
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    Debug.Assert(m_children.Contains(m_parent[args.Child]));
                    m_children.Remove(m_parent[args.Child]);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                Debug.Assert(args.Parent != null);
            }
        }

        private ObservableCollection<Node<T>> m_children;
        private ReadOnlyObservableCollection<Node<T>> m_childrenReadOnly;

        private readonly T m_item;
        private readonly NodeCollection<T> m_parent;
    }

    public class NodeChildrenChangedArgs<T> : EventArgs
    {
        public NodeChildrenChangedArgs(T parent, T child, NotifyCollectionChangedAction action)
        {
            Contract.Requires<ArgumentNullException>(parent != null);
            Contract.Requires<ArgumentNullException>(child != null);

            if (!(action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Remove))
            {
                throw new ArgumentException("Only supports Add and Remove", "action");
            }

            Parent = parent;
            Child = child;
            Action = action;
        }

        public T Parent { get; private set; }
        public T Child { get; private set; }
        public NotifyCollectionChangedAction Action { get; private set; }
    }

    public class NodeCollection<T> where T : IEquatable<T>
    {
        public NodeCollection(IList<T> nodes)
        {
            Contract.Requires<ArgumentNullException>(nodes != null);
            Contract.Requires(Contract.ForAll(nodes, item => item != null));
            Contract.Requires(nodes.AllUnique(), "All of the nodes must be unique.");

            m_nodeValues = new ObservableCollectionPlus<T>(nodes);

            m_nodes = new Dictionary<T, Node<T>>();
            m_nodeConnections = new Dictionary<T, HashSet<T>>();

            foreach (var node in m_nodeValues)
            {
                m_nodeConnections.Add(node, new HashSet<T>());
            }
        }

        public ReadOnlyCollection<T> Nodes { get { return m_nodeValues.ReadOnly; } }

        public Node<T> this[T value]
        {
            get
            {
                Contract.Requires(HasNode(value));
                if (!m_nodes.ContainsKey(value))
                {
                    m_nodes[value] = new Node<T>(value, this);
                }
                return m_nodes[value];
            }
        }

        [Pure]
        public bool HasNode(T value)
        {
            var has = m_nodeValues.Contains(value);
            Debug.Assert(has == m_nodeConnections.ContainsKey(value));
            return has;
        }

        public void Add(T node)
        {
            Contract.Requires(!HasNode(node));
            m_nodeValues.Add(node);
            m_nodeConnections.Add(node, new HashSet<T>());
        }

        public bool Remove(T node)
        {
            if (m_nodeValues.Contains(node))
            {
                Debug.Assert(m_nodeConnections.ContainsKey(node));

                m_nodeConnections.Values.ForEach(hashSet => hashSet.Remove(node));
                m_nodes.Remove(node);
                m_nodeValues.Remove(node);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddEdge(T node1, T node2)
        {
            if (node1.Equals(node2))
            {
                Debug.Assert(!hasPair(node1, node1));
                throw new ArgumentException("Cannot create an edge between the same node.");
            }
            else if (hasPair(node1, node2))
            {
                throw new ArgumentException("This edge already exists.");
            }
            else
            {
                m_nodeConnections[node1].Add(node2);
                m_nodeConnections[node2].Add(node1);

                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<T>(node1, node2, NotifyCollectionChangedAction.Add));
                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<T>(node2, node1, NotifyCollectionChangedAction.Add));
            }
        }

        public void RemoveEdge(T node1, T node2)
        {
            Contract.Requires(!node1.Equals(node2));

            if (hasPair(node1, node2))
            {
                m_nodeConnections[node1].Remove(node2);
                m_nodeConnections[node2].Remove(node1);

                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<T>(node1, node2, NotifyCollectionChangedAction.Remove));
                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<T>(node2, node1, NotifyCollectionChangedAction.Remove));
            }
            else
            {
                throw new ArgumentException("This edge does not exist");
            }
        }

        public event EventHandler<NodeChildrenChangedArgs<T>> NodeChildrenChanged;

        protected void OnNodeChildrenChanged(NodeChildrenChangedArgs<T> args)
        {
            var handler = NodeChildrenChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #region Demo Methods

        public void MoreFriends(T node)
        {
            Contract.Requires(HasNode(node), "node");

            HashSet<T> connections = m_nodeConnections[node];
            if (connections.Count < (m_nodeConnections.Count - 1))
            {
                var options = m_nodeConnections.Keys
                  .Where(newNode => !newNode.Equals(node))
                  .Where(newNode => !connections.Contains(newNode))
                  .ToArray();

                Debug.Assert(!options.IsEmpty());
                AddEdge(node, options.Random());
            }
        }

        public void LessFriends(T node)
        {
            Contract.Requires(HasNode(node));
            HashSet<T> connections = m_nodeConnections[node];
            if (connections.Count > c_minConnections)
            {
                RemoveEdge(node, connections.ToArray().Random());
            }
        }

        public void Churn()
        {
            if (m_nodeConnections.Count < 3)
            {
                return;
            }

            // step one: pick a node to play with
            T item = m_nodeValues.Random();

            double currentRatio = m_nodeConnections[item].Count / (double)(m_nodeConnections.Count - 1);
            Debug.Assert(currentRatio <= 1);
            Debug.Assert(currentRatio >= 0);
            if (currentRatio <= c_idealConnectionRatio || m_nodeConnections[item].Count < c_minConnections)
            {
                Debug.Assert(m_nodeConnections[item].Count != (m_nodeConnections.Count - 1));

                var toAdd = m_nodeConnections.Keys
                  .Where(node => !node.Equals(item))
                  .Except(m_nodeConnections[item])
                  .ToArray()
                  .Random();

                AddEdge(item, toAdd);
            }
            else
            {
                Debug.Assert(m_nodeConnections[item].Count > 0);

                var toRemove = m_nodeConnections[item].ToArray().Random();
                RemoveEdge(item, toRemove);
            }

            validateConnections();
        }

        #endregion

        #region Implementation

        internal List<Node<T>> GetChildren(T item)
        {
            Debug.Assert(m_nodeConnections.ContainsKey(item));

            return m_nodeConnections[item].Select(child => this[child]).ToList();
        }

        [Conditional("DEBUG")]
        private void validateConnections()
        {
            Dictionary<T, object> _verified = new Dictionary<T, object>();
            foreach (T item in m_nodeConnections.Keys)
            {
                foreach (T connection in m_nodeConnections[item])
                {
                    if (!_verified.ContainsKey(connection))
                    {
                        Debug.Assert(m_nodeConnections[connection].Contains(item));
                    }
                }
                _verified.Add(item, null);
            }
        }

        private bool hasPair(T node1, T node2, bool check = true)
        {
            bool value;
            HashSet<T> hashset;
            if (m_nodeConnections.TryGetValue(node1, out hashset))
            {
                value = hashset.Contains(node2);
            }
            else
            {
                value = false;
            }
            if (check)
            {
                Debug.Assert(value == hasPair(node2, node1, false));
            }
            return value;
        }

        private readonly Dictionary<T, Node<T>> m_nodes;
        private readonly Dictionary<T, HashSet<T>> m_nodeConnections;

        private readonly ObservableCollectionPlus<T> m_nodeValues;

        private const int c_minConnections = 2;
        private const double c_idealConnectionRatio = .4;

        #endregion
    }
}