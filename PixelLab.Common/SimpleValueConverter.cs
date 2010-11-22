using System;
using System.Diagnostics.Contracts;
using System.Windows.Data;

namespace PixelLab.Common
{
    public abstract class SimpleValueConverter<TSource, TTarget> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertBase((TSource)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertBackBase((TTarget)value);
        }

        protected abstract TTarget ConvertBase(TSource input);

        protected virtual TSource ConvertBackBase(TTarget input)
        {
            throw new NotSupportedException();
        }
    }

    public class FuncValueConverter<TSource, TTarget> : SimpleValueConverter<TSource, TTarget>
    {
        public FuncValueConverter(Func<TSource, TTarget> func)
        {
            Contract.Requires(func != null);
            m_func = func;
        }
        protected override TTarget ConvertBase(TSource input)
        {
            return m_func(input);
        }

        private readonly Func<TSource, TTarget> m_func;
    }

}
