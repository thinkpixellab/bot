using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public class AnimatingTilePanel : AnimatingPanel
    {
        #region public properties

        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        public static double GetItemWidth(DependencyObject element)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            return (double)element.GetValue(ItemWidthProperty);
        }

        public static void SetItemWidth(DependencyObject element, double itemWidth)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            element.SetValue(ItemWidthProperty, itemWidth);
        }

        public static readonly DependencyProperty ItemWidthProperty =
            CreateDoubleDP("ItemWidth", 50, FrameworkPropertyMetadataOptions.AffectsMeasure, 0, double.PositiveInfinity, true);

        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        public static double GetItemHeight(DependencyObject element)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            return (double)element.GetValue(ItemHeightProperty);
        }

        public static void SetItemHeight(DependencyObject element, double itemHeight)
        {
            Contract.Requires<ArgumentNullException>(element != null);
            element.SetValue(ItemHeightProperty, itemHeight);
        }

        public static readonly DependencyProperty ItemHeightProperty =
            CreateDoubleDP("ItemHeight", 50, FrameworkPropertyMetadataOptions.AffectsMeasure, 0, double.PositiveInfinity, true);

        #endregion

        #region protected override

        protected override Size MeasureOverride(Size availableSize)
        {
            onPreApplyTemplate();

            Size theChildSize = getItemSize();

            foreach (UIElement child in Children)
            {
                child.Measure(theChildSize);
            }

            int childrenPerRow;

            // Figure out how many children fit on each row
            if (availableSize.Width == Double.PositiveInfinity)
            {
                childrenPerRow = this.Children.Count;
            }
            else
            {
                childrenPerRow = Math.Max(1, (int)Math.Floor(availableSize.Width / this.ItemWidth));
            }

            // Calculate the width and height this results in
            double width = childrenPerRow * this.ItemWidth;
            double height = this.ItemHeight * (Math.Floor((double)this.Children.Count / childrenPerRow) + 1);
            height = (height.IsValid()) ? height : 0;
            return new Size(width, height);
        }

        protected override sealed Size ArrangeOverride(Size finalSize)
        {
            // Calculate how many children fit on each row
            int childrenPerRow = Math.Max(1, (int)Math.Floor(finalSize.Width / this.ItemWidth));
            Size theChildSize = getItemSize();

            for (int i = 0; i < this.Children.Count; i++)
            {
                // Figure out where the child goes
                Point newOffset = calculateChildOffset(i, childrenPerRow,
                    this.ItemWidth, this.ItemHeight,
                    finalSize.Width, this.Children.Count);

                ArrangeChild(Children[i], new Rect(newOffset, theChildSize));
            }

            m_arrangedOnce = true;
            return finalSize;
        }

        protected override Point ProcessNewChild(UIElement child, Rect providedBounds)
        {
            var startLocation = providedBounds.Location;
            if (m_arrangedOnce)
            {
                if (m_itemOpacityAnimation == null)
                {
                    m_itemOpacityAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        Duration = new Duration(TimeSpan.FromSeconds(.5))
                    };
                    m_itemOpacityAnimation.Freeze();
                }

                child.BeginAnimation(UIElement.OpacityProperty, m_itemOpacityAnimation);
                startLocation -= new Vector(providedBounds.Width, 0);
            }
            return startLocation;
        }

        #endregion

        #region Implementation

        #region private methods

        private Size getItemSize() { return new Size(ItemWidth, ItemHeight); }

        private void bindToParentItemsControl(DependencyProperty property, DependencyObject source)
        {
            if (DependencyPropertyHelper.GetValueSource(this, property).BaseValueSource == BaseValueSource.Default)
            {
                Binding binding = new Binding();
                binding.Source = source;
                binding.Path = new PropertyPath(property);
                base.SetBinding(property, binding);
            }
        }

        private void onPreApplyTemplate()
        {
            if (!m_appliedTemplate)
            {
                m_appliedTemplate = true;

                DependencyObject source = base.TemplatedParent;
                if (source is ItemsPresenter)
                {
                    source = TreeHelpers.FindParent<ItemsControl>(source);
                }

                if (source != null)
                {
                    bindToParentItemsControl(ItemHeightProperty, source);
                    bindToParentItemsControl(ItemWidthProperty, source);
                }
            }
        }

        // Given a child index, child size and children per row, figure out where the child goes
        private static Point calculateChildOffset(
            int index,
            int childrenPerRow,
            double itemWidth,
            double itemHeight,
            double panelWidth,
            int totalChildren)
        {
            double fudge = 0;
            if (totalChildren > childrenPerRow)
            {
                fudge = (panelWidth - childrenPerRow * itemWidth) / childrenPerRow;
                Debug.Assert(fudge >= 0);
            }

            int row = index / childrenPerRow;
            int column = index % childrenPerRow;
            return new Point(.5 * fudge + column * (itemWidth + fudge), row * itemHeight);
        }

        #endregion

        private bool m_appliedTemplate;
        private bool m_arrangedOnce;
        private DoubleAnimation m_itemOpacityAnimation;

        #endregion
    } //*** class AnimatingTilePanel
}
