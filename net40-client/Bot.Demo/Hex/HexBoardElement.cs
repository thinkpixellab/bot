using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.Hex
{
    public class HexBoardElement : FrameworkElement
    {
        public HexBoardElement()
            : this(HexBoard.DefaultSize) { }

        public HexBoardElement(int size)
        {
            _hexBoard = new HexBoard(size);
            _hexBoard.BoardReset += delegate(object sender, EventArgs e)
            {
                setData();
            };

            _boardButtons = new Button[size * size];
            _decorations = new ContentControl[(_hexBoard.Size - 1) * 4];
        }

        public HexBoard Board
        {
            get
            {
                return _hexBoard;
            }
        }

        #region FE overrides
        protected override void OnInitialized(EventArgs e)
        {
            Button button;

            for (int i = 0; i < _boardButtons.Length; i++)
            {
                button = new Button();

                _boardButtons[i] = button;

                this.AddVisualChild(button);
            }
            setData();
            setDecorations();

            base.OnInitialized(e);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < _boardButtons.Length)
            {
                return _boardButtons[index];
            }
            index -= _boardButtons.Length;
            return _decorations[index];
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return _boardButtons.Length + _decorations.Length;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _boardButtons.ForEach(element => element.Measure(s_defaultItemSize));
            _decorations.ForEach(element => element.Measure(s_defaultItemSize));
            return getBoardSize();
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            PointInt pnt;
            Rect rect;
            for (int i = 0; i < _boardButtons.Length; i++)
            {
                pnt = getPoint(i, _hexBoard.Size);

                rect = new Rect(HexHelper.GetTopLeft(_hexBoard.Size, c_defaultItemHeight, pnt), s_defaultItemSize);

                _boardButtons[i].Arrange(rect);
            }

            for (int i = 0; i < _decorations.Length; i++)
            {
                pnt = ((HexPiece)_decorations[i].Content).Point;
                _decorations[i].Arrange(new Rect(HexHelper.GetTopLeft(_hexBoard.Size, c_defaultItemHeight, pnt), s_defaultItemSize));
            }

            return getBoardSize();
        }
        #endregion

        #region private methods
        private Size getBoardSize()
        {
            return new Size(s_defaultItemSize.Width * (3 * _hexBoard.Size / 2d - .5d), _hexBoard.Size * s_defaultItemSize.Height);
        }
        private static PointInt getPoint(int index, int size)
        {
            Debug.Assert(index >= 0 && index < (size * size));

            return new PointInt(index % size, index / size);
        }
        private void setDecorations()
        {
            //1st (size-1): white top left
            //2nd (size-1): white bottom right
            //3rd (size-1): black bottom left
            //4th (size-1): black top right

            ContentControl contentControl;
            HexPiece hexPiece;
            PointInt pnt;
            for (int i = 0; i < _decorations.Length; i++)
            {
                bool isWhite = (i / ((_hexBoard.Size - 1) * 2)) == 0;

                pnt = getDecoratorPnt(i);

                hexPiece = new HexPiece(pnt);
                hexPiece.State = isWhite ? HexPieceState.White : HexPieceState.Black;

                contentControl = new Button();
                contentControl.Opacity = .3;
                contentControl.IsEnabled = false;
                contentControl.DataContext = contentControl.Content = hexPiece;

                _decorations[i] = contentControl;

                this.AddVisualChild(contentControl);
            }
        }

        private PointInt getDecoratorPnt(int i)
        {
            int section = i / ((_hexBoard.Size - 1));
            switch (section)
            {
                case 0:
                    return new PointInt(i + 1, -1);
                case 1:
                    return new PointInt((i - _hexBoard.Size) + 1, _hexBoard.Size);
                case 2:
                    return new PointInt(-1, 1 + i - ((_hexBoard.Size - 1) * 2));
                case 3:
                    return new PointInt(_hexBoard.Size, i - ((_hexBoard.Size - 1) * 3));
                default:
                    throw new ArgumentException("shouldn't get here", "i");
            }
        }

        private void setData()
        {
            for (int i = 0; i < _boardButtons.Length; i++)
            {
                var current = _boardButtons[i];
                _boardButtons[i].Content = _boardButtons[i].DataContext = _hexBoard[i];
                _boardButtons[i].Click += delegate(object sender, RoutedEventArgs args)
                {
                    _hexBoard.Play(((HexPiece)current.Content).Point);
                };
            }
        }

        #endregion

        #region fields

        private readonly HexBoard _hexBoard;
        private readonly Button[] _boardButtons;
        private readonly ContentControl[] _decorations;

        #endregion

        private const double c_defaultItemHeight = 40;
        private static readonly Size s_defaultItemSize = new Size(c_defaultItemHeight / HexHelper.HeightOverWidth, c_defaultItemHeight);
    }
}