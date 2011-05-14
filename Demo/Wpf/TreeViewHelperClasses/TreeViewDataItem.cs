using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo
{
    public class TreeViewDataItem : Changeable
    {
        public TreeViewDataItem(object dataItem, string childrenPropertyName) : this(dataItem, childrenPropertyName, null) { }
        internal TreeViewDataItem(object dataItem, string childrenPropertyName, TreeViewDataItem parent)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException("dataItem");
            }
            if (childrenPropertyName == null)
            {
                throw new ArgumentNullException("childrenPropertyName");
            }

            _dataItem = dataItem;
            _childrenPropertyName = childrenPropertyName;
            _parent = parent;
        }

        public IList<TreeViewDataItem> Children
        {
            get
            {
                if (_dataItemsChildrenCache == null)
                {
                    _dataItemsChildrenCache = new List<TreeViewDataItem>();

                    PropertyInfo childrenProperty = _dataItem.GetType().GetProperty(_childrenPropertyName, BindingFlags.Instance | BindingFlags.Public);
                    if (childrenProperty != null)
                    {
                        IEnumerable children = childrenProperty.GetGetMethod().Invoke(_dataItem, null) as IEnumerable;

                        if (children != null)
                        {
                            foreach (object child in children)
                            {
                                _dataItemsChildrenCache.Add(new TreeViewDataItem(child, _childrenPropertyName, this));
                            }
                        }
                    }
                }
                return _dataItemsChildrenCache;
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(IsExpandedPropertyChangedArg);
                }
            }
        }

        public void ExpandAll()
        {
            this.IsExpanded = true;
            Children.ForEach(item => item.ExpandAll());
        }

        public TreeViewDataItem Parent
        {
            get
            {
                return _parent;
            }
        }

        public IList<TreeViewDataItem> ParentChain
        {
            get
            {
                List<TreeViewDataItem> parents = new List<TreeViewDataItem>();
                TreeViewDataItem current = _parent;
                while (current != null)
                {
                    parents.Insert(0, current);
                    current = _parent.Parent;
                    parents.Reverse();
                }
                return parents;
            }
        }

        public string Name { get { return _dataItem.ToString(); } }

        public override string ToString()
        {
            return Name;
        }

        #region implementation

        private bool _isExpanded;

        private object _dataItem;
        private string _childrenPropertyName;
        private IList<TreeViewDataItem> _dataItemsChildrenCache;
        private TreeViewDataItem _parent;

        private static readonly PropertyChangedEventArgs IsExpandedPropertyChangedArg = new PropertyChangedEventArgs("IsExpanded");

        #endregion
    }
}

