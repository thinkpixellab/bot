using System.Windows;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.Core
{
    public class IsStringEmptyConverter : SimpleValueConverter<string, Visibility>
    {
        protected override Visibility ConvertBase(string input)
        {
            return input.IsNullOrWhiteSpace() ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
