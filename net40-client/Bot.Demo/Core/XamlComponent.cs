using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Baml2006;
using Microsoft.Xaml.Tools.XamlDom;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo.Core
{
    public static class XamlComponent
    {
        public static IEnumerable<DemoMetadata> GetDemos(Assembly sourceAssembly)
        {
            var resourceSets = from resourceName in sourceAssembly.GetManifestResourceNames()
                               let resourceStream = sourceAssembly.GetManifestResourceStream(resourceName)
                               select new ResourceSet(resourceStream);

            var bamlEntries = from set in resourceSets
                              from entry in set.Cast<DictionaryEntry>()
                              where entry.Key is string
                              where entry.Value is Stream
                              let value = new { Path = (string)entry.Key, Stream = (Stream)entry.Value }
                              where value.Path.EndsWith(".baml")
                              select value;

            var rootTypes = from entry in bamlEntries
                            let domObject = (XamlDomObject)XamlDomServices.Load(new Baml2006Reader(entry.Stream))
                            let demoName = domObject.GetAttatchedPropertyValueOrDefault<string>(DemoMetadataProperties.DemoNameProperty)
                            where demoName != null
                            let demoDescription = domObject.GetAttatchedPropertyValueOrDefault<string>(DemoMetadataProperties.DemoDescriptionProperty)
                            select new { Path = entry.Path, demoName, demoDescription };

            foreach (var entry in rootTypes)
            {
                var path = entry.Path.Replace(".baml", ".xaml");
                path = string.Format("/{0};component/{1}", sourceAssembly.GetName().Name, path);

                var uri = new Uri(path, UriKind.Relative);

                Func<FrameworkElement> factory = new Func<FrameworkElement>(() =>
                {
                    return (FrameworkElement)Application.LoadComponent(uri);
                });

                yield return new DemoMetadata(entry.demoName, entry.demoDescription, factory);
            }
        }
    }
}
