using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Threading;

namespace PixelLab.Common
{
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
    public sealed class ActionOnDispose : IDisposable
    {
        /// <summary>
        ///     Creats a new <see cref="ActionOnDispose"/>
        ///     using the provided <see cref="Action"/>.
        /// </summary>
        /// <param name="unlockAction">
        ///     The <see cref="Action"/> to invoke when <see cref="Dispose"/> is called.
        /// </param>
        /// <exception cref="ArgumentNullException">if <paramref name="unlockAction"/> is null.</exception>
        public ActionOnDispose(Action unlockAction)
        {
            Contract.Requires(unlockAction != null);

            m_unlockDelegate = unlockAction;
        }

        /// <summary>
        ///     Calls the provided Action if it has not been called;
        ///     otherwise, throws an <see cref="Exception"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="Dispose()"/> has already been called.</exception>
        public void Dispose()
        {
            Action action = Interlocked.Exchange(ref m_unlockDelegate, null);
            Util.ThrowUnless<ObjectDisposedException>(action != null, "Dispose has already been called on this object.");
            action();
        }

        #region Implementation

        private Action m_unlockDelegate;

        #endregion
    } //*** class ActionOnDispose
} //*** PixelLab.Common
