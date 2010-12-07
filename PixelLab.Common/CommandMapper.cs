using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PixelLab.Common
{
    public class CommandMapper
    {
        public CommandMapper()
        {
            m_manager = new CommandHandler(this);
        }

        public CommandHandler CommandManager { get { return m_manager; } }

        public void AddCommand(string commandName, Action executeAction, Func<bool> canExecuteFunc = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(commandName));
            Contract.Requires(executeAction != null);
            m_commands.Add(commandName, new CommandData(executeAction, canExecuteFunc));
        }

        public void AddKeyBinding(string commandName, Key key, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(commandName));
            Contract.Requires(HasCommand(commandName));
            var binding = new KeyBinding(key, modifierKeys);
            m_keyBindings.Add(binding, commandName);
        }

        public void CanExecuteChanged(string commandName)
        {
            Contract.Requires<ArgumentException>(HasCommand(commandName), "The provided command name is not owned by this mapper.");
            WeakEnumerable<MappedCommand> commands;
            if (m_mappedCommands.TryGetValue(commandName, out commands))
            {
                Contract.Assume(commands != null);
                commands.ForEach(c => c.OnCanExecuteChanged());
            }
        }

        public bool TryExecuteKeyboardCommand(Key key, ModifierKeys modifierKeys)
        {
            string commandName;
            if (m_keyBindings.TryGetValue(new KeyBinding(key, modifierKeys), out commandName))
            {
                if (CanExecuteCommand(commandName))
                {
                    ExecuteCommand(commandName);
                    return true;
                }
            }
            return false;
        }

        [Pure]
        public bool HasCommand(string commandName)
        {
            return m_commands.ContainsKey(commandName);
        }

        #region CommandProperty

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(string), typeof(MappedCommand), null);

        public static void SetCommand(FrameworkElement element, string value)
        {
            Util.RequireNotNull(element, "element");
            Util.RequireNotNullOrEmpty(value, "value");

            if (element is ButtonBase)
            {
                var button = (ButtonBase)element;
                button.SetValue(CommandProperty, value);
                button.Loaded += commandElement_loaded;
            }
            else
            {
                Debug.WriteLine("MappedCommand: Command property of {0} set on {1}. We don't support that yet.", value, element);
            }
        }

        public static string GetCommand(FrameworkElement element)
        {
            Contract.Requires(element != null);
            return (string)element.GetValue(CommandProperty);
        }

        #endregion

        #region Implementation

        private void ExecuteCommand(string commandName)
        {
            CommandData commandData;
            if (m_commands.TryGetValue(commandName, out commandData))
            {
                Contract.Assume(CanExecuteCommand(commandName), "Should not execute a command if CanExecute is false");
                Contract.Assume(commandData != null);
                commandData.Execute();
            }
            else
            {
                throw new ArgumentException(string.Format("The command '{0}' doesn't exist here", commandName));
            }
        }

        private bool CanExecuteCommand(string commandName)
        {
            CommandData data;
            if (m_commands.TryGetValue(commandName, out data))
            {
                Contract.Assume(data != null);
                return data.CanExecute;
            }
            else
            {
                return false;
            }
        }

        private void wire(ButtonBase button)
        {
            Contract.Requires(button != null);
            var commandName = GetCommand(button);
            Contract.Assume(!string.IsNullOrEmpty(commandName));
            Contract.Assume(HasCommand(commandName));
            Contract.Assume(button.Command == null);

            var newCommand = new MappedCommand(this, commandName);

            var mappedCommands = m_mappedCommands.EnsureItem(commandName, () => new WeakEnumerable<MappedCommand>());
            Contract.Assume(mappedCommands != null);
            mappedCommands.Add(newCommand);

            button.Command = newCommand;
            if (ToolTipService.GetToolTip(button) == null && m_commands.ContainsKey(commandName))
            {
                var commandText = commandName;

                var keyBindings = m_keyBindings.Where(kvp => kvp.Value == commandName).ToArray();
                if (keyBindings.Length == 1)
                {
                    commandText = "{0} ({1})".DoFormat(commandText, keyBindings[0].Key);
                }
                ToolTipService.SetToolTip(button, commandText);
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(m_mappedCommands != null);
            Contract.Invariant(m_commands != null);
        }

        private readonly Dictionary<string, CommandData> m_commands = new Dictionary<string, CommandData>();
        private readonly Dictionary<string, WeakEnumerable<MappedCommand>> m_mappedCommands = new Dictionary<string, WeakEnumerable<MappedCommand>>();
        private readonly Dictionary<KeyBinding, string> m_keyBindings = new Dictionary<KeyBinding, string>();
        private readonly CommandHandler m_manager;

        #endregion

        #region static impl

        private static void commandElement_loaded(object sender, EventArgs args)
        {
            Contract.Requires(sender != null);
            ((ButtonBase)sender).Loaded -= commandElement_loaded;
            tryWire((ButtonBase)sender);
        }

        private static void tryWire(ButtonBase button)
        {
            Contract.Requires(button != null);
            var value = (string)button.GetValue(CommandProperty);

            button.Loaded -= commandElement_loaded;

            var mapper = FindMapper(button, value);
            if (mapper != null)
            {
                mapper.wire(button);
            }
            else
            {
                button.IsEnabled = false;
                Debug.WriteLine("MappedCommand: Could not find an owner for {0}. Disabling element {1}.", value, button);
            }
        }

        private static CommandMapper FindMapper(UIElement element, string commandName)
        {
            Contract.Requires(element != null);
            return element.GetAncestors()
                  .OfType<IMapCommandTarget>()
                  .Select(target => target.Handler.Mapper)
                  .FirstOrDefault(mapper => mapper.HasCommand(commandName));

        }

        #endregion

        private class MappedCommand : ICommand
        {
            public MappedCommand(CommandMapper mapper, string commandName)
            {
                Contract.Requires(mapper != null);
                Contract.Requires(!string.IsNullOrEmpty(commandName));
                m_mapper = mapper;
                m_commandName = commandName;
            }

            public bool CanExecute(object parameter)
            {
                return m_mapper.CanExecuteCommand(m_commandName);
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                m_mapper.ExecuteCommand(m_commandName);
            }

            internal void OnCanExecuteChanged()
            {
                var handler = CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(m_mapper != null);
                Contract.Invariant(!string.IsNullOrEmpty(m_commandName));
            }

            private readonly CommandMapper m_mapper;
            private readonly string m_commandName;
        }

        private class CommandData
        {
            public CommandData(Action execute, Func<bool> canExecute)
            {
                Contract.Requires(execute != null);
                m_execute = execute;
                m_canExecute = canExecute;
            }

            public Action Execute
            {
                get
                {
                    Contract.Ensures(Contract.Result<Action>() != null);
                    return m_execute;
                }
            }

            public bool CanExecute
            {
                get
                {
                    if (m_canExecute == null)
                    {
                        return true;
                    }
                    else
                    {
                        return m_canExecute();
                    }
                }
            }

            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(m_execute != null);
            }

            private readonly Action m_execute;
            private readonly Func<bool> m_canExecute;
        }
    }
}
