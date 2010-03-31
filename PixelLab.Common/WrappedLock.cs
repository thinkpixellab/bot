using System;
using System.Collections.Generic;

namespace PixelLab.Common {

  public class WrappedLock : WrappedLock<object> {
    public WrappedLock(Action actionOnLock, Action actionOnUnlock) : base(WrapMaybeNull(actionOnLock), WrapMaybeNull(actionOnUnlock)) { }

    public IDisposable GetLock() {
      return GetLock(null);
    }

    private static Action<object> WrapMaybeNull(Action action) {
      return (param) => {
        if (action != null) { action(); }
      };
    }
  }

  public class WrappedLock<T> {
    public WrappedLock(Action<T> actionOnLock, Action<T> actionOnUnlock) {
      m_actionOnLock = actionOnLock ?? new Action<T>((param) => { });
      m_actionOnUnlock = actionOnUnlock ?? new Action<T>((param) => { });
    }

    public IDisposable GetLock(T param) {
      if (m_stack.Count == 0) {
        m_actionOnLock(param);
      }

      var g = Guid.NewGuid();
      m_stack.Add(g);
      return new ActionOnDispose(() => unlock(g, param));
    }

    public bool IsLocked { get { return m_stack.Count > 0; } }

    private void unlock(Guid g, T param) {
      if (m_stack.Count > 0 && m_stack[m_stack.Count - 1] == g) {
        m_stack.RemoveLast();

        if (m_stack.Count == 0) {
          m_actionOnUnlock(param);
        }
      }
      else {
        throw new InvalidOperationException("Unlock happened in the wrong order or at a weird time or too many times");
      }
    }

    readonly List<Guid> m_stack = new List<Guid>();
    readonly Action<T> m_actionOnUnlock;
    readonly Action<T> m_actionOnLock;
  }
}
