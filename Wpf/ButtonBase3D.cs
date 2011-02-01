using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PixelLab.Wpf
{
    public class ButtonBase3D : UIElement3D
    {
        public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(ButtonBase3D));

        protected virtual void OnClick()
        {
            RoutedEventArgs e = new RoutedEventArgs(ClickEvent, this);
            base.RaiseEvent(e);
        }

        public event RoutedEventHandler Click
        {
            add
            {
                base.AddHandler(ClickEvent, value);
            }
            remove
            {
                base.RemoveHandler(ClickEvent, value);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            OnClick();
            base.OnMouseLeftButtonDown(e);
        }
    }
}
