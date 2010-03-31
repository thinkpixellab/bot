using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.OrgTree {
    public class OrgViewItem {
        private readonly OrgItem _dataRoot;
        private OrgViewItem[] _children;
        private ReadOnlyCollection<OrgViewItem> _childrenRO;
        private ObservableCollection<OrgViewItem> _hidden;
        private ReadOnlyObservableCollection<OrgViewItem> _hiddenRO;
        private ObservableCollection<OrgViewItem> _visible;
        private ReadOnlyObservableCollection<OrgViewItem> _visibleRO;

        public OrgViewItem(OrgItem root) {
            Util.RequireNotNull(root, "root");
            _dataRoot = root;
        }

        public ReadOnlyObservableCollection<OrgViewItem> VisibleChildren {
            get {
                if (_visibleRO == null) {
                    if (_visible == null) {
                        _visible = new ObservableCollection<OrgViewItem>(Children.Where(child => child.Data.IsVisible));
                    }
                    _visibleRO = new ReadOnlyObservableCollection<OrgViewItem>(_visible);
                }
                return _visibleRO;
            }
        }

        public ReadOnlyObservableCollection<OrgViewItem> HiddenChildren {
            get {
                if (_hiddenRO == null) {
                    if (_hidden == null) {
                        _hidden = new ObservableCollection<OrgViewItem>(Children.Where(child => !child.Data.IsVisible));
                    }
                    _hiddenRO = new ReadOnlyObservableCollection<OrgViewItem>(_hidden);
                }
                return _hiddenRO;
            }
        }

        public ReadOnlyCollection<OrgViewItem> Children {
            get {
                if (_childrenRO == null) {
                    if (_children == null) {
                        _children = _dataRoot.Children.Select(child => new OrgViewItem(child)).ToArray();
                        _children.ForEach(
                            child => {
                                child.Data.PropertyChanged += (sender, args) => {
                                    if (args.PropertyName == "IsVisible") {
                                        refreshVisible(child);
                                    }
                                };
                            });
                    }
                    _childrenRO = new ReadOnlyCollection<OrgViewItem>(_children);
                }
                return _childrenRO;
            }
        }

        public OrgItem Data {
            get { return _dataRoot; }
        }

        private void refreshVisible(OrgViewItem child) {
            Debug.Assert(!_visible.Contains(child) == _hidden.Contains(child));
            if (child.Data.IsVisible && !_visible.Contains(child)) {
                _visible.Add(child);
                _hidden.Remove(child);
            }
            else if (!child.Data.IsVisible && _visible.Contains(child)) {
                _visible.Remove(child);
                _hidden.Add(child);
            }
        }

        public static OrgViewItem[] GetViewArray(IEnumerable<OrgItem> items) {
            return items.Select(item => new OrgViewItem(item)).ToArray();
        }
    }

    public class CountHiddenConverter : SimpleValueConverter<int, Visibility> {
        protected override Visibility ConvertBase(int input) {
            return input == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class ShowContextMenuConverter : SimpleValueConverter<int, bool> {
        protected override bool ConvertBase(int input) {
            return input > 0;
        }
    }
}
