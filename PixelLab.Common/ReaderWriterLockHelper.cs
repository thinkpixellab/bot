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
using System.Diagnostics;
using System.Threading;

namespace PixelLab.Common
{
    /// <summary>
    ///     Provides services of <see cref="ReaderWriterLockSlim"/>
    ///     with <c>using(){...}</c> semantics.
    /// </summary>
    public sealed class ReaderWriterLockHelper : IDisposable
    {
        #region Read

        /// <summary>
        ///     Aquires a read lock.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that can be used in a C# 'lock' block.</returns>
        /// <example><code>
        /// using(lockHelper.GetReadLock())
        /// {
        ///     //do reading here
        /// }
        /// </code></example>
        public IDisposable GetReadLock()
        {
            m_rwLockSlim.EnterReadLock();

            return new ActionOnDispose(m_rwLockSlim.ExitReadLock);
        }

        /// <summary>
        ///     Gets a value that indicates whether the current thread has entered the lock in read mode.
        /// </summary>
        /// <returns>true if it holds the lock; otherwise, false.</returns>
        public bool IsReadLockHeld { get { return m_rwLockSlim.IsReadLockHeld; } }

        /// <summary>
        ///     Throws an exception if <see cref="IsReadLockHeld"/> is false.
        /// </summary>
        /// <exception cref="Exception">If <see cref="IsReadLockHeld"/> is false.</exception>
        [DebuggerStepThrough]
        public void VerifyReadAccess()
        {
            if (!IsReadLockHeld)
            {
                throw new InvalidOperationException("Code was run that does not have the nessesary lock.");
            }
        }

        #endregion Read

        #region Write

        /// <summary>
        ///     Aquires a write lock.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that can be used in a C# 'lock' block.</returns>
        /// using(lockHelper.GetWriteLock())
        /// {
        ///     //do reading/writing here
        /// }
        public IDisposable GetWriteLock()
        {
            m_rwLockSlim.EnterWriteLock();

            return new ActionOnDispose(m_rwLockSlim.ExitWriteLock);
        }

        /// <summary>
        ///     Gets a value that indicates whether the current thread has entered the lock in write mode.
        /// </summary>
        /// <returns>true if it holds the lock; otherwise, false.</returns>
        public bool IsWriteLockHeld { get { return m_rwLockSlim.IsWriteLockHeld; } }

        /// <summary>
        ///     Throws an exception if <see cref="IsWriteLockHeld"/> is false.
        /// </summary>
        /// <exception cref="Exception">If <see cref="IsWriteLockHeld"/> is false.</exception>
        [DebuggerStepThrough]
        public void VerifyWriteAccess()
        {
            if (!IsWriteLockHeld)
            {
                throw new InvalidOperationException("Code was run that does not have the nessesary lock.");
            }
        }

        #endregion Write

        /// <summary>
        ///     Implementation of <see cref="IDisposable.Dispose()"/>.
        ///     Disposes the nested <see cref="ReaderWriterLockSlim"/>.
        /// </summary>
        public void Dispose()
        {
            m_rwLockSlim.Dispose();
        }

        #region Implementation

        private readonly ReaderWriterLockSlim m_rwLockSlim = new ReaderWriterLockSlim();

        #endregion


    } //*** class LockHelper

} //*** namespace PixelLab.Common