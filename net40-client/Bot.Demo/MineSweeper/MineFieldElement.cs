using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.MineSweeper
{
    public class SquareTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            int number = (int)item;

            if (number > 0)
            {
                if (_nonZeroTemplate == null)
                {
                    ContentPresenter cp = container as ContentPresenter;
                    _nonZeroTemplate = (DataTemplate)cp.FindResource("NonZero");
                }
                return _nonZeroTemplate;
            }
            else
            {
                if (_zeroTemplate == null)
                {
                    ContentPresenter cp = container as ContentPresenter;
                    object test = cp.FindResource("Zero");
                    _zeroTemplate = (DataTemplate)test;
                }
                return _zeroTemplate;
            }
        }

        DataTemplate _nonZeroTemplate;
        DataTemplate _zeroTemplate;
    }

    public class MineFieldElement : FrameworkElement
    {
        public MineFieldElement() : this(new MineField()) { }
        public MineFieldElement(MineField mineField)
        {
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            _timer.Tick += timerTick;

            _newGameCommand = new DelegateCommand(NewGame);
            _contentControls = new ContentControl[mineField.Squares.Count];

            Initialize(mineField);
        }

        private static readonly DependencyPropertyKey SecondsElapsedPropertyKey = DependencyProperty.RegisterReadOnly("SecondsElapsed", typeof(int), typeof(MineFieldElement), null);

        public static readonly DependencyProperty SecondsElapsedProperty = SecondsElapsedPropertyKey.DependencyProperty;

        public int SecondsElapsed
        {
            get
            {
                return (int)GetValue(SecondsElapsedProperty);
            }
            private set
            {
                SetValue(SecondsElapsedPropertyKey, value);
            }
        }

        private static readonly DependencyPropertyKey MineFieldPropertyKey = DependencyProperty.RegisterReadOnly("MineField", typeof(MineField), typeof(MineFieldElement), null);

        public static readonly DependencyProperty MineFieldProperty = MineFieldPropertyKey.DependencyProperty;

        public MineField MineField
        {
            get
            {
                return (MineField)GetValue(MineFieldProperty);
            }
            private set
            {
                SetValue(MineFieldPropertyKey, value);
            }
        }

        public ICommand NewGameCommand
        {
            get
            {
                return _newGameCommand;
            }
        }

        internal void NewGame()
        {
            cleanUp();
            Initialize(new MineField(MineField.Rows, MineField.Columns, MineField.MineCount));
            CreateElements();
            if (_playState == PlayState.Started)
            {
                stopTimer();
            }
            else if (_playState == PlayState.Finished)
            {
                resetTimer();
            }
            this.InvalidateVisual();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            CreateElements();
        }

        private void CreateElements()
        {
            Debug.Assert(_contentControls.Length == MineField.Squares.Count);

            for (int i = 0; i < MineField.Squares.Count; i++)
            {
                var cc = new ContentControl()
                {
                    Content = MineField.Squares[i]
                };

                cc.MouseDown += cc_MouseDown;
                cc.MouseUp += cc_MouseUp;

                _contentControls[i] = cc;

                AddVisualChild(cc);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MineField.Columns * _childSize, MineField.Rows * _childSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size measureSize = new Size(_childSize, _childSize);
            Square square;

            foreach (ContentControl element in _contentControls)
            {
                square = element.Content as Square;
                if (square != null)
                {
                    Rect rect = new Rect(square.Column * _childSize, square.Row * _childSize, _childSize, _childSize);
                    element.Arrange(rect);
                }
            }

            return new Size(MineField.Columns * _childSize, MineField.Rows * _childSize);
        }

        protected override Visual GetVisualChild(int index)
        {
            return _contentControls[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                if (_contentControls != null)
                {
                    return _contentControls.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        #region private instance implementation

        private void Initialize(MineField mineField)
        {
            Util.ThrowUnless<ArgumentNullException>(mineField != null);

            if (MineField != null)
            {
                MineField.PropertyChanged -= MineField_PropertyChanged;
            }

            MineField = mineField;

            MineField.PropertyChanged += MineField_PropertyChanged;
        }

        private void MineField_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                stopTimer();
            }
        }

        private void cleanUp()
        {
            foreach (var control in _contentControls)
            {
                RemoveVisualChild(control);
            }
        }

        private void cc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cc = (ContentControl)sender;
            _clickCount = 0;

            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    _middleDownControl = cc;
                    break;
                case MouseButton.Left:
                    _leftDownControl = cc;
                    _clickCount = e.ClickCount;
                    break;
                case MouseButton.Right:
                    var square = (Square)cc.Content;

                    checkTimer();
                    MineField.ToggleSquare(square.Column, square.Row);
                    break;
            }
        }

        private void cc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var cc = (ContentControl)sender;
            var square = (Square)cc.Content;

            if (MineField.State == WinState.Unknown)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Middle:

                        if (cc == _middleDownControl)
                        {
                            checkTimer();
                            MineField.ClearSquare(square.Column, square.Row);
                        }
                        _middleDownControl = null;
                        break;
                    case MouseButton.Left:

                        if (cc == _leftDownControl)
                        {
                            checkTimer();
                            MineField.RevealSquare(square.Column, square.Row, _clickCount > 1);
                        }
                        _leftDownControl = null;
                        break;
                }
            }
        }

        private void checkTimer()
        {
            if (_playState == PlayState.NotStarted)
            {
                startTimer();
            }
        }

        private void startTimer()
        {
            Debug.Assert(SecondsElapsed == 0);
            Debug.Assert(_playState == PlayState.NotStarted);
            Debug.Assert(!_timer.IsEnabled);

            _timer.Start();

            _playState = PlayState.Started;
        }

        private void stopTimer()
        {
            Debug.Assert(_timer.IsEnabled);
            Debug.Assert(_playState == PlayState.Started);

            _timer.Stop();

            _playState = PlayState.Finished;
        }

        private void resetTimer()
        {
            Debug.Assert(_playState == PlayState.Finished);
            Debug.Assert(!_timer.IsEnabled);

            SecondsElapsed = 0;
            _playState = PlayState.NotStarted;
        }

        private void timerTick(object sender, EventArgs args)
        {
            Debug.Assert(_playState == PlayState.Started);

            SecondsElapsed++;
        }

        private const double _childSize = 20;

        private readonly DispatcherTimer _timer;
        private readonly DelegateCommand _newGameCommand;
        private readonly ContentControl[] _contentControls;

        private ContentControl _leftDownControl;
        private ContentControl _middleDownControl;
        private int _clickCount;
        private PlayState _playState = PlayState.NotStarted;

        #endregion
    }

    public enum PlayState { NotStarted, Started, Finished }
}
