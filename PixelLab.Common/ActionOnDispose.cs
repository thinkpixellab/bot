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
using System.Threading;

namespace PixelLab.Common {
  /// <summary>
  ///     Provides a wrapper over <see cref="IDisposable"/> that
  ///     invokes the provided delegate when <see cref="IDisposable.Dispose()"/>
  ///     is called.
  /// </summary>
  /// <example>
  /// <code>
  /// SqlConnection conn = new SqlConnection(connectionString);
  /// using(new ActionOnDispose(new Action(conn.Close))
  /// {
  ///     // Do work here...
  ///     // For cases where you want the connection closed
  ///     // but not disposed
  /// }
  /// </code>
  /// </example>
  public sealed class ActionOnDispose : IDisposable {
    /// <summary>
    ///     Creats a new <see cref="ActionOnDispose"/>
    ///     using the provided <see cref="Action"/>.
    /// </summary>
    /// <param name="unlockAction">
    ///     The <see cref="Action"/> to invoke when <see cref="Dispose"/> is called.
    /// </param>
    /// <exception cref="ArgumentNullException">if <paramref name="unlockAction"/> is null.</exception>
    public ActionOnDispose(Action unlockAction) {
      Util.RequireNotNull(unlockAction, "unlockAction");

      m_unlockDelegate = unlockAction;
    }

    /// <summary>
    ///     Calls the provided Action if it has not been called; 
    ///     otherwise, throws an <see cref="Exception"/>.
    /// </summary>
    /// <exception cref="Exception">If <see cref="Dispose()"/> has already been called.</exception>
    public void Dispose() {
      Action action = Interlocked.Exchange(ref m_unlockDelegate, null);
      if (action != null) {
        action();
      }
      else {
        throw new InvalidOperationException("Dispose was called more than once.");
      }
    }

    #region Implementation

    private Action m_unlockDelegate;

    #endregion

  } //*** class ActionOnDispose

} //*** PixelLab.Common
