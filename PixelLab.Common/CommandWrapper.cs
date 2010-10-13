using System;
using System.Diagnostics.Contracts;
using System.Windows.Input;

namespace PixelLab.Common {
  public class CommandWrapper : CommandWrapper<object> {
    public CommandWrapper(Action action) : this(action, () => true) { }
    public CommandWrapper(Action action, Func<bool> canExecute)
      : base(param => action(), param => canExecute()) {
    }
  }

  public class CommandWrapper<T> {
    public CommandWrapper(Action<T> action)
      : this(action, (param) => true) {
      Contract.Requires(action != null);
    }
    public CommandWrapper(Action<T> action, Func<T, bool> canExecute) {
      Contract.Requires(action != null);
      Contract.Requires(canExecute != null);
      m_action = action;
      m_canExecute = canExecute;
      m_command = new CommandImpl(this);
    }

    public void UpdateCanExecute() {
      m_command.OnCanExecuteChanged();
    }

    public ICommand Command { get { return m_command; } }

    [ContractInvariantMethod]
    void ObjectInvariant() {
      Contract.Invariant(m_command != null);
      Contract.Invariant(m_action != null);
      Contract.Invariant(m_canExecute != null);
    }

    private readonly CommandImpl m_command;
    private readonly Action<T> m_action;
    private readonly Func<T, bool> m_canExecute;

    private class CommandImpl : ICommand {
      public CommandImpl(CommandWrapper<T> owner) {
        Contract.Requires(owner != null);
        m_owner = owner;
      }

      public bool CanExecute(object parameter) {
        return m_owner.m_canExecute((T)parameter);
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter) {
        m_owner.m_action((T)parameter);
      }

      internal void OnCanExecuteChanged() {
        EventHandler handler = CanExecuteChanged;
        if (handler != null) {
          handler(this, EventArgs.Empty);
        }
      }

      [ContractInvariantMethod]
      private void ObjectInvariant() {
        Contract.Invariant(m_owner != null);
      }

      private readonly CommandWrapper<T> m_owner;
    }

  }
}
