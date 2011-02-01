using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.Set
{
    public static class SetCardDrawingFactory
    {
        static SetCardDrawingFactory()
        {
            s_borderPen = new Pen(Brushes.Transparent, 1);
            s_borderPen.Freeze();

            s_greenPen = new Pen(Brushes.Green, 2);
            s_greenPen.Freeze();

            s_redPen = new Pen(Brushes.Red, 2);
            s_redPen.Freeze();

            s_purplePen = new Pen(Brushes.Purple, 2);
            s_purplePen.Freeze();

            s_stripedGreenBrush = GetStripeBrush(Brushes.Green);
            s_stripedGreenBrush.Freeze();

            s_stripedPurpleBrush = GetStripeBrush(Brushes.Purple);
            s_stripedPurpleBrush.Freeze();

            s_stripedRedBrush = GetStripeBrush(Brushes.Red);
            s_stripedRedBrush.Freeze();

            Pen grayPen = new Pen(Brushes.LightGray, 1);
            grayPen.Freeze();

            RectangleGeometry geometry = new RectangleGeometry(new Rect(0, 0, 100, 140), 10, 10);
            s_cardDrawing = new GeometryDrawing(Brushes.White, grayPen, geometry);
            s_cardDrawing.Freeze();
        }

        public static Drawing GetFullCardDrawing(SetCard setCard)
        {
            Drawing designDrawing = GetCardDesignDrawing(setCard);

            DrawingGroup group = new DrawingGroup();

            group.Children.Add(s_cardDrawing);
            group.Children.Add(designDrawing);

            return group;
        }

        public static Drawing GetCardDesignDrawing(SetCard setCard)
        {
            Contract.Requires<ArgumentNullException>(setCard != null);

            DrawingGroup drawingGroup = new DrawingGroup();

            #region Border

            RectangleGeometry cardBorderGeometry = new RectangleGeometry(new Rect(s_cardSize));
            GeometryDrawing cardBorderDrawing =
                new GeometryDrawing(null, s_borderPen, cardBorderGeometry);

            drawingGroup.Children.Add(cardBorderDrawing);

            #endregion

            #region Brush

            Brush itemBrush = null;
            switch (setCard.Fill)
            {
                case SetFill.Solid:
                    switch (setCard.Color)
                    {
                        case SetColor.Green:
                            itemBrush = Brushes.Green;
                            break;
                        case SetColor.Purple:
                            itemBrush = Brushes.Purple;
                            break;
                        case SetColor.Red:
                            itemBrush = Brushes.Red;
                            break;
                    }
                    break;

                case SetFill.Stripe:
                    switch (setCard.Color)
                    {
                        case SetColor.Green:
                            itemBrush = s_stripedGreenBrush;
                            break;
                        case SetColor.Purple:
                            itemBrush = s_stripedPurpleBrush;
                            break;
                        case SetColor.Red:
                            itemBrush = s_stripedRedBrush;
                            break;
                    }
                    break;

                case SetFill.Empty:
                    itemBrush = Brushes.Transparent;
                    break;
            }

            #endregion

            Pen itemPen = null;
            switch (setCard.Color)
            {
                case SetColor.Green:
                    itemPen = s_greenPen;
                    break;
                case SetColor.Purple:
                    itemPen = s_purplePen;
                    break;
                case SetColor.Red:
                    itemPen = s_redPen;
                    break;
            }

            Point startCenter = new Point(s_cardSize.Width / 2, s_cardSize.Height / 2);

            if (setCard.Count == 2)
            {
                startCenter.Y -= c_centerOffset / 2;
            }
            else if (setCard.Count == 3)
            {
                startCenter.Y -= c_centerOffset;
            }

            GeometryGroup geometryGroup = new GeometryGroup();

            for (int i = 0; i < setCard.Count; i++)
            {
                Geometry itemGeometry = null;

                switch (setCard.Shape)
                {
                    case SetShape.Diamond:

                        itemGeometry = GetDiamondGeometry();
                        break;

                    case SetShape.Oval:

                        itemGeometry = new RectangleGeometry(
                            new Rect(s_itemSize),
                            s_itemSize.Height / 2,
                            s_itemSize.Height / 2);
                        break;

                    case SetShape.Squiggle:

                        itemGeometry = new PathGeometry(s_squigglePathFigureCollection);
                        break;
                }

                itemGeometry.Transform = GetCenterTransform(itemGeometry, startCenter);

                geometryGroup.Children.Add(itemGeometry);

                startCenter.Y += c_centerOffset;
            }

            Debug.Assert(itemBrush != null);
            Debug.Assert(itemPen != null);

            GeometryDrawing itemDrawing =
                new GeometryDrawing(itemBrush, itemPen, geometryGroup);

            drawingGroup.Children.Add(itemDrawing);

            return drawingGroup;
        }

        #region Implementation

        private static TranslateTransform GetCenterTransform(Geometry geometry, Point center)
        {
            Rect bounds = geometry.Bounds;

            Vector centerAtOrigin = (Vector)center + ((Vector)bounds.TopLeft - (Vector)bounds.BottomRight) / 2;

            return new TranslateTransform(centerAtOrigin.X, centerAtOrigin.Y);
        }

        private static Geometry GetDiamondGeometry()
        {
            Point[] points = new Point[]{
                new Point(0, s_itemSize.Height / 2),
                new Point(s_itemSize.Width / 2, s_itemSize.Height),
                new Point(s_itemSize.Width, s_itemSize.Height / 2),
                new Point(s_itemSize.Width / 2, 0)};

            PathSegmentCollection psc = new PathSegmentCollection(
                points.Select(point => new LineSegment(point, true)).OfType<PathSegment>());

            PathFigure pf = new PathFigure(points[0], psc, true);

            PathGeometry geometry = new PathGeometry(new PathFigure[] { pf }, FillRule.EvenOdd, null);

            return geometry;
        }

        private static DrawingBrush GetStripeBrush(SolidColorBrush solidBrush)
        {
            Geometry solidGeometry = new RectangleGeometry(new Rect(0, 0, 5, 5));
            Drawing solidDrawing = new GeometryDrawing(solidBrush, null, solidGeometry);

            Geometry blankGeometry = new RectangleGeometry(new Rect(5, 0, 5, 5));
            Drawing blankDrawing = new GeometryDrawing(Brushes.Transparent, null, blankGeometry);

            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(solidDrawing);
            drawing.Children.Add(blankDrawing);

            DrawingBrush drawingBrush = new DrawingBrush(drawing);

            drawingBrush.TileMode = TileMode.Tile;
            drawingBrush.Stretch = Stretch.None;
            drawingBrush.ViewportUnits = BrushMappingMode.Absolute;

            drawingBrush.Viewport = new Rect(0, 0, 4, 2);

            drawingBrush.Freeze();

            return drawingBrush;
        }

        #endregion

        #region Private Fields

        private static readonly Pen s_redPen, s_greenPen, s_purplePen;
        private static readonly Brush s_stripedRedBrush, s_stripedGreenBrush, s_stripedPurpleBrush;
        private static readonly GeometryDrawing s_cardDrawing;

        private static readonly Pen s_borderPen;

        // To match the h/w ratio of normal playing cards: 2.5" x 3.5"
        // http://en.wikipedia.org/wiki/Playing_card (2008-01-02)
        private static readonly Size s_cardSize = new Size(100, 140);

        private static readonly Size s_itemSize = new Size(70, 30);

        // vertical offset between items
        private const double c_centerOffset = 45;

        private static readonly PathFigureCollection s_squigglePathFigureCollection = PathFigureCollection.Parse(c_squigglePathString);

        private const string c_squigglePathString =
            "M0,24C0,24 0,30 6,30 12,30 16,24 25,24 35,24 36,28 48,28 " +
            "60,27 70,18 70,10 70,2 68,0 64,0 60,0 54,6 45,7 35,7 31,2 18,2 5,1 0,11 0,18z";

        #endregion
    }

    public class CardImageSourceConverter : SimpleValueConverter<SetCard, DrawingImage>
    {
        protected override DrawingImage ConvertBase(SetCard input)
        {
            return new DrawingImage(SetCardDrawingFactory.GetCardDesignDrawing(input));
        }
    }
}