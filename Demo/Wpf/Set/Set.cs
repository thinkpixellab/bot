using System;
using System.Collections;
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

namespace PixelLab.Wpf.Demo.Set
{
    public class SetGame : Changeable
    {
        public SetGame()
        {
            m_playedSetsRO = new ReadOnlyObservableCollection<Set>(m_playedSets);
            newGame();
        }

        #region Public Properties

        public SetCard this[int index]
        {
            get
            {
                Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < c_boardSize);

                if (m_board[index] != c_noCard)
                {
                    return s_deck[m_board[index]];
                }
                else
                {
                    return null;
                }
            }
        }

        public bool CanPlay { get { return m_canPlay; } }

        public int SetsOnBoard
        {
            get
            {
                return setsOnBoard.Count;
            }
        }

        public int CardsInDeck
        {
            get
            {
                return m_cardsInDeck;
            }
        }

        public ReadOnlyObservableCollection<Set> PlayedSets
        {
            get
            {
                return m_playedSetsRO;
            }
        }

        #endregion

        public bool TryPlay(int boardIndex1, int boardIndex2, int boardIndex3)
        {
            RequirePlaying();

            if (IsSet(boardIndex1, boardIndex2, boardIndex3))
            {
                Play(boardIndex1, boardIndex2, boardIndex3);

                SetToAvailable(boardIndex1);
                SetToAvailable(boardIndex2);
                SetToAvailable(boardIndex3);

                if (SetsOnBoard == 0)
                {
                    if (m_cardsInDeck == 0)
                    {
                        m_canPlay = false;
                    }
                    else
                    {
                        // Are there any sets possible with all remaining cards?
                        Set[] sets = GetPossibleSetsInRemainingCards().ToArray();
                        if (sets.Length == 0)
                        {
                            // Even through there are cards left in the deck, no combination of them and
                            //  what's on the board will enable a set.
                            // Rare, but important to catch.
                            m_canPlay = false;
                        }
                        else
                        {
                            // There are some edge cases where this could spin a while, although
                            //  a good set will come up eventually.
                            // Alternative implementations that don't spin like this are
                            //  pretty crazy to implement with little real benefit.
                            do
                            {
                                // The cards that have been played don't enable new sets
                                // So unplay them and start over.
                                UnPlaceCard(boardIndex1);
                                UnPlaceCard(boardIndex2);
                                UnPlaceCard(boardIndex3);

                                SetToAvailable(boardIndex1);
                                SetToAvailable(boardIndex2);
                                SetToAvailable(boardIndex3);
                            }
                            while (SetsOnBoard == 0);
                        } // else(sets.Length = 0)
                    } // else(m_candInDeck.Length > 0)
                } // if(SetsOnBoard == 0)

                onPropertiesChanged();
                return true;
            } // if(IsSet...
            else
            {
                onPropertiesChanged();
                return false;
            } // else (IsSet...
        } //*** bool TryPlay(...

        public void NewGame()
        {
            newGame();
            onPropertiesChanged();
        }

        [Pure]
        public static bool IsSet(SetCard setCard1, SetCard setCard2, SetCard setCard3)
        {
            Contract.Requires<ArgumentNullException>(setCard1 != null);
            Contract.Requires<ArgumentNullException>(setCard2 != null);
            Contract.Requires<ArgumentNullException>(setCard3 != null);

            Contract.Requires<ArgumentOutOfRangeException>(setCard1 != setCard2);
            Contract.Requires<ArgumentOutOfRangeException>(setCard3 != setCard1 && setCard3 != setCard2);

            IList<int> c1 = setCard1.Profile;
            IList<int> c2 = setCard2.Profile;
            IList<int> c3 = setCard3.Profile;

            bool isSet = true;
            for (int i = 0; i < 4; i++)
            {
                if (c1[i] == c2[i])
                {
                    isSet = isSet && (c2[i] == c3[i]);
                }
                else
                {
                    isSet = isSet && ((c1[i] != c3[i]) && (c2[i] != c3[i]));
                }

                if (!isSet)
                {
                    break;
                }
            }

            return isSet;
        } // bool IsSet()

        #region Public Debug Members

#if DEBUG

        public void Test()
        {
            RequirePlaying();

            if (SetsOnBoard > 0)
            {
                var item = setsOnBoard.Random();
                TryPlay(
                    item[0],
                    item[1],
                    item[2]);
            }
        }

        public IList<IList<int>> SetsOnBoardList
        {
            get
            {
                return setsOnBoard;
            }
        }

#endif

        #endregion

        #region Private Methods

        private void newGame()
        {
            m_playedSets.Clear();

            do
            {
                m_playedCards.SetAll(false);
                m_cardsInDeck = c_deckSize;

                for (int i = 0; i < m_board.Length; i++)
                {
                    m_board[i] = c_noCard;
                    SetToAvailable(i);
                }
            } while (SetsOnBoard == 0);

            m_canPlay = true;
        }

        private void UnPlaceCard(int boardIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(boardIndex >= 0 && boardIndex < c_boardSize);

            int deckIndex = m_board[boardIndex];
            if (deckIndex != c_noCard)
            {
                m_playedCards[deckIndex] = false;
                m_cardsInDeck++;
                m_board[boardIndex] = c_noCard;
            }
        }

        private void Play(int boardIndex1, int boardIndex2, int boardIndex3)
        {
            if (IsSet(boardIndex1, boardIndex2, boardIndex3))
            {
                m_playedSets.Insert(0, new Set(this[boardIndex1], this[boardIndex2], this[boardIndex3]));

                m_board[boardIndex1] = c_noCard;
                m_board[boardIndex2] = c_noCard;
                m_board[boardIndex3] = c_noCard;

                m_setsOnBoard = null;
            }
            else
            {
                throw new ArgumentException("Not a set!");
            }
        }

        private void SetToAvailable(int boardIndex)
        {
            Contract.Requires(m_cardsInDeck == 0 || m_board[boardIndex] == c_noCard);
            Contract.Requires<ArgumentOutOfRangeException>(m_cardsInDeck == 0 || boardIndex >= 0 && boardIndex < c_boardSize);

            if (m_cardsInDeck > 0)
            {
                int nthCard = m_random.Next(m_cardsInDeck);

                int deckIndex = -1;
                while (nthCard >= 0)
                {
                    deckIndex++;
                    if (!m_playedCards[deckIndex])
                    {
                        nthCard--;
                    }
                }

                SetBoardCard(boardIndex, deckIndex);
            }
        }

        // Changes stuff...
        private void SetBoardCard(int boardIndex, int deckIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(deckIndex >= 0 && deckIndex < 81);
            Contract.Requires(!m_playedCards[deckIndex], "deckIndex");
            Contract.Requires(m_board[boardIndex] == c_noCard, "boardIndex");
            Contract.Requires<ArgumentOutOfRangeException>(boardIndex >= 0 && boardIndex < c_boardSize);

            m_board[boardIndex] = deckIndex;
            m_playedCards[deckIndex] = true;
            m_cardsInDeck--;
            m_setsOnBoard = null;
        }

        /// <summary>
        ///     Given 3 indices into the *board* returns
        ///     true if they are a set; otherwise, false.
        /// </summary>
        private bool IsSet(int boardIndex1, int boardIndex2, int boardIndex3)
        {
            Contract.Requires<ArgumentOutOfRangeException>(boardIndex1 >= 0 && boardIndex1 < c_boardSize);
            Contract.Requires<ArgumentOutOfRangeException>(boardIndex2 >= 0 && boardIndex2 < c_boardSize);
            Contract.Requires<ArgumentOutOfRangeException>(boardIndex3 >= 0 && boardIndex3 < c_boardSize);

            Contract.Requires(boardIndex2 != boardIndex1, "index2");
            Contract.Requires(boardIndex3 != boardIndex1 && boardIndex3 != boardIndex2, "index3");

            Contract.Requires(m_board[boardIndex1] != c_noCard, "index1");
            Contract.Requires(m_board[boardIndex2] != c_noCard, "index2");
            Contract.Requires(m_board[boardIndex3] != c_noCard, "index3");

            return IsSet(this[boardIndex1], this[boardIndex2], this[boardIndex3]);
        } // bool IsSet()

        [DebuggerStepThrough]
        private void RequirePlaying()
        {
            if (!m_canPlay)
            {
                throw new InvalidOperationException("Cann't play in the current board state.");
            }
        }

        private IList<IList<int>> setsOnBoard
        {
            get
            {
                if (m_setsOnBoard == null)
                {
#if DEBUG
                    m_setsOnBoard = GetSetsOnBoard().ToReadOnlyCollection();
#else
                    m_setsOnBoard = GetSetsOnBoard().ToArray();
#endif
                }
                return m_setsOnBoard;
            }
        }

        private IEnumerable<IList<int>> GetSetsOnBoard()
        {
            for (int i = 0; i < m_board.Length; i++)
            {
                if (m_board[i] != c_noCard)
                {
                    for (int j = (i + 1); j < m_board.Length; j++)
                    {
                        if (m_board[j] != c_noCard)
                        {
                            for (int k = (j + 1); k < m_board.Length; k++)
                            {
                                if (m_board[k] != c_noCard)
                                {
                                    if (IsSet(i, j, k))
                                    {
#if DEBUG
                                        yield return new ReadOnlyCollection<int>(new int[] { i, j, k });
#else
                                        yield return new int[] { i, j, k };
#endif
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<Set> GetPossibleSetsInRemainingCards()
        {
            SetCard[] cardsLeft = GetCardsOnBoard().Concat(GetCardsLeftinDeck()).ToArray();

            for (int i = 0; i < cardsLeft.Length; i++)
            {
                for (int j = i + 1; j < cardsLeft.Length; j++)
                {
                    for (int k = j + 1; k < cardsLeft.Length; k++)
                    {
                        if (IsSet(cardsLeft[i], cardsLeft[j], cardsLeft[k]))
                        {
                            yield return new Set(cardsLeft[i], cardsLeft[j], cardsLeft[k]);
                        }
                    }
                }
            }
        }

        private IEnumerable<SetCard> GetCardsOnBoard()
        {
            return m_board.Where(index => (index != c_noCard)).Select(index => s_deck[index]);
        }

        private IEnumerable<SetCard> GetCardsLeftinDeck()
        {
            return s_deck.Where(setCard => !m_playedCards[setCard.Index]);
        }

        private void onPropertiesChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("SetsOnBoard"));
            OnPropertyChanged(new PropertyChangedEventArgs("CanPlay"));
            OnPropertyChanged(new PropertyChangedEventArgs("CardsInDeck"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnPropertyChanged(new PropertyChangedEventArgs("PlayedSets"));
#if DEBUG
            OnPropertyChanged(new PropertyChangedEventArgs("SetsOnBoardList"));
#endif
        }

        private static SetCard[] GetDeck()
        {
            SetCard[] cards = new SetCard[c_deckSize];

            for (int i = 0; i < c_deckSize; i++)
            {
                SetCard setCard = new SetCard(i);
                cards[setCard.Index] = setCard;
            }

            return cards;
        }

        #endregion

        #region Private Fields

        private bool m_canPlay;
        private int m_cardsInDeck;
        private IList<IList<int>> m_setsOnBoard;

        // The indicies into s_deck
        private readonly int[] m_board = new int[c_boardSize];
        private readonly BitArray m_playedCards = new BitArray(c_deckSize);

        private readonly ObservableCollection<Set> m_playedSets = new ObservableCollection<Set>();
        private readonly ReadOnlyObservableCollection<Set> m_playedSetsRO;

        private readonly Random m_random = new Random();

        private static readonly ReadOnlyCollection<SetCard> s_deck =
            new ReadOnlyCollection<SetCard>(GetDeck());

        private const int c_boardSize = 12;
        private const int c_deckSize = 81;
        private const int c_noCard = -1234567;

        #endregion
    }

    public class Set : ReadOnlyCollection<SetCard>
    {
        public Set(params SetCard[] cards)
            : base(cards.OrderBy(card => card.Index).ToArray())
        {
            Contract.Requires(cards.Length == 3);
            Contract.Requires(SetGame.IsSet(cards[0], cards[1], cards[2]));
        }

        public override string ToString()
        {
            return string.Format("Set: {0}, {1}, {2}",
                this[0], this[1], this[2]);
        }
    }

    public class SetCard : IEquatable<SetCard>
    {
        internal SetCard(int deckIndex)
        {
            Contract.Requires<ArgumentOutOfRangeException>(deckIndex >= 0 && deckIndex < 81, "deckIndex");

            Index = deckIndex;
            int[] profile = new int[] { deckIndex / 27, (deckIndex / 9) % 3, (deckIndex / 3) % 3, deckIndex % 3 };

#if DEBUG
            // This is *insane* paranoia. It's internal
            // Not much of a reason not to expose just an array
            // ...but then again, I'm crazy.
            Profile = new ReadOnlyCollection<int>(profile);
#else
            Profile = profile;
#endif
        }

        public int Index { get; private set; }
        public int Count { get { return Profile[0] + 1; } }
        public SetColor Color { get { return (SetColor)Profile[1]; } }
        public SetFill Fill { get { return (SetFill)Profile[2]; } }
        public SetShape Shape { get { return (SetShape)Profile[3]; } }

        internal IList<int> Profile { get; private set; }

        public override string ToString()
        {
            return string.Format("SetCard: ({0} {1} {2} {3})", Count, Color, Fill, Shape);
        }

        public bool Equals(SetCard other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                // This is weird, but important.
                // '==' is used for SetCard equality in SetGame, but it's not implemented
                // Since '==' is weird on a reference type. This doesn't matter because
                //  the only place cards are created is in the type-constructor of SetGame
                // So, Index equality should always correspond to reference equality.
                // This Assert exists to baby-sit future hackers who mess with the implementation
                //  of SetGame.
                Debug.Assert((this.Index == other.Index) == Object.ReferenceEquals(this, other),
                    "Should only ever be one instance of a given card in a given 'context'.");

                return Object.ReferenceEquals(this, other);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SetCard);
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }

    public enum SetColor { Green, Purple, Red }
    public enum SetFill { Empty, Stripe, Solid }
    public enum SetShape { Oval, Diamond, Squiggle }
}