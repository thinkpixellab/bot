using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using PixelLab.Contracts;

namespace PixelLab.SL
{
    public class DoubleClickBehavior : Behavior<UIElement>
    {
        private const int Delta = 250;

        private long _lastDown = int.MinValue;

        public event EventHandler DoubleClick;

        protected virtual void OnDoubleClick(EventArgs e = null)
        {
            var handler = DoubleClick;
            if (handler != null)
            {
                handler(this, e ?? EventArgs.Empty);
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.AddHandler(UIElement.MouseLeftButtonDownEvent, (MouseButtonEventHandler)target_mouse_down, true);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.MouseLeftButtonDownEvent, (MouseButtonEventHandler)target_mouse_down);
        }

        public static DoubleClickBehavior Get(UIElement element)
        {
            Contract.Requires(element != null);
            return Interaction.GetBehaviors(element).OfType<DoubleClickBehavior>().SingleOrDefault();
        }

        private void target_mouse_down(object sender, MouseButtonEventArgs e)
        {
            var tick = Environment.TickCount;
            long delta = tick - _lastDown;
            if (delta < Delta)
            {
                OnDoubleClick();
                _lastDown = int.MinValue;
            }
            else
            {
                _lastDown = tick;
            }
        }
    }
}
