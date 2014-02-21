//#define HashDebug
//#define ThinkMoveDebug
//#define UseOnlyThinkFromThisTurn
//#define CheckIfMoveOrderingHasMoves

using System;
using System.Collections.Generic;
using System.Text;
#if !SILVERLIGHT
using System.Windows.Forms;
#endif

namespace LousyChess
{
   class SearchMove
   {
      // pointers to other classes
      public Board board;
      public TranspositionTable transpositionTable;
      public MoveGenerator moveGenerator;
      public Evaluator evaluator;
      public Attack attack;

      public delegate void SearchMoveHasNewResultsDelegate(bool alsoNewPV);

      // options
      public bool useTTable = true;
      public bool StoreUpperBoundsInTT = true;      // true gives 53.6% +- 4%
      public bool moveOrdering_SearchPV = true;
      public bool moveOrdering_SearchTT = true;
      public bool moveOrdering_StaticPositionValue = true;
      public bool moveOrdering_UseSEE = true;
      public bool useMoveOrdering_History = true;
      public bool use2HistoryTables = true;
      public bool useKillerMoves = true;
      public bool useNullMove = true;
      public bool dontUseNullMoveAtRoot = true;
      public bool useFutilityPruning = true;
      public bool dontStoreNullMoveInPV = true;    // don't store moves in null-move ply in PV (?)
      public bool UsePVSearch = true;
      public bool UseExtensions = true;

      public bool useLateMoveReduction = true;
      public bool useOnly1LateMoveReduction = false;

      // **** Extended time ****
      // finish a ply if the first couple of root-moves have been searched already.
      // or quit, if the maxNrSecondsForThisMove is met.
      public bool UseExtendedTime = true;    
      // the extra time will be nrSearchTimeExtensions * nrSecondsForThisMove
      public double nrSearchTimeExtensions = 2.0;  
      // To go into extended time, a couple of root moves must already have been finished
      public int MinNrRootMovesFinishedForExtendedTime = 3;
      // Use this as an estimate how much time the next move will take, based on the time of the previous move
      public double MoveTimeMultiplier = 2.5;

      // results
      public Move[] currentMoves;
      public int nrCurrentMovesFromPV;
      public int currentScore;
      public int currentDepth = 0;
      public double currentDepthFinishedTime = 0;
      public int currentNrNodes;
      public int currentNrQNodes;
      public double currentSearchTime;  // the time in seconds, searched so far
      public int currentTTFullPerMill;  // the amount the deep slots of the TT are filled
      private int currentRootMoveNr;

      // operation
      public bool abortSearch;
      public int MaximumAllowedSearchDepth = 1000;

      // statistics
      private int nodeCount = 0;
      private int qsNodeCount = 0;


      // scales down (shifts down) the history between moves. Must be between 1 & 32
      public int HistoryShiftDownBetweenMoves = 4;   

      int maxNrPlies = 50;             // maxNrPlies will be increased, if neccesary.
      int maxNrExpectedPVMoves = 50;   // used to initialize PV_Matrix size
      int maxNrThinkMoves = 50;
      int maxQuiescenceDepth = 100;

      public SearchMoveHasNewResultsDelegate SearchMoveHasNewResults = null;

      Move[][] moveMatrix;           // Holds the generated moves for each ply. [plyNr, moveNr]
      int[][] scoreMatrix;           // Used for move ordering : the 'score' of each move in a ply.  [plyNr, moveNr]
      int[] nrGeneratedMovesInPly;   // the nr of generated moves in a ply.  [plyNr]

      Move[] PrincipalVariation;

      Move[][] PV_Matrix;        // Temp storage for the best found moves
      int[] nrMovesInPVLine;     // The number of moves in each PV_Matrix[] array

      int plyNr = 0;                  // the current PlyNr to investigate 
      bool isFollowingThePV;
      int rootColorToMove;

      Move[] KillerMoves1;
      Move[] KillerMoves2;

      // store from->to positions which cause a beta cut-off
      int[][,] History;
      int[] maxHistoryValue = new int[Const.NrColors];   // the maximum value of any History element. Needed for down-scaling.

      // stuff for time checking
      double nrSecondsForThisMove = 0;
      double maxNrSecondsForThisMove = 0;     // the absolute maximum allowed time. Used for InExtendedTime.
      bool isInExtendedTime = false;          // is in between nrSecondsForThisMove and maxNrSecondsForThisMove
      bool isThinking;
      object abortThinkingTime_SyncObject = new object();
      DateTime startedThinkingTime;
      DateTime abortThinkingTime;
      int nrNodesSearchedAfterLastTimeCheck;
      const int maxNrNodeSearchesBeforeTimeCheck = 10000;   // 10000 = ~ 50 ms

      // reporting 
      DateTime lastTimeReportWasSent;  // the last time the current results was reported to the program


      int nullMoveCounter = 0;


      public SearchMove()
      {
         CreateStorage();
      }


      #region Create and optionally extend storage

      private void CreateStorage()
      {
         // Creates the initial storage. It will be extended if needed.
         nrGeneratedMovesInPly = new int[maxNrPlies];
         moveMatrix = new Move[maxNrPlies][];
         scoreMatrix = new int[maxNrPlies][];
         PV_Matrix = new Move[maxNrPlies][];
         nrMovesInPVLine = new int[maxNrPlies];
         for (int i = 0; i < maxNrPlies; i++)
            PV_Matrix[i] = new Move[maxNrExpectedPVMoves];
         KillerMoves1 = new Move[maxNrPlies];
         KillerMoves2 = new Move[maxNrPlies];
         // create history for both colors
         History = new int[2][,];
         History[0] = new int[Const.NrSquares, Const.NrSquares];
         History[1] = new int[Const.NrSquares, Const.NrSquares];
      }

      private void CheckEnoughNrPliesStorage(int plyNr)
      {
         // optionally create extra storage when the new PlyNr exceeds the maxnrPlies.
         if (plyNr < maxNrPlies)
               return;
         //
         int newMaxNrPlies = maxNrPlies + 10;
         Move[][] newMoveMatrix = new Move[newMaxNrPlies][];
         int[][] newScoreMatrix = new int[newMaxNrPlies][];
         int[] newNrGeneratedMovesInPly = new int[newMaxNrPlies];
         Move[][] newPV_Matrix = new Move[newMaxNrPlies][];
         int[] newNrMovesInPVLine = new int[newMaxNrPlies];
         Move[] newKillerMoves1 = new Move[newMaxNrPlies];
         Move[] newKillerMoves2 = new Move[newMaxNrPlies];
         for (int i = 0; i < maxNrPlies; i++)
         {
            newMoveMatrix[i] = moveMatrix[i];
            newScoreMatrix[i] = scoreMatrix[i];
            newNrGeneratedMovesInPly[i] = nrGeneratedMovesInPly[i];
            newPV_Matrix[i] = PV_Matrix[i];
            newNrMovesInPVLine[i] = nrMovesInPVLine[i];
            newKillerMoves1[i] = KillerMoves1[i];
            newKillerMoves2[i] = KillerMoves2[i];
         }
         // initialize the appended storage
         for (int i = maxNrPlies; i < newMaxNrPlies; i++)
         {
            // moveMatrix[] will be handled (created) by MoveGenerator.GenerateMoves
            newPV_Matrix[i] = new Move[maxNrExpectedPVMoves];
         }
         moveMatrix = newMoveMatrix;
         scoreMatrix = newScoreMatrix;
         nrGeneratedMovesInPly = newNrGeneratedMovesInPly;
         PV_Matrix = newPV_Matrix;
         nrMovesInPVLine = newNrMovesInPVLine;
         KillerMoves1 = newKillerMoves1;
         KillerMoves2 = newKillerMoves2;
         maxNrPlies = newMaxNrPlies;
      }

      #endregion


      private void Log(string s)
      {
         string LogFile = @"c:\tmp\lousy.txt";
         string timeStr = DateTime.Now.ToString();
         using (System.IO.StreamWriter sw = System.IO.File.AppendText(LogFile))
               sw.WriteLine(timeStr + " : " + s);
      }

      #region Store current thinking

      private void StoreCurrentThinking(int score)
      {
         // stores the best found mobes, so far and the score
         currentScore = score;

         int nrPVMoves = PrincipalVariation.Length;
         if (nrPVMoves > maxNrThinkMoves)
            nrPVMoves = maxNrThinkMoves;
         bool TTStillMatchesPV = true;         // this signals if the PV matches the TT
         int nrThinkMoves = 0;
         Move[] thinkMoves = new Move[maxNrThinkMoves];
         ulong[] thinkMoveHashValues = new ulong[maxNrThinkMoves];

         // First store everything in a clone, since making captures reorders the indices of pieces in PiecePos.
         // This reorders future moves. Somehow, this gives problems
         Board clone = new Board(false);
         clone.LoadFrom(board);
         //
         for (int i = 0; i < nrPVMoves; i++)
         {
            if (TTStillMatchesPV)
            {
               int ttIndex = transpositionTable.GetIndex(board.HashValue, 0);
               if (ttIndex >= 0)
               {
                  Move ttMove = new Move(transpositionTable.slots[ttIndex].compressedMove);
                  if (ttMove != PrincipalVariation[i])
                     TTStillMatchesPV = false;
               }
               else
                  TTStillMatchesPV = false;
            }
            thinkMoves[i] = PrincipalVariation[i];
            thinkMoveHashValues[i] = board.HashValue;
            board.MakeMove(PrincipalVariation[i]);
            nrThinkMoves++;
         }
         // finished the PrincipalVariation. Now follow the TT
         nrCurrentMovesFromPV = nrThinkMoves;
         int ttIndex2;
         while ((ttIndex2 = transpositionTable.GetIndex(board.HashValue, 0)) != -1)
         {
            Move move = new Move(transpositionTable.slots[ttIndex2].compressedMove);
            if (move.moveType == Const.NoMoveID)
               break;
            if (nrThinkMoves >= maxNrThinkMoves - 1)
               break;
            thinkMoves[nrThinkMoves] = move;
            thinkMoveHashValues[nrThinkMoves] = board.HashValue;
            board.MakeMove(move);
            nrThinkMoves++;
            // Check if this move has occured before. Otherwise an endless loop would occurr.
            // Allow for 3 entries, since this is possible for the 3-move rule (?)
            int nrSameEntries = 0;
            for (int i = 0; i < nrThinkMoves; i++)
               if (thinkMoveHashValues[i] == board.HashValue)
                  nrSameEntries++;
            if (nrSameEntries >= 3)
               break;
         }
         // now rewind the board by undoing the moves made            
         for (int i = nrThinkMoves - 1; i >= 0; i--)
            board.UnMakeMove(thinkMoves[i]);
         // switch back to the original board
         board.LoadFrom(clone);
         // 
         // So found the moves, now store them
         currentMoves = new Move[nrThinkMoves];
         for (int i = 0; i < nrThinkMoves; i++)
            currentMoves[i] = thinkMoves[i];
      }

      #endregion


      #region timing

      public void SetAllowedSearchTime(double _nrSecondsForThisMove, double _maxNrSecondsForThisMove)
      {
         // Set the normal and maximum allowed time for this move.
         // This can be set before thinking, and during pondering
         nrSecondsForThisMove = _nrSecondsForThisMove;
         maxNrSecondsForThisMove = _maxNrSecondsForThisMove;
         lock (abortThinkingTime_SyncObject)
         {
            if (isThinking)
               // This is called during pondering, when the ponder-move was correct.
               // The engine is already thinking. Update the abortThinkingTime
               abortThinkingTime = startedThinkingTime.AddSeconds(nrSecondsForThisMove);
         }
      }


      private void StartThinkingTimer()
      {
         lock (abortThinkingTime_SyncObject)
         {
            if (isThinking)
               throw new Exception("SetAbortThinkingTime : the engine is already thinking");
            startedThinkingTime = DateTime.Now;
            abortThinkingTime = startedThinkingTime.AddSeconds(nrSecondsForThisMove);
            isThinking = true;
         }
      }


      private DateTime GetAbortThinkingTime()
      {
         lock (abortThinkingTime_SyncObject)
         {
            return abortThinkingTime;
         }
      }


      private double NrSecondsLeft()
      {
         lock (abortThinkingTime_SyncObject)
         {
            return (abortThinkingTime - DateTime.Now).TotalSeconds;
         }
      }


      private bool IsSearchTimeFinished()
      {
         if (DateTime.Now < GetAbortThinkingTime())
            return false;
         // The time has past the AbortThinkingTime.
         // Maybe we can extend it a bit 
         if (!UseExtendedTime || isInExtendedTime || currentRootMoveNr < MinNrRootMovesFinishedForExtendedTime)
            return true;
         else
         {
            // Extend the search time : allow this ply to be finished, until maxTimeForThisMove
            isInExtendedTime = true;
            double newTime = nrSecondsForThisMove + nrSearchTimeExtensions * nrSecondsForThisMove;
            if (newTime > maxNrSecondsForThisMove)
               newTime = maxNrSecondsForThisMove;
            lock (abortThinkingTime_SyncObject)
            {
               abortThinkingTime = startedThinkingTime.AddSeconds(newTime);
            }
            // maybe this doesn't even help ?
            if (DateTime.Now > GetAbortThinkingTime())
               return true;
            else
               return false;
         }
      }

      #endregion


      #region FindBestMove

      public void FindBestMove()
      {
         bool reportIsSent = false;
         isThinking = false;
         isInExtendedTime = false;
         StartThinkingTimer();
         nrNodesSearchedAfterLastTimeCheck = 0;
         nodeCount = 0;
         qsNodeCount = 0;
         DateTime prevDepthFinishedTime = DateTime.Now;
         lastTimeReportWasSent = DateTime.Now;
         //
         abortSearch = false;
         //
         // maxDepth must be >= 1
         rootColorToMove = board.colorToMove;
         PrincipalVariation = new Move[0];    // otherwise OrderMove crashes
         int startDepth = 1;         
         if (useKillerMoves)
            ClearKillerMoves();
            // ShiftDownKillerMoves();   does not help
         if (useMoveOrdering_History)
         {
            ScaleDownHistory(Const.White, HistoryShiftDownBetweenMoves);
            if (use2HistoryTables)
               ScaleDownHistory(Const.Black, HistoryShiftDownBetweenMoves);
         }
         for (int depth = startDepth; depth <= MaximumAllowedSearchDepth; depth++)
         {
            nullMoveCounter = 0;
            plyNr = 0;
            currentRootMoveNr = -1;
            isFollowingThePV = true;
            // don't use int.MaxValue : gives problems with lazy eval
            int score;
            if (dontUseNullMoveAtRoot)
               score = AlphaBeta(depth, -1000000000, 1000000000, false, true);
            else
               score = AlphaBeta(depth, -1000000000, 1000000000, true, true);
            if (rootColorToMove == Const.Black)
               score = -score;     // since NegaMax always returns : larger is better
            // maybe the maximum time got exceeded :
            if (abortSearch)               
               break;  // yes, just use the results of the previous depth
            //
            // This depth was completed in time. Store the results.
            PrincipalVariation = new Move[nrMovesInPVLine[0]];
            Array.Copy(PV_Matrix[0], PrincipalVariation, nrMovesInPVLine[0]);
            // save current results : this must be done thread safe !!!
            StoreCurrentThinking(score);
            currentDepth = depth;
            currentNrNodes = nodeCount;
            currentNrQNodes = qsNodeCount;
            currentScore = score;
            currentSearchTime = (int)((DateTime.Now - startedThinkingTime).TotalMilliseconds) / 1000.0;
            currentDepthFinishedTime = currentSearchTime;
            currentTTFullPerMill = transpositionTable.GetTTFullPerMill();
            //  give the calling function opportunity to show results. Only for depth > 3
            if (SearchMoveHasNewResults != null && depth > 3)
            {
               reportIsSent = true;
               SearchMoveHasNewResults(true);
            }
            // see if it results in mate (-1, since depth starts at 1 , and PlyNr at 0) :
            //          if (Math.Abs(currentScore) >= Evaluator.MateValue - depth -1)
            //            break;
            if (Math.Abs(currentScore) > Evaluator.FutureMate)
               break;
            if (currentScore == 0)
               break;                       // 0 is reserved for draws.
            if (UseExtendedTime)
            {
               // see if there is enough time left to do a next depth-level
               double nrSecondsUsedForThisMove = (DateTime.Now - prevDepthFinishedTime).TotalSeconds;
               prevDepthFinishedTime = DateTime.Now;
               if (UseExtendedTime
                  && (isInExtendedTime || NrSecondsLeft() < MoveTimeMultiplier * nrSecondsUsedForThisMove))
                  break;
            }
         }
         // If nothing has been reported yet, pass the current results back to the engine, if finished.
         // (the previous SearchMoveHasNewResults was only called, if depth > 3) 
         if (SearchMoveHasNewResults != null && !reportIsSent)
            SearchMoveHasNewResults(true);
         //
         isThinking = false;
      }

      #endregion


      #region AlphaBeta explanation

      /*
      AlphaBeta explanation : http://www.math-info.univ-paris5.fr/~bouzy/Doc/PJR/RACours.pdf

I'll try an example. Let's say that your worst enemy has a bunch of bags sitting in front of him.
The bags are there because he lost a bet, and now he has to give you something. Each bag
contains a few things. You are going to get one of the things. You get to pick which bag the
thing will come out of, but he gets to pick what you get out of that bag. You are going to look
through one bag at a time, and you are going to look through the items in each bag one item at a
time. Obviously, what is going to happen is that your enemy is going to give you the worst
thing in the bag you choose, so your goal is to pick the bag that has the nicest worst thing. It's
easy to see how you'd apply min-max to this problem. You are the maximizing player, and you
are going to take the best bag. Your enemy is the minimizing player. He's going to try to make
sure that the best bag is as bad as possible, by giving you the worst thing out of it. All you have
to do to use min-max is take the bag that has the best worst thing in it, if that makes any sense.
The problem with min-max is that you have to look at everything in every bag, which takes a
lot of time.
       * 
But how can we do this more efficiently than min-max? Let's start with the first bag, and look at
everything, and score the bag. Let's say that this bag contained a peanut butter sandwich and the
keys to a new car. You figure that the sandwich is worth less, so if you take this bag you will
get a sandwich. The fact that there are also car keys in the bag doesn't matter, as long as we
assume that your opponent can also evaluate items correctly, which we will. Now you start in
on the next bag. The process you use is a little different than min-max this time. You look at
items one at a time, and compare them against the best thing you know you can get (the
sandwich). As long as items are better than the sandwich, you handle this as you do in min-max
-- you just keep track of the worst one -- since maybe the worst one is better than the sandwich,
which would mean that you'll take this bag over the one with the sandwich. But if you find
another sandwich in this bag, or something you think is worse than the sandwich, you discard
this bag without looking at any more items. The reason is that you know that if you take this
bag, the absolute best you can do is no better than the sandwich (and might be worse). Let's say
that the first item in the bag is a twenty-dollar bill, which is better than the sandwich. If
everything else in the bag is no worse than this, that's the item that your opponent will be forced
to give you if you take this bag, and this becomes your bag of choice. The next thing in the bag
is a six-pack of pop. You think this is better than the sandwich, but it's worse than the twenty,
so this bag is still your bag of choice. The next thing is a rotten fish. This is worse than the
sandwich. You say no thanks, hand the bag back, and forget about this bag.It doesn't matter
what else is in the bag. There could be the keys to another car, which doesn't matter, since you
are going to get the fish. There could be something much worse than the fish (I leave this to
your imagination). This doesn't matter either, since the fish is already bad enough, and you
know you can do better by taking the bag with the sandwich.

Alpha-beta works just like this. The idea is that two scores are passed around in the search. The
first one is alpha, which is the best score that can be forced by some means. Anything worth
less than this is of no use, because there is a strategy that is known to result in a score of alpha.
Anything less than or equal to alpha is no improvement.

The second score is beta. Beta is the worst-case scenario for the opponent. It's the worst thing
that the opponent has to endure, because it's known that there is a way for the opponent to force
a situation no worse than beta, from the opponent's point of view. If the search finds something
that returns a score of beta or better, it's too good, so the side to move is not going to get a
chance to use this strategy. When searching moves, each move searched returns a score that has
some relation to alpha and beta, and the relation is very important and might mean that the
search can stop and return a value.

If a move results in a score that is greater than or equal to beta, this whole node is trash, since
the opponent is not going to let the side to move achieve this position, because there is some
choice the opponent can make that will avoid it. So if we find something with a score of beta or
better, it has been proven that this whole node is not going to happen, so the rest of the legal
moves do not have to be searched.

If a move results in a score that is greater than alpha, but less than beta, this is the move that the
side to move is going to plan to play, unless something changes later on. So alpha is increased
to reflect this new value. It's possible, and in fact quite common, that none of the legal moves
result in a score that exceeds alpha, in which case this position was junk from our point of view,
and we will avoid it by making a different choice somewhere above here in the game tree.
      */

      #endregion


      #region AlphaBeta


      private int AlphaBeta(int depth, int alpha, int beta, bool canDoNullMove, bool canDoLMR)
      {
         // Alpha = the current best score that can be forced by some means.
         // Beta  = the worst-case scenario for the opponent.
         //       = upper bound of what the opponent can achieve
         // Score >= Beta, the opponent won't let you get into this position : he has a better previous move.
         //
         nrNodesSearchedAfterLastTimeCheck++;
         // Check the time.
         if (nrNodesSearchedAfterLastTimeCheck >= maxNrNodeSearchesBeforeTimeCheck)
         {
            nrNodesSearchedAfterLastTimeCheck = 0;
            abortSearch = IsSearchTimeFinished();              // check the time

            if ((DateTime.Now - lastTimeReportWasSent).TotalSeconds > 0.5)
            {
               // send a report, once a second
               if (SearchMoveHasNewResults != null)
               {
                  currentNrNodes = nodeCount;
                  currentSearchTime = (int)((DateTime.Now - startedThinkingTime).TotalMilliseconds) / 1000.0;
                  currentTTFullPerMill = transpositionTable.GetTTFullPerMill();
                  SearchMoveHasNewResults(false);
               }
               lastTimeReportWasSent = DateTime.Now;
            }
#if !SILVERLIGHT
            Application.DoEvents();
#endif
         }
         //
         // If we are out of time, quit. Don't care about the score, since this entire depth will be discarded.
         if (abortSearch)
            return 0;
         //
         nodeCount++;
         int initialAlpha = alpha;

         // check for enough storage
         CheckEnoughNrPliesStorage(plyNr);
         //
         nrMovesInPVLine[plyNr] = 0;
         //
         if (plyNr > 0 && board.IsPracticallyDrawn())
            return 0;                  // check for 50-move rule ,3x repetition & not enough material

         // First look in the Transposition Table
         // But not for PlyNr = 0, otherwise no move is made on finding an exact score !
         // Not for depth <=0, since this should go to QSearch below
         if (useTTable && plyNr > 0 && depth > 0)
         {
            int index = transpositionTable.GetIndex(board.HashValue, depth);
            if (index >= 0)
            {
               int hashScore = transpositionTable.slots[index].score;
               
               /*
               // correct the mate-score ?? This doesn't work, since the stored hashScore is incorrect.
                     if (Math.Abs(hashScore) > Evaluator.FutureMate && Math.Abs(hashScore) <= Evaluator.MateValue)
                     {
                        if (hashScore > Evaluator.FutureMate)
                           hashScore -= plyNr;
                        else
                           hashScore += plyNr;
                     }
               */

               int bound = transpositionTable.slots[index].flags;

               // http://www.cs.ualberta.ca/~jonathan/Courses/657/Notes/4.DAGs.pdf
               // Also see Beowulf, comp.c
               switch (bound)
               {
                  case TranspositionTable.exactBound:
                     // This score is precisely known, so return it.
                     return hashScore;
                  case TranspositionTable.upperBound:
                     // Upperbound : true score is hashScore or less.
                     if (hashScore <= alpha)
                        // This was an upper bound, but still isn't greater than alpha, so return a fail-low
                        return hashScore;   // was alpha 
                     if (hashScore < beta)
                        // This was an upper bound, but was greater than alpha, so adjust beta if necessary
                        beta = hashScore;
                     break;
                  case TranspositionTable.lowerBound:
                     // Lowerbound (caused by beta cutoff) : true score is hashScore or larger.
                     if (hashScore >= beta)
                        // This was a lower bound, but was still greater than beta, so return a fail-high
                        return hashScore;
                     if (hashScore > alpha)
                        // This was a lower bound, but was not greater than beta, so adjust alpha if necessary
                        alpha = hashScore;
                     break;
               }
            }
         }
         
         bool isInCheck = board.IsInCheck();

         // maybe extend the remaining depth in certain circumstances 
         bool haveExtended = false;
         if (UseExtensions)
         {
            if (isInCheck)
            {
               depth++;
               haveExtended = true;
            }
         }

         // due to the extensions, we do not enter QSearch while in check.
         if (depth <= 0)
            return QSearch(maxQuiescenceDepth, alpha, beta);

         if (useNullMove)
         {
            // If making no move at all would produce a beta cut-off, it is reasonable to assume 
            // that _do_ making a move would _definitely_ produce a cut-off. 
            // (assuming that making a move always improves the position).
            // Test it (= make no move) quickly, with a reduced depth (-2).
            // alpha == beta - 1 : only try null moves when not in the PV
            if (canDoNullMove && alpha == beta - 1 && depth >= 2 && !isInCheck && !board.HasZugZwang())
            {
               Move nullMove = Move.NullMove();
               board.MakeMove(nullMove);
               // This might screw up the PV !!
               nullMoveCounter++;
               plyNr++;
               int nullMoveScore;
               // adaptive null-move pruning
               if (depth > 6)
                  nullMoveScore = -AlphaBeta(depth - 1 - 3, -beta, -beta + 1, false, canDoLMR);  // 3-depth reduction
               else
                  nullMoveScore = -AlphaBeta(depth - 1 - 2, -beta, -beta + 1, false, canDoLMR);  // 2-depth reduction
               plyNr--;
               nullMoveCounter--;
               board.UnMakeMove(nullMove);
               if (nullMoveScore >= beta)
                  return nullMoveScore;            // was beta
            }
         }
         

         
         // extended futility pruning
         // not sure is this is done in the right way, but it seems to work very well.
         bool doPruneFutilities = false;
         if (useFutilityPruning)
         {
            if ( (depth==2 || depth == 3) && Math.Abs(alpha) < Evaluator.FutureMate && !isInCheck && !haveExtended)
            {
               //int fastEval = evaluator.GetFastEvaluation();
               // do full evaluation (?)
               int fastEval = evaluator.GetEvaluation(alpha, beta);
               // nb : depth=0 is unused, since it already went to QSearch
               // nb : depth=1 makes just 1 move and then goes to QSearch. Pruning this does not work very well.
               int[] margin = { 0, 0, 125, 300 };
               if (fastEval + margin[depth] < alpha)
                  doPruneFutilities = true;
            }
         }


         // generate moves
         moveMatrix[plyNr] = moveGenerator.GenerateMoves(moveMatrix[plyNr]);
         nrGeneratedMovesInPly[plyNr] = moveGenerator.nrGeneratedMoves;
         // statically order the moves, so the (hopefully) best are tried first : Good for AlphaBeta
         ScoreMoves(plyNr); 

         // singular move extension
         //   if (nrGeneratedMovesInPly[plyNr] == 1)
         //      depth++;

         // loop over all generated moves
         bool legalMoveIsMade = false;
         Move bestMove = Move.NoMove();
         int bestScore = -10 * Evaluator.MateValue;

         for (int i = 0; i < nrGeneratedMovesInPly[plyNr]; i++)
         {
            // If we are out of time, quit. Don't care about the score, since this entire depth is not used.
            if (abortSearch)
               return 0;

            Move currentMove = moveMatrix[plyNr][FindBestMoveNr(plyNr)];

            // check if this move is legal
            if (!board.MakeMove(currentMove))
            {
               // the move is illegal, e.g. by illegal castle or leaving king in check.
               board.UnMakeMove(currentMove);
               continue;
            }

            bool moveGivesCheck = board.IsInCheck();

            if (doPruneFutilities)
            {
               // don't futility prune : captures, 'special'-moves && putting king in check, no legal move was made
               if (legalMoveIsMade && !moveGivesCheck && currentMove.seeScore <= 0
                                   && currentMove.moveType < Const.EnPassantCaptureID)
               {
                  board.UnMakeMove(currentMove);
                  continue;
               }
            }

            legalMoveIsMade = true;
            //
            if (plyNr == 0)
               currentRootMoveNr = i;        // keep track of which root-move we are trying
            //
            int score;
            plyNr++;

            if (UsePVSearch)
            {
               if (i == 0)
                  // assume the first move is the best (&legal). Search it with the normal window.
                  score = -AlphaBeta(depth - 1, -beta, -alpha, true, canDoLMR);
               else
               {
                  // The next moves are considered to be worse. 
                  // Check this with a 'very' narrow window

                  // try reduction on the 'not' so important moves
                  // But : not for captures, pawn-moves, special moves, checks, root-moves
                  if (useLateMoveReduction && canDoLMR
                     && i >= 4 && depth >= 3     // depth 3 & 5 gave identical results after 43 games
                     && !haveExtended
                     && currentMove.captureInfo == Const.NoCaptureID
                     && currentMove.moveType < Const.PawnID     // dont reduces pawn- & special-moves
               //      && currentMove.moveType < Const.CastleQSID     // dont reduces pawn- & special-moves
                     && plyNr > 1                               // 1=root : don't reduce the root  
                     && !isInCheck
                     && !moveGivesCheck)
                  {
                     // a reduced PV search
                     bool canDoMoreRecuctions = useOnly1LateMoveReduction ? false : true;
                     score = -AlphaBeta(depth - 2, -alpha - 1, -alpha, true, canDoMoreRecuctions);
                     // If it was not worse but better, research it with a normal & unreduced window.
                     if (score > alpha)
                        score = -AlphaBeta(depth - 1, -beta, -alpha, true, canDoLMR);
                  }
                  else
                  {
                     // a normal PV search
                     score = -AlphaBeta(depth - 1, -alpha - 1, -alpha, true, canDoLMR);
                     // If it was not worse but better, research it with a normal window.
                     // If it's >= beta, don't worry, it will be cut-off.
                     if (score > alpha && score < beta)
                        score = -AlphaBeta(depth - 1, -beta, -score, true, canDoLMR);
                  }
               }
            }
            else
               score = -AlphaBeta(depth - 1, -beta, -alpha, true, canDoLMR);

            plyNr--;
            board.UnMakeMove(currentMove);

            // If we are out of time, quit. Don't care about the score, since this entire depth will be discarded.
            if (abortSearch)
               return 0;

            if (score > bestScore)
            {
               // The score is better then was attained before. Store it and compare it to alpha and beta.
               bestScore = score;
               bestMove = currentMove;

               if (bestScore > alpha)
               {
                  if (bestScore >= beta)
                  {
                     // To good to be true. The opponent will never let us get in this position.
                     // Store the move which caused the cut-off, and quit.
                     StoreKillerAndHistory(currentMove, depth);
                     if (bestScore != 0)          // don't store draw-values ??
                        transpositionTable.Put(board.HashValue, bestScore, TranspositionTable.lowerBound
                                              , depth, board.halfMoveNr, bestMove.Compress());
                     return bestScore;
                  }

                  alpha = bestScore;
                  // update the PV_Matrix;
                  if (!dontStoreNullMoveInPV || nullMoveCounter == 0)
                  {
                     int nrPVMovesToCopy = nrMovesInPVLine[plyNr + 1];
                     if (PV_Matrix[plyNr].Length < nrPVMovesToCopy + 1)
                        PV_Matrix[plyNr] = new Move[nrPVMovesToCopy + 10];
                     // store the current move, since it was better
                     PV_Matrix[plyNr][0] = currentMove;
                     // Append the current moves of the searched tree, since the current move is better.
                     Array.Copy(PV_Matrix[plyNr + 1], 0, PV_Matrix[plyNr], 1, nrPVMovesToCopy);
                     nrMovesInPVLine[plyNr] = nrPVMovesToCopy + 1;
                  }
               }
            }
         }

         if (legalMoveIsMade)
         {
            // update transposition table.
            if (useTTable)
            {

               /*
               // correct the mate-score ??                            
               if (Math.Abs(ttScore) > Evaluator.FutureMate && Math.Abs(ttScore) <= Evaluator.MateValue)
               {
                  if (ttScore > Evaluator.FutureMate)
                     ttScore += plyNr;
                  else
                     ttScore -= plyNr;
               }
               */
               if (bestScore != 0)          // don't store draw-values ??
               {
                  if (bestScore > initialAlpha)
                     transpositionTable.Put(board.HashValue, bestScore, TranspositionTable.exactBound
                                            , depth, board.halfMoveNr, bestMove.Compress());
                  else if (StoreUpperBoundsInTT)
                     transpositionTable.Put(board.HashValue, bestScore, TranspositionTable.upperBound
                                           , depth, board.halfMoveNr, bestMove.Compress());
               }
            }
            return bestScore;   // the current best possible score.
         }
         else
         {
            // no legal move could be made : either CheckMate or StaleMate
            if (board.IsInCheck())
               return -Evaluator.MateValue + plyNr ;   // CheckMate.    +PlyNr : promote fastest checkmates
              // return -Evaluator.MateValue + plyNr - 1;   // CheckMate.    +PlyNr : promote fastest checkmates
            else
               return 0;              // StaleMate : this must be done better !!
         }
      }

      #endregion


      #region QuiescenceSearch

      private int QSearch(int Qdepth, int alpha, int beta)
      {
         // Check the time
         nrNodesSearchedAfterLastTimeCheck++;
         if (nrNodesSearchedAfterLastTimeCheck >= maxNrNodeSearchesBeforeTimeCheck)
         {
            nrNodesSearchedAfterLastTimeCheck = 0;
            abortSearch = IsSearchTimeFinished();
#if !SILVERLIGHT
            Application.DoEvents();
#endif
         }
         // If we are out of time, quit. Don't care about the score, since this entire depth will be discarded.
         if (abortSearch)
            return 0;
         //
         qsNodeCount++;
         //
         // a draw ?
         if (board.IsPracticallyDrawn())
            return 0;                  // check for 50-move rule ,3x repetition & not enough material
         // bestScore is the score, if it's better not to make any (more) captures
         int bestScore = evaluator.GetEvaluation(alpha, beta);
         // if the maximum Quiescence depth is reached, return it always
         if (Qdepth == 0)
            return beta;
         // pruning
         if (bestScore >= beta)
            return bestScore;
         if (bestScore > alpha)
            alpha = bestScore;  // the evaluation is better then alpha, so update it.
         //
         // check for enough storage
         CheckEnoughNrPliesStorage(plyNr);
         // generate quiescence moves
         moveMatrix[plyNr] = moveGenerator.GenerateQuiescenceMoves(moveMatrix[plyNr]);
         nrGeneratedMovesInPly[plyNr] = moveGenerator.nrGeneratedMoves;
         // loop over all generated moves
         if (nrGeneratedMovesInPly[plyNr] == 0)
         {
            // there are no capturing moves, return the current score
            return bestScore;    // score
         }

         // statically order the moves, so the (hopefully) best are tried first : Good for AlphaBeta
         ScoreQMoves(plyNr);

         for (int i = 0; i < nrGeneratedMovesInPly[plyNr]; i++)
         {
            Move currentMove = moveMatrix[plyNr][FindBestMoveNr(plyNr)];
            // check if this move is legal
            if (!board.MakeMove(currentMove))
            {
               // the move is illegal, e.g. by illegal castle or leaving king in check.
               // This also quickly removes the InCheck non-evasions, if useCheckEvasionsInQSearch=true.
               board.UnMakeMove(currentMove);
               continue;
            }
            //
            plyNr++;
            int score = -QSearch(Qdepth - 1, -beta, -alpha);          // recursive call to deeper plies
            plyNr--;
            //
            board.UnMakeMove(currentMove);
            //
            // If we are out of time, quit. Don't care about the score, since this entire depth will be discarded.
            if (abortSearch)
               return 0;
            //
            if (score > bestScore)
            {
               bestScore = score;
               if (bestScore >= beta)
                  break;                // To worse for the opponent. He won't let you get into this position.
               if (bestScore > alpha)
                  alpha = bestScore;
            }
         }
         //
         return bestScore;             // return the current best possible score.
      }

      #endregion


      #region move ordering


      private void ScoreMoves(int plyNr)
      {
         const int transpostionTableScore = 1 << 30;
         const int followPVScore = 1 << 29;
         const int pawnPromotionOffset = 1 << 27;
         const int winningCaptureScoreOffset = 1 << 25;
         const int equalCaptureScoreOffset = 1 << 24;
         const int killerMove1Score = 1 << 21;
         const int killerMove2Score = killerMove1Score + 100;    // this seems to speed it up by a very little
         const int losingCaptureScoreOffset = 1 << 20;
         const int historyMoveScore = 1 << 14;    // 16384
         //

         int colorToMove = board.colorToMove;
         int historyTableNr;
         if (use2HistoryTables)
            historyTableNr = colorToMove;
         else
            historyTableNr = Const.White;         // use the same table for both colors
         bool haveFoundPVMove = false;
         // assign some static score to each move and order them
         if (moveOrdering_SearchPV)
         {
            if (plyNr >= PrincipalVariation.Length)
               isFollowingThePV = false;   // Searching beyond the PrincipalVariation length.
         }
         //
         bool searchForTTMove = false;
         Move ttMove = Move.NoMove();
         if (moveOrdering_SearchTT)
         {
            int index = transpositionTable.GetIndex(board.HashValue, 0);
            if (index >= 0)
            {
               int compressedMove = transpositionTable.slots[index].compressedMove;
               ttMove = new Move(compressedMove);
               if (ttMove.moveType != Const.NoMoveID)
                  searchForTTMove = true;
            }
         }
         //
         int nrMoves = nrGeneratedMovesInPly[plyNr];
         if (scoreMatrix[plyNr] == null || scoreMatrix[plyNr].Length < nrMoves)
            scoreMatrix[plyNr] = new int[nrMoves + 1];
         Move[] moves = moveMatrix[plyNr];   // just a pointer
         int[] scores = scoreMatrix[plyNr];   // just a pointer
         //
         // History : find the min and max for this set of moves
         int maxHistoryValue = 0;
         if (useMoveOrdering_History)
         {
            // find the highest history value
            for (int i = 0; i < nrMoves; i++)
            {
               int from = moves[i].fromPosition;
               int to = moves[i].toPosition;
               if (History[historyTableNr][from, to] > maxHistoryValue)
                  maxHistoryValue = History[historyTableNr][from, to];
            }
         }


         //
         for (int i = 0; i < nrMoves; i++)
         {
            // first set the score to 0
            scores[i] = 0;
            // see if anyting can be found in the transposition table
            if (searchForTTMove)
            {
               if (moves[i].fromPosition == ttMove.fromPosition
                  && moves[i].toPosition == ttMove.toPosition
                  && moves[i].moveType == ttMove.moveType)
               {
                  searchForTTMove = false;
                  scores[i] += transpostionTableScore;
               }
            }
            // check for the PV
            if (moveOrdering_SearchPV)
            {
               // is this move in the correct place in the principal variation ?
               if (isFollowingThePV && !haveFoundPVMove)
               {
                  if (moves[i].fromPosition == PrincipalVariation[plyNr].fromPosition
                     && moves[i].toPosition == PrincipalVariation[plyNr].toPosition
                     && moves[i].moveType == PrincipalVariation[plyNr].moveType)
                  {
                     haveFoundPVMove = true;
                     scores[i] += followPVScore;
                  }
               }
            }
            // is this a capture ?
            if (moves[i].captureInfo != Const.NoCaptureID)
            {
               if (moveOrdering_UseSEE)
               {
                  int seeScore = moves[i].seeScore;
                  // multiply by (historyMoveScore+1), to not let HistoryMoveScore interfere with SeeScore's
                  if (seeScore > 0)
                     scores[i] += winningCaptureScoreOffset + seeScore * (historyMoveScore + 1);
                  else if (seeScore < 0)
                     scores[i] += losingCaptureScoreOffset + seeScore * (historyMoveScore + 1);    // seeScore is negative
                  else
                     scores[i] += equalCaptureScoreOffset;
               }
               else
               {
                  // NB : pawn = 5, queen = 0
                  int capturedPieceType = moves[i].captureInfo & Const.PieceTypeBitMask;
                  int capturingPieceType = (moves[i].captureInfo >> Const.NrPieceTypeBits) & Const.PieceTypeBitMask;
                  // positive=winning , negative=losing
                  int captureScore = capturingPieceType - capturedPieceType;
                  if (captureScore > 0)
                     scores[i] += winningCaptureScoreOffset + captureScore * (historyMoveScore + 1);
                  else if (captureScore == 0)
                     scores[i] += equalCaptureScoreOffset;
                  else
                     scores[i] += losingCaptureScoreOffset + captureScore * (historyMoveScore + 1);
               }
            }
          else
            {
               // Not a capture : is it a Killer move ?
               if (useKillerMoves)
               {
                  if (moves[i] == KillerMoves1[plyNr])
                     scores[i] += killerMove1Score;
                  else if (moves[i] == KillerMoves2[plyNr])
                     scores[i] += killerMove2Score;
               }
            }
            // is it a pawn promotion ? Only score the Queen promotion. Let the minor promotions get score = 0;
            if (moves[i].moveType == Const.PawnPromoteQueenID)
               scores[i] += pawnPromotionOffset;
            /*
            if (moves[i].moveType >= Const.SpecialMoveID)
            {
               switch (moves[i].moveType)
               {
                  case Const.PawnPromoteQueenID: scores[i] += pawnPromotionOffset + 4 * (historyMoveScore + 1); break;
                  // don't care about the minor promotions
                  case Const.PawnPromoteRookID: scores[i] -= pawnPromotionOffset + 3 * (historyMoveScore + 1);  break;
                  case Const.PawnPromoteBishopID: scores[i] -= pawnPromotionOffset + 2 * (historyMoveScore + 1); break;
                  case Const.PawnPromoteKnightID: scores[i] -= pawnPromotionOffset + 1 * (historyMoveScore + 1); break;
               }
            }
            */
            if (useMoveOrdering_History && maxHistoryValue != 0)
            {
               // if maxHistoryValue == 0, History is empty. Dividing by it yields Int.MinValue !!
               int moveFromPos = moves[i].fromPosition;
               int moveToPos = moves[i].toPosition;
               // history now scores from historyMoveScore to 2*historyMoveScore
               scores[i] += historyMoveScore + (int)(historyMoveScore * 1.0 * History[historyTableNr][moveFromPos, moveToPos] / maxHistoryValue);

#if CheckIfMoveOrderingHasMoves
              if (scores[i] == int.MinValue)
                  MessageBox.Show("oo");
#endif

            }
            if (moveOrdering_StaticPositionValue)
            {
               // trick from Rebel : sort moves by their static position evaluation.
               // This might help a little for otherwise unsorted moves.
               // this probably only works for a very simple evaluation !!!
               // A better approach might be internal deepening
               int moveFromPos = moves[i].fromPosition;
               int moveToPos = moves[i].toPosition;
               int moveType = moves[i].moveType;
               switch (moves[i].moveType)
               {
                  case Const.KingID:
                  case Const.QueenID:
                  case Const.RookID:
                  case Const.BishopID:
                  case Const.KnightID:
                     scores[i] += evaluator.PieceSquareValues[colorToMove][moveType][moveToPos]
                                  - evaluator.PieceSquareValues[colorToMove][moveType][moveFromPos];
                     break;
                  case Const.PawnID:
                  case Const.Pawn2StepID:
                  case Const.EnPassantCaptureID:
                     scores[i] += evaluator.PieceSquareValues[colorToMove][Const.PawnID][moveToPos]
                                  - evaluator.PieceSquareValues[colorToMove][Const.PawnID][moveFromPos];
                     break;
                  case Const.CastleKSID:
                  case Const.CastleQSID:
                     // the from/to pos is that of the king
                     scores[i] += evaluator.PieceSquareValues[colorToMove][Const.KingID][moveToPos]
                                  - evaluator.PieceSquareValues[colorToMove][Const.KingID][moveFromPos];
                     break;
               }
            }
         }
         //
         if (moveOrdering_SearchPV && !haveFoundPVMove)
            isFollowingThePV = false;    // lost the PV track

#if CheckIfMoveOrderingHasMoves
         for (int i = 0; i < nrGeneratedMovesInPly[plyNr]; i++)
         {
            if (scores[i] == int.MinValue)
               MessageBox.Show("OhOh");
         }
#endif

      }



      private void ScoreQMoves(int plyNr)
      {
         const int transpostionTableScore = 1 << 30;
         const int pawnPromotionOffset = 1 << 27;
         const int winningCaptureScoreOffset = 1 << 25;
         const int equalCaptureScoreOffset = 1 << 24;
         const int losingCaptureScoreOffset = 1 << 20;
         //
         // assign some static score to each move
         //

         bool searchForTTMove = false;
         Move ttMove = Move.NoMove();
         if (moveOrdering_SearchTT)
         {
            int index = transpositionTable.GetIndex(board.HashValue, 0);
            if (index >= 0)
            {
               int compressedMove = transpositionTable.slots[index].compressedMove;
               ttMove = new Move(compressedMove);
               if (ttMove.moveType != Const.NoMoveID)
                  searchForTTMove = true;
            }
         }
         
         int nrMoves = nrGeneratedMovesInPly[plyNr];
         if (scoreMatrix[plyNr] == null || scoreMatrix[plyNr].Length < nrMoves)
            scoreMatrix[plyNr] = new int[nrMoves + 1];
         Move[] moves = moveMatrix[plyNr];    // just a pointer
         int[] scores = scoreMatrix[plyNr];   // just a pointer

         for (int i = 0; i < nrMoves; i++)
         {
            // first set the score to 0
            scores[i] = 0;

            if (searchForTTMove)
            {
               // see if anyting can be found in the transposition table
               if (moves[i].fromPosition == ttMove.fromPosition
                  && moves[i].toPosition == ttMove.toPosition
                  && moves[i].moveType == ttMove.moveType)
               {
                  searchForTTMove = false;
                  scores[i] += transpostionTableScore;
               }
            }
             
            // is this a capture ?
            if (moves[i].captureInfo != Const.NoCaptureID)
            {
               if (moveOrdering_UseSEE)
               {
                  int seeScore = moves[i].seeScore;
                  if (seeScore > 0)
                     scores[i] += winningCaptureScoreOffset + seeScore;
                  else if (seeScore < 0)
                     scores[i] += losingCaptureScoreOffset + seeScore;    // seeScore is negative
                  else
                     scores[i] += equalCaptureScoreOffset;
               }
               else
               {
                  // NB : pawn = 5, queen = 0
                  int capturedPieceType = moves[i].captureInfo & Const.PieceTypeBitMask;
                  int capturingPieceType = (moves[i].captureInfo >> Const.NrPieceTypeBits) & Const.PieceTypeBitMask;
                  // positive=winning , negative=losing
                  int captureScore = capturingPieceType - capturedPieceType;
                  if (captureScore > 0)
                     scores[i] += winningCaptureScoreOffset + captureScore;
                  else if (captureScore == 0)
                     scores[i] += equalCaptureScoreOffset;
                  else
                     scores[i] += losingCaptureScoreOffset + captureScore;
               }
            }

            // is it a pawn promotion ? Only score the Queen promotion. Let the minor promotions get score = 0;
            if (moves[i].moveType == Const.PawnPromoteQueenID)
               scores[i] = pawnPromotionOffset;
            /*
            if (moves[i].moveType >= Const.SpecialMoveID)
            {
               switch (moves[i].moveType)
               {
                  case Const.PawnPromoteQueenID: scores[i] += pawnPromotionOffset + 4; break;
                  // don't care about these
                  case Const.PawnPromoteRookID: scores[i] -= pawnPromotionOffset + 3; break;
                  case Const.PawnPromoteBishopID: scores[i] -= pawnPromotionOffset + 2; break;
                  case Const.PawnPromoteKnightID: scores[i] -= pawnPromotionOffset + 1; break;
               }
            }          
             */
         }
      }


      private void StoreKillerAndHistory(Move currentMove, int currentDepth)
      {
         if (useKillerMoves && currentMove.captureInfo == Const.NoCaptureID)
         {
            // It gives a cut-off. Remember if for move-ordening
            // Don't store capturing moves (they already get high move-order priority)
            // And make sure KillerMove2 does not becomes equal to KillerMove1
            if (KillerMoves1[plyNr] != currentMove)
            {
               KillerMoves2[plyNr] = KillerMoves1[plyNr];
               KillerMoves1[plyNr] = currentMove;
            }
            else
            {
               // KillerMove1 is already set to CurrentMove. Try KillerMove2
               if (KillerMoves2[plyNr] != currentMove)
                  KillerMoves2[plyNr] = currentMove;
            }
         }
         if (useMoveOrdering_History)
         {
            int color;
            if (use2HistoryTables)
               color = board.colorToMove;
            else
               color = Const.White;             // use the same table for both colors
            int[,] history = History[color];
            history[currentMove.fromPosition, currentMove.toPosition] += 2 << currentDepth;
            if (history[currentMove.fromPosition, currentMove.toPosition] > maxHistoryValue[color])
            {
               maxHistoryValue[color] = history[currentMove.fromPosition, currentMove.toPosition];
               if (maxHistoryValue[color] > 1 << 30)
                  ScaleDownHistory(color, 2);
            }
         }
      }

      public void ClearKillerMoves()
      {
         for (int i = 0; i < KillerMoves1.Length; i++)
         {
            KillerMoves1[i] = Move.NoMove();
            KillerMoves2[i] = Move.NoMove();
         }
      }

      public void ShiftDownKillerMoves()
      {
         // This does not help, though !!
         // shift down the killer moves by 1 ply. To be used between moves (on switching colors)
         for (int i = 0; i < KillerMoves1.Length-1; i++)
         {
            KillerMoves1[i] = KillerMoves1[i + 1];
            KillerMoves2[i] = KillerMoves2[i + 1];
         }
         KillerMoves1[KillerMoves1.Length - 1] = Move.NoMove();
         KillerMoves2[KillerMoves1.Length - 1] = Move.NoMove();

      }

      public void ClearHistory()
      {
         for (int i = 0; i < Const.NrSquares; i++)
            for (int j = 0; j < Const.NrSquares; j++)
            {
               History[Const.White][i, j] = 0;
               History[Const.Black][i, j] = 0;
            }
         maxHistoryValue[Const.White] = 0;
         maxHistoryValue[Const.Black] = 0;
      }


      private void ScaleDownHistory(int colorToMove, int shift)
      {
         // reduce all history values to prevent overflow, or scale between moves
         for (int i = 0; i < Const.NrSquares; i++)
            for (int j = 0; j < Const.NrSquares; j++)
               History[colorToMove][i, j] = History[colorToMove][i,j] >> shift;
         maxHistoryValue[colorToMove] = maxHistoryValue[colorToMove] >> shift;
      }


      private int FindBestMoveNr(int plyNr)
      {
         Move[] moves = moveMatrix[plyNr];   // just a pointer
         int[] scores = scoreMatrix[plyNr];   // just a pointer
         int nrMoves = nrGeneratedMovesInPly[plyNr];

#if CheckIfMoveOrderingHasMoves
         bool ok = false;
         for (int i=0; i<nrMoves; i++)
            if (scores[i] > int.MinValue)
            {
               ok = true;
               break;
            }
         if (!ok)
            MessageBox.Show("no moves !!");
#endif

         int maxScore = int.MinValue;
         int bestMoveNr = -1;

         for (int i = 0; i < nrMoves; i++)
            if (scores[i] >= maxScore)
            {
               maxScore = scores[i];
               bestMoveNr = i;
            }
         scores[bestMoveNr] = int.MinValue;   // don't pick this one again
         return bestMoveNr;
      }

      #endregion


   }
}
