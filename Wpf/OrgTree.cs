using System.Windows;
using System.Windows.Controls;

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
