using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Linq;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public class ReorderListBox : ListBox
    {
        #region Mouse Interaction

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (m_isDragging && m_dragAdorner != null)
            {
                // update the position of the adorner

                var current = e.GetPosition(this);
                m_dragAdorner.OffsetX = current.X - m_mouseDown.X;
                m_dragAdorner.OffsetY = current.Y - m_mouseDown.Y;

                // find the item that we are dragging over
                var element = this.InputHitTest(new Point(e.GetPosition(this).X, e.GetPosition(this).Y)) as UIElement;

                if (element != null)
                {
                    var itemOver = TreeHelpers.GetItemContainerFromChildElement(this, element) as FrameworkElement;

                    if (itemOver != null)
                    {
                        var p = Mouse.GetPosition(itemOver);
                        var q = PointToQuadrant(itemOver, p);

                        if (itemOver != m_lastMouseOverItem || q != m_lastMouseOverQuadrant)
                        {
                            if (q == ReorderQuadrant.BottomLeft || q == ReorderQuadrant.BottomRight)
                            {
                                m_lastMoveOverPlacement = ReorderPlacement.After;
                            }
                            else
                            {
                                m_lastMoveOverPlacement = ReorderPlacement.Before;
                            }
                            PreviewInsert(itemOver, m_lastMoveOverPlacement);
                            m_lastMouseOverItem = itemOver;
                            m_lastMouseOverQuadrant = q;
                        }
                    }
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (m_isDragging)
            {
                this.ReleaseMouseCapture();
                this.m_adornerLayer.Remove(m_dragAdorner);
                m_isDragging = false;

                // raise an event to update the underlying datasource
                if (m_lastMouseOverItem != null && m_dragItemIndex != m_dragInsertIndex)
                {
                    var insertItem = ItemContainerGenerator.ContainerFromIndex(m_dragInsertIndex);
                    OnReorderRequested(new ReorderEventArgs(m_dragItem, insertItem));
                }
                else
                {
                    RaiseReorderCancelEvent();
                }

                m_dragItem.ClearValue(IsDraggingPropertyKey);
                m_dragItem.Visibility = Visibility.Visible;

                // If items are manually added, just re-order them.
                if (this.ItemsSource == null)
                {
                    this.Items.Remove(m_dragItem);
                    this.Items.Insert(m_dragInsertIndex, m_dragItem);
                }

                // reset the transform on all of the items
                for (var i = 0; i < Items.Count; i++)
                {
                    var item = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i);
                    if (item != null)
                    {
                        TranslateItem(item, 0, 0, Orientation);
                    }
                }

                e.Handled = true;
            }

            // clean-up dragging variables
            m_dragInsertIndex = m_dragItemIndex = int.MinValue;
            m_dragItem = m_lastMouseOverItem = null;

            base.OnMouseUp(e);
        }

        #endregion

        #region Duration

        /// <summary>
        /// Duration Dependency Property
        /// </summary>
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(int), typeof(ReorderListBox),
                new FrameworkPropertyMetadata((int)250));

        /// <summary>
        /// Gets or sets the Duration property.  This dependency property
        /// indicates ....
        /// </summary>
        public int Duration
        {
            get { return (int)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        #endregion

        #region Orientation

        public static readonly DependencyProperty OrientationProperty = DependencyPropHelper.Register<ReorderListBox, Orientation>("Orientation", Orientation.Vertical);

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        #region Private_DragPreviewStoryboard (Attached Readonly DependencyProperty)

        /// <summary>
        /// Private_DragPreviewStoryboard Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyProperty s_dragPreviewStoryboardProperty
            = DependencyProperty.RegisterAttached("Private_DragPreviewStoryboard", typeof(Storyboard), typeof(ReorderListBox),
                new FrameworkPropertyMetadata((Storyboard)null));

        #endregion

        #region IsDragElement (Attached DependencyProperty)

        /// <summary>
        /// IsDragElement Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsDragElementProperty =
            DependencyProperty.RegisterAttached("IsDragElement", typeof(bool), typeof(ReorderListBox),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsDragElementChanged)));

        /// <summary>
        /// Gets the IsDragElement property.  This dependency property
        /// indicates ....
        /// </summary>
        public static bool GetIsDragElement(DependencyObject d)
        {
            return (bool)d.GetValue(IsDragElementProperty);
        }

        /// <summary>
        /// Sets the IsDragElement property.  This dependency property
        /// indicates ....
        /// </summary>
        public static void SetIsDragElement(DependencyObject d, bool value)
        {
            d.SetValue(IsDragElementProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsDragElement property.
        /// </summary>
        private static void OnIsDragElementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (UIElement)obj;
            if ((bool)args.NewValue)
            {
                element.MouseLeftButtonDown += element_MouseLeftButtonDown;
            }
            else
            {
                element.MouseLeftButtonDown -= element_MouseLeftButtonDown;
            }
        }

        private static void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            // find the ReoderListBox parent of the element
            var reorderListBox = TreeHelpers.FindParent<ReorderListBox>(element);

            if (reorderListBox != null)
            {
                // find the ItemContainer
                FrameworkElement f = TreeHelpers.GetItemContainerFromChildElement(reorderListBox, element) as FrameworkElement;
                if (f != null)
                {
                    reorderListBox.BeginDrag(f);
                }
            }
        }

        #endregion

        #region IsDragging (Attached, Inherited Readonly DependencyProperty)

        /// <summary>
        /// IsDragging Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsDraggingPropertyKey
            = DependencyProperty.RegisterAttachedReadOnly("IsDragging", typeof(bool), typeof(ReorderListBox),
                new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty IsDraggingProperty
            = IsDraggingPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsDragging property.  This dependency property
        /// indicates ....
        /// </summary>
        public static bool GetIsDragging(DependencyObject d)
        {
            return (bool)d.GetValue(IsDraggingProperty);
        }

        /// <summary>
        /// Provides a secure method for setting the IsDragging property.
        /// This dependency property indicates ....
        /// </summary>
        private static void SetIsDragging(DependencyObject d, bool value)
        {
            d.SetValue(IsDraggingPropertyKey, value);
        }

        #endregion

        #region ReorderRequested Event

        public static readonly RoutedEvent ReorderRequestedEvent = EventManager.RegisterRoutedEvent("ReorderRequested", RoutingStrategy.Bubble, typeof(EventHandler<ReorderEventArgs>), typeof(ReorderListBox));

        public event EventHandler<ReorderEventArgs> ReorderRequested
        {
            add { AddHandler(ReorderRequestedEvent, value); }
            remove { RemoveHandler(ReorderRequestedEvent, value); }
        }

        protected virtual void OnReorderRequested(ReorderEventArgs e)
        {
            RaiseEvent(e);
        }

        #endregion

        #region ReorderBeginEvent Event

        public static readonly RoutedEvent ReorderBeginEvent = EventManager.RegisterRoutedEvent("ReorderBegin", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ReorderListBox));

        public event RoutedEventHandler ReorderBegin
        {
            add { AddHandler(ReorderBeginEvent, value); }
            remove { RemoveHandler(ReorderBeginEvent, value); }
        }

        void RaiseReorderBeginEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ReorderListBox.ReorderBeginEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        #region ReorderCancelEvent Event

        public static readonly RoutedEvent ReorderCancelEvent = EventManager.RegisterRoutedEvent("ReorderCancel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ReorderListBox));

        public event RoutedEventHandler ReorderCancel
        {
            add { AddHandler(ReorderCancelEvent, value); }
            remove { RemoveHandler(ReorderCancelEvent, value); }
        }

        void RaiseReorderCancelEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ReorderListBox.ReorderCancelEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        #region Private Helper Methods

        private void BeginDrag(FrameworkElement dragContainer)
        {
            m_dragItem = dragContainer;
            if (m_dragItem == null) return;

            // get the index of the item (and make sure that it is a valid child)
            m_dragItemIndex = this.ItemContainerGenerator.IndexFromContainer(m_dragItem);
            if (m_dragItemIndex == -1) return;

            // create an adorner
            m_dragAdorner = new DragPreviewAdorner(m_dragItem, m_dragItem);
            m_dragAdorner.IsHitTestVisible = false;
            this.m_adornerLayer.Add(m_dragAdorner);

            // tell the item it's dragging and hide it
            SetIsDragging(m_dragItem, true);
            m_dragItem.Visibility = Visibility.Hidden;

            // get the current location of the mouse
            m_mouseDown = Mouse.GetPosition(this);

            // set mouse capture (so that we are dragging)
            Mouse.Capture(this);
            m_isDragging = true;

            // raise an event to signal that we've started ragging
            RaiseReorderBeginEvent();
        }

        private static ReorderQuadrant PointToQuadrant(FrameworkElement element, Point p)
        {
            if (p.Y >= (element.ActualHeight / 2))
            {
                // top half
                if (p.X >= (element.ActualWidth / 2))
                {
                    return ReorderQuadrant.BottomRight;
                }
                else
                {
                    return ReorderQuadrant.BottomLeft;
                }
            }
            else
            {
                // bottom half
                if (p.X >= (element.ActualWidth / 2))
                {
                    return ReorderQuadrant.TopRight;
                }
                else
                {
                    return ReorderQuadrant.TopLeft;
                }
            }
        }

        private void PreviewInsert(FrameworkElement relativeTo, ReorderPlacement placement)
        {
            if (m_isDragging && m_dragItem != null && relativeTo != null)
            {
                // get the index of the item being dragged
                var relativeToIndex = ItemContainerGenerator.IndexFromContainer(relativeTo);

                // get the index of insertion
                var offset = (placement == ReorderPlacement.Before) ? 0 : 1;
                m_dragInsertIndex = relativeToIndex + offset;

                for (int i = 0; i < Items.Count; i++)
                {
                    double delta;
                    if (i > m_dragItemIndex && i < m_dragInsertIndex)
                    {
                        delta = -1 * getOrientedDimension(m_dragItem);
                    }
                    else if (i < m_dragItemIndex && i >= m_dragInsertIndex)
                    {
                        delta = getOrientedDimension(m_dragItem);
                    }
                    else
                    {
                        delta = 0;
                    }

                    TranslateItem((FrameworkElement)ItemContainerGenerator.ContainerFromIndex(i), delta, this.Duration, Orientation);
                }

                // if the insert location is after the current location, we need to decrement it
                // by one after we've made the visual adjustments so that the actual drop index
                // will be accurate
                if (m_dragInsertIndex > m_dragItemIndex)
                {
                    m_dragInsertIndex--;
                }
            }
        }

        private double getOrientedDimension(FrameworkElement element)
        {
            switch (Orientation)
            {
                case System.Windows.Controls.Orientation.Vertical:
                    return element.ActualHeight;
                case System.Windows.Controls.Orientation.Horizontal:
                    return element.ActualWidth;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void TranslateItem(FrameworkElement element, double delta, int milliseconds, Orientation orientation)
        {
            var storyboard = (Storyboard)element.GetValue(s_dragPreviewStoryboardProperty);
            SplineDoubleKeyFrame keyframe;

            if (storyboard == null)
            {
                var t = new TranslateTransform();
                element.RenderTransform = t;

                keyframe = new SplineDoubleKeyFrame
                {
                    KeySpline = new KeySpline(0, 0.7, 0.7, 1)
                };
                var animation = new DoubleAnimationUsingKeyFrames();
                animation.KeyFrames.Add(keyframe);

                Storyboard.SetTarget(animation, element);
                PropertyPath propertyPath;
                if (orientation == Orientation.Vertical)
                {
                    propertyPath = new PropertyPath("(RenderTransform).(TranslateTransform.Y)");
                }
                else
                {
                    propertyPath = new PropertyPath("(RenderTransform).(TranslateTransform.X)");
                }
                Storyboard.SetTargetProperty(animation, propertyPath);

                storyboard = new Storyboard();
                storyboard.Children.Add(animation);

                element.SetValue(s_dragPreviewStoryboardProperty, storyboard);
            }
            else
            {
                keyframe = storyboard.Children.Cast<DoubleAnimationUsingKeyFrames>().Single().KeyFrames.Cast<SplineDoubleKeyFrame>().Single();
            }

            keyframe.Value = delta;
            keyframe.KeyTime = TimeSpan.FromMilliseconds(milliseconds);
            element.BeginStoryboard(storyboard);
        }

        private AdornerLayer m_adornerLayer
        {
            get
            {
                if (m_adornerLayerCache == null)
                {
                    m_adornerLayerCache = AdornerLayer.GetAdornerLayer(this);
                }
                return m_adornerLayerCache;
            }
        }

        #endregion

        #region Fields

        private bool m_isDragging;
        private DragPreviewAdorner m_dragAdorner;
        private FrameworkElement m_dragItem;
        private int m_dragItemIndex;
        private Point m_mouseDown;
        private int m_dragInsertIndex;
        private FrameworkElement m_lastMouseOverItem;
        private ReorderQuadrant m_lastMouseOverQuadrant;
        private ReorderPlacement m_lastMoveOverPlacement;
        private AdornerLayer m_adornerLayerCache;

        #endregion
    }

    public class ReorderEventArgs : RoutedEventArgs
    {
        public ReorderEventArgs(DependencyObject itemContainer, DependencyObject toContainer)
            : base(ReorderListBox.ReorderRequestedEvent)
        {
            Contract.Requires<ArgumentNullException>(itemContainer != null);
            Contract.Requires<ArgumentNullException>(toContainer != null);

            ItemContainer = itemContainer;
            ToContainer = toContainer;
        }

        public DependencyObject ItemContainer { get; private set; }
        public DependencyObject ToContainer { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} - From {1} to {2}", base.ToString(), ItemContainer, ToContainer);
        }
    }

    public enum ReorderQuadrant { TopLeft, TopRight, BottomLeft, BottomRight }

    public enum ReorderPlacement { Before, After }
}
