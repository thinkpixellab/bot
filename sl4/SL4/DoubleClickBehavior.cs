using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using PixelLab.Common;
using PixelLab.Contracts;

namespace PixelLab.SL
{
    public class DoubleClickBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyPropHelper.Register<DoubleClickBehavior, ICommand>("Command");

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyPropHelper.Register<DoubleClickBehavior, object>("CommandParameter");

        private const int Delta = 250;

        private long _lastDown = int.MinValue;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public event EventHandler DoubleClick;

        protected virtual void OnDoubleClick(EventArgs e = null)
        {
            var handler = DoubleClick;
            if (handler != null)
            {
                handler(this, e ?? EventArgs.Empty);
            }
            this.ExecuteCommand();
        }

        protected override void OnAttached()
        {
            AssociatedObject.AddHandler(UIElement.MouseLeftButtonDownEvent, (MouseButtonEventHandler)target_mouse_down, true);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.MouseLeftButtonDownEvent, (MouseButtonEventHandler)target_mouse_down);
        }

        /// <returns>An instance of DoubleClickBehavior if one is defined, otherwise null.</returns>
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

        private void ExecuteCommand()
        {
            var commandParameter = this.CommandParameter;
            var command = this.Command;
            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }
    }
}
