using System.ComponentModel;
using System.Windows;

namespace PixelLab.Common
{
    /// <summary>
    /// Provides a proxy object for binding to the ActualWidth and ActualHeight of a FrameworkElement.
    /// </summary>
    public class ActualSizePropertyProxy : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// The associated <see cref="FrameworkElement"/>. This is a dependency property.
        /// </summary>
        public static readonly DependencyProperty ElementProperty = DependencyPropHelper.Register<ActualSizePropertyProxy, FrameworkElement>("Element", OnElementPropertyChanged);

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the associated element.
        /// </summary>
        /// <value>The associated element.</value>
        public FrameworkElement Element
        {
            get { return (FrameworkElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }

        /// <summary>
        /// Gets the actual height value.
        /// </summary>
        /// <value>The actual height value.</value>
        public double ActualHeightValue
        {
            get { return this.Element == null ? 0 : this.Element.ActualHeight; }
        }

        /// <summary>
        /// Gets the actual width value.
        /// </summary>
        /// <value>The actual width value.</value>
        public double ActualWidthValue
        {
            get { return this.Element == null ? 0 : this.Element.ActualWidth; }
        }

        private static void OnElementPropertyChanged(ActualSizePropertyProxy proxy, FrameworkElement newValue, FrameworkElement oldValue)
        {
            proxy.OnElementChanged(oldValue, newValue);
        }

        private void OnElementChanged(FrameworkElement oldElement, FrameworkElement newElement)
        {
            if (newElement != null)
            {
                newElement.SizeChanged += this.element_SizeChanged;
            }
            if (oldElement != null)
            {
                oldElement.SizeChanged -= this.element_SizeChanged;
            }

            this.NotifyPropChange();
        }

        private void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.NotifyPropChange();
        }

        private void NotifyPropChange()
        {
            var handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs("ActualWidthValue"));
                handler(this, new PropertyChangedEventArgs("ActualHeightValue"));
            }
        }
    }
}
