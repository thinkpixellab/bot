using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace PixelLab.Wpf.Transitions
{
    public class RotateTransition : Transition3D
    {
        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(RotateTransition), new UIPropertyMetadata(Duration.Automatic));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(RotateTransition), new UIPropertyMetadata(90.0), IsAngleValid);

        private static bool IsAngleValid(object value)
        {
            double angle = (double)value;
            return angle >= 0 && angle < 180;
        }

        public RotateDirection Direction
        {
            get { return (RotateDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(RotateDirection), typeof(RotateTransition), new UIPropertyMetadata(RotateDirection.Left));

        public bool Contained
        {
            get { return (bool)GetValue(ContainedProperty); }
            set { SetValue(ContainedProperty, value); }
        }
        public static readonly DependencyProperty ContainedProperty =
            DependencyProperty.Register("Contained", typeof(bool), typeof(RotateTransition), new UIPropertyMetadata(true));

        protected override void BeginTransition3D(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent, Viewport3D viewport)
        {
            Size size = transitionElement.RenderSize;

            Point3D origin = new Point3D(); // origin of 2nd face
            Vector3D u = new Vector3D(), v = new Vector3D(); // u & v vectors of 2nd face

            double angle = Angle;
            Point3D rotationCenter;
            Vector3D rotationAxis;
            RotateDirection direction = Direction;

            TranslateTransform3D translation = null;
            double angleRads = Angle * Math.PI / 180;
            if (direction == RotateDirection.Left || direction == RotateDirection.Right)
            {
                if (Contained)
                {
                    rotationCenter = new Point3D(direction == RotateDirection.Left ? size.Width : 0, 0, 0);
                    translation = new TranslateTransform3D();
                    DoubleAnimation x = new DoubleAnimation(direction == RotateDirection.Left ? -size.Width : size.Width, Duration);
                    translation.BeginAnimation(TranslateTransform3D.OffsetXProperty, x);
                }
                else
                {
                    rotationCenter = new Point3D(size.Width / 2, 0, size.Width / 2 * Math.Tan(angle / 2 * Math.PI / 180));
                }

                rotationAxis = new Vector3D(0, 1, 0);

                if (direction == RotateDirection.Left)
                {
                    u.X = -size.Width * Math.Cos(angleRads);
                    u.Z = size.Width * Math.Sin(angleRads);

                    origin.X = size.Width;
                }
                else
                {
                    u.X = -size.Width * Math.Cos(angleRads);
                    u.Z = -size.Width * Math.Sin(angleRads);

                    origin.X = -u.X;
                    origin.Z = -u.Z;
                }
                v.Y = size.Height;
            }
            else
            {
                if (Contained)
                {
                    rotationCenter = new Point3D(0, direction == RotateDirection.Up ? size.Height : 0, 0);
                    translation = new TranslateTransform3D();
                    DoubleAnimation y = new DoubleAnimation(direction == RotateDirection.Up ? -size.Height : size.Height, Duration);
                    translation.BeginAnimation(TranslateTransform3D.OffsetYProperty, y);
                }
                else
                {
                    rotationCenter = new Point3D(0, size.Height / 2, size.Height / 2 * Math.Tan(angle / 2 * Math.PI / 180));
                }

                rotationAxis = new Vector3D(1, 0, 0);

                if (direction == RotateDirection.Up)
                {
                    v.Y = -size.Height * Math.Cos(angleRads);
                    v.Z = size.Height * Math.Sin(angleRads);

                    origin.Y = size.Height;
                }
                else
                {
                    v.Y = -size.Height * Math.Cos(angleRads);
                    v.Z = -size.Height * Math.Sin(angleRads);

                    origin.Y = -v.Y;
                    origin.Z = -v.Z;
                }
                u.X = size.Width;
            }

            double endAngle = 180 - angle;
            if (direction == RotateDirection.Right || direction == RotateDirection.Up)
                endAngle = -endAngle;

            ModelVisual3D m1, m2;
            viewport.Children.Add(m1 = MakeSide(oldContent, new Point3D(), new Vector3D(size.Width, 0, 0), new Vector3D(0, size.Height, 0), endAngle, rotationCenter, rotationAxis, null));
            viewport.Children.Add(m2 = MakeSide(newContent, origin, u, v, endAngle, rotationCenter, rotationAxis, delegate
            {
                EndTransition(transitionElement, oldContent, newContent);
            }));

            m1.Transform = m2.Transform = translation;
        }

        private ModelVisual3D MakeSide(
            ContentPresenter content,
            Point3D origin,
            Vector3D u,
            Vector3D v,
            double endAngle,
            Point3D rotationCenter,
            Vector3D rotationAxis,
            EventHandler onCompleted)
        {
            MeshGeometry3D sideMesh = CreateMesh(origin, u, v, 1, 1, new Rect(0, 0, 1, 1));

            GeometryModel3D sideModel = new GeometryModel3D();
            sideModel.Geometry = sideMesh;

            Brush clone = CreateBrush(content);
            sideModel.Material = new DiffuseMaterial(clone);

            AxisAngleRotation3D rotation = new AxisAngleRotation3D(rotationAxis, 0);
            sideModel.Transform = new RotateTransform3D(rotation, rotationCenter);

            DoubleAnimation da = new DoubleAnimation(endAngle, Duration);
            if (onCompleted != null)
                da.Completed += onCompleted;

            rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, da);

            ModelVisual3D side = new ModelVisual3D();
            side.Content = sideModel;
            return side;
        }
    }

    public enum RotateDirection
    {
        Up,
        Down,
        Left,
        Right
    }
}
