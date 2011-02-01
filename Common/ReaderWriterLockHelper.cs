using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace PixelLab.Common
{
    public static class ReaderWriterLockHelper
    {
        public static IDisposable BeginReadLock(this ReaderWriterLockSlim slimLock)
        {
            Contract.Requires(slimLock != null);
            slimLock.EnterReadLock();
            return new ActionOnDispose(slimLock.ExitReadLock);
        }

        public static IDisposable BeginWriteLock(this ReaderWriterLockSlim slimLock)
        {
            Contract.Requires(slimLock != null);
            slimLock.EnterWriteLock();
            return new ActionOnDispose(slimLock.ExitWriteLock);
        }
    } //*** class LockHelper
} //*** namespace PixelLab.Common
