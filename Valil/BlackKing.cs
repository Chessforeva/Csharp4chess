using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a black king.
    /// </summary>
    public sealed class BlackKing : BlackPiece, IKing
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
                from == Board.E8 && to == Board.C8 &&
                board.Status.BlackCouldCastleLong &&//check if the king or the rook didn't already move
                board.IsPathClear(Board.A8, Board.E8) &&// check if the path is clear 
                !board.IsAttackedByWhite(Board.E8) && !board.IsAttackedByWhite(Board.D8) && !board.IsAttackedByWhite(Board.C8)// check if the squares traversed by king are not attacked
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
                from == Board.E8 && to == Board.G8 &&
                board.Status.BlackCouldCastleShort &&//check if the king or the rook didn't already move
                board.IsPathClear(Board.E8, Board.H8) &&// check if the path is clear
                !board.IsAttackedByWhite(Board.E8) && !board.IsAttackedByWhite(Board.F8) && !board.IsAttackedByWhite(Board.G8)// check if the squares traversed by king are not attacked
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
                move = new CastlingMove(board.Status, Board.E8, Board.C8, new Move(board.Status, Board.A8, Board.D8));

                move.ChangeSideToMove();// change side to move 
                move.MakeBlackLongCastlingUnavail();// reset castling availability
                move.MakeBlackShortCastlingUnavail();// reset castling availability
                move.SetEnPassantTarget(null);// reset en passant target
                move.IncrementPly();// increment the ply
                move.IncrementMoves();// the number of moves is incremented after Black moves

                return move;
            }

            // if it's a castling move
            // CanCastleShort already verifies if the king will 
            // end in check so we don't have to verify it again
            if (CanCastleShort(board, from, to))
            {
                move = new CastlingMove(board.Status, Board.E8, Board.G8, new Move(board.Status, Board.H8, Board.F8));

                move.ChangeSideToMove();// change side to move  
                move.MakeBlackLongCastlingUnavail();// reset castling availability
                move.MakeBlackShortCastlingUnavail();// reset castling availability
                move.SetEnPassantTarget(null);// reset en passant target
                move.IncrementPly();// increment the ply
                move.IncrementMoves();// the number of moves is incremented after Black moves

                return move;
            }

            // if it's a regular move
            // just reset castling availability 
            move = base.GenerateMove(board, from, to);
            if (move != null)
            {
                move.MakeBlackLongCastlingUnavail();
                move.MakeBlackShortCastlingUnavail();

                return move;
            }
            else
            {
                return null;
            }
        }

    }
}
