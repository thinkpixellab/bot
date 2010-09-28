using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Baml2006;
using System.Xaml;
using PixelLab.Wpf.Demo;

namespace PixelLab.Working {
  public static class XamlComponent {

    public static void LoadDemos() {

      var assembly = typeof(App).Assembly;

      var resourceSets = from resourceName in assembly.GetManifestResourceNames()
                         let resourceStream = assembly.GetManifestResourceStream(resourceName)
                         select new ResourceSet(resourceStream);

      var bamlEntries = from set in resourceSets
                        from entry in set.Cast<DictionaryEntry>()
                        where entry.Key is string
                        where entry.Value is Stream
                        let value = new { Name = (string)entry.Key, Stream = (Stream)entry.Value }
                        where value.Name.EndsWith(".baml")
                        select value;

      var rootTypes = from entry in bamlEntries
                      let reader = new Baml2006Reader(entry.Stream)
                      let type = (from myReader in reader.Enumerate()
                                  where myReader.NodeType == System.Xaml.XamlNodeType.StartObject
                                  select new { myReader.Type, Name = getDemoName(myReader) })
                                            .FirstOrDefault()
                      where type != null
                      where type.Type.UnderlyingType != null
                      where type.Type.UnderlyingType.IsSubclassOf(typeof(FrameworkElement))
                      let result = new { entry.Name, entry.Stream, type.Type, DemoName = type.Name }
                      where result.DemoName != null
                      select result;

      foreach (var entry in rootTypes) {
        var path = entry.Name.Replace(".baml", ".xaml");
        path = string.Format("/{0};component/{1}", assembly.GetName().Name, path);

        var uri = new Uri(path, UriKind.Relative);
        var thing = entry.Type.Invoker.CreateInstance(new object[0]);

        Debug.WriteLine(thing.GetType());
      }
    }


    private static string getDemoName(XamlReader reader) {
      Debug.Assert(reader.NodeType == XamlNodeType.StartObject);
      var subReader = reader
        .ReadSubtree()
        .ReadMembers()
        .Where(r => r.Member.DeclaringType.UnderlyingType == typeof(DemoMetadataProperties))
        .Where(r => r.Member.Name == DemoMetadataProperties.DemoNameProperty.Name)
        .FirstOrDefault();

      if (subReader != null) {
        subReader.Read(); // move to value, hopefully
        if (subReader.NodeType == XamlNodeType.Value) {
          return (string)subReader.Value;
        }
      }

      return null;
    }
  }


}
