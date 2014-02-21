using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements an en passant capture move.
    /// </summary>
    public sealed class EnPassantCaptureMove : Move
    {
        /// <summary>
        /// The en passant target position.
        /// </summary>
        public int Target
        {
            get { return Board.Position(Board.Rank(from), Board.File(to)); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="before">The before status</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        internal EnPassantCaptureMove(BoardStatus before, int from, int to)
            : base(before, from, to)
        {
        }

        /// <summary>
        /// Makes the move, it doesn't check if it's a valid move
        /// </summary>
        /// <param name="board">The board</param>
        internal override void Make(Board board)
        {
            capture = board[Target];// set the capture as the en passant target
            board[Target] = null;// empty the en passant target square
            board[to] = board[from];// put the piece on the ending square
            board[from] = null;// empty the starting square
            board.Status = after;// set the board status to the after board status
        }

        /// <summary>
        /// Takes back the move, it doesn't check if it's a valid move
        /// </summary>
        /// <param name="board">The board</param>
        internal override void TakeBack(Board board)
        {
            board.Status = before;// set the board status to the before board status
            board[from] = board[to];// put the piece on the starting square
            board[Target] = capture;// put back the en passant target
            board[to] = null;// empty the ending square
        }
    }
}
