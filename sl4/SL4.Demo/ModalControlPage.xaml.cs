using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo
{
    [DemoMetadata("Modal Control")]
    public partial class ModalControlPage : UserControl
    {
        private int _count = 0;

        public ModalControlPage()
        {
            InitializeComponent();

            _rootButton.Click += (sender, args) => openModal();

            _modalControl.SetTarget(_rootButton);
        }

        private void openModal()
        {
            var thickness = new Thickness(3);

            var text = new TextBlock { Text = "Modal at level {0}".DoFormat(++_count), Margin = thickness, Padding = thickness };

            var openButton = new Button { Content = "Open another...", Margin = thickness, Padding = thickness };
            openButton.Click += (sender, args) => openModal();

            var closeButton = new Button { Content = "Close me!", Margin = thickness, Padding = thickness };

            var stack = new StackPanel();
            stack.Children.Add(text);
            stack.Children.Add(openButton);
            stack.Children.Add(closeButton);

            var border = new Border
            {
                BorderBrush = Colors.Black.ToCachedBrush(),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(3),
                Padding = thickness,
                Child = stack,
                Background = ColorHelper.HsbToRgb(Util.Rnd.NextFloat(), 0.6f, 1).ToBrush()
            };

            var topLeft = this.GetLocationFromRootVisual() + new Vector(Util.Rnd.Next(500), Util.Rnd.Next(500));
            var token = _modalControl.Open(border, ModalPosition.TopLeft, topLeft);

            closeButton.Click += (sender, args) =>
            {
                _modalControl.Close(token);
                _count--;
            };
        }
    }
}
