using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.MineSweeper
{
    public class MineField : Changeable
    {
        public MineField() : this(DefaultHeight, DefaultWidth, DefaultMineCount) { }

        public MineField(int rows, int columns, int mineCount)
        {
            if (rows < 1)
            {
                throw new ArgumentOutOfRangeException("rows");
            }
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException("columns");
            }
            if (mineCount < 1 || mineCount >= rows * columns)
            {
                throw new ArgumentOutOfRangeException("mineCount");
            }

            _rows = rows;
            _columns = columns;
            _mineCount = mineCount;
            _clearedCount = 0;
            _squareCount = _rows * _columns;
            _state = WinState.Unknown;

            bool[] live = new bool[_squareCount];
            Random rnd = Util.Rnd;
            int ith;
            for (int i = 0; i < _mineCount; i++)
            {
                ith = rnd.Next(_squareCount - i);
                for (int j = 0; j < _squareCount; j++)
                {
                    if (live[j] == false)
                    {
                        if (ith == 0)
                        {
                            live[j] = true;
                            break;
                        }
                        ith--;
                    }
                }
            }

            _squares = new List<Square>();
            int index;
            Square square;
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    index = y * _columns + x;

                    square = new Square(this, y, x, live[index]);
                    square.PropertyChanged += new PropertyChangedEventHandler(square_PropertyChanged);

                    _squares.Add(square);
                }
            }
        }

        public ReadOnlyCollection<Square> Squares
        {
            get
            {
                if (_squaresROWrapperCache == null)
                {
                    _squaresROWrapperCache = new ReadOnlyCollection<Square>(_squares);
                }
                return _squaresROWrapperCache;
            }
        }
        public int Rows
        {
            get { return _rows; }
        }
        public int Columns
        {
            get { return _columns; }
        }
        public int MineCount
        {
            get { return _mineCount; }
        }
        public int ClearedCount
        {
            get
            {
#if DEBUG
                int clearCheck = 0;
                foreach (Square s in _squares)
                {
                    if (s.State == SquareState.Exposed)
                    {
                        clearCheck++;
                    }
                }

                Debug.Assert(clearCheck == _clearedCount);
#endif
                return _clearedCount;
            }
        }
        public int MinesLeft { get { return _mineCount - _flagCount; } }
        public WinState State { get { return _state; } }

        public void ClearSquare(int column, int row)
        {
            Debug.Assert(!_working);
            _working = true;
            internalClearSquare(column, row);
            _working = false;
            FireDefered();
        }
        public void RevealSquare(int col, int row)
        {
            Debug.Assert(!_working);
            _working = true;
            intervalRevealSquare(col, row);
            _working = false;
            FireDefered();
        }
        public void ToggleSquare(int column, int row)
        {
            if (_state == WinState.Unknown)
            {
                Square s = GetSquare(column, row);
                if (s.State != SquareState.Exposed)
                {
                    switch (s.State)
                    {
                        case SquareState.Unknown:
                            s.State = SquareState.Flagged;
                            break;
                        case SquareState.Flagged:
                            s.State = SquareState.Question;
                            break;
                        case SquareState.Question:
                            s.State = SquareState.Unknown;
                            break;
                    }
                }
            }
        }

        private void internalClearSquare(int column, int row)
        {
            Debug.Assert(_working);

            if (_state == WinState.Unknown)
            {
                Square target = GetSquare(column, row);
                if (target.State == SquareState.Exposed)
                {
                    Debug.Assert(target.IsMine != true);
                    //see if there are any mines around
                    if (target.AdjacentNumber > 0)
                    {
                        IList<Square> adjacent = GetAdjacentSquares(column, row);

                        int adjacentFlagCount = 0;
                        int adjacentQuestionCount = 0;
                        foreach (Square s in adjacent)
                        {
                            if (s.State == SquareState.Flagged)
                            {
                                adjacentFlagCount++;
                            }
                            if (s.State == SquareState.Question)
                            {
                                adjacentQuestionCount++;
                            }
                        }

                        if (adjacentFlagCount == target.AdjacentNumber && adjacentQuestionCount == 0)
                        {
                            foreach (Square s in adjacent)
                            {
                                if (s.State == SquareState.Unknown)
                                {
                                    intervalRevealSquare(s.Column, s.Row);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void intervalRevealSquare(int col, int row)
        {
            Debug.Assert(_working);
            if (_state == WinState.Unknown)
            {
                Square target = GetSquare(col, row);

                if (target.State == SquareState.Unknown)
                {
                    target.State = SquareState.Exposed;

                    if (target.IsMine)
                    {
                        Lost();
                        return;
                    }

                    IncrementCleared();

                    if (target.AdjacentNumber == 0 && !target.IsMine)
                    {
                        IList<Square> adjacent = GetAdjacentSquares(col, row);
                        foreach (Square adjacentSquare in adjacent)
                        {
                            if (adjacentSquare.State == SquareState.Unknown)
                            {
                                intervalRevealSquare(adjacentSquare.Column, adjacentSquare.Row);
                            }
                        }
                    }
                }
            }
        }
        private void FireDefered()
        {
            if (_clearedCountChanged)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(ClearedCountPropertyName));
                _clearedCountChanged = false;
            }
            if (_minesLeftCountChanged)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(MinesLeftPropertyName));
                _minesLeftCountChanged = false;
            }
        }

        private void IncrementCleared()
        {
            Debug.Assert(_state == WinState.Unknown);
            Debug.Assert(_working);
            _clearedCount++;
            _clearedCountChanged = true;
            //PropertyChanged(this, new PropertyChangedEventArgs(ClearedCountPropertyName));

            int targetClear = _rows * _columns - _mineCount;
            if (_clearedCount == targetClear)
            {
                Won();
            }
        }
        private void Won()
        {
#if DEBUG
            Debug.Assert(_state == WinState.Unknown);
            foreach (Square square in _squares)
            {
                if (!square.IsMine)
                {
                    Debug.Assert(square.State == SquareState.Exposed);
                }
            }
#endif
            this._state = WinState.Won;
            OnPropertyChanged(new PropertyChangedEventArgs(StatePropertyName));
        }
        private void Lost()
        {
            this._state = WinState.Lost;
            OnPropertyChanged(new PropertyChangedEventArgs(StatePropertyName));
            foreach (Square s in _squares)
            {
                if (s.IsMine && s.State != SquareState.Flagged)
                {
                    s.State = SquareState.Exposed;
                }
            }
        }

        void square_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                //recount Flags
                var newFlagCount = _squares.Count(s => s.State == SquareState.Flagged);
                if (newFlagCount != _flagCount)
                {
                    _flagCount = newFlagCount;
                    _minesLeftCountChanged = true;
                }

                if (((Square)sender).State == SquareState.Exposed)
                {
                    Debug.Assert(_working);
                    _minesLeftCountChanged = true; //que up a defered property changed
                    //PropertyChanged(this, new PropertyChangedEventArgs(MinesLeftPropertyName));
                }

                FireDefered();
            }
        }

        private Square GetSquare(int col, int row)
        {
            int squareIndex = row * _columns + col;
            Square target = _squares[squareIndex];

            Debug.Assert(col == target.Column && row == target.Row);
            return target;
        }
        internal int GetAdjacent(int col, int row)
        {
            int count = 0;
            foreach (Square s in GetAdjacentSquares(col, row))
            {
                if (s.IsMine)
                {
                    count++;
                }
            }
            return count;
        }

        private IList<Square> GetAdjacentSquares(int col, int row)
        {
            List<Square> adjacent = new List<Square>();
            int lookingCol, lookingRow;

            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        lookingCol = col + x;
                        lookingRow = row + y;

                        if (lookingCol >= 0 && lookingCol < _columns)
                        {
                            if (lookingRow >= 0 && lookingRow < _rows)
                            {
                                adjacent.Add(GetSquare(lookingCol, lookingRow));
                            }
                        }
                    }
                }
            }
            return adjacent;
        }

        private List<Square> _squares;
        private ReadOnlyCollection<Square> _squaresROWrapperCache;
        private int _squareCount;
        private int _columns;
        private int _mineCount;
        private int _clearedCount;
        private int _rows;
        private WinState _state;
        private int _flagCount;

        private bool _working;

        private bool _clearedCountChanged;
        private bool _minesLeftCountChanged;

        public const string ClearedCountPropertyName = "ClearedCount";
        public const string MinesLeftPropertyName = "MinesLeft";
        public const string StatePropertyName = "StateLeft";

        private const int DefaultMineCount = 40;
        private const int DefaultWidth = 16;
        private const int DefaultHeight = 16;
    }

    public class Square : Changeable
    {
        public Square(MineField owner, int row, int column, bool isMine)
        {
            Contract.Requires<ArgumentNullException>(owner != null);

            if (row < 0 || row >= owner.Rows)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < 0 || column >= owner.Columns)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            _owner = owner;
            _row = row;
            _column = column;
            _isMine = isMine;
            _state = SquareState.Unknown;

            _numberCache = -1;
        }

        public int Row
        {
            get { return _row; }
        }
        public int Column
        {
            get { return _column; }
        }
        public bool IsMine
        {
            get { return _isMine; }
        }
        public SquareState State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }
        public override string ToString()
        {
            return string.Format("Square: {0}, {1}", _column, _row);
        }
        public int AdjacentNumber
        {
            get
            {
                if (_numberCache == -1)
                {
                    _numberCache = this._owner.GetAdjacent(_column, _row);
                }
                return _numberCache;
            }
        }

        private MineField _owner;
        private int _row;
        private bool _isMine;
        private int _column;
        private SquareState _state;
        private int _numberCache;
    }

    public enum SquareState { Unknown, Flagged, Question, Exposed }
    public enum WinState { Unknown, Won, Lost }
}