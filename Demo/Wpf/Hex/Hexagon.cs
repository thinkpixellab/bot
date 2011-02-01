using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelLab.Wpf.Demo.Hex
{
    public class Hexagon : Shape
    {
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius",
            typeof(double),
            typeof(Hexagon),
            new FrameworkPropertyMetadata(DefaultRadius,
            FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnRadiusInvalidated),
            null));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        private static void OnRadiusInvalidated(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Hexagon rp = (Hexagon)o;
            rp._geometryCacheValid = false;
        }

        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(
            "StartAngle",
            typeof(double),
            typeof(Hexagon),
            new FrameworkPropertyMetadata(DefaultStartAngle,
            FrameworkPropertyMetadataOptions.AffectsMeasure,
            new PropertyChangedCallback(StartAngleInvalidated),
            null));

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        private static void StartAngleInvalidated(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Hexagon hexagon = (Hexagon)o;
            hexagon._geometryCacheValid = false;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (_geometryCache == null || !_geometryCacheValid)
                {
                    _geometryCache = getGeometry();
                    _geometryCacheValid = true;
                }
                return _geometryCache;
            }
        }

        private PathGeometry getGeometry()
        {
            PointCollection pointCollection = new PointCollection();
            PathFigure pathFigure = new PathFigure();

            double radiansPerPoint = Math.PI * 2 / PointCount;
            for (int i = 0; i < PointCount; i++)
            {
                Point p = new Point(
                    Math.Sin(radiansPerPoint * i + StartAngle) * Radius + Radius,
                    Math.Cos(radiansPerPoint * i + StartAngle) * Radius + Radius);
                pointCollection.Add(p);
            }

            pointCollection = OffsetPoints(pointCollection);

            pathFigure.StartPoint = pointCollection[0];
            pathFigure.Segments.Add(new PolyLineSegment(pointCollection, true));
            pathFigure.IsClosed = true;

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private static PointCollection OffsetPoints(PointCollection points)
        {
            double minLeft = double.MaxValue;
            double minTop = double.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].X < minLeft)
                    minLeft = points[i].X;
                if (points[i].Y < minTop)
                    minTop = points[i].Y;
            }

            PointCollection pc = new PointCollection();
            for (int i = 0; i < points.Count; i++)
            {
                pc.Add(new Point(points[i].X - minLeft, points[i].Y - minTop));
            }
            return pc;
        }

        private Geometry _geometryCache;
        private bool _geometryCacheValid;
        private int PointCount = 6;

        private const double DefaultRadius = 100;
        private const double DefaultStartAngle = Math.PI / 6;
    }
}
