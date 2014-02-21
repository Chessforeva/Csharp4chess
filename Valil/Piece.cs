using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a chess piece
    /// </summary>
    public abstract class Piece
    {
        /// <summary>
        /// The number of piece types.
        /// </summary>
        public const int TypesNo = 12;

        /// <summary>
        /// Checks if the piece might move on this "board", 
        /// from the "from" square to the "to" square according to the chess rules.
        /// It doesn't verify if its own king is in check after the move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public abstract bool MightMove(Board board, int from, int to);

        /// <summary>
        /// Verifies if the piece attacks the "to" square, on this board, from the "from" square.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        public virtual bool Attacks(Board board, int from, int to)
        {
            return MightMove(board, from, to);// usually a piece attacks a square if it can move there
        }

        /// <summary>
        /// Generates the move.
        /// In this class, the move is not verified if it puts its own king in check. 
        /// This is implemented in the BlackPiece and WhitePiece subclasses.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        internal virtual Move GenerateMove(Board board, int from, int to)
        {
            if (!MightMove(board, from, to)) { return null; }

            Move move = new Move(board.Status, from, to);


            move.ChangeSideToMove();// change side to move
            move.SetEnPassantTarget(null);// reset the en passant target

            if (board[to] == null)// if there is no capture
            {
                move.IncrementPly();// increment the ply 
            }
            else// if there is a capture
            {
                move.ResetPly();// reset the ply 

                // if there this moves captures a rook
                // and the ending squares is one of the board corners
                // reset castling availability
                if (board[to] is WhiteRook)
                {
                    if (to == Board.A1)
                    {
                        move.MakeWhiteLongCastlingUnavail();
                    }
                    else if (to == Board.H1)
                    {
                        move.MakeWhiteShortCastlingUnavail();
                    }
                }
                else if (board[to] is BlackRook)
                {
                    if (to == Board.A8)
                    {
                        move.MakeBlackLongCastlingUnavail();
                    }
                    else if (to == Board.H8)
                    {
                        move.MakeBlackShortCastlingUnavail();
                    }
                }
            }

            return move;
        }

    }

}
