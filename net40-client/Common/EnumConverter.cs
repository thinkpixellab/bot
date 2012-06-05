using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace PixelLab.Common
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = parameter as string ?? string.Empty;

            var valueName = getName(value);

            var match = m_regex.Match(param);

            if (match.Success)
            {
                // 1 -> source value
                // 2 -> target value
                var source = match.Groups[1].Value;
                if (source.Equals(valueName, StringComparison.InvariantCultureIgnoreCase))
                {
                    // we have a match! use the target value
                    var target = match.Groups[2].Value;
                    var targetValue = parse(targetType, target);
                    return targetValue;
                }
                else
                {
                    // no match, use the fallback
                    if (match.Groups[4].Success)
                    {
                        // user provided fallback at index 4
                        string elseString = match.Groups[4].Value;
                        var elseValue = parse(targetType, elseString);
                        return elseValue;
                    }
                    else
                    {
                        // return unset value
                        return DependencyProperty.UnsetValue;
                    }
                }
            }
            else
            {
                Debug.WriteLine("Could not parse the provided paramater: '{0}'", parameter);
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static string getName(object value)
        {
            if (value is Enum)
            {
                var enumType = value.GetType();
                return Enum.GetName(enumType, value);
            }
            else if (value is bool)
            {
                return (bool)value ? bool.TrueString : bool.FalseString;
            }
            else if (value is string)
            {
                return (string)value;
            }
            else if (value == null)
            {
                return "null";
            }
            else
            {
                return value.ToString();
            }
        }

        private static object parse(Type targetType, string value)
        {
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value, true);
            }
            else if (targetType == typeof(bool))
            {
                return bool.Parse(value);
            }
            else if (targetType == typeof(string))
            {
                return value;
            }
            else
            {
                throw new ArgumentException("Can only target boolean, string, and enum properties.");
            }
        }

        private readonly Regex m_regex = new Regex(s_match, RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private static readonly string s_match = "^({0})->({1})(,({2}))?$".DoFormat(c_validName, c_validName, c_validName);
        private const string c_validName = "[a-zA-Z_][a-zA-Z0-9_]*";
    }
}
