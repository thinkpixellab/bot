using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public abstract class WrapperElement<TElement> : FrameworkElement
        where TElement : UIElement
    {
        protected WrapperElement(TElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null);

            m_element = element;

            AddVisualChild(m_element);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            m_element.Measure(availableSize);
            return m_element.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            m_element.Arrange(new Rect(finalSize));
            return m_element.RenderSize;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            Debug.Assert(index == 0);
            return m_element;
        }

        protected TElement WrappedElement { get { return m_element; } }

        private readonly TElement m_element;
    }
}