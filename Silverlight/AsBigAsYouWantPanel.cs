using System.Windows;
using System.Windows.Controls;
using PixelLab.Common;

namespace PixelLab.SL
{
    public class AsBigAsYouWantPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double widest = 0, tallest = 0;

            foreach (UIElement element in this.Children)
            {
                element.Measure(GeoHelper.SizeInfinite);

                double dw = element.DesiredSize.Width;
                dw = dw.IsValid() ? dw : 0;

                double dh = element.DesiredSize.Height;
                dh = dh.IsValid() ? dh : 0;

                if (dw > widest) widest = dw;
                if (dh > tallest) tallest = dh;
            }

            return new Size(widest, tallest);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double widest = 0, tallest = 0;

            foreach (var element in this.Children)
            {
                double dw = element.DesiredSize.Width;
                dw = dw.IsValid() ? dw : 0;

                double dh = element.DesiredSize.Height;
                dh = dh.IsValid() ? dh : 0;

                element.Arrange(new Rect(0, 0, dw, dh));

                if (dw > widest) widest = dw;
                if (dh > tallest) tallest = dh;
            }

            return new Size(widest, tallest);
        }
    }
}
