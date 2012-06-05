using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PixelLab.Wpf.Transitions
{
    [System.Windows.Markup.ContentProperty("Content")]
    public class TransitionPresenter : FrameworkElement
    {
        static TransitionPresenter()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(TransitionPresenter), new FrameworkPropertyMetadata(null, CoerceClipToBounds));
        }

        // Force clip to be true if the active Transition requires it
        private static object CoerceClipToBounds(object element, object value)
        {
            TransitionPresenter transitionElement = (TransitionPresenter)element;
            bool clip = (bool)value;
            if (!clip && transitionElement.IsTransitioning)
            {
                Transition transition = transitionElement.Transition;
                if (transition.ClipToBounds)
                    return true;
            }
            return value;
        }

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content",
                typeof(object),
                typeof(TransitionPresenter),
                new UIPropertyMetadata(null, OnContentChanged, CoerceContent));

        // Don't update content until done transitioning
        private static object CoerceContent(object element, object value)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            if (te.IsTransitioning)
                return te.CurrentContentPresenter.Content;
            return value;
        }

        private static void OnContentChanged(object element, DependencyPropertyChangedEventArgs e)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            te.BeginTransition();
        }

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate",
                typeof(DataTemplate),
                typeof(TransitionPresenter),
                new UIPropertyMetadata(null, OnContentTemplateChanged));

        private static void OnContentTemplateChanged(object element, DependencyPropertyChangedEventArgs e)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            te.CurrentContentPresenter.ContentTemplate = (DataTemplate)e.NewValue;
        }

        public DataTemplateSelector ContentTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty); }
            set { SetValue(ContentTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty ContentTemplateSelectorProperty =
            DependencyProperty.Register("ContentTemplateSelector",
                typeof(DataTemplateSelector),
                typeof(TransitionPresenter),
                new UIPropertyMetadata(null, OnContentTemplateSelectorChanged));

        private static void OnContentTemplateSelectorChanged(object element, DependencyPropertyChangedEventArgs e)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            te.CurrentContentPresenter.ContentTemplateSelector = (DataTemplateSelector)e.NewValue;
        }

        public bool IsTransitioning
        {
            get { return (bool)GetValue(IsTransitioningProperty); }
            private set { SetValue(IsTransitioningPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsTransitioningPropertyKey =
            DependencyProperty.RegisterReadOnly("IsTransitioning",
                typeof(bool),
                typeof(TransitionPresenter),
                new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsTransitioningProperty =
            IsTransitioningPropertyKey.DependencyProperty;

        public Transition Transition
        {
            get { return (Transition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(Transition), typeof(TransitionPresenter), new UIPropertyMetadata(null, null, CoerceTransition));

        private static object CoerceTransition(object element, object value)
        {
            TransitionPresenter te = (TransitionPresenter)element;
            if (te.IsTransitioning) return te._activeTransition;
            return value;
        }

        public TransitionSelector TransitionSelector
        {
            get { return (TransitionSelector)GetValue(TransitionSelectorProperty); }
            set { SetValue(TransitionSelectorProperty, value); }
        }

        public static readonly DependencyProperty TransitionSelectorProperty =
            DependencyProperty.Register("TransitionSelector", typeof(TransitionSelector), typeof(TransitionPresenter), new UIPropertyMetadata(null));

        public TransitionPresenter()
        {
            _children = new UIElementCollection(this, null);
            ContentPresenter currentContent = new ContentPresenter();
            _currentHost = new AdornerDecorator();
            _currentHost.Child = currentContent;
            _children.Add(_currentHost);

            ContentPresenter previousContent = new ContentPresenter();
            _previousHost = new AdornerDecorator();
            _previousHost.Child = previousContent;
        }

        private void BeginTransition()
        {
            TransitionSelector selector = TransitionSelector;

            Transition transition = selector != null ?
                selector.SelectTransition(CurrentContentPresenter.Content, Content) :
                Transition;

            if (transition != null)
            {
                // Swap content presenters
                AdornerDecorator temp = _previousHost;
                _previousHost = _currentHost;
                _currentHost = temp;
            }

            ContentPresenter currentContent = CurrentContentPresenter;
            // Set the current content
            currentContent.Content = Content;
            currentContent.ContentTemplate = ContentTemplate;
            currentContent.ContentTemplateSelector = ContentTemplateSelector;

            if (transition != null)
            {
                ContentPresenter previousContent = PreviousContentPresenter;

                if (transition.IsNewContentTopmost)
                    Children.Add(_currentHost);
                else
                    Children.Insert(0, _currentHost);

                IsTransitioning = true;
                _activeTransition = transition;
                CoerceValue(TransitionProperty);
                CoerceValue(ClipToBoundsProperty);
                transition.BeginTransition(this, previousContent, currentContent);
            }
        }

        // Clean up after the transition is complete
        internal void OnTransitionCompleted()
        {
            _children.Clear();
            _children.Add(_currentHost);
            _currentHost.Visibility = Visibility.Visible;
            _previousHost.Visibility = Visibility.Visible;
            ((ContentPresenter)_previousHost.Child).Content = null;

            IsTransitioning = false;
            _activeTransition = null;
            CoerceValue(TransitionProperty);
            CoerceValue(ClipToBoundsProperty);
            CoerceValue(ContentProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _currentHost.Measure(availableSize);
            return _currentHost.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement uie in _children)
                uie.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException("index");
            return _children[index];
        }

        internal UIElementCollection Children
        {
            get { return _children; }
        }

        private ContentPresenter PreviousContentPresenter
        {
            get { return ((ContentPresenter)_previousHost.Child); }
        }

        private ContentPresenter CurrentContentPresenter
        {
            get { return ((ContentPresenter)_currentHost.Child); }
        }

        private UIElementCollection _children;

        private AdornerDecorator _currentHost;
        private AdornerDecorator _previousHost;

        private Transition _activeTransition;
    }
}
