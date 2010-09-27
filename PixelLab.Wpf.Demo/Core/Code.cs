using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo {
  public class IsStringEmptyConverter : SimpleValueConverter<string, Visibility> {
    protected override Visibility ConvertBase(string input) {
      return input.IsNullOrWhiteSpace() ? Visibility.Collapsed : Visibility.Visible;
    }
  }

  public static class Helpers {
    public static IList<string> GetPicturePaths() {
      IList<string> picturePaths = new string[0];

      string[] commandLineArgs = null;
      try {
        commandLineArgs = Environment.GetCommandLineArgs();
      }
      catch (NotSupportedException) { }
      catch (SecurityException) { } // In an XBap


      if (commandLineArgs != null) {
        foreach (string arg in commandLineArgs) {
          picturePaths = GetPicturePaths(arg);
          if (picturePaths.Count > 0) {
            break;
          }
        }
      }

      if (picturePaths.Count == 0) {
        for (int i = 0; i < s_defaultPicturePaths.Length; i++) {
          picturePaths = GetPicturePaths(s_defaultPicturePaths[i]);
          if (picturePaths.Count > 0) {
            break;
          }
        }
      }

      return picturePaths;
    }

    internal static IList<string> GetPicturePaths(string sourceDirectory) {

      if (!string.IsNullOrEmpty(sourceDirectory)) {
        try {
          DirectoryInfo di = new DirectoryInfo(sourceDirectory);
          if (di.Exists) {
            List<string> imagePaths = new List<string>();
            foreach (string imagePath in GetImageFiles(di)) {
              imagePaths.Add(imagePath);
              if (imagePaths.Count > c_maxImageReturnCount) {
                break;
              }
            }

            return imagePaths;
          }
        }
        catch (IOException) { }
        catch (ArgumentException) { }
        catch (SecurityException) { }
      }
      return new string[0];
    }

    internal static IEnumerable<string> GetImageFiles(DirectoryInfo directory) {
      Util.RequireNotNull(directory, "directory");
      foreach (FileInfo image in directory.GetFiles(c_defaultImageSearchPattern)) {
        yield return image.FullName;
      }
      foreach (DirectoryInfo subDir in directory.GetDirectories()) {
        foreach (string subDirImage in GetImageFiles(subDir)) {
          yield return subDirImage;
        }
      }
    }

    public static IList<BitmapImage> GetBitmapImages(int maxCount) {
      IList<string> imagePaths = GetPicturePaths();
      if (maxCount < 0) {
        maxCount = imagePaths.Count;
      }

      BitmapImage[] images = new BitmapImage[Math.Min(imagePaths.Count, maxCount)];
      for (int i = 0; i < images.Length; i++) {
        images[i] = new BitmapImage(new Uri(imagePaths[i]));
      }
      return images;
    }

    private static string[] s_defaultPicturePaths = GetDefaultPicturePaths();

    private static string[] GetDefaultPicturePaths() {
      if (BrowserInteropHelper.IsBrowserHosted) {
        return new string[0];
      }
      else {
        return new string[]{@"C:\Users\Public\Pictures\Sample Pictures\",
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)};
      }
    }

    private const string c_defaultImageSearchPattern = @"*.jpg";
    private const int c_maxImageReturnCount = 50;
  }

  public class ImagePathHolder {
    public IList<string> ImagePaths {
      get {
        return Helpers.GetPicturePaths();
      }
    }
    public IList<BitmapImage> BitmapImages {
      get {
        return Helpers.GetBitmapImages(-1);
      }
    }
    public IList<BitmapImage> BitmapImages6 {
      get {
        return Helpers.GetBitmapImages(6);
      }
    }

  }
}