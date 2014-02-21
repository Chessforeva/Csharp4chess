using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a white pawn.
    /// </summary>
    public sealed class WhitePawn : WhitePiece, IPawn
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
                // a pawn can move one square in front to an empty square
                (
                    to == from - Board.SideSquareNo &&// it's the front square
                    board[to] == null// it's an empty square
                ) ||
                // a pawn can move one square in front diagonally if there is an opposite side piece
                (
                    Board.Rank(to) == Board.Rank(from) - 1 &&// it's on the front row 
                    Math.Abs(Board.File(to) - Board.File(from)) == 1 &&// it's a diagonal move
                    board[to] is BlackPiece// it's an opposite side piece
                ) ||
                IsTwoSquaresMove(board, from, to) ||// it's the two-squares move
                IsEnPassantCaptureMove(board, from, to);// it's the en passant move
        }

        /// <summary>
        /// Verifies if the piece attacks the "to" square, on this board, from the "from" square.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public override bool Attacks(Board board, int from, int to)
        {
            // a pawn attacks one square in front diagonaly
            // and only if it's an empty square or an opposite side piece
            return
                Board.Rank(to) == Board.Rank(from) - 1 &&// it's on the front row
                Math.Abs(Board.File(to) - Board.File(from)) == 1 &&// it's a diagonal move
                !(board[to] is WhitePiece);// it's not occupied by a same side piece
        }

        /// <summary>
        /// Checks if it's the two-squares move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public bool IsTwoSquaresMove(Board board, int from, int to)
        {
            return
                Board.Rank(from) == Board.SideSquareNo - 2 && Board.Rank(to) == Board.SideSquareNo - 4 &&
                Board.File(from) == Board.File(to) &&// the same file
                board[from - Board.SideSquareNo] == null && board[from - (Board.SideSquareNo << 1)] == null;// the first and second squares are empty
        }

        /// <summary>
        /// Checks if it's the en passant move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public bool IsEnPassantCaptureMove(Board board, int from, int to)
        {
            return
                board.Status.EnPassantTarget != null &&// there is a en passant target 
                to == board.Status.EnPassantTarget.Value && Board.Rank(from) == 3 && Board.Rank(board.Status.EnPassantTarget.Value) == 2 &&
                Math.Abs(Board.File(from) - Board.File(to)) == 1;// it's a diagonal move
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
            Move move;

            // if it's an en passant capture
            if (IsEnPassantCaptureMove(board, from, to))
            {
                move = new EnPassantCaptureMove(board.Status, from, to);

                move.ChangeSideToMove();// change the side to move
                move.SetEnPassantTarget(null);// reset the en passant target
                move.ResetPly();// reset the ply

                // we need to verify for check
                move.Make(board);
                bool result = !board.WhiteKingInCheck();
                move.TakeBack(board);
                return result ? move : null;
            }

            move = base.GenerateMove(board, from, to);


            if (move != null)
            {
                move.ResetPly();// reset the ply

                // if it's the two-squares move
                if (IsTwoSquaresMove(board, from, to))
                {
                    move.SetEnPassantTarget(to + Board.SideSquareNo);// set the en passant target
                }

                // if it's a promotion
                if (Board.Rank(to) == 0)
                {
                    // we don't need to verify for check again
                    // so the promotion delegate will not be triggered
                    // later we will change the promotion type as needed
                    move = new PromotionMove(board.Status, from, to);

                    move.ChangeSideToMove();// change the side to move
                    move.SetEnPassantTarget(null);// reset the en passant target
                    move.ResetPly();// reset the ply
                    (move as PromotionMove).SetCapture(board[to]);// the capture information is set
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
