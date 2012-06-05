using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PixelLab.SL
{
    public class CollapseOnDisableBehavior : Behavior<Control>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.IsEnabledChanged += AssociatedObject_IsEnabledChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AssociatedObject.Visibility = AssociatedObject.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
