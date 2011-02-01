using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PixelLab.SL
{
    public static class SilverLightExtentions
    {
        public static IEnumerable<Assembly> GetLoadedAssemblies(this Deployment deployment)
        {
            return from part in deployment.Parts
                   let stream = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative))
                   select new AssemblyPart().Load(stream.Stream);
        }
    }
}
