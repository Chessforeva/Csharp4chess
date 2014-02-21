using Cuckoo;
using System.Collections.Generic;

namespace Cuckoo
{

/**
 *
 * @author petero
 */
public class TranspositionTable {
    public class TTEntry {
        public ulong key;               // Zobrist hash key
        public short move;     // from + (to<<6) + (promote<<12)
        public short score;    // Score from search
        public short depthSlot; // Search depth (bit 0-14) and hash slot (bit 15).
        public byte generation;        // Increase when OTB position changes
        public byte type;       // exact score, lower bound, upper bound
        public short evalScore;        // Score from static evaluation 
        // FIXME!!! Test storing both upper and lower bound in each hash entry.

        public const int T_EXACT = 0;   // Exact score
        public const int T_GE = 1;      // True score >= this.score
        public const int T_LE = 2;      // True score <= this.score
        public const int T_EMPTY = 3;   // Empty hash slot
        
        /** Return true if this object is more valuable than the other, false otherwise. */
        public bool betterThan(TTEntry other, int currGen) {
            if ((generation == currGen) != (other.generation == currGen)) {
                return generation == currGen;   // Old entries are less valuable
            }
            if ((type == T_EXACT) != (other.type == T_EXACT)) {
                return type == T_EXACT;         // Exact score more valuable than lower/upper bound
            }
            if (getDepth() != other.getDepth()) {
                return getDepth() > other.getDepth();     // Larger depth is more valuable
            }
            return false;   // Otherwise, pretty much equally valuable
        }

        /** Return true if entry is good enough to spend extra time trying to avoid overwriting it. */
        public bool valuable(int currGen) {
            if (generation != currGen)
                return false;
            return (type == T_EXACT) || (getDepth() > 3 * Search.plyScale);
        }

        public void getMove(Move m) {
            m.from = move & 63;
            m.to = (move >> 6) & 63;
            m.promoteTo = (move >> 12) & 15;
        }
        public void setMove(Move move) {
            this.move = (short)(move.from + (move.to << 6) + (move.promoteTo << 12));
        }
        
        /** Get the score from the hash entry, and convert from "mate in x" to "mate at ply". */
        public int getScore(int ply) {
            int sc = score;
            if (sc > Search.MATE0 - 1000) {
                sc -= ply;
            } else if (sc < -(Search.MATE0 - 1000)) {
                sc += ply;
            }
            return sc;
        }
        
        /** Convert score from "mate at ply" to "mate in x", and store in hash entry. */
        public void setScore(int score, int ply) {
            if (score > Search.MATE0 - 1000) {
                score += ply;
            } else if (score < -(Search.MATE0 - 1000)) {
                score -= ply;
            }
            this.score = (short)score;
        }

        /** Get depth from the hash entry. */
        public int getDepth() {
            return depthSlot & 0x7fff;
        }

        /** Set depth. */
        public void setDepth(int d) {
            int sd = depthSlot;
            sd &= 0x8000;
            sd |= ((short)d) & 0x7fff;
            depthSlot = (short)sd;
        }

        public int getHashSlot() {
            return depthSlot >> 15;
        }

        public void setHashSlot(int s) {
            int sd = depthSlot;
            sd &= 0x7fff;
            sd |= (s << 15);
            depthSlot = (short)sd;
        }
    }
    TTEntry[] table;
    TTEntry emptySlot;
    byte generation;

    /** Constructor. Creates an empty transposition table with numEntries slots. */
    public TranspositionTable(int log2Size) {
        int numEntries = (1 << log2Size);
        table = new TTEntry[numEntries];
        for (int i = 0; i < numEntries; i++) {
            TTEntry ent = new TTEntry();
            ent.key = 0;
            ent.depthSlot = 0;
            ent.type = TTEntry.T_EMPTY;
            table[i] = ent;
        }
        emptySlot = new TTEntry();
        emptySlot.type = TTEntry.T_EMPTY;
        generation = 0;
    }

    public void Insert(ulong key, Move sm, int type, int ply, int depth, int evalScore) {
        if (depth < 0) depth = 0;
        int idx0 = h0(key);
        int idx1 = h1(key);
        TTEntry ent = table[idx0];
        byte hashSlot = 0;
        if (ent.key != key) {
            ent = table[idx1];
            hashSlot = 1;
        }
        if (ent.key != key) {
            if (table[idx1].betterThan(table[idx0], generation)) {
                ent = table[idx0];
                hashSlot = 0;
            }
            if (ent.valuable(generation)) {
                int altEntIdx = (ent.getHashSlot() == 0) ? h1(ent.key) : h0(ent.key);
                if (ent.betterThan(table[altEntIdx], generation)) {
                    TTEntry altEnt = table[altEntIdx];
                    altEnt.key = ent.key;
                    altEnt.move = ent.move;
                    altEnt.score = ent.score;
                    altEnt.depthSlot = ent.depthSlot;
                    altEnt.generation = (byte)ent.generation;
                    altEnt.type = ent.type;
                    altEnt.setHashSlot(1 - ent.getHashSlot());
                    altEnt.evalScore = ent.evalScore;
                }
            }
        }
        bool doStore = true;
        if ((ent.key == key) && (ent.getDepth() > depth) && (ent.type == type)) {
            if (type == TTEntry.T_EXACT) {
                doStore = false;
            } else if ((type == TTEntry.T_GE) && (sm.score <= ent.score)) {
                doStore = false;
            } else if ((type == TTEntry.T_LE) && (sm.score >= ent.score)) {
                doStore = false;
            }
        }
        if (doStore) {
            if ((ent.key != key) || (sm.from != sm.to))
                ent.setMove(sm);
            ent.key = key;
            ent.setScore(sm.score, ply);
            ent.setDepth(depth);
            ent.generation = (byte)generation;
            ent.type = (byte)type;
            ent.setHashSlot(hashSlot);
            ent.evalScore = (short)evalScore;
        }
    }

    /** Retrieve an entry from the hash table corresponding to "pos". */
    public TTEntry probe(ulong key) {
        int idx0 = h0(key);
        TTEntry ent = table[idx0];
        if (ent.key == key) {
            return ent;
        }
        int idx1 = h1(key);
        ent = table[idx1];
        if (ent.key == key) {
            return ent;
        }
        return emptySlot;
    }

    /**
     * Increase hash table generation. This means that subsequent inserts will be considered
     * more valuable than the entries currently present in the hash table.
     */
    public void nextGeneration() {
        generation++;
    }

    /** Clear the transposition table. */
    public void clear() {
        for (int i = 0; i < table.Length; i++) {
            table[i].type = TTEntry.T_EMPTY;
        }
    }

    /**
     * Extract a list of PV moves, starting from "rootPos" and first move "m".
     */
    public List<Move> extractPVMoves(Position rootPos, Move m) {
        Position pos = new Position(rootPos);
        m = new Move(m);
        List<Move> ret = new List<Move>();
        UndoInfo ui = new UndoInfo();
        List<ulong> hashHistory = new List<ulong>();
        MoveGen moveGen = new MoveGen();
        while (true) {
            ret.Add(m);
            pos.makeMove(m, ui);
            if (hashHistory.Contains(pos.zobristHash())) {
                break;
            }
            hashHistory.Add(pos.zobristHash());
            TTEntry ent = probe(pos.historyHash());
            if (ent.type == TTEntry.T_EMPTY) {
                break;
            }
            m = new Move(0,0,0);
            ent.getMove(m);
            MoveGen.MoveList moves = moveGen.pseudoLegalMoves(pos);
            MoveGen.RemoveIllegal(pos, moves);
            bool contains = false;
            for (int mi = 0; mi < moves.size; mi++)
                if (moves.m[mi].equals(m)) {
                    contains = true;
                    break;
                }
            if  (!contains)
                break;
        }
        return ret;
    }

    /** Extract the PV starting from pos, using hash entries, both exact scores and bounds. */
    public string extractPV(Position pos) {
        string ret = "";
        pos = new Position(pos);    // To avoid modifying the input parameter
        bool first = true;
        TTEntry ent = probe(pos.historyHash());
        UndoInfo ui = new UndoInfo();
        List<ulong> hashHistory = new List<ulong>();
        bool repetition = false;
        while (ent.type != TTEntry.T_EMPTY) {
            string type = "";
            if (ent.type == TTEntry.T_LE) {
                type = "<";
            } else if (ent.type == TTEntry.T_GE) {
                type = ">";
            }
            Move m = new Move(0,0,0);
            ent.getMove(m);
            MoveGen MG = new MoveGen();
            MoveGen.MoveList moves = MG.pseudoLegalMoves(pos);
            MoveGen.RemoveIllegal(pos, moves);
            bool contains = false;
            for (int mi = 0; mi < moves.size; mi++)
                if (moves.m[mi].equals(m)) {
                    contains = true;
                    break;
                }
            if  (!contains)
                break;
            string moveStr = TextIO.moveTostring(pos, m, false);
            if (repetition)
                break;
            if (!first) {
                ret += " ";
            }
            ret += type + moveStr;
            pos.makeMove(m, ui);
            if (hashHistory.Contains(pos.zobristHash())) {
                repetition = true;
            }
            hashHistory.Add(pos.zobristHash());
            ent = probe(pos.historyHash());
            first = false;
        }
        return ret;
    }

    /** Print hash table statistics. */
    public void printStats() {
        int unused = 0;
        int thisGen = 0;
        List<int> depHist = new List<int>();
        int maxDepth = 20;
        for (int i = 0; i < maxDepth; i++) {
            depHist.Add(0);
        }

        for (int i = 0; i < table.Length; i++) {
            TTEntry ent = table[i];

            if (ent.type == TTEntry.T_EMPTY) {
                unused++;
            } else {
                if (ent.generation == generation) {
                    thisGen++;
                }
                int dp = ent.getDepth();
                if (dp < maxDepth) {
                    depHist[dp]++;
                }
            }
        }
        SystemHelper.printf("Hash stats: unused:" + unused.ToString()+
            " thisGen:" + thisGen.ToString());
        for (int i = 0; i < maxDepth; i++) {
            SystemHelper.printf( i.ToString("##") + " " + depHist[i].ToString());
        }
    }
    
    private int h0(ulong key) {
        return (int)((int)key & (table.Length - 1));
    }
    
    private int h1(ulong key) {
        return (int)((int)(key >> 32) & (table.Length - 1));
    }
}

}
