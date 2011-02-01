using System;
using System.Reflection;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    public static class InstanceFactory
    {
        public static T GetInstance<T, TParam>(TParam param)
        {
            Contract.Requires(typeof(T).IsVisible);
            return CreateInstance<T>(new Type[] { typeof(TParam) }, new object[] { param });
        }

        public static T GetInstance<T, TParam1, TParam2>(TParam1 param1, TParam2 param2)
        {
            Contract.Requires(typeof(T).IsVisible);
            return CreateInstance<T>(new Type[] { typeof(TParam1), typeof(TParam2) }, new object[] { param1, param2 });
        }

        public static T CreateInstance<T>(Type[] paramTypes, object[] ctorParams)
        {
            Contract.Requires(paramTypes != null);
            Contract.Requires(ctorParams != null);
            Contract.Requires(paramTypes.Length == ctorParams.Length);
            Contract.Requires(typeof(T).IsVisible);

            var type = typeof(T);
            return (T)CreateInstance(type, paramTypes, ctorParams);
        }

        public static object CreateInstance(Type type, Type[] paramTypes, object[] ctorParams)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.IsVisible);
            Contract.Requires(paramTypes != null);
            Contract.Requires(ctorParams != null);
            Contract.Requires(paramTypes.Length == ctorParams.Length);

            var ctorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null);
            if (ctorInfo != null)
            {
                return ctorInfo.Invoke(ctorParams);
            }
            else
            {
                throw new ArgumentException("The provided type does not have a matching constructor");
            }
        }
    }
}
