using System;
using System.Collections.Generic;
using System.Linq;
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
        public static T CreateInstance<T, TParam>(TParam param)
        {
            Contract.Requires(typeof(T).IsVisible);
            return CreateInstance<T>(new Type[] { typeof(TParam) }, new object[] { param });
        }

        public static T CreateInstance<T, TParam1, TParam2>(TParam1 param1, TParam2 param2)
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

        public static T CreateInstance<T>(params object[] ctorParams)
        {
            return (T)typeof(T).CreateInstance(ctorParams);
        }

        public static object CreateInstance(this Type type, params object[] ctorParams)
        {
            var ctor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Single(ci => ci.IsCompatible(ctorParams));
            return ctor.Invoke(ctorParams);
        }

        public static bool IsCompatible(this ConstructorInfo ctorInfo, IList<object> ctorParams)
        {
            Contract.Requires(ctorInfo != null);
            Contract.Requires(ctorParams != null);

            var pInfos = ctorInfo.GetParameters();
            if (pInfos.Length != ctorParams.Count)
            {
                return false;
            }
            for (int i = 0; i < pInfos.Length; i++)
            {
                var type = pInfos[i].ParameterType;
                var p = ctorParams[i];
                if (!IsCompatible(type, p))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsCompatible(this Type type, object p)
        {
            if (p == null)
            {
                if (type.IsValueType)
                {
                    return false;
                }
            }
            else if (!type.IsAssignableFrom(p.GetType()))
            {
                return false;
            }
            return true;
        }
    }
}
