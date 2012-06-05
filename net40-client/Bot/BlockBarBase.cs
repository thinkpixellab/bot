using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public abstract class BlockBarBase : FrameworkElement
    {
        static BlockBarBase()
        {
            BlockBarBase.MinHeightProperty.OverrideMetadata(typeof(BlockBarBase), new FrameworkPropertyMetadata((double)10));
            BlockBarBase.MinWidthProperty.OverrideMetadata(typeof(BlockBarBase), new FrameworkPropertyMetadata((double)10));
            BlockBarBase.ClipToBoundsProperty.OverrideMetadata(typeof(BlockBarBase), new FrameworkPropertyMetadata(true));
        }

        #region properties

        public static readonly DependencyProperty BlockCountProperty =
            DependencyProperty.Register("BlockCount", typeof(int), typeof(BlockBarBase),
            new FrameworkPropertyMetadata((int)5, FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                new CoerceValueCallback(CoerceBlockCount))
            );

        public int BlockCount
        {
            get { return (int)GetValue(BlockCountProperty); }
            set { SetValue(BlockCountProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(BlockBarBase),
            new FrameworkPropertyMetadata((double)0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                new CoerceValueCallback(CoerceValue))
            );

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty BlockMarginProperty =
            DependencyProperty.Register("BlockMargin", typeof(double), typeof(BlockBarBase),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.AffectsRender,
                null,
                new CoerceValueCallback(CoerceBlockMargin))
            );

        public double BlockMargin
        {
            get { return (double)GetValue(BlockMarginProperty); }
            set { SetValue(BlockMarginProperty, value); }
        }

        protected Pen BorderBen
        {
            get
            {
                if (m_borderPen == null || m_borderPen.Brush != Foreground)
                {
                    m_borderPen = new Pen(Foreground, 4);
                    m_borderPen.Freeze();
                }
                return m_borderPen;
            }
        }

        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(typeof(BlockBarBase), new FrameworkPropertyMetadata(Brushes.Navy, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty = Control.BackgroundProperty.AddOwner(typeof(BlockBarBase), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        #endregion

        protected static int GetThreshold(double value, int blockCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(value >= 0 && value <= 1);
            Contract.Requires<ArgumentOutOfRangeException>(blockCount > 0);

            int blockNumber = Math.Min((int)(value * (blockCount + 1)), blockCount);

            Debug.Assert(blockNumber <= blockCount && blockNumber >= 0);

            return blockNumber;
        }

        private static object CoerceValue(DependencyObject element, object value)
        {
            double input = (double)value;
            if (input < 0 || double.IsNaN(input))
            {
                return 0;
            }
            else if (input > 1)
            {
                return 1;
            }
            else
            {
                return input;
            }
        }

        private static object CoerceBlockCount(DependencyObject element, object value)
        {
            int input = (int)value;

            if (input < 1)
            {
                return 1;
            }
            else
            {
                return input;
            }
        }

        private static object CoerceBlockMargin(DependencyObject element, object value)
        {
            double input = (double)value;
            if (input < 0 || double.IsNaN(input) || double.IsInfinity(input))
            {
                return 0;
            }
            else
            {
                return input;
            }
        }

        private Pen m_borderPen;
    }
}
