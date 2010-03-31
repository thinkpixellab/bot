using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using PixelLab.Common;

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

    public class NodeChildrenChangedArgs<TNode> : EventArgs
    {
        public NodeChildrenChangedArgs(TNode parent, TNode child, NotifyCollectionChangedAction action)
        {
            Util.RequireNotNull(parent, "parent");
            Util.RequireNotNull(child, "child");

            if (!(action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Remove))
            {
                throw new ArgumentException("Only supports Add and Remove", "action");
            }

            Parent = parent;
            Child = child;
            Action = action;
        }

        public TNode Parent { get; private set; }
        public TNode Child { get; private set; }
        public NotifyCollectionChangedAction Action { get; private set; }
    }

    public class NodeCollection<TNode> where TNode : IEquatable<TNode>
    {
        public NodeCollection(IEnumerable<TNode> nodes)
        {
            Util.RequireNotNull(nodes, "nodes");

            List<TNode> nodeList = nodes.ToList();

            Util.RequireArgument(
                nodeList.All(item => item != null),
                "nodes",
                "All items must be non-null.");

            Util.RequireArgument(nodeList.AllUnique(), "nodes");

            m_nodes = new Dictionary<TNode, Node<TNode>>();
            m_nodeConnections = new Dictionary<TNode, HashSet<TNode>>();

            m_nodeValues = new ObservableCollection<TNode>(nodeList);
            m_nodesReadOnly = new ReadOnlyObservableCollection<TNode>(m_nodeValues);

            foreach (TNode node in nodeList)
            {
                m_nodeConnections.Add(node, new HashSet<TNode>());
            }
        }

        public ReadOnlyCollection<TNode> Nodes
        {
            get
            {
                return m_nodesReadOnly;
            }
        }

        public Node<TNode> this[TNode value]
        {
            get
            {
                if (!m_nodes.ContainsKey(value))
                {
                    if (!m_nodeConnections.ContainsKey(value))
                    {
                        throw new ArgumentException(
                            "No node exists with the provided value.",
                            "node");
                    }
                    else
                    {
                        m_nodes[value] = new Node<TNode>(value, this);
                    }
                }
                return m_nodes[value];
            }
        }

        public void Add(TNode node)
        {
            if (m_nodeValues.Contains(node))
            {
                throw new ArgumentException("Already have this value.", "node");
            }
            else
            {
                m_nodeConnections.Add(node, new HashSet<TNode>());
                m_nodeValues.Add(node);
            }
        }

        public bool Remove(TNode node)
        {
            if (m_nodeConnections.ContainsKey(node))
            {
                Debug.Assert(m_nodeValues.Contains(node));

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

        public void AddEdge(TNode node1, TNode node2)
        {
            if (node1.Equals(node2))
            {
                Debug.Assert(!m_nodeConnections[node1].Contains(node1));
                Debug.Assert(!m_nodeConnections[node2].Contains(node2));

                throw new ArgumentException("Cannot create an edge between the same node.");
            }
            else if (m_nodeConnections[node1].Contains(node2))
            {
                Debug.Assert(m_nodeConnections[node2].Contains(node1));
                throw new ArgumentException("This edge already exists.");
            }
            else
            {
                Debug.Assert(!m_nodeConnections[node2].Contains(node1));

                m_nodeConnections[node1].Add(node2);
                m_nodeConnections[node2].Add(node1);

                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<TNode>(node1, node2, NotifyCollectionChangedAction.Add));
                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<TNode>(node2, node1, NotifyCollectionChangedAction.Add));
            }
        }

        public void RemoveEdge(TNode node1, TNode node2)
        {
            if (node1.Equals(node2))
            {
                Debug.Assert(!m_nodeConnections[node1].Contains(node1));
                Debug.Assert(!m_nodeConnections[node2].Contains(node2));

                throw new ArgumentException("One cannot create an edge between the same node.");
            }
            else if (m_nodeConnections[node1].Contains(node2))
            {
                Debug.Assert(m_nodeConnections[node2].Contains(node1));

                m_nodeConnections[node1].Remove(node2);
                m_nodeConnections[node2].Remove(node1);

                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<TNode>(node1, node2, NotifyCollectionChangedAction.Remove));
                OnNodeChildrenChanged(
                    new NodeChildrenChangedArgs<TNode>(node2, node1, NotifyCollectionChangedAction.Remove));
            }
            else
            {
                Debug.Assert(!m_nodeConnections[node2].Contains(node1));
                throw new ArgumentException("This edge does not exist");
            }
        }

        public event EventHandler<NodeChildrenChangedArgs<TNode>> NodeChildrenChanged;

        protected void OnNodeChildrenChanged(NodeChildrenChangedArgs<TNode> args)
        {
            EventHandler<NodeChildrenChangedArgs<TNode>> handler = NodeChildrenChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #region Demo Methods

        public void MoreFriends(TNode node)
        {
            if (!m_nodeConnections.ContainsKey(node))
            {
                throw new ArgumentException();
            }
            else
            {
                HashSet<TNode> connections = m_nodeConnections[node];
                if (connections.Count < (m_nodeConnections.Count - 1))
                {
                    foreach (TNode newNode in m_nodeConnections.Keys)
                    {
                        if (!newNode.Equals(node))
                        {
                            if (!connections.Contains(newNode))
                            {
                                AddEdge(node, newNode);
                                break;
                            }
                        }
                    }

                }
            }
        }

        public void LessFriends(TNode node)
        {
            if (!m_nodeConnections.ContainsKey(node))
            {
                throw new ArgumentException();
            }
            else
            {
                HashSet<TNode> connections = m_nodeConnections[node];
                if (connections.Count > c_minConnections)
                {
                    RemoveEdge(node, connections.First());
                }
            }
        }

        public void Churn()
        {
            if (m_nodeConnections.Count < 3)
            {
                throw new InvalidOperationException("need at least three nodes to play");
            }
            //step one: pick a node to play with
            int itemIndex = Util.Rnd.Next(m_nodeConnections.Count);
            TNode item = this.m_nodesReadOnly[itemIndex];

            //stop two: pick something to do. Either add a node or remove a node.
            bool? doAdd = null;
            if (m_nodeConnections[item].Count < c_minConnections)
            {
                doAdd = true;
            }
            else
            {
                double currentRatio = m_nodeConnections[item].Count / (double)(m_nodeConnections.Count - 1);
                Debug.Assert(currentRatio <= 1);
                Debug.Assert(currentRatio > 0);
                if (currentRatio <= c_idealConnectionRatio)
                {
                    doAdd = true;
                }
                else
                {
                    doAdd = false;
                }
            }

            if (doAdd == true)
            {
                if (m_nodeConnections[item].Count == (m_nodeConnections.Count - 1))
                {
                    throw new Exception("this should never happen...");
                }
                int indexToAdd = Util.Rnd.Next(m_nodeConnections.Count - m_nodeConnections[item].Count);
                Debug.Assert(indexToAdd < m_nodeConnections.Count);
                Debug.Assert(indexToAdd >= 0);
                Debug.Assert(indexToAdd < (m_nodeConnections.Count - (m_nodeConnections[item].Count - 1)));

                foreach (TNode child in m_nodeConnections.Keys)
                {
                    if (!child.Equals(item))
                    {
                        if (indexToAdd == 0 && !m_nodeConnections[item].Contains(child))
                        {
                            AddEdge(item, child);
                            break;
                        }
                        else
                        {
                            indexToAdd--;
                        }
                    }
                }
            }
            else if (doAdd == false)
            {
                if (m_nodeConnections[item].Count < 1)
                {
                    throw new Exception("this should never happen");
                }
                int indexToRemove = Util.Rnd.Next(m_nodeConnections[item].Count);
                foreach (TNode child in m_nodeConnections[item])
                {
                    if (indexToRemove == 0)
                    {
                        RemoveEdge(item, child);
                        break;
                    }
                    else
                    {
                        indexToRemove--;
                    }
                }
            }
            else
            {
                throw new Exception("should never be null");
            }

            validateConnections();
        }

        #endregion

        #region Implementation

        internal List<Node<TNode>> GetChildren(TNode item)
        {
            Debug.Assert(m_nodeConnections.ContainsKey(item));

            return m_nodeConnections[item].Select(child => this[child]).ToList();
        }

        [Conditional("DEBUG")]
        private void validateConnections()
        {
            Dictionary<TNode, object> _verified = new Dictionary<TNode, object>();
            foreach (TNode item in m_nodeConnections.Keys)
            {
                foreach (TNode connection in m_nodeConnections[item])
                {
                    if (!_verified.ContainsKey(connection))
                    {
                        Debug.Assert(m_nodeConnections[connection].Contains(item));
                    }
                }
                _verified.Add(item, null);
            }
        }

        private readonly Dictionary<TNode, Node<TNode>> m_nodes;
        private readonly Dictionary<TNode, HashSet<TNode>> m_nodeConnections;

        private readonly ObservableCollection<TNode> m_nodeValues;
        private readonly ReadOnlyObservableCollection<TNode> m_nodesReadOnly;

        private const int c_minConnections = 2;
        private const double c_idealConnectionRatio = .4;

        #endregion

    }

}