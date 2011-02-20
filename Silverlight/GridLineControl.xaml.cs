using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            DependencyProperty.Register("MajorGridLineDistance", typeof(int), typeof(GridLineControl),
            new PropertyMetadata(40, new PropertyChangedCallback(OnMajorGridLineDistanceChanged)));

        private static void OnMajorGridLineDistanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnMajorGridLineDistanceChanged(e);
        }

        protected virtual void OnMajorGridLineDistanceChanged(DependencyPropertyChangedEventArgs e)
        {
            RequestGridUpdate();
        }

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
            DependencyProperty.Register("MajorGridLineColor", typeof(Color), typeof(GridLineControl),
            new PropertyMetadata(Colors.Black, new PropertyChangedCallback(OnMajorGridLineColorChanged)));

        private static void OnMajorGridLineColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnMajorGridLineColorChanged(e);
        }

        protected virtual void OnMajorGridLineColorChanged(DependencyPropertyChangedEventArgs e)
        {
            RequestGridUpdate();
        }

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
            DependencyProperty.Register("MinorGridLineSubDivisions", typeof(int), typeof(GridLineControl),
            new PropertyMetadata(4, new PropertyChangedCallback(OnMinorGridLineSubDivisionsChanged)));

        private static void OnMinorGridLineSubDivisionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnMinorGridLineSubDivisionsChanged(e);
        }

        protected virtual void OnMinorGridLineSubDivisionsChanged(DependencyPropertyChangedEventArgs e)
        {
            RequestGridUpdate();
        }

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
            DependencyProperty.Register("MinorGridLineColor", typeof(Color), typeof(GridLineControl),
            new PropertyMetadata(Colors.LightGray, new PropertyChangedCallback(OnMinorGridLineColorChanged)));

        private static void OnMinorGridLineColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnMinorGridLineColorChanged(e);
        }

        protected virtual void OnMinorGridLineColorChanged(DependencyPropertyChangedEventArgs e)
        {
            RequestGridUpdate();
        }

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
            DependencyProperty.Register("Scale", typeof(double), typeof(GridLineControl),
            new PropertyMetadata(1.0, new PropertyChangedCallback(OnScaleChanged)));

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnScaleChanged(e);
        }

        protected virtual void OnScaleChanged(DependencyPropertyChangedEventArgs e)
        {
            RequestGridUpdate();
        }

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
            DependencyProperty.Register("OffsetX", typeof(double), typeof(GridLineControl),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnOffsetXChanged)));

        private static void OnOffsetXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnOffsetXChanged(e);
        }

        protected virtual void OnOffsetXChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateOffset();
        }

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
            DependencyProperty.Register("OffsetY", typeof(double), typeof(GridLineControl),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnOffsetYChanged)));

        private static void OnOffsetYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridLineControl)d).OnOffsetYChanged(e);
        }

        protected virtual void OnOffsetYChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateOffset();
        }

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
