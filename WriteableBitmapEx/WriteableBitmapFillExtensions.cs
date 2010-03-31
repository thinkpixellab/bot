#region Header
//
//   Project:           WriteableBitmapEx - Silverlight WriteableBitmap extensions
//   Description:       Collection of draw extension methods for the Silverlight WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2010-03-05 15:55:46 -0800 (Fri, 05 Mar 2010) $
//   Changed in:        $Revision: 40760 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapFillExtensions.cs $
//   Id:                $Id: WriteableBitmapFillExtensions.cs 40760 2010-03-05 23:55:46Z unknown $
//
//
//   Copyright © 2009-2010 Rene Schulte and WriteableBitmapEx Contributors
//
//   This Software is weak copyleft open source. Please read the License.txt for details.
//
#endregion

namespace System.Windows.Media.Imaging {
  /// <summary>
  /// Collection of draw extension methods for the Silverlight WriteableBitmap class.
  /// </summary>
  public static partial class WriteableBitmapExtensions {
    #region Methods

    #region Fill Shapes

    /// <summary>
    /// Draws a filled rectangle.
    /// x2 has to be greater than x1 and y2 has to be greater than y1.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
    /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
    /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
    /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
    /// <param name="color">The color.</param>
    public static void FillRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color) {
      bmp.FillRectangle(x1, y1, x2, y2, (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// x2 has to be greater than x1 and y2 has to be greater than y1.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
    /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
    /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
    /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
    /// <param name="color">The color.</param>
    public static void FillRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color) {
      // Use refs for faster access (really important!) speeds up a lot!
      int w = bmp.PixelWidth;
      int h = bmp.PixelWidth;
      int[] pixels = bmp.Pixels;

      // Check boundaries
      if (x1 < 0) { x1 = 0; }
      if (y1 < 0) { y1 = 0; }
      if (x2 < 0) { x2 = 0; }
      if (y2 < 0) { y2 = 0; }
      if (x1 >= w) { x1 = w - 1; }
      if (y1 >= h) { y1 = h - 1; }
      if (x2 >= w) { x2 = w - 1; }
      if (y2 >= h) { y2 = h - 1; }


      // Fill first line
      int startY = y1 * w;
      int startYPlusX1 = startY + x1;
      int endOffset = startY + x2;
      for (int x = startYPlusX1; x <= endOffset; x++) {
        pixels[x] = color;
      }

      // Copy first line
      int len = (x2 - x1) * SizeOfARGB;
      int srcOffsetBytes = startYPlusX1 * SizeOfARGB;
      int offset2 = y2 * w + x1;
      for (int y = startYPlusX1 + w; y < offset2; y += w) {
        Buffer.BlockCopy(pixels, srcOffsetBytes, pixels, y * SizeOfARGB, len);
      }
    }

    #endregion

    #endregion
  }
}