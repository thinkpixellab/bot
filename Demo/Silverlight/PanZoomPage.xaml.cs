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
    }
}
