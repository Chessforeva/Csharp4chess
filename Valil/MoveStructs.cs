using System;

namespace Valil.Chess.Engine
{
    /// <summary>
    /// Implements a move.
    /// </summary>
    internal struct Move
    {
        /// <summary>
        /// The empty move.
        /// </summary>
        /// <value></value>
        public static Move Empty
        {
            get { return new Move(); }
        }

        public byte from;
        public byte to;

        // the piece to which the pawn promotes to, if this is a promotion move
        public byte promote;

        // describes the move type: 
        // 1 - move has a capture; 2 - castling move; 4 - en passant capture move
        // 8 - pawn 2 squares move; 16 - pawn move; 32 - promotion move
        public byte bits;

        /// <summary>
        /// Two moves are the same if they have the same starting square, ending square and promotion type.
        /// </summary>
        public static bool operator ==(Move m1, Move m2)
        {
            return m1.from == m2.from && m1.to == m2.to && m1.promote == m2.promote;
        }

        /// <summary>
        /// Two moves are not the same if they differ by starting square, ending square or promotion type.
        /// </summary>
        public static bool operator !=(Move m1, Move m2)
        {
            return m1.from != m2.from || m1.to != m2.to || m1.promote != m2.promote;
        }
		
		// override just avoid warnings...
		public override int GetHashCode () {  return GetHashCode(); }
		public override bool Equals (object o) {  return Equals(o); }

        /// <summary>
        /// Parses a regular move (without promotion information) fron its CAN.
        /// </summary>
        /// <param name="can"></param>
        /// <returns></returns>
        public static Move ParseRegularCAN(string can)
        {
            Move m = new Move();

            try
            {
                if (can != null && can.Length >= 4)
                {
                    m.from = (byte)((('8' - can[1]) << 3) + can[0] - 'a');
                    m.to = (byte)((('8' - can[3]) << 3) + can[2] - 'a');
                }
            }
            catch
            {
            }

            return m;
        }

        public override string ToString()
        {
            if (this == Empty)
                return null;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(5);

            sb.Append((char)((from & 7) + 'a'));
            sb.Append((char)('8' - (from >> 3)));

            sb.Append((char)((to & 7) + 'a'));
            sb.Append((char)('8' - (to >> 3)));

            switch (promote)
            {
                case ChessEngine.QUEEN:
                    sb.Append('q');
                    break;
                case ChessEngine.ROOK:
                    sb.Append('r');
                    break;
                case ChessEngine.BISHOP:
                    sb.Append('b');
                    break;
                case ChessEngine.KNIGHT:
                    sb.Append('n');
                    break;
            }

            return sb.ToString();
        }

    }

    /// <summary>
    /// A move with a score so it can be sorted.
    /// </summary>
    internal struct ValuedMove
    {
        public Move move;
        public int score;
    }

    /// <summary>
    /// A move together with the information needed to take the move back.
    /// </summary>
    internal struct HistoryMove
    {
        public Move move;
        public int capture;
        public int castle;
        public int ep;
        public int fifty;
    }
}