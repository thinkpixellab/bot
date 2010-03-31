using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.OrgTree {
    public abstract class OrgItem : INotifyPropertyChanged {
        protected OrgItem(string name, IEnumerable<OrgItem> children) {
            Util.RequireNotNullOrEmpty(name, "name");

            if (children == null) {
                children = Enumerable.Empty<OrgItem>();
            }

            Name = name;
            _children = new ReadOnlyCollection<OrgItem>(children.ToArray());
            _children.ForEach(child => child.SetParent(this));
            _isVisible = false;
            _isExpanded = false;
        }

        public string Name { get; private set; }

        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (value != _isVisible) {
                    _isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (value != _isExpanded) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        public OrgItem Parent { get { return _parent; } }

        public ReadOnlyCollection<OrgItem> Children {
            get { return _children; }
        }

        public void RequestExpand() {
            if (_parent != null) {
                _parent.IsExpanded = true;
                _parent.RequestExpand();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, args);
            }
        }

        protected void OnPropertyChanged(string propertyName) {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() {
            return String.Format("OrgItem: '{0}'", Name);
        }

        #region private instance impl

        private void SetParent(OrgItem parent) {
            _parent = parent;
        }

        private bool _isVisible;
        private bool _isExpanded;
        private readonly ReadOnlyCollection<OrgItem> _children;
        private OrgItem _parent;

        #endregion

        public static ICommand ToggleVisibilityCommand { get { return s_toggleVisibilityCommand.Command; } }

        private static readonly CommandWrapper<OrgItem> s_toggleVisibilityCommand =
            new CommandWrapper<OrgItem>(oi => {
                oi.IsVisible = !oi.IsVisible;
                if (oi.IsVisible) {
                    oi.RequestExpand();
                }
            });
    }

    public class Company : OrgItem {
        public Company(string name, IEnumerable<OrgItem> children) : base(name, children) { }
    }

    public class Department : OrgItem {
        public Department(string name, IEnumerable<OrgItem> children) : base(name, children) { }
    }

    public class Employee : OrgItem {
        public Employee(string name, IEnumerable<OrgItem> children) : base(name, children) { }
    }
}
