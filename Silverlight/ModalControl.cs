using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PixelLab.Common;
using System.Windows.Data;
using System.Windows.Shapes;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.SL
{
    [TemplatePart(Name = ContentElementName, Type = typeof(Grid))]
    public class ModalControl : Control
    {
        private const string ContentElementName = "ContentElement";

        private readonly List<ContentWrapper> _content = new List<ContentWrapper>();
        private readonly RectangleGeometry _clip;

        private bool? _previousTargetEnabledState;
        private Grid _contentElement;
        private Control _targetElement;
        private bool _targetPointUpdateEnqued;

        public ModalControl()
        {
            DefaultStyleKey = typeof(ModalControl);

            Clip = _clip = new RectangleGeometry();

            KeyDown += (sender, e) =>
            {
                if (!e.Handled && e.Key == System.Windows.Input.Key.Escape && _content.Any())
                {
                    e.Handled = true;
                    _content.Last().OnEscPressed();
                }
            };

            LayoutUpdated += (sender, ars) =>
            {
                enqueTargetPointUpdate();
                if (Clip != _clip)
                {
                    Clip = _clip;
                }
                _clip.Rect = new Rect(new Point(), RenderSize);
            };
        }

        public bool IsOpen
        {
            get
            {
                return !_content.IsEmpty();
            }
        }

        public bool IsTargetSet
        {
            get
            {
                this.VerifyAccess();
                return _targetElement != null;
            }
        }

        public IModalToken Open(FrameworkElement content, ModalPosition position, Point? location)
        {
            Contract.Requires(content != null);

            var wrapper = new ContentWrapper(this, content, position, location);
            if (_content.Any())
            {
                Debug.Assert(_content.Count(cw => cw.IsEnabled) == 1);
                Debug.Assert(_content.Last().IsEnabled);
                _content.Last().IsEnabled = false;
            }
            _content.Add(wrapper);
            updateContent();

            updateState();
            return wrapper;
        }

        public void Close(IModalToken openToken)
        {
            Contract.Requires(IsOpen);
            Contract.Requires(openToken != null);
            Debug.Assert(!_content.IsEmpty());
            Util.ThrowUnless(openToken == _content.Last());

            Debug.Assert(_content.Count > 0);
            var last = _content.Last();
            last.Close();
            _content.RemoveLast();
            if (_content.IsEmpty())
            {
                updateState();
            }
            else
            {
                Debug.Assert(_content.All(cw => !cw.IsEnabled));
                _content.Last().IsEnabled = true;
            }
        }

        public void SetTarget(Control target)
        {
            Contract.Requires(target != null);
            Contract.Requires(!IsTargetSet);
            Contract.Requires(!IsOpen);
            this.VerifyAccess();
            _targetElement = target;
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        protected virtual void OnOpened(EventArgs e = null)
        {
            var handler = Opened;
            if (handler != null)
            {
                handler(this, e ?? EventArgs.Empty);
            }
        }

        protected virtual void OnClosed(EventArgs e = null)
        {
            var handler = Closed;
            if (handler != null)
            {
                handler(this, e ?? EventArgs.Empty);
            }
        }

        public override void OnApplyTemplate()
        {
            _contentElement = GetTemplateChild(ContentElementName) as Grid;
            updateContent();
        }

        private void updateState(bool animate = true)
        {
            this.VerifyAccess();
            if (IsTargetSet)
            {
                if (IsOpen)
                {
                    if (!_previousTargetEnabledState.HasValue)
                    {
                        _previousTargetEnabledState = _targetElement.IsEnabled;
                        _targetElement.IsEnabled = false;
                    }
                }
                else
                {
                    Debug.Assert(_previousTargetEnabledState.HasValue);
                    Debug.Assert(!_targetElement.IsEnabled);
                    _targetElement.IsEnabled = _previousTargetEnabledState.Value;
                    _previousTargetEnabledState = null;
                }
            }

            if (IsOpen)
            {
                OnOpened();
                Dispatcher.BeginInvoke(delayedFocusContent);
            }
            else
            {
                OnClosed();
            }
        }

        private void maskMouseLeftButtonDown(ContentWrapper wrapper)
        {
            Debug.Assert(wrapper == _contentElement.Children.Last());

            var lastContent = _content.LastOrDefault();
            if (lastContent != null)
            {
                lastContent.OnMaskClicked();
            }
        }

        private void enqueTargetPointUpdate()
        {
            this.VerifyAccess();
            if (!_targetPointUpdateEnqued)
            {
                _targetPointUpdateEnqued = true;
                Dispatcher.BeginInvoke(targetPointUpdate);
            }
        }

        private void targetPointUpdate()
        {
            this.VerifyAccess();
            _targetPointUpdateEnqued = false;
            if (_contentElement != null && this.RenderSize.Width > 0)
            {
                var location = this.GetLocationFromRootVisual();
                foreach (var item in _content)
                {
                    item.UpdateLocation(location);
                }
            }
        }

        private void delayedFocusContent()
        {
            if (IsOpen && _contentElement != null)
            {
                var content = _contentElement.Children.LastOrDefault() as Control;
                if (content != null)
                {
                    content.Focus();
                }
            }
        }

        private void updateContent()
        {
            if (_contentElement != null)
            {
                foreach (var child in _content)
                {
                    if (!_contentElement.Children.Contains(child))
                    {
                        Debug.Assert(child.State == ContentState.Unloaded);
                        _contentElement.Children.Add(child);
                    }
                }
            }
        }

        private void RemoveContent(ContentWrapper contentWrapper)
        {
            Contract.Requires(contentWrapper != null);
            Util.ThrowUnless(!_content.Contains(contentWrapper), "The content wrapper should already be removed");
            Util.ThrowUnless(_contentElement != null && _contentElement.Children.Contains(contentWrapper));
            contentWrapper.ClearChild();
            _contentElement.Children.Remove(contentWrapper);
        }

        private sealed class ContentWrapper : UserControl, IModalToken
        {
            private static readonly DependencyProperty ContentWrapperProperty = DependencyPropHelper.RegisterAttached<ContentWrapper, FrameworkElement, ContentWrapper>("ContentWrapper");

            private readonly Grid _container;
            private readonly Rectangle _mask;
            private readonly FrameworkElement _element;
            private readonly CompositeTransform _transform;
            private readonly Point? _location;
            private readonly ModalControl _parent;

            private ContentState _state;
            private Storyboard _storyboard;

            public ContentWrapper(ModalControl parent, FrameworkElement element, ModalPosition position, Point? location)
            {
                Contract.Requires(parent != null);
                Contract.Requires(element != null);
                Contract.Requires(element.Parent == null);
                element.VerifyAccess();
                Util.ThrowUnless(element.GetValue(ContentWrapperProperty) == null);

                _mask = new Rectangle { Opacity = 0 };
                _mask.MouseLeftButtonDown += (sender, args) => parent.maskMouseLeftButtonDown(this);

                _element = element;

                Content = _container = new Grid();
                _container.Children.Add(_mask);
                _container.Children.Add(_element);

                _parent = parent;

                switch (position)
                {
                    case ModalPosition.Center:
                        element.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        element.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        element.RenderTransformOrigin = new Point(0.5, 0.5);
                        break;
                    case ModalPosition.TopLeft:
                        element.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        element.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        element.RenderTransformOrigin = new Point(0, 0);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                _location = location;
                State = ContentState.Unloaded;

                _element.RenderTransform = _transform = new CompositeTransform { ScaleX = 0, ScaleY = 0 };
                _element.SetValue(ContentWrapperProperty, this);
                _element.Loaded += _element_Loaded;

                _mask.SetBinding(Rectangle.FillProperty, new Binding("Background") { Source = parent });

                IsEnabled = false;
            }

            public ContentState State { get { return _state; } private set { _state = value; } }

            public void Close()
            {
                switch (State)
                {
                    case ContentState.Open:
                        Debug.Assert(_storyboard == null);
                        startCloseAnimation();
                        break;
                    case ContentState.Opening:
                        Debug.Assert(_storyboard != null);
                        _storyboard.Stop();
                        _storyboard = null;
                        startCloseAnimation();
                        break;
                    case ContentState.Unloaded:
                        // noop
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                Debug.Assert(!IsEnabled);
            }

            public void UpdateLocation(Point parentLocation)
            {
                if (_location.HasValue)
                {
                    var newTraslate = _location.Value.Subtract(parentLocation);
                    _transform.TranslateX = newTraslate.X;
                    _transform.TranslateY = newTraslate.Y;
                }
            }

            public event EventHandler MaskClicked;

            public void OnMaskClicked()
            {
                var handler = MaskClicked;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public event EventHandler EscPressed;

            public void OnEscPressed()
            {
                var handler = EscPressed;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public void ClearChild()
            {
                Debug.Assert(Content == _container);
                Debug.Assert(_container.Children.Count == 2);
                Debug.Assert(_container.Children[0] == _mask);
                Debug.Assert(_container.Children[1] == _element);
                Debug.Assert(_element.GetValue(ContentWrapperProperty) == this);
                _container.Children.Clear();
                Content = null;
                _element.ClearValue(ContentWrapperProperty);
            }

            private void startCloseAnimation()
            {
                Debug.Assert(_storyboard == null);
                State = ContentState.Closing;

                var ct = (CompositeTransform)_element.RenderTransform;

                _storyboard = doAnimation(_element, _mask, false);
                _storyboard.Completed += _storyboard_CloseCompleted;
                _storyboard.Begin();

                IsEnabled = false;
            }

            private void _storyboard_CloseCompleted(object sender, EventArgs e)
            {
                Debug.Assert(sender == _storyboard);
                Debug.Assert(State == ContentState.Closing);
                _storyboard = null;
                State = ContentState.Closed;
                _parent.RemoveContent(this);
            }

            private void _element_Loaded(object sender, RoutedEventArgs e)
            {
                Debug.Assert(sender == _element);
                Debug.Assert(State == ContentState.Unloaded);
                Debug.Assert(_storyboard == null);
                Debug.Assert(!IsEnabled);
                _element.Unloaded += element_Unloaded;
                State = ContentState.Opening;

                var ct = (CompositeTransform)_element.RenderTransform;
                Debug.Assert(ct.ScaleX == 0);
                Debug.Assert(ct.ScaleY == 0);

                _storyboard = doAnimation(_element, _mask, true);
                _storyboard.Completed += _storyboard_OpenCompleted;
                _storyboard.Begin();

                IsEnabled = true;
            }

            private void _storyboard_OpenCompleted(object sender, EventArgs e)
            {
                Debug.Assert(State == ContentState.Opening);
                _storyboard = null;
                State = ContentState.Open;
            }

            private void element_Unloaded(object sender, RoutedEventArgs e)
            {
                Debug.Assert(sender == _element);
                Debug.Assert(State != ContentState.Unloaded);
                if (State == ContentState.Opening)
                {
                    _storyboard.Completed -= _storyboard_OpenCompleted;
                }
                else if (State == ContentState.Closing)
                {
                    _storyboard.Completed -= _storyboard_CloseCompleted;
                }
                State = ContentState.Unloaded;
                _element.Unloaded -= element_Unloaded;
            }

            private static Storyboard doAnimation(FrameworkElement element, Rectangle mask, bool animateIn)
            {
                var scaleStoryboard = AnimateScale((CompositeTransform)element.RenderTransform, animateIn);

                var opacityAnimation = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromSeconds(0.3) };
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { Value = animateIn ? 0 : 0.5, KeyTime = TimeSpan.FromSeconds(0) });
                opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { Value = animateIn ? 0.5 : 0, KeyTime = TimeSpan.FromSeconds(0.3) });
                Storyboard.SetTarget(opacityAnimation, mask);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

                var parent = new Storyboard();
                parent.Children.Add(scaleStoryboard);
                parent.Children.Add(opacityAnimation);

                return parent;
            }

            public static Storyboard AnimateScale(CompositeTransform ct, bool animateIn)
            {
                Contract.Requires(ct != null);

                var storyBoard = new Storyboard();

                var animation = GetScaleAnimation(animateIn);
                Storyboard.SetTargetProperty(animation, new PropertyPath("ScaleX"));
                storyBoard.Children.Add(animation);

                animation = GetScaleAnimation(animateIn);
                Storyboard.SetTargetProperty(animation, new PropertyPath("ScaleY"));
                storyBoard.Children.Add(animation);

                Storyboard.SetTarget(storyBoard, ct);

                return storyBoard;
            }

            internal static readonly IEasingFunction EaseBackIn = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut };

            internal static readonly IEasingFunction EaseBackOut = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseIn };

            private static Timeline GetScaleAnimation(bool animateIn)
            {
                return new DoubleAnimation
                {
                    Duration = TimeSpan.FromSeconds(0.25),
                    To = animateIn ? 1 : 0,
                    EasingFunction = animateIn ? EaseBackIn : EaseBackOut
                };
            }
        }

        private enum ContentState { Unloaded, Opening, Open, Closing, Closed }
    }

    public interface IModalToken
    {
        event EventHandler MaskClicked;
        event EventHandler EscPressed;
    }

    public interface IHaveModal
    {
        ModalControl ModalControl { get; }
    }

    public enum ModalPosition
    {
        Center, TopLeft
    }
}
