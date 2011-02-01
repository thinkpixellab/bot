using System;

namespace PixelLab.Common
{
    public static class InstanceFactory
    {
        public static T CreateInstance<T>(params object[] args)
        {
            return (T)typeof(T).CreateInstance(args);
        }

        public static object CreateInstance(this Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }
    }
}
