using System;

namespace Valil.Chess.Engine
{
    public sealed partial class ChessEngine
    {
        // the list contains all the pseudo-legal moves for all plies
        // the move list for ply "n" starts at moveIndexList[n] and ends at moveIndexList[n + 1] - 1.
        private ValuedMove[] moveList = new ValuedMove[MAX_MOV];
        private int[] moveIndexList = new int[MAX_PLY];

        // a triangular PV array
        private Move[,] pv = new Move[MAX_PLY, MAX_PLY];
        private int[] pvLength = new int[MAX_PLY];
        private bool followPV;

        // true if the engine is thinking, false otherwise
        private volatile bool thinking;

        /// <summary>
        /// Searches for the best move.
        /// </summary>
        private void Think(int depth)
        {
            int score;

            for (int i = 1; i <= depth; i++)
            {
                followPV = true;

                score = Search(-10000, 10000, i);

                if (score > 9000 || score < -9000) break;
            }
        }

        /// <summary>
        /// A recursive negamax search function with alpha-beta cutoffs.
        /// </summary>
        private int Search(int alpha, int beta, int depth)
        {
            // exit if the move was aborted
            if (!thinking) throw new InvalidOperationException();

            // the maximum depth has been reached
            // call QuiesceSearch to get the score
            if (depth == 0) return QuiescentSearch(alpha, beta);

            pvLength[ply] = ply;

            // if the last move was the first one and it was the repetition move, 
            // it is draw by repetition
            if (ply == 1 && history[0].move == repMove) return 0;

            // check if it's too deep
            if (ply >= MAX_PLY - 1) return Evaluate();

            // if in check, search deeper
            bool inCheck = InCheck(side);
            if (inCheck) depth++;

            // generate the current ply pseudo-legal moves
            GeneratePlyMoves();

            // if the PV is followed call the SortPV
            if (followPV) SortPV();

            // sort the current ply moves
            QuickSortMoveList(moveIndexList[ply], moveIndexList[ply + 1] - 1);

            bool hasMoves = false;
            int score;

            // loop through the current ply moves
            for (int i = moveIndexList[ply]; i < moveIndexList[ply + 1]; i++)
            {
                // check if the move is legal
                if (!Make(moveList[i].move)) continue;

                hasMoves = true;

                score = -Search(-beta, -alpha, depth - 1);

                TakeBack();

                if (score > alpha)
                {
                    // this move caused a cutoff, so increase the history 
                    // value so it gets ordered high next time it is searched
                    heuristicHistory[moveList[i].move.from, moveList[i].move.to] += depth;

                    if (score >= beta) return beta;

                    alpha = score;

                    // update the PV
                    pv[ply, ply] = moveList[i].move;
                    for (int j = ply + 1; j < pvLength[ply + 1]; j++) pv[ply, j] = pv[ply + 1, j];
                    pvLength[ply] = pvLength[ply + 1];
                }
            }

            // if there are no legal moves then it is checkmate or stalemate
            if (!hasMoves) { return inCheck ? -10000 + ply : 0; }

            // fifty move draw rule
            if (fifty >= 100) return 0;

            return alpha;
        }

        /// <summary>
        /// Searches only capture sequences and allows the evaluation function to cut the search off and set alpha. 
        /// The idea is to find a position where there isn't a lot going on so the static evaluation function will work.
        /// </summary>
        private int QuiescentSearch(int alpha, int beta)
        {
            // exit if the move was aborted
            if (!thinking) throw new InvalidOperationException();

            pvLength[ply] = ply;

            // check if it's too deep
            if (ply >= MAX_PLY - 1) return Evaluate();

            // check with the evaluation function
            int score = Evaluate();
            if (score >= beta) return beta;
            if (score > alpha) alpha = score;

            // generate only the current ply pseudo-legal captures moves
            GeneratePlyCaptureMoves();

            // if the PV is followed call the SortPV
            if (followPV) SortPV();

            // sort the current ply moves
            QuickSortMoveList(moveIndexList[ply], moveIndexList[ply + 1] - 1);

            // loop through the current ply moves
            for (int i = moveIndexList[ply]; i < moveIndexList[ply + 1]; i++)
            {
                // check if the move is legal
                if (!Make(moveList[i].move)) continue;

                score = -QuiescentSearch(-beta, -alpha);

                TakeBack();

                if (score > alpha)
                {
                    if (score >= beta) return beta;

                    alpha = score;

                    // update the PV
                    pv[ply, ply] = moveList[i].move;
                    for (int j = ply + 1; j < pvLength[ply + 1]; j++) pv[ply, j] = pv[ply + 1, j];
                    pvLength[ply] = pvLength[ply + 1];
                }
            }

            return alpha;
        }

        /// <summary>
        /// Abort the current search.
        /// </summary>
        public void AbortMove()
        {
            thinking = false;
        }

        /// <summary>
        /// Called when the search function is following the PV. 
        /// </summary>
        private void SortPV()
        {
            followPV = false;

            // look through the current ply moves to see if the PV move is there
            // if so, add 10,000,000 to the move score so it's played first by the search function
            // if not, followPV remains false and the search function stops calling SortPV
            for (int i = moveIndexList[ply]; i < moveIndexList[ply + 1]; i++)
            {
                if (moveList[i].move == pv[0, ply])
                {
                    followPV = true;
                    moveList[i].score += 10000000;
                    return;
                }
            }
        }

        /// <summary>
        /// Sorts the move list between these 2 indexes using QuickSort algorithm.
        /// The moves are sorted by score in descending order, so the moves with a higher score be searched first.
        /// </summary>
        private void QuickSortMoveList(int from, int to)
        {
            if (from < to)
            {
                // split and sort partitions
                int split = PartitionMoveList(from, to);
                QuickSortMoveList(from, split - 1);
                QuickSortMoveList(split + 1, to);
            }
        }

        /// <summary>
        /// QuickSort partition implementation.
        /// </summary>
        private int PartitionMoveList(int from, int to)
        {
            // pivot with first element
            int left = from + 1;
            ValuedMove pivot = moveList[from];
            int right = to;

            // partition array elements
            ValuedMove swap;
            while (left <= right)
            {
                // find item out of place
                while (left <= right && moveList[left].score > pivot.score) 
                    left = left + 1;
                while (left <= right && moveList[right].score <= pivot.score) 
                    right = right - 1;

                // swap values if necessary
                if (left < right)
                {
                    swap = moveList[left];
                    moveList[left] = moveList[right];
                    moveList[right] = swap;
                    left = left + 1;
                    right = right - 1;
                }
            }

            // move pivot element
            swap = moveList[from];
            moveList[from] = moveList[right];
            moveList[right] = swap;

            return right;
        }
    }
}