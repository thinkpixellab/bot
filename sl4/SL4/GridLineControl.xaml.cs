using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelLab.Common;

namespace PixelLab.SL
{
    public partial class GridLineControl : UserControl
    {
        private WriteableBitmap bitmap;
        private Size bitmapSize = new Size();

        public GridLineControl()
        {
            InitializeComponent();
            this.SizeChanged += this.GridLineControl_SizeChanged;
        }

        void GridLineControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((this.bitmapSize.Width != e.NewSize.Width) ||
                (this.bitmapSize.Height != e.NewSize.Height))
            {
                this.bitmapSize = e.NewSize;
                this.bitmap = null;
                this.RequestGridUpdate();
            }

            // create a new clip
            RectangleGeometry r = new RectangleGeometry();
            r.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
            this.RootCanvas.Clip = r;

        }

        /// <summary>
        /// Called internally to let us know that something has changed that requires
        /// a recalculation of the grid. This method will cue up a change for the next
        /// render.
        /// </summary>
        private void RequestGridUpdate()
        {
            if (Visibility.Visible == this.Visibility)
            {
                CompositionTarget.Rendering += this.CompositionTarget_Rendering;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= this.CompositionTarget_Rendering;
            UpdateGrid();
        }

        #region MajorGridLineDistance (DependencyProperty)

        /// <summary>
        /// The distance between each of the major division gridlines
        /// </summary>
        public int MajorGridLineDistance
        {
            get { return (int)GetValue(MajorGridLineDistanceProperty); }
            set { SetValue(MajorGridLineDistanceProperty, value); }
        }
        public static readonly DependencyProperty MajorGridLineDistanceProperty =
            DependencyPropHelper.Register<GridLineControl, int>("MajorGridLineDistance", 40, (e, n, o) => e.RequestGridUpdate());

        #endregion

        #region MajorGridLineColor (DependencyProperty)

        /// <summary>
        /// The color used to draw the major grid lines
        /// </summary>
        public Color MajorGridLineColor
        {
            get { return (Color)GetValue(MajorGridLineColorProperty); }
            set { SetValue(MajorGridLineColorProperty, value); }
        }
        public static readonly DependencyProperty MajorGridLineColorProperty =
            DependencyPropHelper.Register<GridLineControl, Color>("MajorGridLineColor", Colors.Black, (e, n, o) => e.RequestGridUpdate());

        #endregion

        #region MinorGridLineSubDivisions (DependencyProperty)

        /// <summary>
        /// The number of subdividing lines that occur between each major grid line.
        /// </summary>
        public int MinorGridLineSubDivisions
        {
            get { return (int)GetValue(MinorGridLineSubDivisionsProperty); }
            set { SetValue(MinorGridLineSubDivisionsProperty, value); }
        }
        public static readonly DependencyProperty MinorGridLineSubDivisionsProperty =
            DependencyPropHelper.Register<GridLineControl, int>("MinorGridLineSubDivisions", 4, (glc, newVal, oldVal) => glc.RequestGridUpdate());

        #endregion

        #region MinorGridLineColor (DependencyProperty)

        /// <summary>
        /// The color used to draw the minor / subdividing grid lines
        /// </summary>
        public Color MinorGridLineColor
        {
            get { return (Color)GetValue(MinorGridLineColorProperty); }
            set { SetValue(MinorGridLineColorProperty, value); }
        }
        public static readonly DependencyProperty MinorGridLineColorProperty =
            DependencyPropHelper.Register<GridLineControl, Color>("MinorGridLineColor", Colors.LightGray, (glc, n, o) => glc.RequestGridUpdate());

        #endregion

        #region Scale (DependencyProperty)

        /// <summary>
        /// The current scale (e.g. zoom) for the grid lines
        /// </summary>
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyPropHelper.Register<GridLineControl, double>("Scale", 1.0, (e, n, o) => e.RequestGridUpdate());

        #endregion

        #region OffsetX (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public double OffsetX
        {
            get { return (double)GetValue(OffsetXProperty); }
            set { SetValue(OffsetXProperty, value); }
        }
        public static readonly DependencyProperty OffsetXProperty =
            DependencyPropHelper.Register<GridLineControl, double>("OffsetX", 0.0, (e, n, o) => e.UpdateOffset());

        #endregion

        #region OffsetY (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public double OffsetY
        {
            get { return (double)GetValue(OffsetYProperty); }
            set { SetValue(OffsetYProperty, value); }
        }
        public static readonly DependencyProperty OffsetYProperty =
            DependencyPropHelper.Register<GridLineControl, double>("OffsetY", 0.0, (e, n, o) => e.UpdateOffset());

        #endregion

        private void UpdateOffset()
        {
            // as a perf optimization, we overdraw the canvas by two major divisions and
            // this gives us the ability to just translate the image in order to acount for offsets

            double major = this.MajorGridLineDistance * this.Scale;

            ImageHost.SetValue(Canvas.LeftProperty, Math.Round((OffsetX % major) - major));
            ImageHost.SetValue(Canvas.TopProperty, Math.Round((OffsetY % major) - major));
        }

        static int count;

        private void UpdateGrid()
        {
            count++;

            double major = this.MajorGridLineDistance * this.Scale;
            double minor = major / (double)this.MinorGridLineSubDivisions;

            int width = (int)this.bitmapSize.Width + (int)(2.0 * (major + 1));
            int height = (int)this.bitmapSize.Height + (int)(2.0 * (major + 1));

            var majorAlpha = MajorGridLineColor.A + 1;
            var majorColor =
                (MajorGridLineColor.A << 24) |
                ((byte)((MajorGridLineColor.R * majorAlpha) >> 8) << 16) |
                ((byte)((MajorGridLineColor.G * majorAlpha) >> 8) << 8) |
                ((byte)((MajorGridLineColor.B * majorAlpha) >> 8));

            var minorAlpha = MinorGridLineColor.A + 1;
            var minorColor =
                (MinorGridLineColor.A << 24) |
                ((byte)((MinorGridLineColor.R * minorAlpha) >> 8) << 16) |
                ((byte)((MinorGridLineColor.G * minorAlpha) >> 8) << 8) |
                ((byte)((MinorGridLineColor.B * minorAlpha) >> 8));

            if (bitmap == null || bitmap.PixelWidth != width || bitmap.PixelHeight != height)
            {
                DisplayImage.Source = null;
                bitmap = new WriteableBitmap(width, height);
                DisplayImage.Source = bitmap;
                System.GC.Collect();
            }
            else
            {
                for (int offset = 0; offset < width * height; offset++) bitmap.Pixels[offset] = 0;
            }

            // draw minor lines

            double nextY = 0;
            for (int y = 0; y < height; y = (int)Math.Round(nextY))
            {
                nextY += minor;
                for (int x = 0; x < width; x++)
                {
                    bitmap.Pixels[(y * width) + x] = minorColor;
                }
            }

            double nextX = 0;
            for (int x = 0; x < width; x = (int)Math.Round(nextX))
            {
                nextX += minor;
                for (int y = 0; y < height; y++)
                {
                    bitmap.Pixels[(y * width) + x] = minorColor;
                }
            }

            // draw major lines

            nextY = 0;
            for (int y = 0; y < height; y = (int)Math.Round(nextY))
            {
                nextY += major;
                for (int x = 0; x < width; x++)
                {
                    bitmap.Pixels[(y * width) + x] = majorColor;
                }
            }

            nextX = 0;
            for (int x = 0; x < width; x = (int)Math.Round(nextX))
            {
                nextX += major;
                for (int y = 0; y < height; y++)
                {
                    bitmap.Pixels[(y * width) + x] = majorColor;
                }
            }

            //bitmap.Invalidate();
            this.UpdateOffset();
        }
    }
}
