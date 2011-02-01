using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.OrgTree
{
    public abstract class OrgItem : Changeable
    {
        protected OrgItem(string name, IEnumerable<OrgItem> children)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(name));

            if (children == null)
            {
                children = Enumerable.Empty<OrgItem>();
            }

            Name = name;
            m_children = new ReadOnlyCollection<OrgItem>(children.ToArray());
            m_children.ForEach(child => child.SetParent(this));
            m_isVisible = true;
        }

        public string Name { get; private set; }

        public bool IsVisible
        {
            get { return m_isVisible; }
            set
            {
                if (value != m_isVisible)
                {
                    m_isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                if (value != m_isExpanded)
                {
                    m_isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        public OrgItem Parent { get { return m_parent; } }

        public ReadOnlyCollection<OrgItem> Children
        {
            get { return m_children; }
        }

        public void RequestExpand()
        {
            if (m_parent != null)
            {
                m_parent.IsExpanded = true;
                m_parent.RequestExpand();
            }
        }

        public override string ToString()
        {
            return String.Format("OrgItem: '{0}'", Name);
        }

        #region private instance impl

        private void SetParent(OrgItem parent)
        {
            m_parent = parent;
        }

        private bool m_isVisible;
        private bool m_isExpanded;
        private readonly ReadOnlyCollection<OrgItem> m_children;
        private OrgItem m_parent;

        #endregion

        public static ICommand ToggleVisibilityCommand { get { return s_toggleVisibilityCommand.Command; } }

        private static readonly CommandWrapper<OrgItem> s_toggleVisibilityCommand =
            new CommandWrapper<OrgItem>(oi =>
            {
                oi.IsVisible = !oi.IsVisible;
                if (oi.IsVisible)
                {
                    oi.RequestExpand();
                }
            });
    }

    public class Company : OrgItem
    {
        public Company(string name, IEnumerable<OrgItem> children) : base(name, children) { }
    }

    public class Department : OrgItem
    {
        public Department(string name, IEnumerable<OrgItem> children) : base(name, children) { }
    }

    public class Employee : OrgItem
    {
        public Employee(string name, IEnumerable<OrgItem> children) : base(name, children) { }
    }
}
