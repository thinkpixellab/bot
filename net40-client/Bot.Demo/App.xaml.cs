using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public static readonly ReadOnlyCollection<Color> DemoColors =
          "#E2602D,#1E94C0,#B7596B,#FF9C00,#93C6B9,#70634D,#FDCE4E,#759C00"
          .Split(',')
          .Select(cs => (Color)ColorConverter.ConvertFromString(cs))
          .ToReadOnlyCollection();
    }
}
