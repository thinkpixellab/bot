using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.OrgTree
{
    public class OrgViewItem
    {
        public OrgViewItem(OrgItem root)
        {
            Contract.Requires<ArgumentNullException>(root != null);
            _dataRoot = root;
        }

        public ReadOnlyObservableCollection<OrgViewItem> VisibleChildren
        {
            get
            {
                if (_visible == null)
                {
                    _visible = new ObservableCollectionPlus<OrgViewItem>(Children.Where(child => child.Data.IsVisible));
                }
                return _visible.ReadOnly;
            }
        }

        public ReadOnlyObservableCollection<OrgViewItem> HiddenChildren
        {
            get
            {
                if (_hidden == null)
                {
                    _hidden = new ObservableCollectionPlus<OrgViewItem>(Children.Where(child => !child.Data.IsVisible));
                }
                return _hidden.ReadOnly;
            }
        }

        public ReadOnlyCollection<OrgViewItem> Children
        {
            get
            {
                if (_childrenRO == null)
                {
                    if (_children == null)
                    {
                        _children = _dataRoot.Children.Select(child => new OrgViewItem(child)).ToArray();
                        _children.ForEach(child => child.Data.AddWatcher("IsVisible", () => refreshVisible(child)));
                    }
                    _childrenRO = new ReadOnlyCollection<OrgViewItem>(_children);
                }
                return _childrenRO;
            }
        }

        public OrgItem Data
        {
            get { return _dataRoot; }
        }

        private void refreshVisible(OrgViewItem child)
        {
            Debug.Assert(!_visible.Contains(child) == _hidden.Contains(child));
            if (child.Data.IsVisible && !_visible.Contains(child))
            {
                _visible.Add(child);
                _hidden.Remove(child);
            }
            else if (!child.Data.IsVisible && _visible.Contains(child))
            {
                _visible.Remove(child);
                _hidden.Add(child);
            }
        }

        private OrgViewItem[] _children;
        private ReadOnlyCollection<OrgViewItem> _childrenRO;
        private ObservableCollectionPlus<OrgViewItem> _hidden;
        private ObservableCollectionPlus<OrgViewItem> _visible;
        private readonly OrgItem _dataRoot;

        public static OrgViewItem[] GetViewArray(IEnumerable<OrgItem> items)
        {
            return items.Select(item => new OrgViewItem(item)).ToArray();
        }
    }

    public class CountHiddenConverter : SimpleValueConverter<int, Visibility>
    {
        protected override Visibility ConvertBase(int input)
        {
            return input == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class ShowContextMenuConverter : SimpleValueConverter<int, bool>
    {
        protected override bool ConvertBase(int input)
        {
            return input > 0;
        }
    }
}
