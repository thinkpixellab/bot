using System;
using System.Windows.Input;

namespace PixelLab.Common
{
    public class DeadCommand : ICommand
    {
        private DeadCommand() { }

        public bool CanExecute(object parameter)
        {
            return false;
        }

        public void Execute(object parameter)
        {
            throw new NotSupportedException("This is a dead command. It never works.");
        }

        public static readonly ICommand Instance = new DeadCommand();

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
