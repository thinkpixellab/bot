using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Common;

namespace PixelLab.SL
{
    [TemplatePart(Name = "PART_Horizontal", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_Vertical", Type = typeof(ScrollBar))]
    public class PanZoomControl : ContentControl
    {
        // ********************************************************************
        // private constants
        // ********************************************************************

        private const string PartHorizontal = "PART_Horizontal";
        private const string PartVertical = "PART_Vertical";

        // ********************************************************************
        // private variables
        // ********************************************************************

        private readonly DelegateCommand _resetCommand;
        private ScrollBar _horizontalScrollBar;
        private ScrollBar _verticalScrollBar;
        private bool _isMouseDown;
        private bool _isFirstResize = true;
        private Point _lastDown = new Point();
        private bool _suspendScrollBarUpdates = false;
        double _scrollminx = 0;
        double _scrollmaxx = 0;
        double _scrollminy = 0;
        double _scrollmaxy = 0;

        // ********************************************************************
        // constructor
        // ********************************************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="PanZoomControl"/> class.
        /// </summary>
        public PanZoomControl()
        {
            this.DefaultStyleKey = typeof(PanZoomControl);
            this.SizeChanged += this.PanZoomControl_SizeChanged;
            _resetCommand = new DelegateCommand(Reset, () => !isReset);
            this.CanPan = true;
        }

        // ********************************************************************
        // dependency properties
        // ********************************************************************

        #region GridLineVisibility (DependencyProperty)
        public Visibility GridLineVisibility
        {
            get { return (Visibility)GetValue(GridLineVisibilityProperty); }
            set { SetValue(GridLineVisibilityProperty, value); }
        }
        public static readonly DependencyProperty GridLineVisibilityProperty =
            DependencyPropHelper.Register<PanZoomControl, Visibility>(
                "GridLineVisibility",
                Visibility.Visible);
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

        public static readonly DependencyProperty MajorGridLineColorProperty = DependencyPropHelper.Register<PanZoomControl, Color>("MajorGridLineColor", Colors.Gray);

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
        public static readonly DependencyProperty MinorGridLineColorProperty = DependencyPropHelper.Register<PanZoomControl, Color>("MinorGridLineColor", Colors.LightGray);

        #endregion

        #region MajorGridLineDistance (DependencyProperty)

        /// <summary>
        /// The distance between each of the major division gridlines
        /// </summary>
        public int MajorGridLineDistance
        {
            get { return (int)GetValue(MajorGridLineDistanceProperty); }
            set { SetValue(MajorGridLineDistanceProperty, value); }
        }
        public static readonly DependencyProperty MajorGridLineDistanceProperty = DependencyPropHelper.Register<PanZoomControl, int>("MajorGridLineDistance", 40);

        #endregion

        #region Subdivisions (DependencyProperty)

        /// <summary>
        /// The number of subdivisions between the major grid lines
        /// </summary>
        public int Subdivisions
        {
            get { return (int)GetValue(SubdivisionsProperty); }
            set { SetValue(SubdivisionsProperty, value); }
        }
        public static readonly DependencyProperty SubdivisionsProperty = DependencyPropHelper.Register<PanZoomControl, int>("Subdivisions", 4);

        #endregion

        #region Scale (DependencyProperty)

        /// <summary>
        /// Indicates how the current scale (level of zoom)
        /// </summary>
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty = DependencyPropHelper.Register<PanZoomControl, double>("Scale", 1.0, (pzc, newVal, oldVal) => pzc.OnScaleChanged(oldVal));

        protected virtual void OnScaleChanged(double oldVal)
        {
            double oldX = OffsetToRelativeOffset(this.OffsetX, oldVal, this.ActualWidth, this.ContentSize.Width);
            double oldY = OffsetToRelativeOffset(this.OffsetY, oldVal, this.ActualHeight, this.ContentSize.Height);
            this.RelativeOffsetX = oldX;
            this.RelativeOffsetY = oldY;
            _resetCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region OffsetX (DependencyProperty)

        /// <summary>
        /// The X offset (before scale is applied). OffsetX and OffsetY are always in the "non-scaled" 
        /// coordinate space. This means that if you are scaled at 2x and update OffsetX by 1 pixel, 
        /// the scaled content will appear to move 2 pixels.
        /// </summary>
        public double OffsetX
        {
            get { return (double)GetValue(OffsetXProperty); }
            set { SetValue(OffsetXProperty, value); }
        }

        public static readonly DependencyProperty OffsetXProperty =
            DependencyPropHelper.Register<PanZoomControl, double>(
                "OffsetX",
                0.0,
                (pzc, newVal, oldval) => pzc.OnOffsetXChanged(oldval, newVal));

        protected virtual void OnOffsetXChanged(double oldVal, double newVal)
        {
            if (!IsClose(oldVal, newVal))
            {
                this.ScaledOffsetX = newVal * this.Scale;
                UpdateScrollBars();
                _resetCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region OffsetY (DependencyProperty)

        /// <summary>
        /// The Y offset (before scale is applied). OffsetX and OffsetY are always in the "non-scaled" 
        /// coordinate space. This means that if you are scaled at 2x and update OffsetX by 1 pixel, 
        /// the scaled content will appear to move 2 pixels.
        public double OffsetY
        {
            get { return (double)GetValue(OffsetYProperty); }
            set { SetValue(OffsetYProperty, value); }
        }
        public static readonly DependencyProperty OffsetYProperty =
            DependencyPropHelper.Register<PanZoomControl, double>(
                "OffsetY",
                0.0,
                (pzc, newVal, oldVal) => pzc.OnOffsetYChanged(oldVal, newVal));

        protected virtual void OnOffsetYChanged(double oldVal, double newVal)
        {
            if (!IsClose(oldVal, newVal))
            {
                this.ScaledOffsetY = newVal * this.Scale;
                UpdateScrollBars();
                _resetCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region ScaledOffsetX (DependencyProperty)

        public double ScaledOffsetX
        {
            get { return (double)GetValue(ScaledOffsetXProperty); }
            private set { SetValue(ScaledOffsetXProperty, value); }
        }

        public static readonly DependencyProperty ScaledOffsetXProperty = DependencyPropHelper.Register<PanZoomControl, double>("ScaledOffsetX", 0.0);

        #endregion

        #region ScaledOffsetY (DependencyProperty)

        public double ScaledOffsetY
        {
            get { return (double)GetValue(ScaledOffsetYProperty); }
            private set { SetValue(ScaledOffsetYProperty, value); }
        }
        public static readonly DependencyProperty ScaledOffsetYProperty = DependencyPropHelper.Register<PanZoomControl, double>("ScaledOffsetY", 0.0);

        #endregion

        #region ClipToBounds (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public bool ClipToBounds
        {
            get { return (bool)GetValue(ClipToBoundsProperty); }
            set { SetValue(ClipToBoundsProperty, value); }
        }
        public static readonly DependencyProperty ClipToBoundsProperty = DependencyPropHelper.Register<PanZoomControl, bool>("ClipToBounds", true, (pzc, newVal, oldVal) => pzc.OnCliptTBoundsChanged());

        protected virtual void OnCliptTBoundsChanged()
        {
            this.UpdateClip(new Size(this.ActualHeight, this.ActualHeight));
        }

        #endregion

        // ********************************************************************
        // public methods
        // ********************************************************************

        /// <summary>
        /// Centers the content.
        /// </summary>
        public void CenterContent()
        {
            this.RelativeOffsetX = 0.5;
            this.RelativeOffsetY = 0.5;
        }

        // ********************************************************************
        // public properties
        // ********************************************************************

        /// <summary>
        /// Gets or sets a value indicating whether this instance can pan.
        /// </summary>
        /// <value><c>True</c> if this instance can pan; otherwise, <c>false</c>.</value>
        public bool CanPan { get; set; }

        public Size ContentSize
        {
            get
            {
                FrameworkElement f = this.Content as FrameworkElement;

                if ((f != null) &&
                    (!double.IsInfinity(f.ActualWidth)) &&
                    (!double.IsNaN(f.ActualWidth)) &&
                    (!double.IsInfinity(f.ActualHeight)) &&
                    (!double.IsNaN(f.ActualHeight)))
                {
                    return new Size(f.ActualWidth, f.ActualHeight);
                }
                else
                {
                    return new Size(this.ActualWidth, this.ActualHeight);
                }
            }
        }

        public double RelativeOffsetX
        {
            get
            {
                return PanZoomControl.OffsetToRelativeOffset(
                    this.OffsetX,
                    this.Scale,
                    this.ActualWidth,
                    this.ContentSize.Width);
            }
            set
            {
                this.OffsetX = PanZoomControl.RelativeOffsetToOffset(
                    value,
                    this.Scale,
                    this.ActualWidth,
                    this.ContentSize.Width);
            }
        }

        public double RelativeOffsetY
        {
            get
            {
                return PanZoomControl.OffsetToRelativeOffset(
                    this.OffsetY,
                    this.Scale,
                    this.ActualHeight,
                    this.ContentSize.Height);
            }
            set
            {
                this.OffsetY = PanZoomControl.RelativeOffsetToOffset(
                    value,
                    this.Scale,
                    this.ActualHeight,
                    this.ContentSize.Height);
            }
        }

        public ICommand ResetCommand { get { return _resetCommand; } }

        public void Reset()
        {
            Scale = 1;
            CenterContent();
        }

        // ********************************************************************
        // overrides and event handlers
        // ********************************************************************

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            var of = oldContent as FrameworkElement;
            if (of != null)
            {
                of.SizeChanged -= child_SizeChanged;
            }

            var nf = newContent as FrameworkElement;
            if (nf != null)
            {
                nf.SizeChanged += child_SizeChanged;
            }
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (this.CanPan && !e.Handled)
            {
                e.Handled = true;
                this._lastDown = e.GetPosition(this);
                this._isMouseDown = true;
                this.CaptureMouse();
            }
            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this._isMouseDown = false;
            this.ReleaseMouseCapture();
        }

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.MouseMove"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this._isMouseDown)
            {
                Point currPos = e.GetPosition(this);
                double x = currPos.X - _lastDown.X;
                double y = currPos.Y - _lastDown.Y;

                this.OffsetX += (x / this.Scale);
                this.OffsetY += (y / this.Scale);

                this._lastDown = currPos;

            }
        }

        // ********************************************************************
        // private methods
        // ********************************************************************

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._horizontalScrollBar = this.GetTemplateChild(PartHorizontal) as ScrollBar;
            if (_horizontalScrollBar != null)
            {
                this._horizontalScrollBar.ValueChanged += this._horizontal_ValueChanged;
                this._horizontalScrollBar.LostMouseCapture += _horizontal_LostMouseCapture;
            }

            this._verticalScrollBar = this.GetTemplateChild(PartVertical) as ScrollBar;
            if (_verticalScrollBar != null)
            {
                this._verticalScrollBar.ValueChanged += this._vertical_ValueChanged;
                this._verticalScrollBar.LostMouseCapture += _vertical_LostMouseCapture;
            }
        }

        private void child_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollBars();
        }

        private void PanZoomControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                UpdateClip(e.NewSize);

                if (_isFirstResize)
                {
                    _isFirstResize = false;
                    this.CenterContent();
                }
                else
                {
                    double oldX = OffsetToRelativeOffset(this.OffsetX, this.Scale, (double)e.PreviousSize.Width, this.ContentSize.Width);
                    double oldY = OffsetToRelativeOffset(this.OffsetY, this.Scale, (double)e.PreviousSize.Height, this.ContentSize.Height);
                    this.RelativeOffsetX = oldX;
                    this.RelativeOffsetY = oldY;
                }
            }
        }

        private void _horizontal_LostMouseCapture(object sender, MouseEventArgs e)
        {
            UpdateScrollBars();
        }

        private void _vertical_LostMouseCapture(object sender, MouseEventArgs e)
        {
            UpdateScrollBars();
        }

        private void _horizontal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isMouseDown)
            {
                try
                {
                    _suspendScrollBarUpdates = true;
                    double span = (_scrollmaxx - _scrollminx);
                    this.OffsetX = ((span - (span * (e.NewValue))) + _scrollminx) / this.Scale;
                }
                finally
                {
                    _suspendScrollBarUpdates = false;
                }
            }
        }

        private void _vertical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isMouseDown)
            {
                try
                {
                    _suspendScrollBarUpdates = true;
                    double span = (_scrollmaxy - _scrollminy);
                    this.OffsetY = ((span - (span * (e.NewValue))) + _scrollminy) / this.Scale;
                }
                finally
                {
                    _suspendScrollBarUpdates = false;
                }
            }
        }

        /// <summary>
        /// Computes the relative offset for a given offset
        /// </summary>
        /// <param name="offset">actual non-scaled offset to be converted</param>
        /// <param name="scale">scale</param>
        /// <param name="actual">actual width/height of the viewable area</param>
        /// <param name="content">width/height of the content</param>
        /// <returns></returns>
        private static double OffsetToRelativeOffset(double offset, double scale, double actual, double content)
        {
            if (actual == 0) return 0;
            double s_content = content * scale;
            double s_offset = offset * scale;
            return (s_offset + (s_content / 2)) / actual;
        }

        /// <summary>
        /// Computes the actual (non-scaled) offset for a given relative offset
        /// </summary>
        /// <param name="reloffset">relative offset to be converted</param>
        /// <param name="scale">scale</param>
        /// <param name="actual">actual width/height of the viewable area</param>
        /// <param name="content">width/height of the content</param>
        /// <returns></returns>
        private static double RelativeOffsetToOffset(double reloffset, double scale, double actual, double content)
        {
            double s_content = content * scale;
            double offsentInView = actual * reloffset;
            double offset = offsentInView - (s_content / 2);
            return offset / scale;
        }

        private void UpdateClip(Size s)
        {
            if (this.ClipToBounds)
            {
                var rectClip = Clip as RectangleGeometry;
                if (rectClip == null)
                {
                    Clip = rectClip = new RectangleGeometry();
                }
                rectClip.Rect = new Rect(new Point(), this.RenderSize);
            }
            else
            {
                this.Clip = null;
            }
        }

        private void UpdateScrollBars()
        {
            if (!_suspendScrollBarUpdates)
            {
                if (_horizontalScrollBar != null)
                {
                    // horizontal 

                    double offsetx = this.OffsetX;
                    double scaledoffsetx = (offsetx * this.Scale);
                    double scaledwidth = (this.ContentSize.Width * this.Scale);

                    // do we need to show the scroll bars
                    if (offsetx >= 0 && ((scaledwidth + scaledoffsetx) <= this.ActualWidth))
                    {
                        _horizontalScrollBar.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _scrollminx = scaledwidth * -1;
                        _scrollminx = Math.Min(_scrollminx, scaledoffsetx);

                        _scrollmaxx = this.ActualWidth;
                        _scrollmaxx = Math.Max(_scrollmaxx, scaledoffsetx);

                        _horizontalScrollBar.Value = 1 - ((scaledoffsetx - _scrollminx) / (_scrollmaxx - _scrollminx));
                        _horizontalScrollBar.ViewportSize = this.ActualWidth / (_scrollmaxx - _scrollminx);
                        _horizontalScrollBar.Visibility = Visibility.Visible;
                    }
                }

                if (_verticalScrollBar != null)
                {
                    // vertical 

                    var offsety = this.OffsetY;
                    var scaledoffsety = (offsety * this.Scale);
                    var scaledheight = (this.ContentSize.Height * this.Scale);

                    // do we need to show the scroll bars
                    if (offsety >= 0 && ((scaledheight + scaledoffsety) <= this.ActualHeight))
                    {
                        _verticalScrollBar.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _scrollminy = scaledheight * -1;
                        _scrollminy = Math.Min(_scrollminy, scaledoffsety);

                        _scrollmaxy = this.ActualHeight;
                        _scrollmaxy = Math.Max(_scrollmaxy, scaledoffsety);

                        _verticalScrollBar.Value = 1 - ((scaledoffsety - _scrollminy) / (_scrollmaxy - _scrollminy));
                        _verticalScrollBar.ViewportSize = this.ActualHeight / (_scrollmaxy - _scrollminy);
                        _verticalScrollBar.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private bool isReset
        {
            get
            {
                return IsClose(Scale, 1) && IsClose(RelativeOffsetX, 0.5) && IsClose(RelativeOffsetY, 0.5);
            }
        }

        private static bool IsClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.0000001;
        }
    }
}
