using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    public class TreeMap3D : UIElement3D
    {
        static TreeMap3D()
        {
            EventManager.RegisterClassHandler(
                typeof(TreeMap3D),
                ButtonBase.ClickEvent,
                new RoutedEventHandler(child_click));
        }

        public static readonly RoutedEvent SelectionChangedEvent =
            Selector.SelectionChangedEvent.AddOwner(typeof(TreeMap3D));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                base.AddHandler(SelectionChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(SelectionChangedEvent, value);
            }
        }

        public string WeightBindingPath
        {
            get
            {
                VerifyAccess();

                return m_weightBindingPath;
            }
            set
            {
                VerifyAccess();

                if (m_weightBindingPath != value)
                {
                    foreach (TreeMap3DElement element in m_elements)
                    {
                        element.WeightBindingPath = value;
                    }

                    m_weightBindingPath = value;
                }
            }
        }

        /// <remarks>
        ///     This is a weak ItemsSource. It does not support binding or
        ///     INotifyCollectionChanged.
        /// </remarks>
        public IList ItemsSource
        {
            get
            {
                VerifyAccess();
                return m_itemsSource;
            }
            set
            {
                if (m_itemsSource != value)
                {
                    VerifyAccess();

                    refresh(value);
                    m_itemsSource = value;
                }
            }
        }

        protected override Visual3D GetVisual3DChild(int index)
        {
            return m_elements[index];
        }

        protected override int Visual3DChildrenCount
        {
            get
            {
                return m_elements.Count;
            }
        }

        #region Implementation

        private void refresh(IList newValues)
        {
            VerifyAccess();

            if (newValues == null)
            {
                newValues = new object[0];
            }

            // update/add weights
            double totalWeight = 0;

            TreeMap3DElement element;

            for (int i = 0; i < newValues.Count; i++)
            {
                if (i >= m_elements.Count)
                {
                    element = new TreeMap3DElement(i, newValues.Count);
                    element.Data = newValues[i];
                    element.WeightBindingPath = m_weightBindingPath;

                    AddVisual3DChild(element);
                    m_elements.Add(element);
                }
                else
                {
                    m_elements[i].Data = newValues[i];
                    element = m_elements[i];
                }

                totalWeight += element.Weight;
            }

            // remove any no-longer needed 3D objects
            for (int i = newValues.Count - 1; i >= m_elements.Count; i--)
            {
                element = m_elements[i];
                RemoveVisual3DChild(element);
                m_elements.RemoveAt(i);
            }

            updateMap(totalWeight);
        }

        private static void child_click(object sender, RoutedEventArgs e)
        {
            TreeMap3D treeMap3D = (TreeMap3D)sender;
            TreeMap3DElement element = e.Source as TreeMap3DElement;

            treeMap3D.childClick(element);
        }

        private void childClick(TreeMap3DElement element)
        {
            if (m_lastClickedElement != null && m_lastClickedElement != element)
            {
                m_lastClickedElement.IsSelected = false;
            }

            m_lastClickedElement = element;

            if (element != null)
            {
                if (m_elements.Contains(element))
                {
                    if (element != m_selectedElement)
                    {
                        if (m_selectedElement == null)
                        {
                            foreach (TreeMap3DElement childElement in m_elements)
                            {
                                childElement.IsSelected = false;
                            }
                        }

                        element.IsSelected = true;
                        onSelectionChanged(
                            (m_selectedElement == null) ? null : m_selectedElement.Data,
                            element.Data);
                        m_selectedElement = element;
                    }
                    else
                    {
                        foreach (TreeMap3DElement childElement in m_elements)
                        {
                            childElement.IsSelected = true;
                        }

                        onSelectionChanged(m_selectedElement.Data, null);
                        m_selectedElement = null;
                    }
                }
            }
        }

        private void onSelectionChanged(object oldValue, object newValue)
        {
            object[] oldItems = (oldValue == null) ? new object[0] : new object[] { oldValue };
            object[] newItems = (newValue == null) ? new object[0] : new object[] { newValue };

            SelectionChangedEventArgs args = new SelectionChangedEventArgs(
                SelectionChangedEvent,
                oldItems,
                newItems);

            base.RaiseEvent(args);
        }

        private void updateMap(double totalWeight)
        {
            ComputeWeightMap();
            double maxArea = -1;

            Rect strip = new Rect(new Size(1, 1));

            int arranged = 0;
            while (arranged < m_elements.Count)
            {
                double bestStripWeight = 0;
                double bestRatio = double.PositiveInfinity;

                int i;

                if (strip.Width > strip.Height)
                {
                    double bestWidth = strip.Width;

                    // Arrange Vertically
                    for (i = arranged; i < m_elements.Count; i++)
                    {
                        double stripWeight = bestStripWeight + GetWeight(i);
                        double ratio = double.PositiveInfinity;
                        double width = strip.Width * stripWeight / totalWeight;

                        for (int j = arranged; j <= i; j++)
                        {
                            double height = strip.Height * GetWeight(j) / stripWeight;
                            ratio = Math.Min(ratio, height > width ? height / width : width / height);

                            if (ratio > bestRatio)
                                goto ArrangeVertical;
                        }
                        bestRatio = ratio;
                        bestWidth = width;
                        bestStripWeight = stripWeight;
                    }

                ArrangeVertical:
                    double y = strip.Y;
                    for (; arranged < i; arranged++)
                    {
                        TreeMap3DElement child = getChild(arranged);

                        double height = strip.Height * GetWeight(arranged) / bestStripWeight;
                        child.SetScale(bestWidth, height, bestWidth * height);
                        child.SetLocation(strip.X, y, 0);

                        if (maxArea < bestWidth * height) maxArea = bestWidth * height;

                        y += height;
                    }

                    strip.X = strip.X + bestWidth;
                    strip.Width = Math.Max(0.0, strip.Width - bestWidth);
                }
                else
                {
                    double bestHeight = strip.Height;

                    // Arrange Horizontally
                    for (i = arranged; i < m_elements.Count; i++)
                    {
                        double stripWeight = bestStripWeight + GetWeight(i);
                        double ratio = double.PositiveInfinity;
                        double height = strip.Height * stripWeight / totalWeight;

                        for (int j = arranged; j <= i; j++)
                        {
                            double width = strip.Width * GetWeight(j) / stripWeight;
                            ratio = Math.Min(ratio, height > width ? height / width : width / height);

                            if (ratio > bestRatio)
                                goto ArrangeHorizontal;
                        }
                        bestRatio = ratio;
                        bestHeight = height;
                        bestStripWeight = stripWeight;
                    }

                ArrangeHorizontal:
                    double x = strip.X;
                    for (; arranged < i; arranged++)
                    {
                        TreeMap3DElement child = getChild(arranged);

                        double width = strip.Width * GetWeight(arranged) / bestStripWeight;

                        child.SetScale(width, bestHeight, width * bestHeight);
                        child.SetLocation(x, strip.Y, 0);

                        if (maxArea < width * bestHeight) maxArea = width * bestHeight;

                        x += width;
                    }

                    strip.Y = strip.Y + bestHeight;
                    strip.Height = Math.Max(0.0, strip.Height - bestHeight);
                }
                totalWeight -= bestStripWeight;
            }

            foreach (TreeMap3DElement elem in m_elements)
            {
                elem.ScaleZ(2 * maxArea);
            }

            m_weightMap = null;
        }

        private TreeMap3DElement getChild(int index)
        {
            return m_elements[m_weightMap[index]];
        }

        private double GetWeight(int index)
        {
            return m_elements[m_weightMap[index]].Weight;
        }

        private void ComputeWeightMap()
        {
            m_weightMap = new int[m_elements.Count];

            for (int i = 0; i < m_weightMap.Length; i++)
            {
                m_weightMap[i] = i;
            }

            Array.Sort<int>(m_weightMap, new Comparison<int>(CompareWeights));
        }

        private int CompareWeights(int index1, int index2)
        {
            return m_elements[index2].Weight.CompareTo(m_elements[index1].Weight);
        }

        private string m_weightBindingPath;
        private IList m_itemsSource;
        private TreeMap3DElement m_selectedElement;
        private int[] m_weightMap;
        private TreeMap3DElement m_lastClickedElement;

        private readonly List<TreeMap3DElement> m_elements = new List<TreeMap3DElement>();

        #region Nested Class

        private class TreeMap3DElement : UIElement3D
        {
            public TreeMap3DElement(int index, int count)
            {
                this.Visual3DModel = GenerateTreeMap3DModel(index, count);

                m_translate = new TranslateTransform3D();
                m_scale = new ScaleTransform3D();
                Transform3DGroup t3DGroup = new Transform3DGroup();
                t3DGroup.Children.Add(m_scale);
                t3DGroup.Children.Add(m_translate);
                base.Transform = t3DGroup;
            }

            public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(TreeMap3DElement));

            public bool IsSelected
            {
                get { return m_isSelected; }
                set
                {
                    if (value != m_isSelected)
                    {
                        DoubleAnimation animation = new DoubleAnimation();

                        if (value)
                        {
                            animation.From = m_scale.ScaleZ;
                            animation.To = m_scaleZ;
                        }
                        else
                        {
                            animation.From = m_scale.ScaleZ;
                            animation.To = 0;
                        }

                        m_isSelected = value;

                        animation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                        m_scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, animation);
                    }
                }
            }

            protected override void OnMouseDown(MouseButtonEventArgs e)
            {
                if (!e.Handled)
                {
                    e.Handled = true;
                    onClick();
                }
            }

            private static readonly DependencyProperty WeightProperty =
                DependencyProperty.Register("Weight", typeof(double), typeof(TreeMap3DElement));

            public object Data
            {
                get
                {
                    return m_data;
                }
                set
                {
                    VerifyAccess();
                    if (m_data != value)
                    {
                        resetDataWeightBinding(m_weightBindingPath, m_data);
                        m_data = value;
                    }
                }
            }

            public double Weight
            {
                get { return (double)GetValue(WeightProperty); }
            }

            public string WeightBindingPath
            {
                get
                {
                    return m_weightBindingPath;
                }
                set
                {
                    VerifyAccess();
                    if (m_weightBindingPath != value)
                    {
                        resetDataWeightBinding(value, Data);
                        m_weightBindingPath = value;
                    }
                }
            }

            public void SetScale(double x, double y, double z)
            {
                m_scale.ScaleX = x;
                m_scale.ScaleY = y;
                m_scaleZ = z;
                m_isSelected = true;

                m_scale.ScaleZ = m_scaleZ;
            }

            public void ScaleZ(double val)
            {
                m_scaleZ /= val;
                m_scale.ScaleZ = m_scaleZ;
            }

            public void SetLocation(double x, double y, int z)
            {
                m_translate.OffsetX = x;
                m_translate.OffsetY = y;
                m_translate.OffsetZ = z;
            }

            #region Implementation

            private void onClick()
            {
                RoutedEventArgs e = new RoutedEventArgs(ButtonBase.ClickEvent, this);
                base.RaiseEvent(e);
            }

            private void resetDataWeightBinding(string path, object source)
            {
                BindingOperations.ClearBinding(this, WeightProperty);
                Binding binding = new Binding(path);
                binding.Source = source;

                BindingOperations.SetBinding(this, WeightProperty, binding);
            }

            private object m_data;
            private string m_weightBindingPath;
            private bool m_isSelected;
            private double m_scaleZ;

            private readonly ScaleTransform3D m_scale;
            private readonly TranslateTransform3D m_translate;

            private static Model3D GenerateTreeMap3DModel(int index, int count)
            {
                MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

                Point3DCollection positions = new Point3DCollection();
                positions.Add(new Point3D(0, 0, 1));
                positions.Add(new Point3D(0, 0, 0));
                positions.Add(new Point3D(1, 0, 0));
                positions.Add(new Point3D(1, 0, 1));
                positions.Add(new Point3D(0, 1, 1));
                positions.Add(new Point3D(0, 1, 0));
                positions.Add(new Point3D(1, 1, 0));
                positions.Add(new Point3D(1, 1, 1));
                positions.Freeze();

                Int32Collection triangleIndices = new Int32Collection();
                triangleIndices.Add(0);
                triangleIndices.Add(1);
                triangleIndices.Add(2);

                triangleIndices.Add(2);
                triangleIndices.Add(3);
                triangleIndices.Add(0);

                triangleIndices.Add(4);
                triangleIndices.Add(7);
                triangleIndices.Add(6);

                triangleIndices.Add(6);
                triangleIndices.Add(5);
                triangleIndices.Add(4);

                triangleIndices.Add(0);
                triangleIndices.Add(3);
                triangleIndices.Add(7);

                triangleIndices.Add(7);
                triangleIndices.Add(4);
                triangleIndices.Add(0);

                triangleIndices.Add(1);
                triangleIndices.Add(5);
                triangleIndices.Add(6);

                triangleIndices.Add(6);
                triangleIndices.Add(2);
                triangleIndices.Add(1);

                triangleIndices.Add(3);
                triangleIndices.Add(2);
                triangleIndices.Add(6);

                triangleIndices.Add(6);
                triangleIndices.Add(7);
                triangleIndices.Add(3);

                triangleIndices.Add(0);
                triangleIndices.Add(4);
                triangleIndices.Add(5);

                triangleIndices.Add(5);
                triangleIndices.Add(7);
                triangleIndices.Add(0);

                triangleIndices.Freeze();

                // finally set the data
                meshGeometry3D.TriangleIndices = triangleIndices;
                meshGeometry3D.Positions = positions;

                // create the geometry model
                GeometryModel3D geom3D = new GeometryModel3D();
                geom3D.Geometry = meshGeometry3D;

                Color color = ColorHelper.HsbToRgb(index / (float)count, .9f, 1f);
                SolidColorBrush solidColorBrush = color.ToBrush();
                solidColorBrush.Freeze();

                geom3D.Material = new DiffuseMaterial(solidColorBrush);

                return geom3D;
            }

            #endregion
        }

        #endregion

        #endregion
    }
}