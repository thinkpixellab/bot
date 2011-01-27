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
            Contract.Requires(command != null);

            var binding = new KeyBinding(key, modifierKeys);
            m_keyBindings.Add(binding, command);
        }

        public bool TryExecuteKeyboardCommand(Key key, ModifierKeys modifierKeys)
        {
            ICommand command;
            if (m_keyBindings.TryGetValue(new KeyBinding(key, modifierKeys), out command))
            {
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                    return true;
                }
            }
            return false;
        }

        private readonly Dictionary<KeyBinding, ICommand> m_keyBindings = new Dictionary<KeyBinding, ICommand>();
    }
}
