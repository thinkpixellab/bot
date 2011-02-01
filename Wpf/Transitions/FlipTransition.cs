using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace PixelLab.Wpf.Transitions
{
    public class FlipTransition : Transition3D
    {
        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(FlipTransition), new UIPropertyMetadata(Duration.Automatic));

        public RotateDirection Direction
        {
            get { return (RotateDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(RotateDirection), typeof(FlipTransition), new UIPropertyMetadata(RotateDirection.Left));

        protected override void BeginTransition3D(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent, Viewport3D viewport)
        {
            Size size = transitionElement.RenderSize;

            // Create a rectangle
            MeshGeometry3D mesh = CreateMesh(new Point3D(),
                new Vector3D(size.Width, 0, 0),
                new Vector3D(0, size.Height, 0),
                1,
                1,
                new Rect(0, 0, 1, 1));

            GeometryModel3D geometry = new GeometryModel3D();
            geometry.Geometry = mesh;
            VisualBrush clone = new VisualBrush(oldContent);
            geometry.Material = new DiffuseMaterial(clone);

            ModelVisual3D model = new ModelVisual3D();
            model.Content = geometry;

            viewport.Children.Add(model);

            Vector3D axis;
            Point3D center = new Point3D();
            switch (Direction)
            {
                case RotateDirection.Left:
                    axis = new Vector3D(0, 1, 0);
                    break;
                case RotateDirection.Right:
                    axis = new Vector3D(0, -1, 0);
                    center = new Point3D(size.Width, 0, 0);
                    break;
                case RotateDirection.Up:
                    axis = new Vector3D(-1, 0, 0);
                    break;
                default:
                    axis = new Vector3D(1, 0, 0);
                    center = new Point3D(0, size.Height, 0);
                    break;
            }
            AxisAngleRotation3D rotation = new AxisAngleRotation3D(axis, 0);
            model.Transform = new RotateTransform3D(rotation, center);

            DoubleAnimation da = new DoubleAnimation(0, Duration);
            clone.BeginAnimation(Brush.OpacityProperty, da);

            da = new DoubleAnimation(90, Duration);
            da.Completed += delegate
            {
                EndTransition(transitionElement, oldContent, newContent);
            };
            rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);
        }
    }
}
