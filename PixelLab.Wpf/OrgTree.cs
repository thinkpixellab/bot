using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace PixelLab.Wpf
{
    public class OrgTree : ItemsControl
    {
        static OrgTree()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OrgTree), new FrameworkPropertyMetadata(typeof(OrgTree)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new OrgTreeItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is OrgTreeItem;
        }
    }
}
