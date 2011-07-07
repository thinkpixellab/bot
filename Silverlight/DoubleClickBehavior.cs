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

        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "CommandParameter",
                typeof(ICommand),
                typeof(DoubleClickBehavior),
                null);

        public object CommandParameter
        {
            get
            {
                return (ICommand)GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                "CommandParameterProperty",
                typeof(object),
                typeof(DoubleClickBehavior),
                null);


        protected virtual void OnDoubleClick(EventArgs e = null)
        {
            var handler = DoubleClick;
            if (handler != null)
            {
                handler(this, e ?? EventArgs.Empty);
            }
            this.ExecuteCommand();
        }

        private void ExecuteCommand()
        {
            object commandParameter = this.CommandParameter;
            ICommand command = this.Command;
            if ((command != null) && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
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
