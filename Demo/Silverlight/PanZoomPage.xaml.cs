using System.Windows;
using System.Windows.Controls;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo
{
    [DemoMetadata("PanZoom")]
    public partial class PanZoomPage : UserControl
    {
        public PanZoomPage()
        {
            InitializeComponent();
        }

        private void AsBigAsYouWantPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PanZoom.CenterContent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PanZoom.Scale = 1;
            PanZoom.CenterContent();
        }
    }
}
