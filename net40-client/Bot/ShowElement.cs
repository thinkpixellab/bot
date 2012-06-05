using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public class ShowElement : FrameworkElement
    {
        public void AddItem(UIElement item)
        {
            Contract.Requires<ArgumentNullException>(item != null);
            Debug.Assert(VisualTreeHelper.GetParent(item) == null, "item", "item should not have a parent");

            m_elements.Add(item);
            this.AddVisualChild(item);
            this.InvalidateMeasure();

            item.RenderTransformOrigin = new Point(.5, .5);

            TransformGroup group = new TransformGroup();
            group.Children.Add(new ScaleTransform(.6, .6));
            ScaleTransform animatedScale = new ScaleTransform();
            group.Children.Add(animatedScale);

            RotateTransform rotateTransform = new RotateTransform();
            group.Children.Add(rotateTransform);

            group.Children.Add(new TranslateTransform());

            item.RenderTransform = group;

            if (m_elements.Count >= c_maxCount)
            {
                int oldestCount = m_elements.Count - c_maxCount;

                for (int i = 0; i < oldestCount; i++)
                {
                    UIElement oldest = m_elements[0];
                    m_fadingElements.Add(oldest);
                    m_elements.RemoveAt(0);

                    DoubleAnimation fadeOut = GetFadeOutAnimation();

                    fadeOut.Completed += delegate(object sender, EventArgs e)
                    {
                        m_fadingElements.Remove(oldest);
                        this.RemoveVisualChild(oldest);
                    };

                    oldest.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
            }

            DoubleAnimation rotationAnimation = GetRandomRotateAnimation();
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);

            DoubleAnimation fadeIn = GetFadeInAnimation();
            item.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            DoubleAnimation shrink = GetShrinkAnimation();
            animatedScale.BeginAnimation(ScaleTransform.ScaleXProperty, shrink);
            animatedScale.BeginAnimation(ScaleTransform.ScaleYProperty, shrink);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Action<UIElement> arrange = delegate(UIElement child)
            {
                TransformGroup tg = (TransformGroup)child.RenderTransform;
                ScaleTransform st = (ScaleTransform)tg.Children[0];
                TranslateTransform tt = (TranslateTransform)tg.Children[3];

                Size childSize = child.DesiredSize;

                child.Arrange(new Rect(childSize));

                tt.SetToVector(finalSize.CenterVector() - childSize.CenterVector());

                double scale = GeoHelper.ScaleToFit(finalSize, childSize);

                st.ScaleX = scale;
                st.ScaleY = scale;
            };

            m_elements.ForEach(arrange);
            m_fadingElements.ForEach(arrange);

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            m_elements.ForEach(child => child.Measure(GeoHelper.SizeInfinite));
            m_fadingElements.ForEach(child => child.Measure(GeoHelper.SizeInfinite));

            return new Size();
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return m_elements.Count + m_fadingElements.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < m_fadingElements.Count)
            {
                return m_fadingElements[index];
            }

            index -= m_fadingElements.Count;

            return m_elements[index];
        }

        private static DoubleAnimation GetFadeInAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.Duration = new Duration(TimeSpan.FromSeconds(2));

            return animation;
        }

        private static DoubleAnimation GetRandomRotateAnimation()
        {
            double angle = (.5 - Util.Rnd.NextDouble()) * 20;

            DoubleAnimation animation = new DoubleAnimation();
            animation.To = angle;
            animation.DecelerationRatio = .5;
            animation.Duration = s_defaultDuration;

            animation.Freeze();

            return animation;
        }

        private static DoubleAnimation GetFadeOutAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromSeconds(3));

            return animation;
        }

        private static DoubleAnimation GetShrinkAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.To = .8;
            animation.DecelerationRatio = .5;
            animation.Duration = s_defaultDuration;

            animation.Freeze();

            return animation;
        }

        private readonly List<UIElement> m_elements = new List<UIElement>();
        private readonly List<UIElement> m_fadingElements = new List<UIElement>();

        private static readonly Duration s_defaultDuration = new Duration(TimeSpan.FromSeconds(3));

        private const int c_maxCount = 5;
    }
}