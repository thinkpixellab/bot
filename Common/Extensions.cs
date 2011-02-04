using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Linq;

namespace PixelLab.Common
{
    /// <summary>
    ///     Contains general purpose extention methods.
    /// </summary>
    public static class Extensions
    {
        public static IEqualityComparer<T> ToEqualityComparer<T>(this Func<T, T, bool> func)
        {
            Contract.Requires(func != null);
            return new FuncEqualityComparer<T>(func);
        }

        public static IComparer<T> ToComparer<T>(this Func<T, T, int> compareFunction)
        {
            Contract.Requires(compareFunction != null);
            return new FuncComparer<T>(compareFunction);
        }

        public static TValue EnsureItem<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            Contract.Requires(dictionary != null);
            Contract.Requires(valueFactory != null);
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = valueFactory();
                dictionary.Add(key, value);
            }
            return value;
        }

        [Pure]
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }

        /// <summary>
        /// Verifies that a property name exists in this ViewModel. This method
        /// can be called before the property is used, for instance before
        /// calling RaisePropertyChanged. It avoids errors when a property name
        /// is changed but some places are missed.
        /// <para>This method is only active in DEBUG mode.</para>
        /// </summary>
        /// <param name="element">The object to watch.</param>
        /// <remarks>Thanks to Laurent Bugnion for the idea.</remarks>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void VerifyPropertyNamesOnChange(this INotifyPropertyChanged element)
        {
            Contract.Requires(element != null);
            var myType = element.GetType();
            element.PropertyChanged += (sender, args) =>
            {
                if (myType.GetProperty(args.PropertyName) == null)
                {
                    throw new ArgumentException("Property not found", args.PropertyName);
                }
            };
        }

        public static TResponse UseAndDispose<T, TResponse>(this T source, Func<T, TResponse> func) where T : IDisposable
        {
            using (source)
            {
                return func(source);
            }
        }

        public static string DoFormat(this string source, params object[] args)
        {
            Contract.Requires(source != null);
            Contract.Requires(args != null);
            return string.Format(source, args);
        }

#if SILVERLIGHT
        public static void VerifyAccess(this System.Windows.Deployment deployment)
        {
            if (!deployment.Dispatcher.CheckAccess())
            {
                throw new InvalidOperationException("A call was made off the Dispatcher thread.");
            }
        }
#endif

        public static bool NextBool(this Random rnd)
        {
            Contract.Requires(rnd != null);
            return rnd.Next() % 2 == 0;
        }

        #region impl
        private class FuncComparer<T> : IComparer<T>
        {
            public FuncComparer(Func<T, T, int> func)
            {
                Contract.Requires(func != null);
                m_func = func;
            }

            public int Compare(T x, T y)
            {
                return m_func(x, y);
            }

            [ContractInvariantMethod]
            void ObjectInvariant()
            {
                Contract.Invariant(m_func != null);
            }

            private readonly Func<T, T, int> m_func;
        }

        private class FuncEqualityComparer<T> : IEqualityComparer<T>
        {
            public FuncEqualityComparer(Func<T, T, bool> func)
            {
                Contract.Requires(func != null);
                m_func = func;
            }
            public bool Equals(T x, T y)
            {
                return m_func(x, y);
            }

            public int GetHashCode(T obj)
            {
                return 0; // this is on purpose. Should only use function...not short-cut by hashcode compare
            }

            [ContractInvariantMethod]
            void ObjectInvariant()
            {
                Contract.Invariant(m_func != null);
            }

            private readonly Func<T, T, bool> m_func;
        }
        #endregion
    }
} //*** namespace PixelLab.Common
