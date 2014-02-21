using System;
using System.Reflection;
//using Valil.Chess.Model.Properties;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a promotion move.
    /// </summary>
    public sealed class PromotionMove : Move
    {
        /// <summary>
        /// The piece pawn promotes to.
        /// </summary>
        private Piece promotionPiece;
        /// <summary>
        /// The pawn which is promoted.
        /// </summary>
        private Piece promotedPiece;

        /// <summary>
        /// The piece type pawn promotes to.
        /// Sometimes, the promotion type is not known beforehand, so the PromotionPiece has a setter.
        /// </summary>
        internal Type PromotionType
        {
            get { return promotionPiece != null ? promotionPiece.GetType() : null; }
            set
            {
                if (value == null)
                {
                    // reset the promotion piece
                    promotionPiece = null;
                    return;
                }

                Type[] typeInterfaces = value.GetInterfaces();
                ConstructorInfo constructorInfo = value.GetConstructor(Type.EmptyTypes);

                // if the promotion is not Queen, Rook, Knight, Bishop, it's not the right color or there is no empty-args constructor, set as Queen
                if (
                    (Array.IndexOf(typeInterfaces, typeof(IQueen)) > -1 || Array.IndexOf(typeInterfaces, typeof(IRook)) > -1 || Array.IndexOf(typeInterfaces, typeof(IKnight)) > -1 || Array.IndexOf(typeInterfaces, typeof(IBishop)) > -1) &&
                    ((before.WhiteTurn && value.IsSubclassOf(typeof(WhitePiece))) || (before.BlackTurn && value.IsSubclassOf(typeof(BlackPiece)))) &&
                    (constructorInfo != null)
                    )
                {
                    promotionPiece = constructorInfo.Invoke(null) as Piece;
                }
                else
                {
                    promotionPiece = before.WhiteTurn ? new WhiteQueen() as Piece : new BlackQueen() as Piece;
                }
            }
        }

        /// <summary>
        /// Sets the captured piece.
        /// </summary>
        /// <param name="piece">The piece</param>
        internal void SetCapture(Piece piece)
        {
            capture = piece;
        }



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="before">The before status</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        internal PromotionMove(BoardStatus before, int from, int to)
            : base(before, from, to)
        {
        }

        /// <summary>
        /// Makes the move, it doesn't check if it's a valid move.
        /// The capture must be set before making the move, othewise it throws InvalidOperationException.
        /// </summary>
        /// <param name="board">The board</param>
        internal override void Make(Board board)
        {
            if (promotionPiece != null)
            {
                // set the ending square
                board[to] = promotionPiece;
            }
            else
            {
                // if the promotion was not set throw an exception
                throw new InvalidOperationException("Resources.NoPromotionMsg");
            }

            promotedPiece = board[from];// set the promoted piece
            board[from] = null;// empty the starting square
            board.Status = after;// set the board status to the after board status
        }

        /// <summary>
        /// Takes back the move, it doesn't check if it's a valid move.
        /// </summary>
        /// <param name="board">The board</param>
        internal override void TakeBack(Board board)
        {
            board.Status = before;// set the board status to the before board status
            board[from] = promotedPiece;// put the promoted piece on starting square
            board[to] = capture;// put back the capture
        }
    }
}
