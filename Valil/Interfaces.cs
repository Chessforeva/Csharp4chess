using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Bishop interface.
    /// </summary>
    public interface IBishop { }

    /// <summary>
    /// King interface.
    /// </summary>
    public interface IKing
    {
        /// <summary>
        /// Verifies if the king can castle long.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        bool CanCastleLong(Board board, int from, int to);

        /// <summary>
        /// Verifies if the king can castle short.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        bool CanCastleShort(Board board, int from, int to);
    }

    /// <summary>
    /// Knight interface.
    /// </summary>
    public interface IKnight { }

    /// <summary>
    /// Pawn interface.
    /// </summary>
    public interface IPawn
    {
        /// <summary>
        /// Checks if it's the two-squares move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        bool IsTwoSquaresMove(Board board, int from, int to);

        /// <summary>
        /// Checks if it's the en passant move.
        /// </summary>
        /// <param name="board">The board</param>
        /// <param name="from">The starting square</param>
        /// <param name="to">The ending square</param>
        /// <returns></returns>
        bool IsEnPassantCaptureMove(Board board, int from, int to);
    }

    /// <summary>
    /// Queen interface.
    /// </summary>
    public interface IQueen { }

    /// <summary>
    /// Rook interface.
    /// </summary>
    public interface IRook { }

}