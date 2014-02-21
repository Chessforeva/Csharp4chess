using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a move.
    /// </summary>
    public class Move
    {
        /// <summary>
        /// The board status before the move is made.
        /// </summary>
        protected BoardStatus before;
        /// <summary>
        /// The board status after the move is made.
        /// </summary>
        protected BoardStatus after;
        /// <summary>
        /// The starting square.
        /// </summary>
        protected int from;
        /// <summary>
        /// The ending square.
        /// </summary>
        protected int to;
        /// <summary>
        /// The captured piece.
        /// </summary>
        protected Piece capture;

        /// <summary>
        /// The board status before the move is made.
        /// </summary>
        public BoardStatus Before
        {
            get { return before; }
        }

        /// <summary>
        /// The board status after the move is made.
        /// </summary>
        public BoardStatus After
        {
            get { return after; }
        }

        /// <summary>
        /// The starting square.
        /// </summary>
        public int From
        {
            get { return from; }
        }

        /// <summary>
        /// The ending square.
        /// </summary>
        public int To
        {
            get { return to; }
        }

        /// <summary>
        /// Gets the capture.
        /// </summary>
        internal Piece Capture
        {
            get { return capture; }
        }


        /// <summary>
        /// Returns true if there is a capture, false otherwise.
        /// </summary>
        public bool HasCapture
        {
            get { return capture != null; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="before">The before status</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        internal Move(BoardStatus before, int from, int to)
        {
            this.before = before;
            this.after = before;// the after board status is initialized with the before status
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Makes the move, it doesn't check if it's a valid move.
        /// </summary>
        /// <param name="board">The board</param>
        internal virtual void Make(Board board)
        {
            capture = board[to];// set the capture
            board[to] = board[from];// put the piece on ending square
            board[from] = null;// empty the starting square
            board.Status = after;// set the board status to the after board status
        }

        /// <summary>
        /// Takes back the move, it doesn't check if it's a valid move.
        /// </summary>
        /// <param name="board">The board</param>
        internal virtual void TakeBack(Board board)
        {
            board.Status = before;// set the board status to the before board status	
            board[from] = board[to];// put the piece on starting square
            board[to] = capture;// put back the capture
        }

        /// <summary>
        /// Changes side to move for the after board status.
        /// </summary>
        internal void ChangeSideToMove()
        {
            after.WhiteTurn = after.BlackTurn;
        }

        /// <summary>
        /// Increments the ply for the after board status.
        /// </summary>
        internal void IncrementPly()
        {
            after.Ply++;
        }

        /// <summary>
        /// Resets the ply for the after board status.
        /// </summary>
        internal void ResetPly()
        {
            after.Ply = 0;
        }

        /// <summary>
        /// Increments the moves for the after board status.
        /// </summary>
        internal void IncrementMoves()
        {
            after.Moves++;
        }

        /// <summary>
        /// Sets the en passant target for the after board status.
        /// </summary>
        /// <param name="ep"></param>
        internal void SetEnPassantTarget(int? ep)
        {
            after.EnPassantTarget = ep;
        }

        /// <summary>
        /// Makes white short castling unavailable for the after board status.
        /// </summary>
        internal void MakeWhiteShortCastlingUnavail()
        {
            after.WhiteCouldCastleShort = false;
        }

        /// <summary>
        /// Makes white long castling unavailable for the after board status.
        /// </summary>
        internal void MakeWhiteLongCastlingUnavail()
        {
            after.WhiteCouldCastleLong = false;
        }

        /// <summary>
        /// Makes black short castling unavailable for the after board status.
        /// </summary>
        internal void MakeBlackShortCastlingUnavail()
        {
            after.BlackCouldCastleShort = false;
        }

        /// <summary>
        /// Makes black long castling unavailable for the after board status.
        /// </summary>
        internal void MakeBlackLongCastlingUnavail()
        {
            after.BlackCouldCastleLong = false;
        }
    }
}
