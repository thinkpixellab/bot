#region Header
//
//   Project:           WriteableBitmapEx - Silverlight WriteableBitmap extensions
//   Description:       Collection of transformation extension methods for the Silverlight WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2010-02-04 14:49:02 -0800 (Thu, 04 Feb 2010) $
//   Changed in:        $Revision: 39119 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapTransformationExtensions.cs $
//   Id:                $Id: WriteableBitmapTransformationExtensions.cs 39119 2010-02-04 22:49:02Z unknown $
//
//
//   Copyright © 2009-2010 Rene Schulte and WriteableBitmapEx Contributors
//
//   This Software is weak copyleft open source. Please read the License.txt for details.
//
#endregion

namespace System.Windows.Media.Imaging {
  /// <summary>
  /// Collection of transformation extension methods for the Silverlight WriteableBitmap class.
  /// </summary>
  public static partial class WriteableBitmapExtensions {
    #region Enum

    /// <summary>
    /// The interpolation method.
    /// </summary>
    public enum Interpolation {
      /// <summary>
      /// The nearest neighbor algorithm simply selects the color of the nearest pixel.
      /// </summary>
      NearestNeighbor = 0,

      /// <summary>
      /// Linear interpolation in 2D using the average of 3 neighboring pixels.
      /// </summary>
      Bilinear,
    }

    #endregion

    #region Methods

    #region Crop

    /// <summary>
    /// Creates a new cropped WriteableBitmap.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate of the rectangle that defines the crop region.</param>
    /// <param name="y">The y coordinate of the rectangle that defines the crop region.</param>
    /// <param name="width">The width of the rectangle that defines the crop region.</param>
    /// <param name="height">The height of the rectangle that defines the crop region.</param>
    /// <returns>A new WriteableBitmap that is a cropped version of the input.</returns>
    public static WriteableBitmap Crop(this WriteableBitmap bmp, int x, int y, int width, int height) {
      WriteableBitmap result = new WriteableBitmap(width, height);
      int srcWidth = bmp.PixelWidth;
      for (int line = 0; line < height; line++) {
        var srcOff = ((y + line) * srcWidth + x) * SizeOfARGB;
        var dstOff = line * width * SizeOfARGB;
        Buffer.BlockCopy(bmp.Pixels, srcOff, result.Pixels, dstOff, width * SizeOfARGB);
      }
      return result;
    }

    /// <summary>
    /// Creates a new cropped WriteableBitmap.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="region">The rectangle that defines the crop region.</param>
    /// <returns>A new WriteableBitmap that is a cropped version of the input.</returns>
    public static WriteableBitmap Crop(this WriteableBitmap bmp, Rect region) {
      return bmp.Crop((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height);
    }

    #endregion

    #region Resize

    /// <summary>
    /// Creates a new resized WriteableBitmap.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="width">The new desired width.</param>
    /// <param name="height">The new desired height.</param>
    /// <param name="interpolation">The interpolation method that should be used.</param>
    /// <returns>A new WriteableBitmap that is a resized version of the input.</returns>
    public static WriteableBitmap Resize(this WriteableBitmap bmp, int width, int height, Interpolation interpolation) {
      // Init vars
      var ps = bmp.Pixels;
      var ws = bmp.PixelWidth;
      var hs = bmp.PixelHeight;

      WriteableBitmap result = new WriteableBitmap(width, height);
      var pd = result.Pixels;
      float xs = (float)ws / width;
      float ys = (float)hs / height;

      float fracx, fracy, ifracx, ifracy, sx, sy, l0, l1;
      int c, x0, x1, y0, y1;
      byte c1a, c1r, c1g, c1b, c2a, c2r, c2g, c2b, c3a, c3r, c3g, c3b, c4a, c4r, c4g, c4b;
      byte a = 0, r = 0, g = 0, b = 0;

      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          sx = x * xs;
          sy = y * ys;
          x0 = (int)Math.Floor(sx);
          y0 = (int)Math.Floor(sy);

          // Nearest Neighbor
          if (interpolation == Interpolation.NearestNeighbor) {
            pd[y * width + x] = ps[y0 * ws + x0];
          }

          // Bilinear
          else if (interpolation == Interpolation.Bilinear) {
            // Calculate coordinates of the 4 interpolation points
            fracx = sx - x0;
            fracy = sy - y0;
            ifracx = 1f - fracx;
            ifracy = 1f - fracy;
            x1 = x0 + 1;
            if (x1 >= ws)
              x1 = x0;
            y1 = y0 + 1;
            if (y1 >= hs)
              y1 = y0;


            // Read source color
            c = ps[y0 * ws + x0];
            c1a = (byte)(c >> 24);
            c1r = (byte)(c >> 16);
            c1g = (byte)(c >> 8);
            c1b = (byte)(c);

            c = ps[y0 * ws + x1];
            c2a = (byte)(c >> 24);
            c2r = (byte)(c >> 16);
            c2g = (byte)(c >> 8);
            c2b = (byte)(c);

            c = ps[y1 * ws + x0];
            c3a = (byte)(c >> 24);
            c3r = (byte)(c >> 16);
            c3g = (byte)(c >> 8);
            c3b = (byte)(c);

            c = ps[y1 * ws + x1];
            c4a = (byte)(c >> 24);
            c4r = (byte)(c >> 16);
            c4g = (byte)(c >> 8);
            c4b = (byte)(c);


            // Calculate colors
            // Alpha
            l0 = ifracx * c1a + fracx * c2a;
            l1 = ifracx * c3a + fracx * c4a;
            a = (byte)(ifracy * l0 + fracy * l1);

            if (a > 0) {
              // Red
              l0 = ifracx * c1r * c1a + fracx * c2r * c2a;
              l1 = ifracx * c3r * c3a + fracx * c4r * c4a;
              r = (byte)((ifracy * l0 + fracy * l1) / a);

              // Green
              l0 = ifracx * c1g * c1a + fracx * c2g * c2a;
              l1 = ifracx * c3g * c3a + fracx * c4g * c4a;
              g = (byte)((ifracy * l0 + fracy * l1) / a);

              // Blue
              l0 = ifracx * c1b * c1a + fracx * c2b * c2a;
              l1 = ifracx * c3b * c3a + fracx * c4b * c4a;
              b = (byte)((ifracy * l0 + fracy * l1) / a);
            }

            // Write destination
            pd[y * width + x] = (a << 24) | (r << 16) | (g << 8) | b;
          }
        }
      }

      return result;
    }

    #endregion

    #endregion
  }
}