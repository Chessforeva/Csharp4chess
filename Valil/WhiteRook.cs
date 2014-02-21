using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a white rook.
    /// </summary>
    public sealed class WhiteRook : WhitePiece, IRook
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
                Board.Rank(from) == Board.Rank(to)// the same rank
                ) &&
                board.IsPathClear(from, to);// the path is clear
        }

        /// <summary>
        /// Generates the move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        internal override Move GenerateMove(Board board, int from, int to)
        {
            Move move = base.GenerateMove(board, from, to);

            // just reset castling availability 
            if (move != null)
            {
                if (from == Board.A1)
                {
                    move.MakeWhiteLongCastlingUnavail();
                }
                else if (from == Board.H1)
                {
                    move.MakeWhiteShortCastlingUnavail();
                }
                return move;
            }
            else
            {
                return null;
            }
        }
    }
}
