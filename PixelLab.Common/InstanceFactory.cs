using System;
using System.Diagnostics;
using System.Reflection;

namespace PixelLab.Common
{
    public static class InstanceFactory
    {
        public static T GetInstance<T, TP1>(TP1 param)
        {
            return GetInstance<T>(new Type[] { typeof(TP1) }, new object[] { param });
        }

        private static T GetInstance<T>(Type[] paramTypes, object[] ctorParams)
        {
            Debug.Assert(paramTypes != null);
            Debug.Assert(ctorParams != null);
            Debug.Assert(paramTypes.Length == ctorParams.Length);

            var type = typeof(T);
            if (type.IsVisible)
            {
                var ctorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null);

                if (ctorInfo != null)
                {
                    return (T)ctorInfo.Invoke(ctorParams);
                }
                else
                {
                    throw new ArgumentException("The provided type does not have an empty constructor");
                }
            }
            else
            {
                throw new ArgumentException("The provided type is not visible.");
            }
        }
    }
}
