using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using PixelLab.Common;

namespace PixelLab.Wpf.Transitions
{
    // Uses simple cloth physics to model curtains opening
    public class ClothTransition : Transition3D
    {
        protected override void BeginTransition3D(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent, Viewport3D viewport)
        {
            int xparticles = 15, yparticles = 15;
            Size size = transitionElement.RenderSize;

            if (size.Width > size.Height)
                yparticles = (int)(xparticles * size.Height / size.Width);
            else
                xparticles = (int)(yparticles * size.Width / size.Height);

            MeshGeometry3D mesh = CreateMesh(new Point3D(), new Vector3D(size.Width, 0, 0), new Vector3D(0, size.Height, 0), xparticles - 1, yparticles - 1, new Rect(0, 0, 1, 1));
            Brush cloneBrush = CreateBrush(oldContent);
            Material clone = new DiffuseMaterial(cloneBrush);

            double ustep = size.Width / (xparticles - 1), vstep = size.Height / (yparticles - 1);

            Point3DCollection points = mesh.Positions;

            Random rand = Util.Rnd;
            // add some random movement to the z order
            for (int i = 0; i < points.Count; i++)
                points[i] += 0.1 * ustep * (rand.NextDouble() * 2 - 1) * new Vector3D(0, 0, 1);

            Point3DCollection oldPoints = points.Clone();

            Vector3D acceleration = new Vector3D(0, 700, 0); //gravity
            double timeStep = 1.0 / 60.0;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(timeStep);
            bool fading = false;
            double time = 0;
            double slideVelocity = size.Width / 2.0;
            double windScale = 30 * size.Width * size.Height;
            timer.Tick += delegate
            {
                time = time + timeStep;
                Point mousePos = Mouse.GetPosition(viewport);
                Point3D mousePos3D = new Point3D(mousePos.X, mousePos.Y, -10);

                // Cloth physics based on work of Thomas Jakobsen http://www.teknikus.dk/tj/gdc2001.htm
                for (int i = 0; i < oldPoints.Count; i++)
                {
                    Point3D currentPoint = points[i];
                    Vector3D wind = new Vector3D(0, 0, windScale / (mousePos3D - currentPoint).LengthSquared);
                    Point3D newPoint = currentPoint + (currentPoint - oldPoints[i]) + timeStep * timeStep * (acceleration + wind);

                    if (newPoint.Y > size.Height)
                        newPoint.Y = size.Height;

                    oldPoints[i] = newPoint;
                }

                //for (int j = 0; j < 5; j++)
                for (int i = oldPoints.Count - 1; i > 0; i--)
                {
                    // constrain with point to the left
                    if (i > yparticles)
                        Constrain(oldPoints, i, i - yparticles, ustep);
                    // constrain with point to the top
                    if (i % yparticles != 0)
                        Constrain(oldPoints, i, i - 1, vstep);
                }

                // slide the top row of points to the left
                for (int i = 0; i < xparticles; i += 1)
                    oldPoints[i * yparticles] = new Point3D(Math.Max(0, i * ustep - slideVelocity * time * i / (xparticles - 1)), 0, 0);

                if (!fading && points[points.Count - yparticles].X < size.Width / 2)
                {
                    fading = true;
                    DoubleAnimation da = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(1.5)));
                    da.Completed += delegate
                    {
                        timer.Stop();
                        EndTransition(transitionElement, oldContent, newContent);
                    };
                    cloneBrush.BeginAnimation(Brush.OpacityProperty, da);
                }

                // Swap position arrays
                mesh.Positions = oldPoints;
                oldPoints = points;
                points = mesh.Positions;
            };
            timer.Start();

            GeometryModel3D geo = new GeometryModel3D(mesh, clone);
            geo.BackMaterial = clone;
            ModelVisual3D model = new ModelVisual3D();
            model.Content = geo;
            viewport.Children.Add(model);
        }

        private static void Constrain(Point3DCollection points, int i1, int i2, double length)
        {
            Point3D p1 = points[i1], p2 = points[i2];
            Vector3D delta = p2 - p1;
            double deltalength = delta.Length;
            double diff = (deltalength - length) / deltalength;
            p1 += delta * 0.5 * diff;
            p2 -= delta * 0.5 * diff;

            points[i1] = p1;
            points[i2] = p2;
        }
    }
}
