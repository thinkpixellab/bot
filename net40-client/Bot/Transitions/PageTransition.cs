using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace PixelLab.Wpf.Transitions
{
    // Cloth physics with extra constraints to the sides of the pages
    public class PageTransition : Transition3D
    {
        protected override void BeginTransition3D(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent, Viewport3D viewport)
        {
            int xparticles = 10, yparticles = 10;
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

            Point3DCollection oldPoints = points.Clone();

            double timeStep = 1.0 / 30.0;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(timeStep);
            double time = 0;
            double duration = 2;
            timer.Tick += delegate
            {
                time = time + timeStep;
                Point mousePos = Mouse.GetPosition(viewport);
                Point3D mousePos3D = new Point3D(mousePos.X, mousePos.Y, -10);

                // Cloth physics based on work of Thomas Jakobsen http://www.ioi.dk/~thomas
                for (int i = 0; i < oldPoints.Count; i++)
                {
                    Point3D currentPoint = points[i];
                    Point3D newPoint = currentPoint + 0.9 * (currentPoint - oldPoints[i]);

                    if (newPoint.Y > size.Height)
                        newPoint.Y = size.Height;

                    oldPoints[i] = newPoint;
                }

                for (int a = yparticles - 1; a >= 0; a--)
                    for (int b = xparticles - 1; b >= 0; b--)
                    {
                        int i = b * yparticles + a;
                        // constrain with point to the left
                        if (i > yparticles)
                            Constrain(oldPoints, i, i - yparticles, ustep);
                        // constrain with point to the top
                        if (i % yparticles != 0)
                            Constrain(oldPoints, i, i - 1, vstep);

                        // constrain the sides
                        if (a == 0)
                            oldPoints[i] = new Point3D(oldPoints[i].X, 0, oldPoints[i].Z);
                        if (a == yparticles - 1)
                            oldPoints[i] = new Point3D(oldPoints[i].X, size.Height, oldPoints[i].Z);

                        if (b == 0)
                            oldPoints[i] = new Point3D(0, a * size.Height / (yparticles - 1), 0);

                        if (b == xparticles - 1)
                        {
                            double angle = time / duration * Math.PI / (0.8 + 0.5 * (yparticles - (double)a) / yparticles);
                            oldPoints[i] = new Point3D(size.Width * Math.Cos(angle), a * size.Height / (yparticles - 1), -size.Width * Math.Sin(angle));
                        }
                    }

                if (time > (duration - 0))
                {
                    timer.Stop();
                    EndTransition(transitionElement, oldContent, newContent);
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
