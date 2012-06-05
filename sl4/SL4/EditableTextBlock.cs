using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.SL
{
    /// <summary>
    /// Represents a <see cref="TextBlock"/> control that can be edited by using a <see cref="TextBox"/>.
    /// </summary>
    [TemplateVisualState(GroupName = "EditStates", Name = EditableTextBlock.StateEditing), TemplateVisualState(GroupName = "EditStates", Name = EditableTextBlock.StateNotEditing)]
    [TemplatePart(Name = EditableTextBlock.PartEdit, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = EditableTextBlock.PartCommit, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = EditableTextBlock.PartCancel, Type = typeof(FrameworkElement))]
    public class EditableTextBlock : TextBox
    {
        private const string PartCommit = "PART_Commit";
        private const string PartCancel = "PART_Cancel";
        private const string PartEdit = "PART_Edit";
        private const string StateEditing = "Editing", StateNotEditing = "NotEditing";
        private const int DoubleClickTicks = 250;

        private string _lastValue;
        private long _lastDown = int.MinValue;
        private Control _contentElement;

        /// <summary>
        /// Identifies the InfoText dependency property.
        /// </summary>
        public static readonly DependencyProperty InfoTextProperty =
            DependencyPropHelper.Register<EditableTextBlock, string>(
                "InfoText",
                "Click to edit");

        /// <summary>
        /// Identifies the InfoTextVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty InfoTextVisibilityProperty =
            DependencyPropHelper.Register<EditableTextBlock, Visibility>(
                "InfoTextVisibility",
                Visibility.Collapsed);

        /// <summary>
        /// Identifies the ReadOnlyForeground dependency property.
        /// </summary>
        public static readonly DependencyProperty ReadOnlyForegroundProperty =
            DependencyPropHelper.Register<EditableTextBlock, Brush>(
                "ReadOnlyForeground",
                new SolidColorBrush(Colors.Black));

        public static readonly DependencyProperty IsEditingProperty =
            DependencyPropHelper.Register<EditableTextBlock, bool>("IsEditing", false, (e, n, o) => e.editingChanged());

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableTextBlock"/> class.
        /// </summary>
        public EditableTextBlock()
        {
            this.DefaultStyleKey = typeof(EditableTextBlock);
            TextChanged += (sender, args) => updateInfoTextBlockVisibility();
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the info text. This is a dependency property.
        /// The default value is "Click to edit".
        /// </summary>
        /// <value>The info text.</value>
        public string InfoText
        {
            get { return (string)GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        /// <summary>
        /// Gets the visibility of the info text.
        /// </summary>
        /// <value>The visibility of the info text.</value>
        public Visibility InfoTextVisibility
        {
            get { return (Visibility)GetValue(InfoTextVisibilityProperty); }
        }

        /// <summary>
        /// Gets or sets a brush that provides the foreground of the read-only text.
        /// </summary>
        /// <value>The brush that provides the foreground of the read-only text. 
        /// The default is black.</value>
        public Brush ReadOnlyForeground
        {
            get { return (Brush)GetValue(ReadOnlyForegroundProperty); }
            set { SetValue(ReadOnlyForegroundProperty, value); }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or 
        /// internal processes (such as a rebuilding layout pass) call 
        /// <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>. In simplest terms, 
        /// this means the method is called just before a UI element displays in an application. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var editContral = GetTemplateChild(PartEdit) as FrameworkElement;
            if (editContral != null)
            {
                editContral.MouseLeftButtonDown += (sender, args) => tryEdit();
            }

            var commit = GetTemplateChild(PartCommit) as FrameworkElement;
            if (commit != null)
            {
                commit.MouseLeftButtonDown += (sender, arsg) => Finish(true);
            }

            var cancel = GetTemplateChild(PartCancel) as FrameworkElement;
            if (cancel != null)
            {
                cancel.MouseLeftButtonDown += (sender, args) => Finish(false);
            }

            _contentElement = this.GetTemplateChild("ContentElement") as Control;
            updateInfoTextBlockVisibility();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            this.IsEditing = false;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            tryEdit();
        }

        /// <summary>
        /// Called before <see cref="E:System.Windows.UIElement.MouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event. The event data reports that the left mouse button was pressed.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!this.IsReadOnly && this.IsEnabled && !e.Handled && !this.IsEditing)
            {
                var tick = Environment.TickCount;
                long delta = tick - _lastDown;
                if (delta < DoubleClickTicks)
                {
                    _lastDown = int.MinValue;
                    tryEdit();
                }
                else
                {
                    _lastDown = tick;
                }
                e.Handled = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Called when <see cref="E:System.Windows.UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (IsEditing)
                {
                    if (Key.Enter == e.Key)
                    {
                        // If the user presses ENTER, find another control that can be focused
                        // and focus it to commit the text changes.
                        this.Finish(true);
                    }
                    else if (Key.Escape == e.Key)
                    {
                        this.Finish(false);
                    }
                }
                else
                {
                    if (e.Key == Key.Enter)
                    {
                        tryEdit();
                    }
                }
            }
            base.OnKeyDown(e);
        }

        private void tryEdit()
        {
            if (!IsReadOnly && IsEnabled)
            {
                IsEditing = true;
            }
        }

        private void editingChanged()
        {
            if (IsEditing)
            {
                VisualStateManager.GoToState(this, StateEditing, true);
                if (_contentElement != null)
                {
                    _contentElement.IsEnabled = true;
                }
                var focus = this.Focus();
                SelectAll();
                Debug.Assert(_lastValue == null);
                _lastValue = this.Text;
            }
            else
            {
                VisualStateManager.GoToState(this, StateNotEditing, true);
                if (_contentElement != null)
                {
                    _contentElement.IsEnabled = false;
                }
                _lastValue = null;
            }
        }

        private void Finish(bool commit)
        {
            Debug.Assert(IsEditing);
            if (!commit)
            {
                Text = _lastValue;
            }
            IsEditing = false;
        }

        private void updateInfoTextBlockVisibility()
        {
            Visibility value;
            if (IsReadOnly || (Text != null && Text.Length > 0))
            {
                value = System.Windows.Visibility.Collapsed;
            }
            else
            {
                value = System.Windows.Visibility.Visible;
            }
            SetValue(InfoTextVisibilityProperty, value);
        }
    }
}