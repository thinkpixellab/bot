/*
The MIT License

Copyright (c) 2010 Pixel Lab

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

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
