using System.Windows;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.SL
{
    public static class ResourceHelpers
    {
        public static bool TryFindResource<T>(this FrameworkElement element, object resourceKey, out T value)
        {
            while (element != null)
            {
                if (element.Resources.TryGetResource(resourceKey, out value))
                {
                    return true;
                }
                element = element.Parent as FrameworkElement;
            }

            return Application.Current.Resources.TryGetResource(resourceKey, out value);
        }

        public static bool TryGetResource<T>(this ResourceDictionary dictionary, object key, out T value)
        {
            Contract.Requires(dictionary != null);
            if (dictionary.Contains(key))
            {
                var val = dictionary[key];
                if (val is T)
                {
                    value = (T)val;
                    return true;
                }
            }
            value = default(T);
            return false;
        }
    }
}
