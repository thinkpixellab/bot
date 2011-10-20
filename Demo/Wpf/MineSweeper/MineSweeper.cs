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
        public MineField(int rows = DefaultHeight, int columns = DefaultWidth, int mineCount = DefaultMineCount)
        {
            Contract.Requires(rows > 0);
            Contract.Requires(columns > 0);
            Contract.Requires(mineCount > 0 && mineCount <= rows * columns);

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

            var squares = new List<Square>();
            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    var index = y * _columns + x;

                    var square = new Square(this, y, x, live[index]);
                    square.PropertyChanged += square_PropertyChanged;

                    squares.Add(square);
                }
            }

            _squares = squares.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<Square> Squares
        {
            get
            {
                return _squares;
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
                int clearCheck = _squares.Where(s => s.State == SquareState.Exposed).Count();
                Debug.Assert(clearCheck == _clearedCount);
#endif
                return _clearedCount;
            }
        }

        public int MinesLeft { get { return _mineCount - _flagCount; } }

        public WinState State
        {
            get { return _state; }
            private set
            {
                UpdateProperty("State", ref _state, value);
            }
        }

        public void ClearSquare(int column, int row)
        {
            Debug.Assert(!_working);
            _working = true;
            try
            {
                internalClearSquare(column, row);
            }
            finally
            {
                _working = false;
            }
            FireDefered();
        }

        public void RevealSquare(int col, int row, bool isDouble)
        {
            Debug.Assert(!_working);
            _working = true;
            try
            {
                intervalRevealSquare(col, row, isDouble);
            }
            finally
            {
                _working = false;
            }
            FireDefered();
        }

        public void ToggleSquare(int column, int row)
        {
            if (State == WinState.Unknown)
            {
                var s = GetSquare(column, row);
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

            if (State == WinState.Unknown)
            {
                Square target = GetSquare(column, row);
                if (target.State == SquareState.Exposed)
                {
                    Debug.Assert(target.IsMine != true);
                    //see if there are any mines around
                    if (target.AdjacentNumber > 0)
                    {
                        var adjacent = GetAdjacentSquares(column, row).ToList();

                        var adjacentFlagCount = 0;
                        var adjacentQuestionCount = 0;
                        foreach (var s in adjacent)
                        {
                            if (s.State == SquareState.Flagged)
                            {
                                adjacentFlagCount++;
                            }
                            else if (s.State == SquareState.Question)
                            {
                                adjacentQuestionCount++;
                            }
                        }

                        if (adjacentFlagCount == target.AdjacentNumber && adjacentQuestionCount == 0)
                        {
                            foreach (var s in adjacent.Where(s => s.State == SquareState.Unknown))
                            {
                                intervalRevealSquare(s.Column, s.Row, false);
                            }
                        }
                    }
                }
            }
        }

        private void intervalRevealSquare(int col, int row, bool isDouble)
        {
            Debug.Assert(_working);
            if (State == WinState.Unknown)
            {
                var target = GetSquare(col, row);

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
                        var adjacent = GetAdjacentSquares(col, row);
                        foreach (var adjacentSquare in adjacent.Where(s => s.State == SquareState.Unknown))
                        {
                            intervalRevealSquare(adjacentSquare.Column, adjacentSquare.Row, false);
                        }
                    }
                }
                else if (isDouble && target.State == SquareState.Exposed)
                {
                    // ah ha! count flags
                    // if we have flags == the number, then do an expose on all the non-exposed points
                    var adjacent = GetAdjacentSquares(col, row).ToList();

                    var mineCount = adjacent.Where(s => s.IsMine).Count();
                    var flagCount = adjacent.Where(s => s.State == SquareState.Flagged).Count();

                    Debug.Assert(mineCount == target.AdjacentNumber);

                    if (flagCount > 0 && flagCount == mineCount)
                    {
                        foreach (var adjacentSquare in adjacent.Where(s => s.State == SquareState.Unknown))
                        {
                            intervalRevealSquare(adjacentSquare.Column, adjacentSquare.Row, false);
                        }
                    }
                }
            }
        }

        private void FireDefered()
        {
            if (_clearedCountChanged)
            {
                OnPropertyChanged("ClearedCount");
                _clearedCountChanged = false;
            }
            if (_minesLeftCountChanged)
            {
                OnPropertyChanged("MinesLeft");
                _minesLeftCountChanged = false;
            }
        }

        private void IncrementCleared()
        {
            Debug.Assert(State == WinState.Unknown);
            Debug.Assert(_working);
            _clearedCount++;
            _clearedCountChanged = true;

            int targetClear = _rows * _columns - _mineCount;
            if (_clearedCount == targetClear)
            {
                Won();
            }
        }

        private void Won()
        {
#if DEBUG
            Debug.Assert(State == WinState.Unknown);
            foreach (var square in from s in _squares
                                   where !s.IsMine
                                   select s)
            {
                Debug.Assert(square.State == SquareState.Exposed);
            }
#endif
            State = WinState.Won;
        }

        private void Lost()
        {
            State = WinState.Lost;
            foreach (var s in from sq in _squares
                              where sq.IsMine
                              && sq.State != SquareState.Flagged
                              select sq)
            {
                s.State = SquareState.Exposed;
            }
        }

        private void square_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var square = (Square)sender;
            if (e.PropertyName == "State")
            {
                //recount Flags
                var newFlagCount = _squares.Count(s => s.State == SquareState.Flagged);
                if (newFlagCount != _flagCount)
                {
                    _flagCount = newFlagCount;
                    _minesLeftCountChanged = true;
                }

                if (square.State == SquareState.Exposed)
                {
                    Debug.Assert(_working);
                    _minesLeftCountChanged = true; //que up a defered property changed
                }

                FireDefered();
            }
        }

        private Square GetSquare(int col, int row)
        {
            int squareIndex = row * _columns + col;
            var target = _squares[squareIndex];

            Debug.Assert(col == target.Column && row == target.Row);
            return target;
        }

        internal int GetAdjacentMineCount(int col, int row)
        {
            return GetAdjacentSquares(col, row).Where(s => s.IsMine).Count();
        }

        private IEnumerable<Square> GetAdjacentSquares(int col, int row)
        {
            return from y in Enumerable.Range(row - 1, 3)
                   from x in Enumerable.Range(col - 1, 3)
                   where !(x == col && y == row)
                   where x >= 0 && x < _columns
                   where y >= 0 && y < _rows
                   select GetSquare(x, y);
        }

        private readonly ReadOnlyCollection<Square> _squares;
        private readonly int _squareCount;
        private readonly int _columns;
        private readonly int _mineCount;
        private readonly int _rows;
        private int _clearedCount;
        private WinState _state;
        private int _flagCount;

        private bool _working;

        private bool _clearedCountChanged;
        private bool _minesLeftCountChanged;

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
                UpdateProperty("State", ref _state, value);
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
                    _numberCache = this._owner.GetAdjacentMineCount(_column, _row);
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