#region Header
//
//   Project:           WriteableBitmapEx - Silverlight WriteableBitmap extensions
//   Description:       Collection of draw extension methods for the Silverlight WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2010-02-15 15:16:48 -0800 (Mon, 15 Feb 2010) $
//   Changed in:        $Revision: 39790 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapBaseExtensions.cs $
//   Id:                $Id: WriteableBitmapBaseExtensions.cs 39790 2010-02-15 23:16:48Z unknown $
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
    #region Fields

    private const float PreMultiplyFactor = 1 / 255f;
    private const int SizeOfARGB = 4;

    #endregion

    #region Methods

    #region General

    /// <summary>
    /// Fills the whole WriteableBitmap with a color.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="color">The color used for filling.</param>
    public static void Clear(this WriteableBitmap bmp, Color color) {
      int col = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
      int[] pixels = bmp.Pixels;
      int w = bmp.PixelWidth;
      int h = bmp.PixelHeight;
      int len = w * SizeOfARGB;

      // Fill first line
      for (int x = 0; x < w; x++) {
        pixels[x] = col;
      }

      // Copy first line
      for (int y = 1; y < h; y++) {
        Buffer.BlockCopy(pixels, 0, pixels, y * len, len);
      }
    }

    /// <summary>
    /// Fills the whole WriteableBitmap with an empty color (0).
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    public static void Clear(this WriteableBitmap bmp) {
      Array.Clear(bmp.Pixels, 0, bmp.Pixels.Length);
    }

    /// <summary>
    /// Clones the specified WriteableBitmap.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <returns>A copy of the WriteableBitmap.</returns>
    public static WriteableBitmap Clone(this WriteableBitmap bmp) {
      WriteableBitmap result = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
      Buffer.BlockCopy(bmp.Pixels, 0, result.Pixels, 0, bmp.Pixels.Length * SizeOfARGB);
      return result;
    }

    #endregion

    #region ForEach

    /// <summary>
    /// Applies the given function to all the pixels of the bitmap in 
    /// order to set their color.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="func">The function to apply. With parameters x, y and a color as a result</param>
    public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color> func) {
      int[] pixels = bmp.Pixels;
      int w = bmp.PixelWidth;
      int h = bmp.PixelHeight;
      int index = 0;

      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w; x++) {
          var color = func(x, y);
          float ai = color.A * PreMultiplyFactor;
          pixels[index++] = (color.A << 24) | ((byte)(color.R * ai) << 16) | ((byte)(color.G * ai) << 8) | (byte)(color.B * ai);
        }
      }
    }

    /// <summary>
    /// Applies the given function to all the pixels of the bitmap in 
    /// order to set their color.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="func">The function to apply. With parameters x, y, source color and a color as a result</param>
    public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color, Color> func) {
      int[] pixels = bmp.Pixels;
      int w = bmp.PixelWidth;
      int h = bmp.PixelHeight;
      int index = 0;

      for (int y = 0; y < h; y++) {
        for (int x = 0; x < w; x++) {
          int c = pixels[index];
          var color = func(x, y, Color.FromArgb((byte)(c >> 24), (byte)(c >> 16), (byte)(c >> 8), (byte)(c)));
          float ai = color.A * PreMultiplyFactor;
          pixels[index++] = (color.A << 24) | ((byte)(color.R * ai) << 16) | ((byte)(color.G * ai) << 8) | (byte)(color.B * ai);
        }
      }
    }

    #endregion

    #region GetPixel

    /// <summary>
    /// Gets the color of the pixel at the x, y coordinate as integer.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <returns>The color of the pixel at x, y.</returns>
    public static int GetPixeli(this WriteableBitmap bmp, int x, int y) {
      return bmp.Pixels[y * bmp.PixelWidth + x];
    }

    /// <summary>
    /// Gets the color of the pixel at the x, y coordinate as a Color struct.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <returns>The color of the pixel at x, y as a Color struct.</returns>
    public static Color GetPixel(this WriteableBitmap bmp, int x, int y) {
      int c = bmp.Pixels[y * bmp.PixelWidth + x];
      return Color.FromArgb((byte)(c >> 24), (byte)(c >> 16), (byte)(c >> 8), (byte)(c));
    }

    #endregion

    #region SetPixel

    #region Without alpha

    /// <summary>
    /// Sets the color of the pixel using a precalculated index (faster).
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="index">The coordinate index.</param>
    /// <param name="r">The red value of the color.</param>
    /// <param name="g">The green value of the color.</param>
    /// <param name="b">The blue value of the color.</param>
    public static void SetPixeli(this WriteableBitmap bmp, int index, byte r, byte g, byte b) {
      bmp.Pixels[index] = (255 << 24) | (r << 16) | (g << 8) | b;
    }

    /// <summary>
    /// Sets the color of the pixel.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate (row).</param>
    /// <param name="y">The y coordinate (column).</param>
    /// <param name="r">The red value of the color.</param>
    /// <param name="g">The green value of the color.</param>
    /// <param name="b">The blue value of the color.</param>
    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte r, byte g, byte b) {
      bmp.Pixels[y * bmp.PixelWidth + x] = (255 << 24) | (r << 16) | (g << 8) | b;
    }

    #endregion

    #region With alpha

    /// <summary>
    /// Sets the color of the pixel including the alpha value and using a precalculated index (faster).
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="index">The coordinate index.</param>
    /// <param name="a">The alpha value of the color.</param>
    /// <param name="r">The red value of the color.</param>
    /// <param name="g">The green value of the color.</param>
    /// <param name="b">The blue value of the color.</param>
    public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, byte r, byte g, byte b) {
      float ai = a * PreMultiplyFactor;
      bmp.Pixels[index] = (a << 24) | ((byte)(r * ai) << 16) | ((byte)(g * ai) << 8) | (byte)(b * ai);
    }

    /// <summary>
    /// Sets the color of the pixel including the alpha value.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate (row).</param>
    /// <param name="y">The y coordinate (column).</param>
    /// <param name="a">The alpha value of the color.</param>
    /// <param name="r">The red value of the color.</param>
    /// <param name="g">The green value of the color.</param>
    /// <param name="b">The blue value of the color.</param>
    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, byte r, byte g, byte b) {
      float ai = a * PreMultiplyFactor;
      bmp.Pixels[y * bmp.PixelWidth + x] = (a << 24) | ((byte)(r * ai) << 16) | ((byte)(g * ai) << 8) | (byte)(b * ai);
    }

    #endregion

    #region With System.Windows.Media.Color

    /// <summary>
    /// Sets the color of the pixel using a precalculated index (faster).
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="index">The coordinate index.</param>
    /// <param name="color">The color.</param>
    public static void SetPixeli(this WriteableBitmap bmp, int index, Color color) {
      float ai = color.A * PreMultiplyFactor;
      bmp.Pixels[index] = (color.A << 24) | ((byte)(color.R * ai) << 16) | ((byte)(color.G * ai) << 8) | (byte)(color.B * ai);
    }

    /// <summary>
    /// Sets the color of the pixel.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate (row).</param>
    /// <param name="y">The y coordinate (column).</param>
    /// <param name="color">The color.</param>
    public static void SetPixel(this WriteableBitmap bmp, int x, int y, Color color) {
      float ai = color.A * PreMultiplyFactor;
      bmp.Pixels[y * bmp.PixelWidth + x] = (color.A << 24) | ((byte)(color.R * ai) << 16) | ((byte)(color.G * ai) << 8) | (byte)(color.B * ai);
    }

    /// <summary>
    /// Sets the color of the pixel using an extra alpha value and a precalculated index (faster).
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="index">The coordinate index.</param>
    /// <param name="a">The alpha value of the color.</param>
    /// <param name="color">The color.</param>
    public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, Color color) {
      float ai = a * PreMultiplyFactor;
      bmp.Pixels[index] = (a << 24) | ((byte)(color.R * ai) << 16) | ((byte)(color.G * ai) << 8) | (byte)(color.B * ai);
    }

    /// <summary>
    /// Sets the color of the pixel using an extra alpha value.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate (row).</param>
    /// <param name="y">The y coordinate (column).</param>
    /// <param name="a">The alpha value of the color.</param>
    /// <param name="color">The color.</param>
    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, Color color) {
      float ai = a * PreMultiplyFactor;
      bmp.Pixels[y * bmp.PixelWidth + x] = (a << 24) | ((byte)(color.R * ai) << 16) | ((byte)(color.G * ai) << 8) | (byte)(color.B * ai);
    }

    /// <summary>
    /// Sets the color of the pixel using a precalculated index (faster).
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="index">The coordinate index.</param>
    /// <param name="color">The color.</param>
    public static void SetPixeli(this WriteableBitmap bmp, int index, int color) {
      bmp.Pixels[index] = color;
    }

    /// <summary>
    /// Sets the color of the pixel.
    /// </summary>
    /// <param name="bmp">The WriteableBitmap.</param>
    /// <param name="x">The x coordinate (row).</param>
    /// <param name="y">The y coordinate (column).</param>
    /// <param name="color">The color.</param>
    public static void SetPixel(this WriteableBitmap bmp, int x, int y, int color) {
      bmp.Pixels[y * bmp.PixelWidth + x] = color;
    }

    #endregion

    #endregion

    #endregion
  }
}