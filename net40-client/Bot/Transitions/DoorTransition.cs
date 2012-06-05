using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace PixelLab.Wpf.Transitions
{
    public class DoorTransition : Transition3D
    {
        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(DoorTransition), new UIPropertyMetadata(Duration.Automatic));

        protected override void BeginTransition3D(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent, Viewport3D viewport)
        {
            Brush clone = CreateBrush(oldContent);

            Size size = transitionElement.RenderSize;
            MeshGeometry3D leftDoor = CreateMesh(new Point3D(),
               new Vector3D(size.Width / 2, 0, 0),
               new Vector3D(0, size.Height, 0),
               1,
               1,
               new Rect(0, 0, 0.5, 1));

            GeometryModel3D leftDoorGeometry = new GeometryModel3D();
            leftDoorGeometry.Geometry = leftDoor;
            leftDoorGeometry.Material = new DiffuseMaterial(clone);

            AxisAngleRotation3D leftRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            leftDoorGeometry.Transform = new RotateTransform3D(leftRotation);

            GeometryModel3D rightDoorGeometry = new GeometryModel3D();
            MeshGeometry3D rightDoor = CreateMesh(new Point3D(size.Width / 2, 0, 0),
                 new Vector3D(size.Width / 2, 0, 0),
                 new Vector3D(0, size.Height, 0),
                 1,
                 1,
                 new Rect(0.5, 0, 0.5, 1));

            rightDoorGeometry.Geometry = rightDoor;
            rightDoorGeometry.Material = new DiffuseMaterial(clone);

            AxisAngleRotation3D rightRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            rightDoorGeometry.Transform = new RotateTransform3D(rightRotation, size.Width, 0, 0);

            Model3DGroup doors = new Model3DGroup();
            doors.Children.Add(leftDoorGeometry);
            doors.Children.Add(rightDoorGeometry);

            ModelVisual3D model = new ModelVisual3D();
            model.Content = doors;
            viewport.Children.Add(model);

            DoubleAnimation da = new DoubleAnimation(90 - 0.5 * FieldOfView, Duration);
            leftRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            da = new DoubleAnimation(-(90 - 0.5 * FieldOfView), Duration);
            rightRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            da = new DoubleAnimation(0, Duration);
            da.Completed += delegate
            {
                EndTransition(transitionElement, oldContent, newContent);
            };
            clone.BeginAnimation(Brush.OpacityProperty, da);
        }
    }
}
