using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PixelLab.Common
{
    public class WrapPanel : Panel
    {
        #region Row (Attached DependencyProperty)

        private static readonly DependencyProperty RowProperty =
            DependencyProperty.RegisterAttached("Row", typeof(int), typeof(WrapPanel), new PropertyMetadata(0));

        private static void SetRow(DependencyObject o, int value)
        {
            o.SetValue(RowProperty, value);
        }

        private static int GetRow(DependencyObject o)
        {
            return (int)o.GetValue(RowProperty);
        }

        #endregion

        #region Column (Attached DependencyProperty)

        private static readonly DependencyProperty ColumnProperty =
            DependencyProperty.RegisterAttached("Column", typeof(int), typeof(WrapPanel), new PropertyMetadata(0));

        private static void SetColumn(DependencyObject o, int value)
        {
            o.SetValue(ColumnProperty, value);
        }

        private static int GetColumn(DependencyObject o)
        {
            return (int)o.GetValue(ColumnProperty);
        }

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            double rowHeight = 0;
            double rowWidth = 0;

            double totalRowHeight = 0;
            double maxRowWidth = 0;

            int row = 0;
            int column = 0;

            Size huge = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in this.Children)
            {
                element.Measure(huge);

                rowWidth += element.DesiredSize.Width;

                if (rowWidth > availableSize.Width)
                {
                    rowWidth -= element.DesiredSize.Width;
                    if (rowWidth > maxRowWidth) maxRowWidth = rowWidth;
                    totalRowHeight += rowHeight;

                    // reset the row
                    row++;
                    rowHeight = 0;
                    rowWidth = element.DesiredSize.Width;
                    column = 0;
                }
                else
                {
                    column++;
                }

                if (element.DesiredSize.Height > rowHeight) rowHeight = element.DesiredSize.Height;

                // assign a row and column
                element.SetValue(WrapPanel.RowProperty, row);
                element.SetValue(WrapPanel.ColumnProperty, column);
            }

            if (rowWidth > maxRowWidth) maxRowWidth = rowWidth;
            totalRowHeight += rowHeight;

            //double w = !double.IsInfinity(availableSize.Width) ? Math.Max(maxRowWidth, availableSize.Width) : maxRowWidth;
            double w = maxRowWidth;
            double h = totalRowHeight; // !double.IsInfinity(availableSize.Height) ? Math.Min(totalRowHeight, availableSize.Height) : totalRowHeight;
            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // if we have no children then what's the point?
            if (this.Children.Count == 0) return new Size(0, 0);

            double top = 0;
            double left = 0;
            double maxWidth = 0;
            double leftSpace = 0;
            double innerSpace = 0;

            int rowCount = this.Children.Max(e => (int)e.GetValue(WrapPanel.RowProperty)) + 1;

            for (int row = 0; row < rowCount; row++)
            {
                var elements = this.Children.Where(e => (int)e.GetValue(WrapPanel.RowProperty) == row);
                var maxHeight = elements.Max(e => e.DesiredSize.Height);
                var rowWidth = elements.Sum(e => e.DesiredSize.Width);
                if (rowWidth > maxWidth) maxWidth = rowWidth;

                if (this.WrappingAlignment == WrappingAlignment.Left)
                {
                    // do nothing
                }
                else if (this.WrappingAlignment == WrappingAlignment.Center)
                {
                    leftSpace = (finalSize.Width - rowWidth) / 2;
                    innerSpace = 0;
                }
                else if (this.WrappingAlignment == WrappingAlignment.Justify)
                {
                    int itemsInRow = elements.Count();
                    if (itemsInRow > 1)
                    {
                        leftSpace = 0;
                        innerSpace = (finalSize.Width - rowWidth) / (itemsInRow - 1);
                    }
                    else
                    {
                        leftSpace = 0;// (finalSize.Width - rowWidth) / 2;
                        innerSpace = 0;
                    }
                }

                left = leftSpace;

                foreach (UIElement child in elements)
                {
                    child.Arrange(new Rect(left, top, child.DesiredSize.Width, child.DesiredSize.Height));
                    left += (child.DesiredSize.Width + innerSpace);
                }

                top += maxHeight;
                left = 0;
            }

            double w = !double.IsInfinity(finalSize.Width) ? Math.Max(maxWidth, finalSize.Width) : maxWidth;
            double h = top;
            return new Size(w, h);
        }

        #region WrappingAlignment (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public WrappingAlignment WrappingAlignment
        {
            get { return (WrappingAlignment)GetValue(WrappingAlignmentProperty); }
            set { SetValue(WrappingAlignmentProperty, value); }
        }
        public static readonly DependencyProperty WrappingAlignmentProperty =
            DependencyPropHelper.Register<WrapPanel, WrappingAlignment>("WrappingAlignment", WrappingAlignment.Left, (panel, newVal, oldVal) => panel.OnWrappingAlignmentChanged());

        protected virtual void OnWrappingAlignmentChanged()
        {
            this.InvalidateArrange();
        }

        #endregion
    }

    public enum WrappingAlignment
    {
        Left,
        Center,
        Justify
    }
}
