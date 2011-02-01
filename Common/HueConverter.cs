using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PixelLab.Common
{
    public class HueConverter : IValueConverter
    {
        public Color Convert(int index, int count)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(count > 0);

            index = index % count;

            return ColorHelper.HsbToRgb(index / (float)count, Saturation, Brightness);
        }

        public float Saturation { get; set; }
        public float Brightness { get; set; }

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }

            int index;
            if (value is int)
            {
                index = (int)value;
            }
            else if (value is string)
            {
                index = int.Parse((string)value);
            }
            else if (value is char)
            {
                index = (int)(char)value;
            }
            else
            {
                throw new ArgumentException();
            }

            int count;
            if (parameter is int)
            {
                count = (int)parameter;
            }
            else if (parameter is string)
            {
                count = int.Parse((string)parameter);
            }
            else
            {
                throw new ArgumentException();
            }

            Color color = Convert(index, count);

            if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
            {
                return color.ToBrush();
            }
            else
            {
                return color;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}