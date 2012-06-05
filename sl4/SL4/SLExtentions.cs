using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.SL
{
    public static class SilverLightExtentions
    {
        public static IEnumerable<Assembly> GetLoadedAssemblies(this Deployment deployment)
        {
            Contract.Requires(deployment != null);
            return from part in deployment.Parts
                   let stream = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative))
                   select new AssemblyPart().Load(stream.Stream);
        }

        public static Point GetLocationFromRootVisual(this UIElement element)
        {
            Contract.Requires(element != null);
            return element.TransformToVisual(Application.Current.RootVisual).Transform(new Point());
        }
    }
}
