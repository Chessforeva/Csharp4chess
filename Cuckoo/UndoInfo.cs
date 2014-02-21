using Cuckoo;

namespace Cuckoo
{

    /**
     * Contains enough information to undo a previous move.
     * Set by makeMove(). Used by unMakeMove().
     * @author petero
     */
    public class UndoInfo
    {
        public int capturedPiece;
        public int castleMask;
        public int epSquare;
        public int halfMoveClock;
    }

}
