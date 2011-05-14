using System;
using System.Collections.Generic;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Threading;

namespace PixelLab.Common
{
    /// <summary>
    ///     Provides services of <see cref="Monitor"/>, but while allowing better debugging of deadlocks.
    /// </summary>
    public class LockHelper
    {
        /// <summary>
        ///     Creates a new instance of <see cref="LockHelper"/>.
        /// </summary>
        /// <param name="name">The name to give the helper. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="name"/> is empty.</exception>
        public LockHelper(string name)
        {
            Contract.Requires(!name.IsNullOrWhiteSpace());

            m_name = name;
        }

        /// <summary>
        ///     Aquires the lock.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that can be used in a C# 'lock' block.</returns>
        /// <example><code>
        /// using(lockHelper.GetLock()
        /// {
        ///     //do work here
        /// }</code></example>
        public IDisposable GetLock()
        {
            Monitor.Enter(m_lockObject);

            Thread currentThread = Thread.CurrentThread;

            m_threadStack.Push(currentThread);
            if (string.IsNullOrEmpty(currentThread.Name))
            {
                m_owningThreadName = "Unnamed - ManagedThreadId:{0}".DoFormat(currentThread.ManagedThreadId);
            }
            else
            {
                m_owningThreadName = currentThread.Name;
            }

            return new ActionOnDispose(unlock);
        }

        /// <summary>
        ///     Proxy for the <see cref="Monitor.Pulse"/> method on the current instance.
        /// </summary>
        public void Pulse()
        {
            Monitor.Pulse(m_lockObject);
        }

        /// <summary>
        ///     Proxy for the <see cref="Monitor.Wait(object)"/> method on the current instance.
        /// </summary>
        public void Wait()
        {
            Monitor.Wait(m_lockObject);
        }

        /// <summary>
        ///     Check to see if the calling thread hold this lock on this <see cref="LockHelper"/>.
        /// </summary>
        /// <returns>true if it holds the lock; otherwies, false.</returns>
        public bool CheckAccess()
        {
            return (m_threadStack.Count > 0) &&
                (Thread.CurrentThread == m_threadStack.Peek());
        }

        /// <summary>
        ///     Throws an exception if <see cref="CheckAccess()"/> would returns false.
        /// </summary>
        /// <exception cref="Exception">If <see cref="CheckAccess()"/> would returns false.</exception>
        [DebuggerStepThrough]
        public void VerifyAccess()
        {
            Util.ThrowUnless<InvalidOperationException>(CheckAccess(), "Code was run that does not have the nessesary lock.");
        }

        #region Implementation

        private void unlock()
        {
            VerifyAccess();

            //ideally, this work would be done 'after' Exit, to make sure 'Exit' succeeds
            //BUT, it must be done while the object is locked, or we'll have conflicts
            //with the code that is run after Monitor.Enter

            m_threadStack.Pop();
            if (m_threadStack.Count > 0)
            {
                Thread currentThread = m_threadStack.Peek();
                if (string.IsNullOrEmpty(currentThread.Name))
                {
                    m_owningThreadName = "Unnamed - ManagedThreadId:{0}".DoFormat(currentThread.ManagedThreadId);
                }
                else
                {
                    m_owningThreadName = currentThread.Name;
                }
            }
            else
            {
                m_owningThreadName = null;
            }

            Monitor.Exit(m_lockObject);
        }

        private string m_owningThreadName;

        private readonly Stack<Thread> m_threadStack = new Stack<Thread>();
        private readonly object m_lockObject = new object();
        private readonly string m_name;

        #endregion
    } //*** class LockHelper
} //*** namespace PixelLab.Common