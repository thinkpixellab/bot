using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace PixelLab.Wpf
{
    public class Transition3D : WrapperElement<Viewport3D>
    {
        public Transition3D()
            : base(new Viewport3D())
        {
            // camera to ue
            WrappedElement.Camera = new PerspectiveCamera();

            // the model visual 3D
            ModelVisual3D mv3D = new ModelVisual3D();
            mv3D.Content = new PointLight(Colors.White, new Point3D(0, 0, 0));

            WrappedElement.Children.Add(mv3D);

            MeshGeometry3D plane = new MeshGeometry3D();
            Point3DCollection positions = new Point3DCollection();
            positions.Add(new Point3D(-1, -1, 0));
            positions.Add(new Point3D(-1, 1, 0));
            positions.Add(new Point3D(1, 1, 0));
            positions.Add(new Point3D(1, -1, 0));
            positions.Freeze();
            plane.Positions = positions;

            PointCollection textureCoords = new PointCollection();
            textureCoords.Add(new Point(0, 1));
            textureCoords.Add(new Point(0, 0));
            textureCoords.Add(new Point(1, 0));
            textureCoords.Add(new Point(1, 1));
            textureCoords.Freeze();
            plane.TextureCoordinates = textureCoords;

            Int32Collection indices = new Int32Collection();
            indices.Add(0);
            indices.Add(3);
            indices.Add(1);
            indices.Add(1);
            indices.Add(3);
            indices.Add(2);
            indices.Freeze();
            plane.TriangleIndices = indices;

            Material planeMaterial = new DiffuseMaterial(Brushes.Blue);
            planeMaterial.SetValue(Viewport2DVisual3D.IsVisualHostMaterialProperty, true);

            m_visual3D = new Viewport2DVisual3D();
            m_visual3D.Geometry = plane;
            m_visual3D.Material = planeMaterial;

            Transform3DGroup transform = new Transform3DGroup();
            m_rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 35);
            m_scale = new ScaleTransform3D(0, 0, 0);
            m_translation = new TranslateTransform3D(-2.5, 0, -10);

            transform.Children.Add(m_scale);
            transform.Children.Add(new RotateTransform3D(m_rotation));
            transform.Children.Add(m_translation);

            m_visual3D.Transform = transform;

            WrappedElement.Children.Add(m_visual3D);
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Transition3D),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    isExpandedChanged
                    ));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public Visual Child
        {
            get { return m_child; }
            set
            {
                if (m_child != value)
                {
                    base.RemoveLogicalChild(m_child);
                    m_child = value;
                    m_visual3D.Visual = m_child;
                    base.AddLogicalChild(m_child);
                }
            }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                yield return m_child;
            }
        }

        #region Implementation

        private static void isExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Transition3D element = (Transition3D)d;
            bool isExpanded = (bool)e.NewValue;

            if (isExpanded)
            {
                element.runExpandAnimation();
            }
            else
            {
                element.runCollapseAnimation();
            }
        }

        private void runCollapseAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1.0;
            animation.To = 0.0;
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.3));

            m_scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, animation);
            m_scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, animation);
            m_scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, animation);
        }

        private void runExpandAnimation()
        {
            m_scale.BeginAnimation(ScaleTransform3D.ScaleXProperty, null);
            m_scale.BeginAnimation(ScaleTransform3D.ScaleYProperty, null);
            m_scale.BeginAnimation(ScaleTransform3D.ScaleZProperty, null);

            m_scale.ScaleX = 1.0;
            m_scale.ScaleY = 1.0;
            m_scale.ScaleZ = 1.0;

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 5;
            animation.To = -2.5;
            animation.Duration = new Duration(TimeSpan.FromSeconds(.3));
            m_translation.BeginAnimation(TranslateTransform3D.OffsetXProperty, animation);

            DoubleAnimation rotationAnimation = new DoubleAnimation();
            rotationAnimation.From = 90;
            rotationAnimation.To = 35;
            rotationAnimation.Duration = new Duration(TimeSpan.FromSeconds(.3));
            m_rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, rotationAnimation);
        }

        private Visual m_child;

        private readonly Viewport2DVisual3D m_visual3D;

        private readonly AxisAngleRotation3D m_rotation;
        private readonly ScaleTransform3D m_scale;
        private readonly TranslateTransform3D m_translation;

        #endregion
    }
}