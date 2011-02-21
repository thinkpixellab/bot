#define NOT_FUNKY

using System;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelLab.Common;

namespace PixelLab.SL
{
    public partial class FlipControl : UserControl
    {
        public FlipControl()
        {
            this.WatchDataContextChanged(onDataContextChanged);
            InitializeComponent();
        }

        public Size Size
        {
            get { return m_flipper.PageSize; }
            set { Config(value); }
        }

        public void SetChild(FrameworkElement value)
        {
            Contract.Requires(value != null);
            m_contentPresenter.Content = m_currentContent = value;
        }

        public void Config(Size pageSize)
        {
            Contract.Requires(pageSize.IsValid());
            Debug.Assert(m_flipper == null);

            PageWidth = pageSize.Width;
            PageHeight = pageSize.Height;
            PageDoubleWidth = pageSize.Width * 2;
            m_turningPageBack.Clip = new RectangleGeometry() { Rect = new Rect(new Point(), pageSize) };

            // TODO: HotSpot support need to be enabled with page content model
            m_flipper = new PageFlipper(pageSize, null, m_pageTop, m_curlShadow, m_shadow);

            m_flipper.IsAnimatingChanged += (sender, args) =>
            {
                if (!m_flipper.IsAnimating)
                {
                    m_working = false;
                    var stillWorking = startDance();
                    if (!stillWorking)
                    {
                        OnDoneFlipping();
                    }
                }
            };

            m_turningPageBack.RenderTransform = m_flipper.FlippingPageBackTransform;
            m_page2nd.Clip = m_flipper.NextPageClip;
            m_pageTop.Clip = m_flipper.PageHolderClip;

            m_contentBitmapLeft = new WriteableBitmap((int)pageSize.Width, (int)pageSize.Height);
            m_contentBitmapRight = new WriteableBitmap((int)pageSize.Width, (int)pageSize.Height);
            m_currentContentSnapshot = new WriteableBitmap((int)pageSize.Width * 2, (int)pageSize.Height);
            m_nextContentSnapshot = new WriteableBitmap((int)pageSize.Width * 2, (int)pageSize.Height);

#if FUNKY
      var stack = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Opacity = .67 };

      (new WriteableBitmap[] { m_currentContentSnapshot, m_nextContentSnapshot, m_contentBitmapLeft, m_contentBitmapRight }).ForEach(wp => {
        var image = new Image() { Source = wp, Stretch = Stretch.UniformToFill, Height = 150, Width = 200 };
        stack.Children.Add(image);
      });

      ((Grid)Content).Children.Add(stack);

#endif

            m_leftImage.Source = m_contentBitmapLeft;
            m_rightImage.Source = m_contentBitmapRight;
        }

        public event EventHandler<FlippingEventArgs> FlipStarting;

        public bool IsFlipping
        {
            get
            {
                if (m_flipper.IsAnimating)
                {
                    Debug.Assert(m_working);
                }
                return m_working;
            }
        }

        public event EventHandler DoneFlipping;

        private void OnDoneFlipping()
        {
            Debug.Assert(!IsFlipping);
            var handler = DoneFlipping;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #region dps
        public double PageWidth
        {
            get { return (double)GetValue(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        public static readonly DependencyProperty PageWidthProperty =
            DependencyPropHelper.Register<FlipControl, double>("PageWidth");

        public double PageHeight
        {
            get { return (double)GetValue(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        public static readonly DependencyProperty PageHeightProperty =
            DependencyPropHelper.Register<FlipControl, double>("PageHeight");

        public double PageDoubleWidth
        {
            get { return (double)GetValue(PageDoubleWidthProperty); }
            set { SetValue(PageDoubleWidthProperty, value); }
        }

        public static readonly DependencyProperty PageDoubleWidthProperty =
            DependencyPropHelper.Register<FlipControl, double>("PageDoubleWidth");

        #endregion

        #region private impl

        private void onDataContextChanged(object oldValue, object newValue)
        {
            m_newData = newValue;
            startDance();
        }

        private bool startDance()
        {
            if (m_working)
            {
                if (m_newData != null)
                {
                    // new data and we're working...make the flip turbo!
                    m_flipper.GoTurbo();
                }
            }
            else
            {
                if (m_newData != null)
                {
                    var args = new FlippingEventArgs(m_currentData, m_newData);
                    var handler = FlipStarting;
                    if (handler != null)
                    {
                        handler(this, args);
                    }
                    m_direction = args.FlipDirection;

                    if (m_direction == FlipDirection.None)
                    {
                        m_contentPresenter.DataContext = m_currentData = m_newData;
                    }
                    else
                    {
                        m_working = true;
                        _1_snapCurrentContent();
                    }
                }
                else
                {
                    setImageVisibility(Visibility.Collapsed);
                }
            }

            return m_working;
        }

        private void setImageVisibility(Visibility visibility)
        {
            m_topImage.Visibility = m_leftImage.Visibility = m_rightImage.Visibility = visibility;
        }

        private void _1_snapCurrentContent()
        {
            Debug.Assert(m_working);
            m_contentPresenter.DataContext = m_currentData;

            Dispatcher.BeginInvoke(() =>
            {
                m_currentContentSnapshot.Render(m_contentPresenter, null);
                m_currentContentSnapshot.Invalidate();

                m_topImage.Visibility = Visibility.Visible;
                m_topImage.Source = m_currentContentSnapshot;

                _2_snapNewContent();
            });
        }

        private void _2_snapNewContent()
        {
            m_contentPresenter.DataContext = m_currentData = m_newData;
            m_newData = null;

            Dispatcher.BeginInvoke(() =>
            {
                m_nextContentSnapshot.Render(m_contentPresenter, null);
                m_nextContentSnapshot.Invalidate();

                _3_animate();
            });
        }

        private void _3_animate()
        {
            if (m_direction == FlipDirection.Next)
            {
                setImageVisibility(Visibility.Visible);
                m_nextContentSnapshot.CopyPixels(m_contentBitmapLeft, 0, 0);
                m_nextContentSnapshot.CopyPixels(m_contentBitmapRight, m_contentBitmapRight.PixelWidth, 0);
                m_flipper.NextPage();
            }
            else
            {
                m_topImage.Visibility = Visibility.Collapsed;
                m_leftImage.Visibility = m_rightImage.Visibility = Visibility.Visible;
                Debug.Assert(m_direction == FlipDirection.Previous);
                m_currentContentSnapshot.CopyPixels(m_contentBitmapLeft, 0, 0);
                m_currentContentSnapshot.CopyPixels(m_contentBitmapRight, m_contentBitmapRight.PixelWidth, 0);
                m_flipper.PreviousPage();
            }
        }

        private WriteableBitmap m_contentBitmapLeft;
        private WriteableBitmap m_contentBitmapRight;
        private WriteableBitmap m_currentContentSnapshot;
        private WriteableBitmap m_nextContentSnapshot;

        private FrameworkElement m_currentContent;

        private object m_newData, m_currentData;
        bool m_working;
        private FlipDirection m_direction;

        private PageFlipper m_flipper;

        #endregion
    }

    public class FlippingEventArgs : EventArgs
    {
        public FlippingEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object OldValue { get; private set; }
        public object NewValue { get; private set; }
        public FlipDirection FlipDirection { get; set; }
    }

    public enum FlipDirection { None, Next, Previous }
}
