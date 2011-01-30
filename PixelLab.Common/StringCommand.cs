using System;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PixelLab.Common
{
    public class StringCommand
    {
        public static readonly DependencyProperty CommandStringProperty = DependencyPropHelper.RegisterAttached<StringCommand, FrameworkElement, string>("CommandString", element_commandStringChanged);

        public static string GetCommandString(FrameworkElement element)
        {
            Contract.Requires(element != null);
            return (string)element.GetValue(CommandStringProperty);
        }

        public static void SetCommandString(FrameworkElement element, string value)
        {
            element.SetValue(CommandStringProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyPropHelper.RegisterAttached<StringCommand, FrameworkElement, ICommand>("Command", element_commandChanged);

        public static ICommand GetCommand(FrameworkElement element)
        {
            Contract.Requires(element != null);
            return (ICommand)element.GetValue(CommandProperty);
        }

        public static void SetCommand(FrameworkElement element, ICommand value)
        {
            Contract.Requires(element != null);
            element.SetValue(CommandProperty, value);
        }

        #region impl

        private static void element_commandStringChanged(FrameworkElement source, string newValue, string oldValue)
        {
            Debug.Assert(source != null);
            Debug.Assert(!newValue.IsNullOrWhiteSpace());
            Debug.Assert(oldValue == null);

            source.Loaded += commandStringElement_loaded;
        }

        private static void commandStringElement_loaded(object sender, EventArgs args)
        {
            var element = (FrameworkElement)sender;
            element.Loaded -= commandStringElement_loaded;
            commandStringElement_loaded(element);
        }

        private static void commandStringElement_loaded(FrameworkElement element)
        {
            Contract.Requires(element != null);
            var commandName = GetCommandString(element);
            var value = (from anscestor in element.GetAncestors().OfType<ICommandProxy>()
                         let command = TryGetCommand(anscestor, element, commandName)
                         where command != null
                         select command).FirstOrDefault();

            if (value == null)
            {
                DebugTrace.WriteLine("Could not bind command string '{0}' to {1}", commandName, element);
                value = DeadCommand.Instance;
            }
            SetCommand(element, value);
        }

        private static ICommand TryGetCommand(ICommandProxy proxy, DependencyObject source, string name)
        {
            Contract.Requires(proxy != null);
            Contract.Requires(source != null);
            Contract.Requires(!name.IsNullOrWhiteSpace());

            var realSource = proxy.GetCommandOwner(source);
            if (realSource != null)
            {
                var propInfo = (from property in realSource.GetType().GetProperties()
                                where property.CanRead
                                where property.Name == name
                                where typeof(ICommand).IsAssignableFrom(property.PropertyType)
                                select property).SingleOrDefault();

                if (propInfo != null)
                {
                    return (ICommand)propInfo.GetGetMethod().Invoke(realSource, new object[0]);
                }
            }
            return null;
        }

        private static void element_commandChanged(FrameworkElement source, ICommand newValue, ICommand oldValue)
        {
            if (source is ButtonBase)
            {
                var button = (ButtonBase)source;
                Debug.Assert(button.Command == oldValue);
                button.Command = newValue;
            }
            else
            {
                // TODO: Do we want to ponder how to handled enabled changed? Perhaps disable the element? ...should ponder
                if (oldValue != null)
                {
                    source.MouseLeftButtonDown -= source_MouseLeftButtonDown;
                }
                if (newValue != null)
                {
                    source.MouseLeftButtonDown += source_MouseLeftButtonDown;
                }
            }
        }

        private static void source_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            Debug.Assert(!(element is ButtonBase));
            var command = GetCommand(element);
            Debug.Assert(command != null);
            var param = element.DataContext;

            if (!e.Handled && command.CanExecute(param))
            {
                e.Handled = true;
                command.Execute(param);
            }
        }

        #endregion
    }
}
