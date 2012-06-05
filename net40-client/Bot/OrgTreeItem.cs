using System.Windows;
using System.Windows.Controls;

namespace PixelLab.Wpf
{
    public class OrgTreeItem : HeaderedItemsControl
    {
        static OrgTreeItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OrgTreeItem),
                                                     new FrameworkPropertyMetadata(typeof(OrgTreeItem)));
        }

        #region DPs

        private static readonly DependencyPropertyKey DepthPropertyKey =
            DependencyProperty.RegisterReadOnly("Depth", typeof(int), typeof(OrgTreeItem), new UIPropertyMetadata(0));

        public static readonly DependencyProperty DepthProperty = DepthPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(OrgTreeItem),
                                        new FrameworkPropertyMetadata(false));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public int Depth
        {
            get { return (int)GetValue(DepthProperty); }
            private set { SetValue(DepthPropertyKey, value); }
        }

        #endregion

        #region overrides

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new OrgTreeItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is OrgTreeItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ((OrgTreeItem)element).Depth = Depth + 1;
            base.PrepareContainerForItemOverride(element, item);
        }

        #endregion
    }
}