using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a white king.
    /// </summary>
    public sealed class WhiteKing : WhitePiece, IKing
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
                (Math.Abs(Board.Rank(from) - Board.Rank(to)) <= 1 && Math.Abs(Board.File(from) - Board.File(to)) <= 1) ||// the king can move one square 
                CanCastleLong(board, from, to) ||// the king can castle long
                CanCastleShort(board, from, to)// the king can castle short
                );
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
            // castling is not considered an attack
            // so the king attacks the squares around him
            return
                base.MightMove(board, from, to) &&
                Math.Abs(Board.Rank(from) - Board.Rank(to)) <= 1 && Math.Abs(Board.File(from) - Board.File(to)) <= 1;
        }

        /// <summary>
        /// Verifies if the king can castle long.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public bool CanCastleLong(Board board, int from, int to)
        {
            return (
                from == Board.E1 && to == Board.C1 &&
                board.Status.WhiteCouldCastleLong &&//check if the king or the rook didn't already move 
                board.IsPathClear(Board.A1, Board.E1) &&// check if the path is clear
                !board.IsAttackedByBlack(Board.E1) && !board.IsAttackedByBlack(Board.D1) && !board.IsAttackedByBlack(Board.C1)// check if the squares traversed by king are not attacked
                );
        }

        /// <summary>
        /// Verifies if the king can castle short.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public bool CanCastleShort(Board board, int from, int to)
        {
            return (
                from == Board.E1 && to == Board.G1 &&
                board.Status.WhiteCouldCastleShort &&//check if the king or the rook didn't already move 
                board.IsPathClear(Board.E1, Board.H1) &&// check if the path is clear 
                !board.IsAttackedByBlack(Board.E1) && !board.IsAttackedByBlack(Board.F1) && !board.IsAttackedByBlack(Board.G1)// check if the squares traversed by king are not attacked
                );
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

            // if it's a castling move
            // CanCastleLong already verifies if the king will 
            // end in check so we don't have to verify it again
            if (CanCastleLong(board, from, to))
            {
                move = new CastlingMove(board.Status, Board.E1, Board.C1, new Move(board.Status, Board.A1, Board.D1));

                move.ChangeSideToMove();// change side to move 
                move.MakeWhiteLongCastlingUnavail();// reset castling availability
                move.MakeWhiteShortCastlingUnavail();// reset castling availability
                move.SetEnPassantTarget(null);// reset en passant target
                move.IncrementPly();// increment the ply

                return move;
            }

            // if it's a castling move
            // CanCastleShort already verifies if the king will 
            // end in check so we don't have to verify it again
            if (CanCastleShort(board, from, to))
            {
                move = new CastlingMove(board.Status, Board.E1, Board.G1, new Move(board.Status, Board.H1, Board.F1));

                move.ChangeSideToMove();// change side to move 
                move.MakeWhiteLongCastlingUnavail();// reset castling availability
                move.MakeWhiteShortCastlingUnavail();// reset castling availability
                move.SetEnPassantTarget(null);// reset en passant target
                move.IncrementPly();// increment the ply

                return move;
            }

            // if it's a regular move
            // just reset castling availability
            move = base.GenerateMove(board, from, to);
            if (move != null)
            {
                move.MakeWhiteLongCastlingUnavail();
                move.MakeWhiteShortCastlingUnavail();

                return move;
            }
            else
            {
                return null;
            }
        }
    }
}
