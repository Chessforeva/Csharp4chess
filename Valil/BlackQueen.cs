using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a black queen.
    /// </summary>
    public sealed class BlackQueen : BlackPiece, IQueen
    {
        /// <summary>
        /// Checks if the piece might move on this "board", 
        /// from the "from" square to the "to" square according to the chess rules.
        /// It doesn't verify if its own king is in check after the move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public override bool MightMove(Board board, int from, int to)
        {
            return
                base.MightMove(board, from, to) &&
                (
                Board.File(from) == Board.File(to) ||// the same file
                Board.Rank(from) == Board.Rank(to) ||// the same rank
                Math.Abs(Board.File(from) - Board.File(to)) == Math.Abs(Board.Rank(from) - Board.Rank(to))// it's a diagonal move
                ) &&
                board.IsPathClear(from, to);
        }

    }
}
