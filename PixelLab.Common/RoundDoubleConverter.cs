/*
The MIT License

Copyright (c) 2010 Pixel Lab

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PixelLab.Common {
  public class RoundDoubleConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double val;
      int decimanlPlaces = 1;

      if (value is string) {
        val = double.Parse((string)value);
      }
      else {
        val = (double)value;
      }

      if (parameter is string) {
        decimanlPlaces = int.Parse((string)parameter);
      }
      else if (parameter != null) {
        decimanlPlaces = (int)parameter;
      }

      Util.RequireArgumentRange(decimanlPlaces >= 0, "parameter");

      if (targetType == typeof(string)) {
        string zeroFormat = new string(Enumerable.Repeat('0', decimanlPlaces).ToArray());
        return val.ToString("0." + zeroFormat);
      }
      else {
        val = Math.Round(val, decimanlPlaces);
        return val;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return Convert(value, targetType, parameter, culture);
    }

  }

  public class RoundFormatConverter : IValueConverter {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double val;
      string format;

      if (value is string) {
        val = double.Parse((string)value);
      }
      else if (value is double) {
        val = (double)value;
      }
      else {
        throw new ArgumentException();
      }

      format = (string)parameter;

      return val.ToString(format);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }

    #endregion
  }
}