using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;            // for BackgroundWorker
using System.IO;                        // for crash logging

namespace LousyChess
{

   /// <summary>
   /// A struct with the current results from the engine;
   /// </summary>
   public struct EngineResults
   {
      public int Score;                    // in centi-pawns
      public double SearchTime;            // in seconds
      public int NodeCount;
      public int QNodeCount;               
      public int Depth;                    // the current search depth
      public double DepthFinishedTime;     // The time in seconds this depth was finished
      public Move[] PV;                    // the current found best moves
      public Move CurrentlySearchedMove;   // not used yet
      public int TTFullPerMill;            // the amount in per-mill the deep-slots are filled

      public Move GetBestMove()
      {
         if (PV == null || PV.Length == 0)
            return Move.NoMove();
         else
            return PV[0];
      }

      public Move GetHintMove()
      {
         if (PV == null || PV.Length < 2)
            return Move.NoMove();
         else
            return PV[1];
      }
   }



   class Engine
   {

      public enum EndOfGameReason { None, ByMate, ByStallMate, By50Moves, By3xRepetition, ByMaterial };

      // The main storage of ALL classes.
      public BitBoard bitboard;
      public MagicMoves magicMoves;
      public TranspositionTable transpositionTable;
      public PawnsTT pawnEvalTT;
      public EvalTT evalTT;
      public Board board;
      public Evaluator evaluator;
      public MoveGenerator moveGenerator;
      public SearchMove searchMove;
      public Attack attack;

      // this is set just before StoreCurrentEngineResults calls UciIO.ProgressChanged.
      // It allows UciIO.RunWorkerCompleted to wait until UciIO.ProgressChanged has completed.
      public bool Pending_ReportProgress = false;

      BackgroundWorker backgroundWorker = null;

      private EngineResults currentEngineResults;
      private object engineResultsSyncObject = new object();

      // the current statistics

      public delegate void EngineReportingDelegate(bool alsoNewPV);
      public EngineReportingDelegate EngineReporting = null;

      // the initial position with which the board started. Used in TakeMovesBack
      string setupBoardFenString;

      // The history of the moves made so far.
      int nrMovesInHistory;
      Move[] MoveHistory;         // The moves made so far. [0] = initial (NoMove)


      public Engine()
      {
         // create all classes
         bitboard = new BitBoard();
         magicMoves = new MagicMoves();
         transpositionTable = new TranspositionTable();
         pawnEvalTT = new PawnsTT();
         evalTT = new EvalTT();
         board = new Board(true);
         evaluator = new My_Evaluator();
         moveGenerator = new MoveGenerator();
         searchMove = new SearchMove();
         attack = new Attack();

         // Setup dependency : assign pointers to classes
         SetupClassDependency();

         searchMove.SearchMoveHasNewResults = StoreCurrentEngineResults;
         SetupInitialBoard(true);

         MoveHistory = new Move[100];
      }


      /// <summary>
      /// Assign pointers to classes. Always use this if a class is redeclared.
      /// </summary>
      public void SetupClassDependency()
      {
         // board
         board.magicMoves = magicMoves;
         board.transpositionTable = transpositionTable;
         board.moveGenerator = moveGenerator;
         board.evaluator = evaluator;
         board.bitboard = bitboard;

         // evaluator
         evaluator.board = board;
         if (evaluator is My_Evaluator)
         {
            (evaluator as My_Evaluator).moveGenerator = moveGenerator;
            (evaluator as My_Evaluator).magicMoves = magicMoves;
            (evaluator as My_Evaluator).bitboard = bitboard;
            (evaluator as My_Evaluator).pawnEvalTT = pawnEvalTT;
            (evaluator as My_Evaluator).evalTT = evalTT;
         }

         // MoveGenerator
         moveGenerator.board = board;
         moveGenerator.magicMoves = magicMoves;
         moveGenerator.bitboard = bitboard;
         moveGenerator.attack = attack;

         // searchMove
         searchMove.board = board;
         searchMove.transpositionTable = transpositionTable;
         searchMove.moveGenerator = moveGenerator;
         searchMove.evaluator = evaluator;
         searchMove.attack = attack;

         // attack
         attack.board = board;
         attack.magicMoves = magicMoves;
         attack.bitBoard = bitboard;
         attack.evaluator = evaluator;
         attack.moveGenerator = moveGenerator;
      }


      #region strategic properties


      public bool UseTranspositionTable
      {
         get
         {
            return searchMove.useTTable;
         }
         set
         {
            searchMove.useTTable = value;
         }
      }


      public int TranspositionTableSizeInMB
      {
         set
         {
            transpositionTable.SetTTSizeInMB(value);
         }
      }

      public bool UseScoreNoise
      {

         get
         {
            if (evaluator is My_Evaluator)
               return (evaluator as My_Evaluator).UseScoreNoise;
            else
               return false;
         }
         set
         {
            if (evaluator is My_Evaluator)
               (evaluator as My_Evaluator).UseScoreNoise = value;
         }
      }


      public int ScoreNoise
      {
         get
         {
            if (evaluator is My_Evaluator)
               return (evaluator as My_Evaluator).ScoreNoise;
            else
               return 0;
         }
         set
         {
            if (evaluator is My_Evaluator)
               (evaluator as My_Evaluator).ScoreNoise = Math.Abs(value);
         }
      }


      public bool UseKingBox1AttackBonus
      {
         get
         {
            if (evaluator is My_Evaluator)
               return (evaluator as My_Evaluator).UseKingBox1AttackBonus;
            else
               return false;
         }
         set
         {
            if (evaluator is My_Evaluator)
               (evaluator as My_Evaluator).UseKingBox1AttackBonus = value;
         }
      }

      public bool UseKingBox1DefendBonus
      {
         get
         {
            if (evaluator is My_Evaluator)
               return (evaluator as My_Evaluator).UseKingBox1DefendBonus;
            else
               return false;
         }
         set
         {
            if (evaluator is My_Evaluator)
               (evaluator as My_Evaluator).UseKingBox1DefendBonus = value;
         }
      }

      public bool UseKnightOutpostBonus
      {
         get
         {
            if (evaluator is My_Evaluator)
               return (evaluator as My_Evaluator).UseKnightOutpostBonus;
            else
               return false;
         }
         set
         {
            if (evaluator is My_Evaluator)
               (evaluator as My_Evaluator).UseKnightOutpostBonus = value;
         }
      }


      public int PawnEvalTranspositionTableSizeInKB
      {
         set
         {
            pawnEvalTT.SetTTSizeInKB(value);
         }
      }


      public bool UseKillerMoves
      {
         get
         {
            return searchMove.useKillerMoves;
         }
         set
         {
            searchMove.useKillerMoves = value;
         }
      }

      public bool UseNullMove
      {
         get
         {
            return searchMove.useNullMove;
         }
         set
         {
            searchMove.useNullMove = value;
         }
      }

      public bool UseFutilityPruning
      {
         get
         {
            return searchMove.useFutilityPruning;
         }
         set
         {
            searchMove.useFutilityPruning = value;
         }
      }

      public bool UseLateMoveReduction
      {
         get
         {
            return searchMove.useLateMoveReduction;
         }
         set
         {
            searchMove.useLateMoveReduction = value;
         }
      }

      public bool UsePVSearch
      {
         get
         {
            return searchMove.UsePVSearch;
         }
         set
         {
            searchMove.UsePVSearch = value;
         }
      }

      public bool UseExtensions
      {
         get
         {
            return searchMove.UseExtensions;
         }
         set
         {
            searchMove.UseExtensions = value;
         }
      }


      public bool MoveOrdering_UsePV
      {
         get
         {
            return searchMove.moveOrdering_SearchPV;
         }
         set
         {
            searchMove.moveOrdering_SearchPV = value;
         }
      }

      public bool MoveOrdering_UseStaticPositions
      {
         get
         {
            return searchMove.moveOrdering_StaticPositionValue;
         }
         set
         {
            searchMove.moveOrdering_StaticPositionValue = value;
         }
      }

      public bool MoveOrdering_UseHistory
      {
         get
         {
            return searchMove.useMoveOrdering_History;
         }
         set
         {
            searchMove.useMoveOrdering_History = value;
         }
      }

      #endregion


      #region operational properties

      public int MaximumAllowedSearchDepth
      {
         get
         {
            return searchMove.MaximumAllowedSearchDepth;
         }
         set
         {
            searchMove.MaximumAllowedSearchDepth = value;
         }
      }


      public void SetAllowedSearchTime(double nrSecondsForThisMove, double maxNrSecondsForThisMove)
      {
         // Set the normal and maximum allowed time for this move.
         searchMove.SetAllowedSearchTime(nrSecondsForThisMove, maxNrSecondsForThisMove);
      }


      #endregion


      public int ColorToMove
      {
         get
         {
            return board.colorToMove;
         }
      }

 
      #region Change Evaluator

      public void ChangeEvaluator(Evaluator evaluator)
      {
         // call, if the evaluator is changed mid-games. Might give problems with TakeBack() !
         // change the evaluator of other classes.
         SetupClassDependency();
         // reset the static and material & postional score
         for (int color = 0; color < Const.NrColors; color++)
         {
            int material = 0;
            int positional = 0;
            for (int pieceType = 0; pieceType < Const.NrPieceTypes; pieceType++)
            {
               material += board.NrPieces[color, pieceType] * evaluator.PieceValues[pieceType];
               for (int j = 0; j < board.NrPieces[color, pieceType]; j++)
               {
                  int position = board.PiecePos[color, pieceType, j];
                  positional += evaluator.PieceSquareValues[color][pieceType][position];
               }
            }
            board.StaticMaterialScore[color] = material;
            board.StaticPositionalScore[color] = positional;
         }
      }
      
      #endregion



      public void SetupInitialBoard(bool clearPlayHistory)
      {
         SetupBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", clearPlayHistory);
      }

      public void SetupBoard(string fenString, bool clearPlayHistory)
      {
         setupBoardFenString = fenString;           // remember the initial position
         board.FEN_to_Board(fenString);
         if (clearPlayHistory)
         {
            searchMove.ClearKillerMoves();
            searchMove.ClearHistory();
            transpositionTable.Clear();
            pawnEvalTT.Clear();
            evalTT.Clear();
         }
         nrMovesInHistory = 0;
      }


      public void StoreMoveInHistory(Move move)
      {
         if (nrMovesInHistory == MoveHistory.Length)
         {
            // increase storage space
            Move[] newArray = new Move[MoveHistory.Length + 100];
            Array.Copy(MoveHistory, newArray, MoveHistory.Length);
            MoveHistory = newArray;
         }
         MoveHistory[nrMovesInHistory] = move;
         nrMovesInHistory++;
      }


      public void TakeMovesBack(int nrTakeBackMoves)
      {
         int nrRedoMoves = nrMovesInHistory - nrTakeBackMoves;
         // go back to the initial position, and also clear the play history
         // NB : the MoveHistory[] array is not cleared, but only nrMovesInHistory is set to zero
         SetupBoard(setupBoardFenString, true);
         for (int i = 0; i < nrRedoMoves; i++)
            MakeMove(MoveHistory[i]);
      }


      public bool CanTakeBack()
      {
         return nrMovesInHistory > 0;
      }


      public void Think()
      {
         searchMove.FindBestMove();
      }

      public void ThinkInBackground(BackgroundWorker _backgroundWorker)
      {
         // call this method in cojunction with BackgroundWorker, to be able to report on progress
         backgroundWorker = _backgroundWorker;
#if SILVERLIGHT
            searchMove.FindBestMove();
#else
         try
         {
            searchMove.FindBestMove();
         }
         catch (Exception e)
         {
            string engineName = Environment.GetCommandLineArgs()[0];
            string engineDirectory = System.IO.Path.GetDirectoryName(engineName);
            string timeStr = DateTime.Now.ToString();
            using (StreamWriter sw = File.AppendText(Path.Combine(engineDirectory, "Engine_Crashed.txt")))
               sw.WriteLine(timeStr + " : " + e.Message + " in : " + e.StackTrace);
         }
#endif
      }


      public void MakeMove(string moveString)
      {
         // either a SAN or a e2e4 type string : 
         // e2e4 a1h8 e1g1 (KS castle) , e7e8q (promotion)
         Move move = board.FindMoveOnBoard(moveString);
         if (move.moveType != Const.NoMoveID)
            board.MakeMove(move);
         else
            throw new Exception("illegal move");
         // store the move in MoveHistory
         StoreMoveInHistory(move);
      }

      public void MakeMove(Move move)
      {
         // this must later go, in favour of MakeMove(string)
         if (move.moveType != Const.NoMoveID)
            board.MakeMove(move);
         else
            throw new Exception("illegal move");
         // store the move in MoveHistory
         StoreMoveInHistory(move);
      }

      public void AbortSearching()
      {
         // stop searching immediately. Fill in current search results.
         searchMove.abortSearch = true;
      }

      public bool UserMoveIsLegal(string moveString)
      {
         // slow : only use for validation of user move
         Move move = board.FindMoveOnBoard(moveString);
         if (move == Move.NoMove())
            return false;              // not found
         // check if this move leaves the king in check :
         // Do this with a clone, since making captures reorders the indices of pieces in PiecePos.
         // This reorders future moves. Somehow, this gives problems
         Board clone = new Board(false);
         clone.LoadFrom(board);
         board.MakeMove(move);
         board.ToggleMoveColor();          // this was toggled by MakeMove. Undo it.
         bool leavesKingInCheck = board.IsInCheck();
         board.LoadFrom(clone);

         return !leavesKingInCheck;
      }


      public Move FindMoveOnBoard(string moveString)
      {
         return board.FindMoveOnBoard(moveString);
      }


      private void StoreCurrentEngineResults(bool alsoNewPV)
      {
         // Called by SearchMove, if it has something to report
         lock (engineResultsSyncObject)
         {
            currentEngineResults.SearchTime = searchMove.currentSearchTime;
            currentEngineResults.NodeCount = searchMove.currentNrNodes;
            currentEngineResults.QNodeCount = searchMove.currentNrQNodes;
            currentEngineResults.Depth = searchMove.currentDepth;
            currentEngineResults.TTFullPerMill = searchMove.currentTTFullPerMill;
            if (alsoNewPV)
            {
               currentEngineResults.Score = searchMove.currentScore;
               currentEngineResults.PV = new Move[searchMove.currentMoves.Length];
               currentEngineResults.DepthFinishedTime = searchMove.currentDepthFinishedTime;
               for (int i = 0; i < searchMove.currentMoves.Length; i++)
                  currentEngineResults.PV[i] = searchMove.currentMoves[i];
            }
         }

         // also propagate this to the main program. But only once twice a second
         // for use with Engine running in the main thread
         if (EngineReporting != null)
            EngineReporting(alsoNewPV);

         // for use if Engine running in a BackgroundWorker thread

         if (backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
         {
            int reportNr;
            if (alsoNewPV)
               reportNr = 1;   // full report : score, PV etc. + statistics
            else
               reportNr = 0;   // only some statistics

            // Pending_ReportProgress forces UciIO.RunWorkerCompleted to wait until UciIO.ProgressChanged has completed.
            // It is reset at the end of UciIO.ProgressChanged.
            Pending_ReportProgress = true;
            backgroundWorker.ReportProgress(reportNr, null);
         }

      }


      public EngineResults GetCurrentEngineResults()
      {
         lock (engineResultsSyncObject)
         {
            return currentEngineResults;
         }
      }

 
      public void ClearTT()
      {
         transpositionTable.Clear();
         pawnEvalTT.Clear();
         evalTT.Clear();
      }


      private bool IsCheckOrStallMate(out bool isStallMate)
      {
         // Use this only in between moves, since it's quite 'slow' (20 microseconds);
         // slow, but exact way to see if the color to move is check-mate
         // Do this with a clone, since making captures reorders the indices of pieces in PiecePos.
         // This reorders future moves. Somehow, this gives problems
         Board clone = new Board(false);
         clone.LoadFrom(board);
         Move[] moves = moveGenerator.GenerateMoves(null);
         // try all moves. If no one is valid. It's checkmate.
         bool hasValidMove = false;
         for (int i = 0; i < moveGenerator.nrGeneratedMoves; i++)
         {
            if (board.MakeMove(moves[i]))
            {
               hasValidMove = true;
            }
            board.UnMakeMove(moves[i]);
            if (hasValidMove)
               break;
         }
         board.LoadFrom(clone);
         if (!hasValidMove)
         {
            // maybe it's a stall-mate : in this case, the king is not in check
            isStallMate = !board.IsInCheck();
         }
         else
            isStallMate = false;
         return !hasValidMove;
      }


      public EndOfGameReason GetEndOfGameReason()
      {
         // check for a mate or draw. Return the reason, or None, if the game can continue.
         // Use this only in between moves, since it's quite 'slow' (20 microseconds);
         bool isStallMate;
         if (IsCheckOrStallMate(out isStallMate))
         {
            if (isStallMate)
               return EndOfGameReason.ByStallMate;
            else
               return EndOfGameReason.ByMate;
         }

         if (board.IsDrawnBy50Moves())
            return EndOfGameReason.By50Moves;

         if (board.IsDrawnBy3xRepetition())
            return EndOfGameReason.By3xRepetition;

         if (board.IsDrawnByMaterial())
            return EndOfGameReason.ByMaterial;

         return EndOfGameReason.None;
      }


   }

}
