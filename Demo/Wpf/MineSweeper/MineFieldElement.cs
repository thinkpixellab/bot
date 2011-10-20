using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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

            Initialize(mineField);
        }

        public int SecondsElapsed
        {
            get
            {
                Debug.Assert(_secondsElapsed >= 0);
                if (_secondsElapsed > 0)
                {
                    Debug.Assert(_playState == PlayState.Started || _playState == PlayState.Finished);
                }
                return _secondsElapsed;
            }
            private set
            {
                _secondsElapsed = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SecondsElapsed"));
            }
        }

        public PlayState PlayState
        {
            get
            {
                if (_playState == PlayState.NotStarted || _playState == PlayState.Started)
                {
                    Debug.Assert(_mineField.State == WinState.Unknown);
                }
                else
                {
                    Debug.Assert(_mineField.State != WinState.Unknown);
                }

                return _playState;
            }
        }
        public int MinesLeft { get { return _mineField.MinesLeft; } }
        public int MinesToFind
        {
            get
            {
                return (_mineField.Rows * _mineField.Columns) - (_mineField.ClearedCount + _mineField.MineCount);
            }
        }

        public WinState State { get { return _mineField.State; } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void NewGame()
        {
            cleanUp();
            Initialize(new MineField(_mineField.Rows, _mineField.Columns, _mineField.MineCount));
            CreateElements();
            if (_playState == PlayState.Started)
            {
                stopTimer();
            }
            if (_playState == PlayState.Finished)
            {
                resetTimer();
            }
            this.InvalidateVisual();
            OnPropertyChanged(new PropertyChangedEventArgs(MineField.StatePropertyName));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            CreateElements();
        }
        private void CreateElements()
        {
            //create children
            _contentControls = new ContentControl[_mineField.Squares.Count];
            ContentControl cc;

            for (int i = 0; i < _mineField.Squares.Count; i++)
            {
                cc = new ContentControl();

                cc.Content = _mineField.Squares[i];
                cc.MouseDown += new MouseButtonEventHandler(cc_MouseDown);
                cc.MouseUp += new MouseButtonEventHandler(cc_MouseUp);

                _contentControls[i] = cc;

                AddVisualChild(cc);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(_mineField.Columns * _childSize, _mineField.Rows * _childSize);
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

            return new Size(_mineField.Columns * _childSize, _mineField.Rows * _childSize);
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

            _mineField = mineField;
            _mineField.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == MineField.MinesLeftPropertyName)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(MineField.MinesLeftPropertyName));
                }
                else if (e.PropertyName == MineField.StatePropertyName)
                {
                    stopTimer();
                    OnPropertyChanged(new PropertyChangedEventArgs(MineField.StatePropertyName));
                }
            };
        }
        private void cleanUp()
        {
            foreach (ContentControl control in _contentControls)
            {
                RemoveVisualChild(control);
            }
            _contentControls = null;
        }

        private void cc_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                ContentControl control = (ContentControl)sender;
                _middleDownControl = control;
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                ContentControl control = (ContentControl)sender;
                _leftDownControl = control;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                ContentControl control = (ContentControl)sender;
                Square square = (Square)control.Content;

                checkTimer();
                _mineField.ToggleSquare(square.Column, square.Row);
            }
        }
        private void cc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_mineField.State == WinState.Unknown)
            {
                if (e.ChangedButton == MouseButton.Middle)
                {
                    ContentControl control = (ContentControl)sender;

                    if (control == _middleDownControl)
                    {
                        Square square = (Square)control.Content;
                        checkTimer();
                        _mineField.ClearSquare(square.Column, square.Row);
                    }
                    _middleDownControl = null;
                }
                else if (e.ChangedButton == MouseButton.Left)
                {
                    ContentControl control = (ContentControl)sender;

                    if (control == _leftDownControl)
                    {
                        Square square = (Square)control.Content;
                        checkTimer();
                        _mineField.RevealSquare(square.Column, square.Row);
                    }
                    _leftDownControl = null;
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
        private ContentControl[] _contentControls;

        private MineField _mineField;
        private ContentControl _leftDownControl;
        private ContentControl _middleDownControl;
        private int _secondsElapsed;
        private PlayState _playState = PlayState.NotStarted;

        #endregion
    }

    public enum PlayState { NotStarted, Started, Finished }
}
