using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace PixelLab.Common {
  public class FuncEnumerable<T> : IEnumerable<T> {
    public FuncEnumerable(Func<IEnumerator<T>> func) {
      Contract.Requires<ArgumentNullException>(func != null);
      m_enumeratorFunc = func;
    }

    public IEnumerator<T> GetEnumerator() {
      return m_enumeratorFunc();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    private readonly Func<IEnumerator<T>> m_enumeratorFunc;

    public static FuncEnumerable<T> Get(Func<IEnumerator<T>> func) {
      return new FuncEnumerable<T>(func);
    }
  }
}
