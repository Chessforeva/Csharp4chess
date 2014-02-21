using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a white bishop.
    /// </summary>
    public sealed class WhiteBishop : WhitePiece, IBishop
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
                Math.Abs(Board.File(from) - Board.File(to)) == Math.Abs(Board.Rank(from) - Board.Rank(to)) &&// it's a diagonal move
                board.IsPathClear(from, to);// the path is clear
        }

    }
}
