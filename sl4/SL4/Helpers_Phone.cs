using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PixelLab.SL
{
    public static class Helpers
    {
        public static void WatchDataContextChanged(this FrameworkElement element, Action<object, object> onChangedAction)
        {
            element.SetBinding(DataContextChangedHelperProprety, new Binding());
            element.SetValue(DataContextChangedHelperActionProperty, onChangedAction);
        }

        private static readonly DependencyProperty DataContextChangedHelperProprety = DependencyProperty.RegisterAttached("DataContextChangedHelper", typeof(object), typeof(Helpers), new PropertyMetadata((owner, args) =>
        {
            var action = (Action<object, object>)owner.GetValue(DataContextChangedHelperActionProperty);
            action(args.OldValue, args.NewValue);
        }));

        private static readonly DependencyProperty DataContextChangedHelperActionProperty = DependencyProperty.RegisterAttached("DataContextChangedHelperAction", typeof(Action<object, object>), typeof(Helpers), null);

        public static void CopyPixels(this WriteableBitmap source, WriteableBitmap target, int startX, int startY)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Requires(startX >= 0);
            Contract.Requires(startX + target.PixelWidth <= source.PixelWidth);

            Contract.Requires(startY >= 0);
            Contract.Requires(startY + target.PixelHeight <= source.PixelHeight);

            int targetIndex, sourceIndex;

            for (int y = 0; y < target.PixelHeight; y++)
            {
                sourceIndex = (y + startY) * source.PixelWidth + startX;
                targetIndex = y * target.PixelWidth;

                Array.Copy(source.Pixels, sourceIndex, target.Pixels, targetIndex, target.PixelWidth);
            }

            target.Invalidate();
        }
    }
}
