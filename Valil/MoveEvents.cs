using System;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Move event args.
    /// </summary>
    public class MoveEventArgs : EventArgs
    {
        /// <summary>
        /// The move.
        /// </summary>
        private Move move;
        /// <summary>
        /// The move index.
        /// </summary>
        private int index;

        /// <summary>
        /// The move.
        /// </summary>
        public Move Move
        {
            get { return move; }
        }

        /// <summary>
        /// The move index.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="index"></param>
        public MoveEventArgs(Move move, int index)
        {
            this.move = move;
            this.index = index;
        }

    }

    /// <summary>
    /// Cancel move event args.
    /// </summary>
    public class CancelMoveEventArgs : MoveEventArgs
    {
        /// <summary>
        /// Indicates whether the event should be cancelled.
        /// </summary>
        private bool cancel;

        /// <summary>
        /// Indicates whether the event should be cancelled.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="index"></param>
        public CancelMoveEventArgs(Move move, int index)
            : base(move, index)
        {
        }
    }
}
