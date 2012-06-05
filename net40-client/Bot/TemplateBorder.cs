using System.Windows;
using System.Windows.Controls;

namespace PixelLab.Wpf
{
    public class TemplateBorder : Border
    {
        static TemplateBorder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TemplateBorder), new FrameworkPropertyMetadata(typeof(TemplateBorder)));
        }
    }
}
