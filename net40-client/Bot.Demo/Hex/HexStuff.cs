using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using PixelLab.Common;
using Microsoft.Practices.Prism.Commands;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.Hex
{
    public class HexBoard : Changeable
    {
        public HexBoard() : this(DefaultSize) { }
        public HexBoard(int size)
        {
            Contract.Requires<ArgumentOutOfRangeException>(size >= 2);
            m_size = size;
            _pieces = new HexPiece[m_size * m_size];

            m_resetCommand = new DelegateCommand(Reset, () => m_playCount > 0);

            m_connectionTest = new BitArrayPlus(m_size * m_size);

            initialize();
        }

        public bool Play(int row, int column)
        {
            return Play(new PointInt(row, column));
        }

        internal bool Play(PointInt point)
        {
            if (!IsFinished)
            {
                validatePnt(point, m_size);
                int index = getIndex(point);

                HexPiece piece = _pieces[index];

                if (piece.State == HexPieceState.Unused)
                {
                    piece.State = (CurrentPlayer == Player.Black) ? HexPieceState.Black : HexPieceState.White;
                    piece.Number = (++PlayCount);

                    checkFinished();
                    if (!IsFinished)
                    {
                        CurrentPlayer = (CurrentPlayer == Player.Black) ? Player.White : Player.Black;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Size
        {
            get { return m_size; }
        }
        public int PlayCount
        {
            get { return m_playCount; }
            private set
            {
                if (value != m_playCount)
                {
                    m_playCount = value;
                    m_resetCommand.RaiseCanExecuteChanged();
                    onPropertyChanged("PlayCount");
                }
            }
        }
        public Player CurrentPlayer
        {
            get { return m_currentPlayer; }
            private set
            {
                if (value != m_currentPlayer)
                {
                    m_currentPlayer = value;
                    onPropertyChanged("CurrentPlayer");
                }
            }
        }
        public bool IsFinished
        {
            get { return m_isFinished; }
            private set
            {
                if (value != m_isFinished)
                {
                    m_isFinished = value;
                    onPropertyChanged("IsFinished");
                }
            }
        }

        public HexPiece this[int column, int row]
        {
            get
            {
                return _pieces[getIndex(column, row)];
            }
        }
        internal HexPiece this[PointInt point]
        {
            get
            {
                return _pieces[getIndex(point)];
            }
        }
        public HexPiece this[int index]
        {
            get
            {
                return _pieces[index];
            }
        }

        public void Reset()
        {
            initialize();
            OnBoardReset(EventArgs.Empty);
        }
        public ICommand ResetCommand
        {
            get
            {
                return m_resetCommand;
            }
        }

        public event EventHandler BoardReset;
        protected void OnBoardReset(EventArgs e)
        {
            EventHandler handler = BoardReset;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region private methods
        private void onPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void initialize()
        {
            for (int col = 0; col < m_size; col++)
            {
                for (int row = 0; row < m_size; row++)
                {
                    _pieces[getIndex(col, row)] = new HexPiece(col, row);
                }
            }

            CurrentPlayer = Player.White;
            PlayCount = 0;
            IsFinished = false;
        }
        private void checkFinished()
        {
            Debug.Assert(m_connectionTest.Count == 0);

            //white: top-left to lower-right
            //black: bottom-left to upper-right

            bool result = checkFirstRow();
            Debug.Assert(m_connectionTest.Count == 0);

            if (result)
            {
                IsFinished = true;
            }
        }
        private bool checkFirstRow()
        {
            List<PointInt> firstRowPieces = new List<PointInt>();
            for (int i = 0; i < m_size; i++)
            {
                if (CurrentPlayer == Player.White)
                {
                    if (this[i, 0].State == HexPieceState.White)
                    {
                        firstRowPieces.Add(new PointInt(i, 0));
                    }
                }
                else
                {
                    if (this[0, i].State == HexPieceState.Black)
                    {
                        firstRowPieces.Add(new PointInt(0, i));
                    }
                }
            }
            if (firstRowPieces.Count > 0)
            {
                Debug.Assert(m_connectionTest.Count == 0);

                //need to add the first layer to the found pieces cache so we don't loop through them
                foreach (PointInt p in firstRowPieces)
                {
                    m_connectionTest.Add(getIndex(p));
                }

                bool won = false;

                foreach (PointInt p in firstRowPieces)
                {
                    int currentCount = m_connectionTest.Count;
                    won = checkPiece(p);
                    Debug.Assert(currentCount == m_connectionTest.Count);
                    if (won)
                    {
                        setWinner(p);
                        break;
                    }
                }

                foreach (PointInt p in firstRowPieces)
                {
                    Debug.Assert(m_connectionTest.ContainsKey(getIndex(p)));
                    m_connectionTest.Remove(getIndex(p));
                }
                Debug.Assert(m_connectionTest.Count == 0);

                //_stackDepth--;
                return won;
            }
            else
            {
                //_stackDepth--;
                return false;
            }
        }
        private void setWinner(PointInt pnt)
        {
            _pieces[getIndex(pnt)].IsWinner = true;
        }
        private bool checkPiece(PointInt point)
        {
            //_stackDepth++;

            //should only be checking pieces that match the _currentPlayer
            Debug.Assert(this[point].State == ((CurrentPlayer == Player.White) ? HexPieceState.White : HexPieceState.Black));

            //doing a depth-first search from point
            //use the ptns in _connectionTest to make sure we don't loop back on ourselves

            List<PointInt> adjacentPoints = new List<PointInt>();
            foreach (PointInt p in getAdjacent(point, m_size))
            {
                //figure out which points should stay
                //1) == current kind we're searching for
                if (this[p].State == ((CurrentPlayer == Player.White) ? HexPieceState.White : HexPieceState.Black))
                {
                    //2) not already in the list
                    if (!m_connectionTest.ContainsKey(getIndex(p)))
                    {
                        adjacentPoints.Add(p);
                    }
                }
            }

            //now adjacentPoints contains all of the points we should check for winning
            //if not won, go to the next level

            if (adjacentPoints.Count == 0)
            {
                //_stackDepth--;
                return false;
            }
            else
            {
                bool won = false;

                //check for winner
                if (CurrentPlayer == Player.White)
                {
                    //see if this makes white win
                    foreach (PointInt pnt in adjacentPoints)
                    {
                        if (pnt.Row == (m_size - 1))
                        {
                            won = true;
                            setWinner(pnt);
                            break;
                        }
                    }
                }
                else
                {
                    //see if this makes black win
                    foreach (PointInt pnt in adjacentPoints)
                    {
                        if (pnt.Column == (m_size - 1))
                        {
                            won = true;
                            setWinner(pnt);
                            break;
                        }
                    }
                }

                if (!won)
                {
                    //add adjacentPoints to global
                    foreach (PointInt pnt in adjacentPoints)
                    {
                        Debug.Assert(!m_connectionTest.ContainsKey(getIndex(pnt)));
                        m_connectionTest.Add(getIndex(pnt));
                    }

                    //no winner, next level
                    foreach (PointInt pnt in adjacentPoints)
                    {
                        won = checkPiece(pnt);
                        if (won)
                        {
                            setWinner(pnt);
                            break;
                        }
                    }

                    //make sure to remove nodes added to _connectionTest during this search
                    foreach (PointInt pnt in adjacentPoints)
                    {
                        Debug.Assert(m_connectionTest.ContainsKey(getIndex(pnt)));
                        m_connectionTest.Remove(getIndex(pnt));
                    }
                }

                //_stackDepth--;
                return won;
            }
        }

        private int getIndex(int column, int row) { return getIndex(new PointInt(column, row)); }
        private int getIndex(PointInt point)
        {
            validatePnt(point, m_size);

            return point.Row * m_size + point.Column;
        }

        private static void validatePnt(PointInt point, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            if (point.Column >= size || point.Row >= size)
            {
                throw new ArgumentOutOfRangeException("point");
            }
        }
        private static IList<PointInt> getAdjacent(PointInt point, int size)
        {
            validatePnt(point, size);
            List<PointInt> adjacent = new List<PointInt>();

            if (point.Column > 0)
            {
                adjacent.Add(new PointInt(point.Column - 1, point.Row));
            }
            if (point.Row > 0)
            {
                adjacent.Add(new PointInt(point.Column, point.Row - 1));
            }
            if (point.Column < (size - 1))
            {
                adjacent.Add(new PointInt(point.Column + 1, point.Row));
            }
            if (point.Row < (size - 1))
            {
                adjacent.Add(new PointInt(point.Column, point.Row + 1));
            }
            if (point.Column > 0 && point.Row < (size - 1))
            {
                adjacent.Add(new PointInt(point.Column - 1, point.Row + 1));
            }
            if (point.Column < (size - 1) && point.Row > 0)
            {
                adjacent.Add(new PointInt(point.Column + 1, point.Row - 1));
            }

            return adjacent;
        }
        #endregion

        #region private fields

        private Player m_currentPlayer;
        private int m_playCount;
        private bool m_isFinished;

        private readonly DelegateCommand m_resetCommand;
        private readonly BitArrayPlus m_connectionTest;
        private readonly int m_size;
        private readonly HexPiece[] _pieces;

        #endregion

        public const int DefaultSize = 11;

        private class BitArrayPlus
        {
            public BitArrayPlus(int size)
            {
                if (size < 0)
                {
                    throw new ArgumentOutOfRangeException("size");
                }

                _bits = new BitArray(size);
            }

            public void Add(int index)
            {
                if (!_bits[index])
                {
                    _count++;
                    _bits[index] = true;
                }
            }

            public void Remove(int index)
            {
                if (_bits[index])
                {
                    _bits[index] = false;
                    _count--;
                }
            }

            public bool ContainsKey(int index)
            {
                return _bits[index];
            }

            public int Count
            {
                get
                {
                    return _count;
                }
            }

            private int _count;
            private readonly BitArray _bits;
        }
    }

    public class HexPiece : Changeable
    {
        public HexPiece(int column, int row) : this(new PointInt(column, row)) { }

        internal HexPiece(PointInt point)
        {
            _point = point;
            _state = HexPieceState.Unused;
        }

        public bool IsWinner
        {
            get { return _isWinner; }
            set
            {
                UpdateProperty("IsWinner", ref _isWinner, value);
            }
        }

        public HexPieceState State
        {
            get { return _state; }
            set
            {
                Debug.Assert(_state == HexPieceState.Unused, "shouldn't set the thing after its already been set");
                Debug.Assert(value != HexPieceState.Unused);
                UpdateProperty("State", ref _state, value);
            }
        }

        public int Number
        {
            get { return _number; }
            set
            {
                Debug.Assert(_number == 0, "Shouldn't set this thing after its already been set.");
                Debug.Assert(value > 0);
                UpdateProperty("Number", ref _number, value);
            }
        }

        public PointInt Point
        {
            get { return _point; }
        }

        private bool _isWinner;
        private HexPieceState _state;
        private int _number;
        private readonly PointInt _point;
    }

    public struct PointInt : IEquatable<PointInt>
    {
        public PointInt(int column, int row)
        {
            _column = column;
            _row = row;
        }

        public int Column { get { return _column; } }
        public int Row { get { return _row; } }

        public override string ToString()
        {
            return string.Format("{0},{1}", _column, _row);
        }
        public override int GetHashCode()
        {
            return Util.GetHashCode(_column, _row);
        }
        public override bool Equals(object obj)
        {
            if (obj is PointInt)
            {
                PointInt p = (PointInt)obj;
                return this.Equals(p);
            }
            else
            {
                return false;
            }
        }
        public bool Equals(PointInt other)
        {
            return (this._column == other._column && this._row == other._row);
        }

        private readonly int _row, _column;

        public static bool operator ==(PointInt p1, PointInt p2)
        {
            return p1.Equals(p2);
        }
        public static bool operator !=(PointInt p1, PointInt p2)
        {
            return !p1.Equals(p2);
        }
    }

    public enum HexPieceState { Unused, Black, White }
    public enum Player { Black, White }
}
