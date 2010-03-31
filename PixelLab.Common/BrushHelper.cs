using System.Collections.Generic;
using System.Windows.Media;

namespace PixelLab.Common {
  public static class BrushHelper {

    public static SolidColorBrush ToBrush(this Color color) {
      return new SolidColorBrush(color);
    }

    public static SolidColorBrush ToCachedBrush(this Color color) {
      if (s_brushes == null) {
        s_brushes = new Dictionary<Color, SolidColorBrush>();
      }
      SolidColorBrush brush;
      if (s_brushes.TryGetValue(color, out brush)) {
        return brush;
      }
      else {
        s_brushes[color] = brush = color.ToBrush();
      }
      return brush;
    }

    public static int CacheSize { get { return s_brushes == null ? 0 : s_brushes.Count; } }

    public static void ClearCache() {
      if (s_brushes != null) {
        s_brushes = null;
      }
    }

    private static Dictionary<Color, SolidColorBrush> s_brushes;

  }
}
