using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;

#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    /// <summary>
    ///     Contains general helper methods.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Returns an hash aggregation of an array of elements.
        /// </summary>
        /// <param name="items">An array of elements from which to create a hash.</param>
        public static int GetHashCode(params object[] items)
        {
            if (items == null)
            {
                items = new object[0];
            }

            return items
                .Select(item => (item == null) ? 0 : item.GetHashCode())
                .Aggregate(0, (current, next) =>
                {
                    unchecked
                    {
                        return (current * 397) ^ next;
                    }
                });
        }

        /// <summary>
        ///     Wraps <see cref="Interlocked.CompareExchange{T}(ref T,T,T)"/>
        ///     for atomically setting null fields.
        /// </summary>
        /// <typeparam name="T">The type of the field to set.</typeparam>
        /// <param name="location">
        ///     The field that, if null, will be set to <paramref name="value"/>.
        /// </param>
        /// <param name="value">
        ///     If <paramref name="location"/> is null, the object to set it to.
        /// </param>
        /// <returns>true if <paramref name="location"/> was null and has now been set; otherwise, false.</returns>
        public static bool InterlockedSetIfNotNull<T>(ref T location, T value) where T : class
        {
            Contract.Requires(value != null);

            // Strictly speaking, this null check is not nessesary, but
            // while CompareExchange is fast, it's still much slower than a
            // null check.
            if (location == null)
            {
                // This is a paranoid method. In a multi-threaded environment, it's possible
                // for two threads to get through the null check before a value is set.
                // This makes sure than one and only one value is set to field.
                // This is super important if the field is used in locking, for instance.

                T valueWhenSet = Interlocked.CompareExchange<T>(ref location, value, null);
                return (valueWhenSet == null);
            }
            else
            {
                return false;
            }
        }

        /// Returns true if the provided <see cref="Exception"/> is considered 'critical'
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to evaluate for critical-ness.</param>
        /// <returns>true if the Exception is conisdered critical; otherwise, false.</returns>
        /// <remarks>
        /// These exceptions are consider critical:
        /// <list type="bullets">
        ///     <item><see cref="OutOfMemoryException"/></item>
        ///     <item><see cref="StackOverflowException"/></item>
        ///     <item><see cref="ThreadAbortException"/></item>
        ///     <item><see cref="SEHException"/></item>
        /// </list>
        /// </remarks>
        public static bool IsCriticalException(this Exception exception)
        {
            // Copied with respect from WPF WindowsBase->MS.Internal.CriticalExceptions.IsCriticalException
            // NullReferencException, SecurityException --> not going to consider these critical
            while (exception != null)
            {
                if (exception is OutOfMemoryException ||
                        exception is StackOverflowException ||
                        exception is ThreadAbortException
#if !WP7
 || exception is System.Runtime.InteropServices.SEHException
#endif
)
                {
                    return true;
                }
                exception = exception.InnerException;
            }
            return false;
        } //*** static IsCriticalException

        public static Random Rnd
        {
            get
            {
                Contract.Ensures(Contract.Result<Random>() != null);
                var r = (Random)s_random.Target;
                if (r == null)
                {
                    s_random.Target = r = new Random();
                }
                return r;
            }
        }

        [DebuggerStepThrough]
        public static void ThrowUnless(bool truth, string message = null)
        {
            ThrowUnless<Exception>(truth, message);
        }

        [DebuggerStepThrough]
        public static void ThrowUnless<TException>(bool truth, string message) where TException : Exception
        {
            if (!truth)
            {
                throw InstanceFactory.CreateInstance<TException>(message);
            }
        }

        [DebuggerStepThrough]
        public static void ThrowUnless<TException>(bool truth) where TException : Exception, new()
        {
            if (!truth)
            {
                throw new TException();
            }
        }

        private static readonly WeakReference s_random = new WeakReference(null);
    } //*** public class Util
} //*** namespace PixelLab.Common