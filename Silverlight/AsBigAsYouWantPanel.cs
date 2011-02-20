using System.Windows;
using System.Windows.Controls;

namespace PixelLab.SL
{
    public class AsBigAsYouWantPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double widest = 0;
            double tallest = 0;
            Size YouCanBeAsBigAsYouWant = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in this.Children)
            {
                element.Measure(YouCanBeAsBigAsYouWant);

                double dw = element.DesiredSize.Width;
                if (double.IsNaN(dw) || double.IsInfinity(dw)) dw = 0;

                double dh = element.DesiredSize.Height;
                if (double.IsNaN(dh) || double.IsInfinity(dh)) dh = 0;

                if (dw > widest) widest = dw;
                if (dh > tallest) tallest = dh;
            }

            return new Size(widest, tallest);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double top = 0;
            double left = 0;
            double widest = 0;
            double tallest = 0;


            foreach (UIElement element in this.Children)
            {

                double dw = element.DesiredSize.Width;
                if (double.IsNaN(dw) || double.IsInfinity(dw)) dw = 0;

                double dh = element.DesiredSize.Height;
                if (double.IsNaN(dh) || double.IsInfinity(dh)) dh = 0;

                element.Arrange(new Rect(left, top, dw, dh));

                if (dw > widest) widest = dw;
                if (dh > tallest) tallest = dh;

            }

            return new Size(widest, tallest);
        }
    }
}
