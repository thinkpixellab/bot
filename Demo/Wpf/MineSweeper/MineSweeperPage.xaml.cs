using System;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo.MineSweeper
{
    [DemoMetadata("Mine Sweeper", "An oldy, but a goody. Done in WPF. Custom templates are used a lot here.")]
    public partial class MineSweeperPage : Page
    {
        public MineSweeperPage()
        {
            InitializeComponent();
        }
    }

    public class GameStateBrushConverter : SimpleValueConverter<WinState, Brush>
    {
        protected override Brush ConvertBase(WinState input)
        {
            switch (input)
            {
                case WinState.Unknown:
                    return makeBrush(Colors.Navy);
                case WinState.Lost:
                    return makeBrush(Colors.Maroon);
                case WinState.Won:
                    return makeBrush(Colors.DarkGreen);
                default:
                    throw new NotSupportedException();
            }
        }

        private static GradientBrush makeBrush(Color color)
        {
            return new LinearGradientBrush(new GradientStopCollection()
            {
                new GradientStop(Colors.White,0),
                new GradientStop(color, 1)
            }, 45);
        }
    }
}