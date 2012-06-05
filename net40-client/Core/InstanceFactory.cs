using System;

namespace PixelLab.Common
{
    public static class InstanceFactory
    {
        /// <summary>
        /// A generic convenience method to create the provided type.
        /// </summary>
        public static T CreateInstance<T>(params object[] args)
        {
            return (T)typeof(T).CreateInstance(args);
        }

        /// <summary>
        /// A convenience extension method for Type that calls Activator.CreateInstance
        /// </summary>
        /// <returns>A new instance of the provided object.</returns>
        public static object CreateInstance(this Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }
    }
}
