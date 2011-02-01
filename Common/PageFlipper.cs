using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;
using System.Windows.Media;

namespace PixelLab.Common
{
    public class PageFlipper
    {
        public PageFlipper(Size pageSize, UIElement hotCorner, UIElement pageHolder, FrameworkElement curlShadow, FrameworkElement dropShadow)
        {
            Contract.Requires(pageSize.IsValid());
            m_pageSize = pageSize;

            m_hotCorner = hotCorner;

            m_curlShadow = curlShadow;
            m_dropShadow = dropShadow;

            // can be null
            m_pageHolder = pageHolder;

            #region helper values
            m_pageHalfHeight = m_pageSize.Height * .5;
            m_origin = new Point(m_pageSize.Width, m_pageSize.Height / 2.0);
            m_pageDiagonal = Math.Sqrt(m_pageSize.Width * m_pageSize.Width + m_pageSize.Height * m_pageSize.Height);
            m_pointLeft = new Point(-m_pageSize.Width, m_pageHalfHeight);
            m_pointRight = new Point(m_pageSize.Width, m_pageHalfHeight);
            #endregion

            #region clips and transforms

            m_pageTransformGroup = new TransformGroup();
            m_pageTransformGroup.Children.Add(new TranslateTransform() { Y = -m_pageSize.Height });
            m_pageTransformGroup.Children.Add(m_pageRotateTransform = new RotateTransform());
            m_pageTransformGroup.Children.Add(m_pageTranslateTransform = new TranslateTransform());

            m_pageHolderClip = new RectangleGeometry() { Rect = new Rect(0, 0, m_pageSize.Width, m_pageSize.Height * 2) };
            m_nextPageClip = new RectangleGeometry() { Rect = new Rect(0, 0, m_pageSize.Width * 2, m_pageSize.Height * 2) };

            if (m_curlShadow != null)
            {
                var group = new TransformGroup();
                group.Children.Add(m_curlShadowRotate = new RotateTransform());
                group.Children.Add(m_curlShadowTranslate = new TranslateTransform());
                m_curlShadow.RenderTransform = group;

                m_curlShadow.Height = m_pageDiagonal * 1.5;
                m_curlShadowRotate.CenterX = m_curlShadow.Width;
                m_curlShadowRotate.CenterY = m_pageDiagonal;

                m_curlShadowTranslate.Y = m_pageSize.Height - m_pageDiagonal;
            }

            if (m_dropShadow != null)
            {
                //m_dropShadowBlurEffect = new BlurEffect() { Radius = 20 };
                //m_dropShadow.Effect = m_dropShadowBlurEffect;
                m_dropShadow.RenderTransform = m_pageTransformGroup;
                m_dropShadow.Width = m_pageSize.Width;
                m_dropShadow.Height = m_pageSize.Height;
            }

            #endregion

            // MOST OF THE LOADED FUNCTION IS USED TO SET LOOK UP VALUES
            // BASED ON THE pageWidth AND pageHeight INITIALLY DEFINED.
            // POSITION THE PAGE AND SPINE VISUALS ACCORDINGLY
            m_mouse.X = m_pageSize.Width - 1;
            m_mouse.Y = m_pageHalfHeight - 1;
            m_follow = m_mouse;

            #region mouse handlers

            if (m_hotCorner != null && m_pageHolder != null)
            {
                m_hotCorner.MouseMove += (sender, e) =>
                {
                    m_animating = false;
                    m_listener.StartListening();
                    Point testBounds = (Point)e.GetPosition(m_pageHolder).Subtract(m_origin);
                    if (testBounds.X > m_pageSize.Width && testBounds.Y > m_pageHalfHeight)
                    {
                        m_mouse = m_pointRight;
                    }
                    else
                    {
                        m_mouse = testBounds;
                    }
                };

                m_hotCorner.MouseLeftButtonDown += (sender, e) =>
                {
                    m_hotCorner.CaptureMouse();
                };

                m_hotCorner.MouseLeftButtonUp += (sender, e) =>
                {
                    m_listener.StartListening();
                    m_hotCorner.ReleaseMouseCapture();
                    m_mouse = (m_mouse.X < 0) ? m_pointLeft : m_pointRight;
                };

                m_hotCorner.MouseLeave += (sender, e) =>
                {
                    m_listener.StartListening();
                    m_mouse = m_pointRight;
                };
            }

            #endregion

            m_listener.Rendering += (sender, args) => updateUI();
            m_listener.IsListeningChanged += (sender, args) =>
            {
                var handler = IsAnimatingChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            };
        }

        public void StartListening()
        {
            m_listener.StartListening();
        }

        public Transform FlippingPageBackTransform { get { return m_pageTransformGroup; } }

        public Geometry PageHolderClip { get { return m_pageHolderClip; } }
        public Geometry NextPageClip { get { return m_nextPageClip; } }

        public Size PageSize { get { return m_pageSize; } }

        public void NextPage()
        {
            m_listener.StartListening();
            m_follow = m_pointRight;
            m_mouse = new Point(-m_pageSize.Width / 2, m_pageHalfHeight / 2);
            m_animating = true;
        }

        public void PreviousPage()
        {
            m_listener.StartListening();
            m_follow = m_pointLeft;
            m_mouse = new Point(m_pageSize.Width / 2, m_pageHalfHeight / 2);
            m_animating = true;
        }

        public bool IsAnimating { get { return m_listener.IsListening; } }

        public void GoTurbo()
        {
            m_turbo = true;
        }

        public event EventHandler IsAnimatingChanged;

        #region instance impl

        private void updateUI()
        {
            // THIS IS THE RAW FOLLOW
            if (m_follow.Subtract(m_mouse).Length > 1)
            {
                m_follow.X += (m_mouse.X - m_follow.X) * pageTurnSpeed;
                m_follow.Y += (m_mouse.Y - m_follow.Y) * pageTurnSpeed;
            }
            else
            {
                m_follow = m_mouse;
            }

            if (m_animating && m_mouse.Subtract(m_follow).Length < m_pageSize.Width)
            {
                m_animating = false;
                m_mouse = (m_mouse.X < 0) ? m_pointLeft : m_pointRight;
            }

            var corner = getCorner(m_follow);
            if (corner == m_corner && !m_animating)
            {
                m_turbo = false;
                m_listener.StopListening();
            }
            else
            {
                m_corner = corner;

                // CALCULATE THE BISECTOR AND CREATE THE CRITICAL TRIANGLE
                // DETERMINE THE MIDSECTION POINT
                var bisector = new Point();
                bisector.X = corner.X + .5 * (m_pageSize.Width - corner.X);
                bisector.Y = corner.Y + .5 * (m_pageHalfHeight - corner.Y);
                var bisectorAngle = Math.Atan2(m_pageHalfHeight - bisector.Y, m_pageSize.Width - bisector.X);
                var bisectorTanget = bisector.X - Math.Tan(bisectorAngle) * (m_pageHalfHeight - bisector.Y);
                if (bisectorTanget < 0)
                {
                    bisectorTanget = 0;
                }
                var tangentBottom = new Point(bisectorTanget, m_pageHalfHeight);

                // DETERMINE THE tangentToCorner FOR THE ANGLE OF THE PAGE
                var tangentToCornerAngle = Math.Atan2(tangentBottom.Y - corner.Y, tangentBottom.X - corner.X);

                m_pageTranslateTransform.X = m_origin.X + corner.X;
                m_pageTranslateTransform.Y = m_origin.Y + corner.Y;
                m_pageRotateTransform.Angle = tangentToCornerAngle * 180.0 / Math.PI;

                // DETERMINE THE ANGLE OF THE MAIN MASK RECTANGLE
                Double tanAngle = Math.Atan2(m_pageHalfHeight - bisector.Y, bisector.X - bisectorTanget);

                // VISUALIZE THE CLIPPING RECTANGLE
                var angle = 90 * (tanAngle / Math.Abs(tanAngle)) - tanAngle * 180 / Math.PI;
                angle = double.IsNaN(angle) ? 0 : angle;

                var pageHolderCliptransform = new TransformGroup();
                pageHolderCliptransform.Children.Add(new TranslateTransform() { Y = -m_pageSize.Height / 2 });
                pageHolderCliptransform.Children.Add(new RotateTransform() { Angle = angle, CenterX = m_pageSize.Width, CenterY = m_pageSize.Height });
                pageHolderCliptransform.Children.Add(new TranslateTransform() { X = tangentBottom.X });

                m_pageHolderClip.Transform = pageHolderCliptransform;

                var nextPageClipTransform = new TransformGroup();
                nextPageClipTransform.Children.Add(new TranslateTransform() { X = m_pageSize.Width });
                nextPageClipTransform.Children.Add(pageHolderCliptransform);
                m_nextPageClip.Transform = nextPageClipTransform;

                if (m_curlShadow != null)
                {
                    m_curlShadowTranslate.X = -m_curlShadow.Width + m_pageSize.Width - tangentBottom.X;
                    m_curlShadowRotate.Angle = -angle;

                    var opacity = 1.0;
                    var distance = m_pointLeft.Subtract(m_corner).Length / (m_pageSize.Width / 2);
                    if (distance < 1)
                    {
                        opacity = distance;
                    }
                    m_curlShadow.Opacity = opacity;
                }

                if (m_dropShadow != null)
                {
                    var minDistance = Math.Min(
                      m_corner.Subtract(m_pointLeft).Length,
                      m_corner.Subtract(m_pointRight).Length);

                    minDistance = Math.Min(40, minDistance);
                    minDistance /= 40;

                    m_dropShadow.Opacity = minDistance;
                }
            }
        }

        private Point getCorner(Point follow)
        {
            // CHECK DISTANCE FROM SPINE BOTTOM TO RAW FOLLOW
            var dx = follow.X;
            var dy = m_pageHalfHeight - follow.Y;
            // DETERMINE ANGLE FROM SPINE BOTTOM TO RAW FOLLOW
            var a2f = Math.Atan2(dy, dx);
            // PLOT THE FIXED RADIUS FOLLOW
            var radius = new Point();
            radius.X = Math.Cos(a2f) * m_pageSize.Width;
            radius.Y = m_pageHalfHeight - Math.Sin(a2f) * m_pageSize.Width;
            // DETERMINE THE SHORTER OF THE TWO DISTANCES
            var distanceToFollow = Math.Sqrt((m_pageHalfHeight - follow.Y) * (m_pageHalfHeight - follow.Y) + (follow.X * follow.X));
            double distToRadius = Math.Sqrt((m_pageHalfHeight - radius.Y) * (m_pageHalfHeight - radius.Y) + (radius.X * radius.X));
            // THE SMALLER OF THE TWO RADII DETERMINES THE CORNER
            var corner = new Point();
            if (distToRadius < distanceToFollow)
            {
                corner = radius;
            }
            else
            {
                corner = follow;
            }

            // NOW CHECK FOR THE OTHER CONSTRAINT, FROM THE SPINE TOP TO THE RADIUS OF THE PAGE DIAMETER...
            dx = -corner.X;
            dy = corner.Y + m_pageHalfHeight;
            distanceToFollow = Math.Sqrt(dx * dx + dy * dy);
            a2f = Math.Atan2(dy, dx);
            var radius2 = new Point();
            radius2.X = -Math.Cos(a2f) * m_pageDiagonal;
            radius2.Y = -m_pageHalfHeight + Math.Sin(a2f) * m_pageDiagonal;
            if (distanceToFollow > m_pageDiagonal)
            {
                corner = radius2;
            }

            return corner;
        }

        private double pageTurnSpeed { get { return m_turbo ? c_pageTurnSpeed * 3 : c_pageTurnSpeed; } }

        private Point m_mouse, m_follow, m_corner;
        private bool m_animating;
        private bool m_turbo;

        private readonly double m_pageHalfHeight, m_pageDiagonal;
        private readonly Point m_origin, m_pointLeft, m_pointRight;

        private readonly TransformGroup m_pageTransformGroup;
        private readonly TranslateTransform m_pageTranslateTransform;
        private readonly RotateTransform m_pageRotateTransform;

        private readonly TranslateTransform m_curlShadowTranslate;
        private readonly RotateTransform m_curlShadowRotate;

        //private readonly BlurEffect m_dropShadowBlurEffect;

        private readonly Geometry m_nextPageClip, m_pageHolderClip;

        private readonly Size m_pageSize;
        private readonly UIElement m_hotCorner, m_pageHolder;
        private readonly FrameworkElement m_curlShadow, m_dropShadow;

        private readonly CompositionTargetRenderingListener m_listener = new CompositionTargetRenderingListener();

        #endregion

        private const double c_pageTurnSpeed = .15;
    }
}
