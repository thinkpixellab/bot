using System;
using System.Windows;
using System.Windows.Controls;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public class TreeMapPanel : AnimatingPanel
    {
        public static readonly DependencyProperty AreaProperty =
            DependencyProperty.RegisterAttached(
                "Area",
                typeof(double),
                typeof(TreeMapPanel),
                new FrameworkPropertyMetadata(
                    1.0,
                    FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static double GetArea(DependencyObject element)
        {
            Contract.Requires<ArgumentException>(element != null);

            return (double)element.GetValue(AreaProperty);
        }

        public static void SetArea(DependencyObject element, double value)
        {
            Contract.Requires<ArgumentException>(element != null);

            element.SetValue(AreaProperty, value);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (finalSize.Width < c_tolerance || finalSize.Height < c_tolerance)
            {
                return finalSize;
            }

            UIElementCollection children = InternalChildren;
            ComputeWeightMap(children);

            Rect strip = new Rect(finalSize);
            double remainingWeight = m_totalWeight;

            int arranged = 0;
            while (arranged < children.Count)
            {
                double bestStripWeight = 0;
                double bestRatio = double.PositiveInfinity;

                int i;

                if (finalSize.Width < c_tolerance || finalSize.Height < c_tolerance)
                {
                    return finalSize;
                }

                if (strip.Width > strip.Height)
                {
                    double bestWidth = strip.Width;

                    // Arrange Vertically
                    for (i = arranged; i < children.Count; i++)
                    {
                        double stripWeight = bestStripWeight + GetWeight(i);
                        double ratio = double.PositiveInfinity;
                        double width = strip.Width * stripWeight / remainingWeight;

                        for (int j = arranged; j <= i; j++)
                        {
                            double height = strip.Height * GetWeight(j) / stripWeight;
                            ratio = Math.Min(ratio, height > width ? height / width : width / height);

                            if (ratio > bestRatio)
                            {
                                goto ArrangeVertical;
                            }
                        }
                        bestRatio = ratio;
                        bestWidth = width;
                        bestStripWeight = stripWeight;
                    }

                ArrangeVertical:
                    double y = strip.Y;
                    for (; arranged < i; arranged++)
                    {
                        UIElement child = GetChild(children, arranged);

                        double height = strip.Height * GetWeight(arranged) / bestStripWeight;
                        ArrangeChild(child, new Rect(strip.X, y, bestWidth, height));
                        y += height;
                    }

                    strip.X = strip.X + bestWidth;
                    strip.Width = Math.Max(0.0, strip.Width - bestWidth);
                }
                else
                {
                    double bestHeight = strip.Height;

                    // Arrange Horizontally
                    for (i = arranged; i < children.Count; i++)
                    {
                        double stripWeight = bestStripWeight + GetWeight(i);
                        double ratio = double.PositiveInfinity;
                        double height = strip.Height * stripWeight / remainingWeight;

                        for (int j = arranged; j <= i; j++)
                        {
                            double width = strip.Width * GetWeight(j) / stripWeight;
                            ratio = Math.Min(ratio, height > width ? height / width : width / height);

                            if (ratio > bestRatio)
                            {
                                goto ArrangeHorizontal;
                            }
                        }
                        bestRatio = ratio;
                        bestHeight = height;
                        bestStripWeight = stripWeight;
                    }

                ArrangeHorizontal:
                    double x = strip.X;
                    for (; arranged < i; arranged++)
                    {
                        UIElement child = GetChild(children, arranged);

                        double width = strip.Width * GetWeight(arranged) / bestStripWeight;
                        ArrangeChild(child, new Rect(x, strip.Y, width, bestHeight));
                        x += width;
                    }

                    strip.Y = strip.Y + bestHeight;
                    strip.Height = Math.Max(0.0, strip.Height - bestHeight);
                }
                remainingWeight -= bestStripWeight;
            }

            return finalSize;
        }

        private UIElement GetChild(UIElementCollection children, int index)
        {
            return children[m_weightMap[index]];
        }

        private double GetWeight(int index)
        {
            return m_weights[m_weightMap[index]];
        }

        private void ComputeWeightMap(UIElementCollection children)
        {
            m_totalWeight = 0;

            if (m_weightMap == null || m_weightMap.Length != InternalChildren.Count)
            {
                m_weightMap = new int[InternalChildren.Count];
            }

            if (m_weights == null || m_weights.Length != InternalChildren.Count)
            {
                m_weights = new double[InternalChildren.Count];
            }

            for (int i = 0; i < m_weightMap.Length; i++)
            {
                m_weightMap[i] = i;
                m_weights[i] = GetArea(children[i]);
                m_totalWeight += m_weights[i];
            }

            Array.Sort<int>(m_weightMap, compareWeights);
        }

        private int compareWeights(int index1, int index2)
        {
            return m_weights[index2].CompareTo(m_weights[index1]);
        }

        private double m_totalWeight;
        private int[] m_weightMap;
        private double[] m_weights;

        private const double c_tolerance = 1e-2;
    }
}