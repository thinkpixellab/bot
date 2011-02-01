using System.Collections.Generic;

#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows.Input;

namespace PixelLab.Common
{
    public class CommandKeyMapper
    {
        public void AddKeyBinding(ICommand command, Key key, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            AddKeyBinding(command, null, key, modifierKeys);
        }

        public void AddKeyBinding(ICommand command, object param, Key key, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            Contract.Requires(command != null);

            var binding = new KeyBinding(key, modifierKeys);
            m_keyBindings.Add(binding, new CommandParamPair(command, param));
        }

        public bool TryExecuteKeyboardCommand(Key key, ModifierKeys modifierKeys)
        {
            CommandParamPair commandPair;
            if (m_keyBindings.TryGetValue(new KeyBinding(key, modifierKeys), out commandPair))
            {
                if (commandPair.Command.CanExecute(commandPair.Param))
                {
                    commandPair.Command.Execute(commandPair.Param);
                    return true;
                }
            }

            return false;
        }

        private readonly Dictionary<KeyBinding, CommandParamPair> m_keyBindings = new Dictionary<KeyBinding, CommandParamPair>();

        private class CommandParamPair
        {
            public CommandParamPair(ICommand command, object param)
            {
                Command = command;
                Param = param;
            }

            public ICommand Command { get; private set; }
            public object Param { get; private set; }
        }
    }
}
