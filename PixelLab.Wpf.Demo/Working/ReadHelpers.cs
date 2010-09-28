using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xaml;

namespace PixelLab.Working {
  public static class ReadHelpers {
    public static IEnumerable<T> EnumerateReader<T>(T source, Func<bool> readFunc) {
      while (readFunc()) {
        yield return source;
      }
    }

    public static IEnumerable<T> Enumerate<T>(this T reader) where T : XamlReader {
      return EnumerateReader(reader, reader.Read);
    }

    public static IEnumerable<XamlReader> ReadMembers(this XamlReader reader) {
      Debug.Assert(reader.NodeType == XamlNodeType.StartObject || reader.NodeType == XamlNodeType.None);
      int num = 0;
      while (reader.Read()) {
        if (reader.NodeType == XamlNodeType.StartMember) {
          if (num == 0) {
            yield return reader;
          }
          num++;
        }
        else if (reader.NodeType == XamlNodeType.EndMember) {
          num--;
        }
      }
    }
  }
}
