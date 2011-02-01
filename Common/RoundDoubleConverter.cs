using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    public class RoundDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val;
            int decimanlPlaces = 1;

            if (value is string)
            {
                val = double.Parse((string)value);
            }
            else
            {
                val = (double)value;
            }

            if (parameter is string)
            {
                decimanlPlaces = int.Parse((string)parameter);
            }
            else if (parameter != null)
            {
                decimanlPlaces = (int)parameter;
            }

            return convert(targetType, val, decimanlPlaces);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        private static object convert(Type targetType, double val, int decimalPlaces)
        {
            Contract.Requires<ArgumentOutOfRangeException>(decimalPlaces >= 0);

            if (targetType == typeof(string))
            {
                string zeroFormat = new string(Enumerable.Repeat('0', decimalPlaces).ToArray());
                return val.ToString("0." + zeroFormat);
            }
            else
            {
                val = Math.Round(val, decimalPlaces);
                return val;
            }
        }
    }

    public class RoundFormatConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val;
            string format;

            if (value is string)
            {
                val = double.Parse((string)value);
            }
            else if (value is double)
            {
                val = (double)value;
            }
            else
            {
                throw new ArgumentException();
            }

            format = (string)parameter;

            return val.ToString(format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}