using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.Core
{
    public class SampleImageHelper
    {
        public IEnumerable<string> ImagePaths
        {
            get
            {
                return GetPicturePaths();
            }
        }

        public IList<BitmapImage> BitmapImages
        {
            get
            {
                return GetBitmapImages().ToArray();
            }
        }

        public IList<BitmapImage> BitmapImages6
        {
            get
            {
                return GetBitmapImages(6).ToArray();
            }
        }

        public static IEnumerable<string> GetPicturePaths(int maxCount = -1)
        {
            if (maxCount < 0)
            {
                maxCount = c_maxImageReturnCount;
            }
            else
            {
                maxCount = Math.Min(maxCount, c_maxImageReturnCount);
            }

            string[] commandLineArgs = null;
            try
            {
                commandLineArgs = Environment.GetCommandLineArgs();
            }
            catch (NotSupportedException) { }
            catch (SecurityException) { } // In an XBap

            IEnumerable<string> picturePaths = null;
            if (commandLineArgs != null)
            {
                picturePaths = commandLineArgs
                  .Select(arg => GetPicturePaths(arg))
                  .Where(paths => paths.Any())
                  .FirstOrDefault();
            }

            if (picturePaths == null)
            {
                picturePaths = s_defaultPicturePaths
                  .Select(option => GetPicturePaths(option))
                  .Where(value => value.Any())
                  .FirstOrDefault();
            }

            return picturePaths.EmptyIfNull().Take(maxCount);
        }

        public static IEnumerable<BitmapImage> GetBitmapImages(int maxCount = -1)
        {
            return from path in GetPicturePaths(maxCount)
                   select new BitmapImage(new Uri(path));
        }

        #region impl
        private static IEnumerable<string> GetPicturePaths(string sourceDirectory)
        {
            if (!string.IsNullOrEmpty(sourceDirectory))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(sourceDirectory);
                    if (di.Exists)
                    {
                        return di
                          .EnumerateFiles(c_defaultImageSearchPattern, SearchOption.AllDirectories)
                          .Select(fi => fi.FullName);
                    }
                }
                catch (IOException) { }
                catch (ArgumentException) { }
                catch (SecurityException) { }
            }
            return Enumerable.Empty<string>();
        }

        private static ReadOnlyCollection<string> s_defaultPicturePaths = GetDefaultPicturePaths();

        private static ReadOnlyCollection<string> GetDefaultPicturePaths()
        {
            if (BrowserInteropHelper.IsBrowserHosted)
            {
                return new string[0].ToReadOnlyCollection();
            }
            else
            {
                return new string[]{@"C:\Users\Public\Pictures\Sample Pictures\",
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}.ToReadOnlyCollection();
            }
        }

        private const string c_defaultImageSearchPattern = @"*.jpg";
        private const int c_maxImageReturnCount = 50;
        #endregion
    }
}