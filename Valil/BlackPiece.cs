using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a black chess piece.
    /// </summary>
    public abstract class BlackPiece : Piece
    {
        /// <summary>
        /// Generates the move.
        /// Adds check verification to move generation,
        /// returns null if its own king will be in check.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        internal override Move GenerateMove(Board board, int from, int to)
        {
            Move move = base.GenerateMove(board, from, to);

            if (move != null)
            {
                move.IncrementMoves();// the number of moves is incremented after Black moves

                // verify for king in check 
                move.Make(board);
                bool result = !board.BlackKingInCheck();
                move.TakeBack(board);
                return result ? move : null;
            }
            else
            {
                return null;
            }
        }

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
            // the first condition for a piece to be able to move
            // is that starting square and the ending square be different
            // and the ending square be empty or opposite piece
            return from != to && !(board[to] is BlackPiece);
        }

    }
}
