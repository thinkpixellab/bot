using System.Windows.Data;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class ColorBrushConverter : SimpleValueConverter<Color, SolidColorBrush>
    {
        protected override SolidColorBrush ConvertBase(Color input)
        {
            return new SolidColorBrush(input);
        }

        public static ColorBrushConverter Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ColorBrushConverter();
                }
                return s_instance;
            }
        }

        private static ColorBrushConverter s_instance;
    }
}
