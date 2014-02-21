//#define MeasureLazyEvalDelta
//#define KingBox1_MethodB          // Seems worse. defined : count only nr attacking pieces, not total nr of attacks.

using System;
using System.Collections.Generic;
using System.Text;
//using System.Windows.Forms;
namespace LousyChess
{

   class My_Evaluator : Evaluator
   {

      // pointers to other classes
      public MoveGenerator moveGenerator = null;        // these needed for mobility
      public MagicMoves magicMoves = null;
      public BitBoard bitboard;
      public PawnsTT pawnEvalTT;
      public EvalTT evalTT;

      #region Piece values and piece-square values and outposts

      // These are from : http://chessprogramming.wikispaces.com/CPW-Engine_eval_init

      // king, queen, rook, bishop, knight, pawn
      private const int queenValue = 975;
      private const int rookValue = 500;
      private const int bishopValue = 335;
      private const int knightValue = 325;



      /*****************************************************************
      *                           PAWN PCSQ                            *
      *                                                                *
      *  Unlike TSCP, CPW generally doesn't want to advance its pawns  *
      *  just for the fun of it. It takes into account the following:  *
      *                                                                *
      *  - file-dependent component, encouraging program to capture    *
      *    towards the center                                          *
      *  - small bonus for staying on the 2nd rank                     *
      *  - small bonus for standing on a3/h3                           *
      *  - penalty for d/e pawns on their initial squares              *
      *  - bonus for occupying the center                              *
      *****************************************************************/
      
      int[] pawn_pcsq= {
      0,   0,   0,   0,   0,   0,   0,   0,
     -6,  -4,   1,   1,   1,   1,  -4,  -6,
     -6,  -4,   1,   2,   2,   1,  -4,  -6,
     -6,  -4,   2,   8,   8,   2,  -4,  -6,
     -6,  -4,   5,  10,  10,   5,  -4,  -6,
     -4,  -4,   1,   5,   5,   1,  -4,  -4,
     -6,  -4,   1, -24, -24,   1,  -4,  -6,
      0,   0,   0,   0,   0,   0,   0,   0  };
      

      /****************************************************************
      *    KNIGHT PCSQ                                                *
      *                                                               *
      *   - centralization bonus                                      *
      *   - penalty for not being developed                           *
      ****************************************************************/
      // FdH : modified corners (a la delphi (crafty) )

      int[] knight_pcsq = {
     -15, -15,  -8,  -8,  -8,  -8, -15, -15,
     -15, -10,   0,   0,   0,   0, -10, -15,
     -8,    0,   4,   4,   4,   4,   0,  -8,
     -8,    0,   4,   8,   8,   4,   0,  -8,
     -8,    0,   4,   8,   8,   4,   0,  -8,
     -8,    0,   4,   4,   4,   4,   0,  -8,
     -15, -10,   1,   2,   2,   1, -10,  -15,
     -15, -15,  -8,  -8,  -8,  -8, -15,  -15  };


      /****************************************************************
      *                BISHOP PCSQ                                    *
      *                                                               *
      *   - centralization bonus, smaller than for knight             *
      *   - penalty for not being developed                           *
      *   - good squares on the own half of the board                 *
      ****************************************************************/
 
      int[] bishop_pcsq = {
     -15,-10,  -4,  -4,  -4,  -4, -10, -15,
     -10,  0,   0,   0,   0,   0,   0, -10,
     -4,   0,   2,   4,   4,   2,   0,  -4,
     -4,   0,   4,   6,   6,   4,   0,  -4,
     -4,   0,   4,   6,   6,   4,   0,  -4,
     -4,   1,   2,   4,   4,   2,   1,  -4,
     -10,  2,   1,   1,   1,   1,   2, -10,
     -15,-10, -12,  -4,  -4, -12, -10, -15  };


      // This one is from delphi, a la crafty
      // 7-th rank bonus is done in dynamic evaluation
  
      int[] rook_pcsq = {
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0,
           0,  0,  2,  4,  4,  2,  0,  0  };


      /****************************************************************
      *                     QUEEN PCSQ                                *
      *                                                               *
      * - small bonus for centralization                              *
      * - penalty for staying on the 1st rank, between rooks          *
      ****************************************************************/

      int[] queen_pcsq = {
      0,   0,   0,   0,   0,   0,   0,   0,
      0,   0,   1,   1,   1,   1,   0,   0,
      0,   0,   1,   2,   2,   1,   0,   0,
      0,   0,   2,   3,   3,   2,   0,   0,
      0,   0,   2,   3,   3,   2,   0,   0,
      0,   0,   1,   2,   2,   1,   0,   0,
      0,   0,   1,   1,   1,   1,   0,   0,
     -5,  -5,  -5,  -5,  -5,  -5,  -5,  -5  };

      // this is just a dummy, provided for Board.MovePiece's etc StaticPositionScore
      // The king evaluation is doen in EvaluateKing
      int[] king_pcsq = {
    	0, 0, 0, 0, 0, 0, 0, 0,
	   0, 0, 0, 0, 0, 0, 0, 0,
	   0, 0, 0, 0, 0, 0, 0, 0,
   	0, 0, 0, 0, 0, 0, 0, 0,
    	0, 0, 0, 0, 0, 0, 0, 0,
   	0, 0, 0, 0, 0, 0, 0, 0,
   	0, 0, 0, 0, 0, 0, 0, 0,
	   0, 0, 0, 0, 0, 0, 0, 0  };

      // These are from : http://chessprogramming.wikispaces.com/CPW-Engine_eval_init
      int[] king_midgame_pcsq = {
    -40, -40, -40, -40, -40, -40, -40, -40,
    -40, -40, -40, -40, -40, -40, -40, -40,
    -40, -40, -40, -40, -40, -40, -40, -40,
    -40, -40, -40, -40, -40, -40, -40, -40,
    -40, -40, -40, -40, -40, -40, -40, -40,
    -40, -40, -40, -40, -40, -40, -40, -40,
    -15, -15, -20, -20, -20, -20, -15, -15,
      0,  20,  30, -30,   0, -20,  30,  20  };
 
      int[] king_endgame_pcsq = {
      0,  10,  20,  30,  30,  20,  10,   0,
     10,  20,  30,  40,  40,  30,  20,  10,
     20,  30,  40,  50,  50,  40,  30,  20,
     30,  40,  50,  60,  60,  50,  40,  30,
     30,  40,  50,  60,  60,  50,  40,  30,
     20,  30,  40,  50,  50,  40,  30,  20,
     10,  20,  30,  40,  40,  30,  20,  10,
      0,  10,  20,  30,  30,  20,  10,   0  };
      
/*   worse
// these ones are from kiwi score.cxx
      int[] king_midgame_pcsq = {
     -30,  -30,  -40,  -50,  -50,  -40,  -30,  -30,
     -30,  -30,  -30,  -40,  -40,  -30,  -30,  -30,
     -20,  -20,  -20,  -30,  -30,  -20,  -20,  -20,
     -20,  -20,  -20,  -25,  -25,  -20,  -20,  -20,
     -20,  -20,  -20,  -20,  -20,  -20,  -20,  -20,
     -15,  -15,  -15,  -15,  -15,  -15,  -15,  -15,
       5,    5,    0,   -5,   -5,    0,    5,    5,
      20,   25,   15,    5,    5,   15,   25,   20
                                };

      int[] king_endgame_pcsq = {
     -50,  -33,  -25,  -17,  -17,  -25,  -33,  -50,
     -33,  -17,   -8,    0,    0,   -8,  -17,  -33,
     -25,   -8,    1,    9,    9,    1,   -8,  -25,
     -17,    0,    9,   18,   18,    9,    0,  -17,
     -17,    0,    9,   18,   18,    9,    0,  -17,
     -25,   -8,    1,    9,    9,    1,   -8,  -25,
     -33,  -17,   -8,    0,    0,   -8,  -17,  -33,
     -50,  -33,  -25,  -17,  -17,  -25,  -33,  -50,
                                };
*/

      // knight outpost bonus  (from crafty)
      int[] KnightOutpostBonus = {
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 1, 4, 4, 4, 4, 1, 0,
       0, 2, 6, 8, 8, 6, 2, 0,
       0, 1, 4, 4, 4, 4, 1, 0,
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0};


      // bishop outpost bonus (from crafty)
      int[] BishopOutpostBonus = {
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 1, 1, 1, 1, 0, 0,
       0, 1, 3, 3, 3, 3, 1, 0,
       0, 3, 5, 5, 5, 5, 3, 0,
       0, 1, 2, 2, 2, 2, 1, 0,
       0, 0, 0, 0, 0, 0, 0, 0,
       0, 0, 0, 0, 0, 0, 0, 0};


      #endregion

      int globalScoreStart, globalScoreEnd;

      // MeasureLazyEvalDelta must be defined for this to be effective
#if MeasureLazyEvalDelta
      public bool MeasureLazyEvalDelta = true;
#else
      public bool MeasureLazyEvalDelta = false;
#endif
      public int maxNegLazyTrueScoreDifference = 0;
      public int maxPosLazyTrueScoreDifference = 0;

      public bool UsePawnStructure = true;
      public bool UsePawnShieldStructure = true;
      public bool UseCastleInfo = true;
      public bool UseMobilityBonus = true;             // true : 57% in 237 games 5sec+0.2
      public bool UseKingBox1AttackBonus = true;

      public bool UseKingBox1DefendBonus = false;

      public bool UseKnightOutpostBonus = false;
      public bool UseBishopOutpostBonus = false;

      // add a random number from -X to +X to the score.
      public bool UseScoreNoise = false;
      public int ScoreNoise = 1;             // was 2 

      // the LazyEvalDelta MUST be less the any additional score, which could be added after the 
      // lazy evaluation. Otherwise an inaccurate score is returned.
      // Still a bit shaky, since inaccurate values are stored in the TT.
      public bool UseLazyEvaluation = false;
      public int LazyEvalDelta = 250;

      public bool UsePawnEvalTT = true;
      public bool UseEvalTT = true;

      // Some constants come from http://www.tckerrigan.com/Chess/TSCP

      int[] LostCastlingRightPenalty = { 10, 0 };   // once for QS and once for KS. This also handles blocked rook ?
      int[] HasCastledBonus = { 30, 0 };   // 30,0

      int[] DoubledPawnPenalty = { 10, 10 };   // was 10,10
      int[] IsolatedPawnPenalty = { 20, 20 };  // was 20,20
      int[] BackwardsPawnPenalty = { 8, 8 };   // was 8,8
      int[] PassedPawnBonus = { 10, 20 };      // *** 10,20  was 20,20        // * RankNr

      int[] PawnShield1Bonus = { 10, 0 };
      int[] PawnShield2Bonus = { 5, 0 };
      int[] NoPawnShieldPenalty = { 10, 0 };    // only for AB & GH files


      int[] RookOnSemiOpenFileBonus = {10, 10};
      int[] RookOnOpenFileBonus = {15, 10};         // *** 15,10  was 15,15
      int[] RookOnSeventhRankBonus = {20, 20};      // *** 15,25  was 20,20 
      int[] DoubledRookBonus = { 15, 15 };         // two rooks on the same file
      int[] ConnectedRooksBonus = { 5, 10 };         // two rooks on the same rank

      int[] TwoBishopBonus = { 50, 50 };              // ****   was 15,15

      // development :  consider only the first 16 half-moves
      const int nrOpeningMoves = 16;               // = sum of white and blacks moves
      const int QueenEarlyMovePenalty = 15;        // penalty if moved before 16th halfmove-nr
      int[] DevelopedKnights = { -15, 0, 15 };     // 0,1 or 2 developed knights
      int[] DevelopedBishops = { -10, 0, 10 };
      const int BlockedCentralPawnPenalty = 15;

      // If the calculated score is above TradePiecesScoreThreshold, award an extra
      // TradePiecesIfWinningBonus * (difference in nr of pieces).
      const int TradePiecesScoreThreshold = 200;
      const int TradePiecesIfWinningBonus = 2;    // * difference in total nr pieces

      // mobility. 
      // The constants are from Kiwi , position_evaluate.cxx
      // these constants center the mobility around 0 (if nrMoves=offset, mobilityBonus=0)
      const int QueenMobilityOffset  = 12;    // max nr moves = 21..27
      const int RookMobilityOffset   = 8;     // max nr moves = 14
      const int BishopMobilityOffset = 5;     // max nr moves = 7..13
      const int KnightMobilityOffset = 5;     // max nr moves = 2..8

      const int MobilityMultiplier = 80;     // the base value = 100, 80% seems best
      // the bonus per possible move (-offset)
      /*  these are from kiwi , position_evaluate.cxx
      int[] QueenMobilityBonus = {0, 2};
      int[] RookMobilityBonus = {1, 4};
      int[] BishopMobilityBonus = {4, 4};
      int[] KnightMobilityBonus = {3, 3};
      */
      // these are from toga
      int[] QueenMobilityBonus = { 1, 2 };
      int[] RookMobilityBonus = { 2, 4 };
      int[] BishopMobilityBonus = { 5, 5 };
      int[] KnightMobilityBonus = { 4, 4 };

      // Box around King safety : copied (more or less) from Kiwi
      int[] nrKingBox1Attacks = new int[2];     // holds the number of attacks on the other KingBox1 
      int[] nrKingBox1Defends = new int[2];     // holds the number of defends on my KingBox1 
      int[] totalKingBox1Attack = new int[2];     // holds the sum of the KingBox1Attack
      int[] totalKingBox1Defend = new int[2];     // holds the sum of the KingBox1Defend

      // the importance of the KingBox1 attack/defend :
      // scale the attack/defence on the KingBox1 : few attacks : don't care. Many attacks : Bad.
      const int MaxNrKingBox1AttacksScale = 100;
#if KingBox1_MethodB
      const int KingBox1AttackMultiplier = 225;    // count only 1 attack / piece : 275 seems best
      const int KingBox1DefendMultiplier = 200;    // count only 1 defend / piece, scaled by totalKingBox1Attack/50
      int[] NrKingBox1AttacksScale = { 0, 15, 50, 75, 90, 95, 100, 100 };    // the higher entries are also 100
#else
      const int KingBox1AttackMultiplier = 125;    // base value = 100. Best=125
      const int KingBox1DefendMultiplier = 50;     // base value = 100. Best=??
      int[] NrKingBox1AttacksScale = { 0, 15, 50, 75, 90, 95, 100, 100 };    // the higher entries are also 100
#endif

      // the relative weight of the attack/defence of the piece on the KingBox1
      const int QueenKingBox1Attack = 16;
      const int RookKingBox1Attack = 8;
      const int BishopKingBox1Attack = 4;
      const int KnightKingBox1Attack = 4;
      const int PawnKingBox1Attack = 2;


      #region stuff needed for pawn structure evaluation

      // The value of rankNrFromStartOfLeastAdvancedPawnOnFile[,]
      // if no pawn of the specified color is present.
      const int absRankOfNoPawnOnFile = 100;

      // For each file : the rank-nr  of the least advanced pawn
      // This is calculated from the color's start position : white from 0, black from 7
      int[,] absRankNrOfLeastAdvancedPawnOnFile = new int[2, 8];

      // The bitboards with all squares attacked by the pawns (not e.p.!)
      ulong[] PawnAttackBB = new ulong[2];

      #endregion


      // ************************************************************************************
      // *****   These 3 are used to gradually change scores from opening to endgame   ******

      // The summed material score for both sides at the initial position, excluding the kings
      int initialMaterialScores = 0;

      // Roughly the summed material score of both sides at the start of the end-game (excluding kings)
      int endGameMaterialScores = 2 * 1300;

      // This number runs from 0.0 (start of opening) to 1.0 (start of endgame).
      // It is is based on the MaterialScore
      double gameStage;

      // ************************************************************************************

      public My_Evaluator() : base()
      {
         // setup the static score
         SetStaticMaterialScore(queenValue, rookValue, bishopValue, knightValue);
         // NB : the king_pcsq contains only 0's. The evaluation is done in Evaluator
         SetStaticPositionalScore(king_pcsq, queen_pcsq, rook_pcsq, bishop_pcsq, knight_pcsq, pawn_pcsq);

         int initialSingleSideMaterialScore =
            PieceValues[Const.QueenID]
            + 2 * PieceValues[Const.RookID] + 2 * PieceValues[Const.BishopID] + 2 * PieceValues[Const.KnightID]
            + 8 * PieceValues[Const.PawnID];

         initialMaterialScores = 2 * initialSingleSideMaterialScore;
      }

      /*
      public override int GetGameStage()
      {
         // from ExChess Score.cpp
         int totalNrBWPawns = board.NrPieces[Const.White, Const.PawnID] + board.NrPieces[Const.Black, Const.PawnID];
         int totalNrBWPieces = board.TotalNrPieces[Const.White] + board.TotalNrPieces[Const.Black];
         int totalNrMajorBWPieces = totalNrBWPieces - totalNrBWPawns;
         //
         if (totalNrBWPawns > 13 && totalNrMajorBWPieces > 13)
            return Const.Opening;
    //     else if (totalNrBWPawns > 8 && totalNrMajorBWPieces > 8)
    //        return Const.EarlyMidGame;
    //     else if (totalNrBWPawns > 6 && totalNrMajorBWPieces > 6)
    //        return Const.LateMidGame;
         else if (totalNrBWPawns > 7 && totalNrMajorBWPieces > 7)
            return Const.MidGame;
         else
            return Const.EndGame;
      }
      */

      private void CalculateGameStage()
      {
         // See how far we are in the mid-game = how near to the end-game.
         // The midGameStageProgress[] runs from 0 (Start of Opening) to 1.0 (start of EndGame).
         double materialScoresWithoutKings = board.StaticMaterialScore[Const.White] + board.StaticMaterialScore[Const.Black]
                                           - 2 * PieceValues[Const.KingID];
         if (materialScoresWithoutKings >= endGameMaterialScores)
            gameStage = 1.0 -  (materialScoresWithoutKings - endGameMaterialScores)
                             / (initialMaterialScores - endGameMaterialScores);
         else
            gameStage = 1.0;
      }


      private int CalcStagedScore(int scoreStart, int scoreEnd)
      {
         return (int)Math.Round((1.0 - gameStage) * scoreStart + gameStage * scoreEnd);
      }


      public override int GetFastEvaluation()
      {
         int sw0, sb0, sw1, sb1;

         if (board.IsPracticallyDrawn())
            return 0;                  // check for 50-move rule ,3x repetition & not enough material


         // initialize the scores
         globalScoreStart = 0;
         globalScoreEnd = 0;

         // sets gameStage : runs from 0.0 at start of game , 1.0 at start of endgame
         CalculateGameStage();

         // Material scores
         globalScoreStart += board.StaticMaterialScore[Const.White] - board.StaticMaterialScore[Const.Black];
         globalScoreEnd += board.StaticMaterialScore[Const.White] - board.StaticMaterialScore[Const.Black];
         // The king positional score is not includes in Board.StaticPositionalScore.
         // It depends on the game stage
         int whiteKingPos = flip[board.PiecePos[Const.White, Const.KingID, 0]];
         int blackKingPos = board.PiecePos[Const.Black, Const.KingID, 0];
         globalScoreStart += king_midgame_pcsq[whiteKingPos] - king_midgame_pcsq[blackKingPos];
         globalScoreEnd   += king_endgame_pcsq[whiteKingPos] - king_endgame_pcsq[blackKingPos];

         // piece-square values
         globalScoreStart += board.StaticPositionalScore[Const.White] - board.StaticPositionalScore[Const.Black];
         globalScoreEnd += board.StaticPositionalScore[Const.White] - board.StaticPositionalScore[Const.Black];

         // pawn evaluation 
         int pawnScoreStart = 0;
         int pawnScoreEnd = 0;

         if (UsePawnEvalTT)
         {
            int index = pawnEvalTT.GetIndex(board.bwPawnsHashValue);
            // Use (index > 0) , since no pawns gives hash(=0) equal to not initialized(=0).
            // This will give an index of 0. Treat it as not found.
            if (index > 0)
            {

               int[,] leastAdvancedPawnsCopy = pawnEvalTT.slots[index].LeastAdvancedPawns;    // just a pointer
               for (int i = 0; i < 2; i++)
                  for (int j = 0; j < 8; j++)
                     absRankNrOfLeastAdvancedPawnOnFile[i, j] = leastAdvancedPawnsCopy[i, j];

               if (UsePawnStructure)
               {
                  pawnScoreStart += pawnEvalTT.slots[index].pawnDiffScoreStart;
                  pawnScoreEnd += pawnEvalTT.slots[index].pawnDiffScoreEnd;
               }

               PawnAttackBB[0] = pawnEvalTT.slots[index].WhitePawnAttackBB;
               PawnAttackBB[1] = pawnEvalTT.slots[index].BlackPawnAttackBB;
            }
            else
            {
               InitPawnEvaluation();   // calculate least advanced pawns & PawnAttackBB's
               if (UsePawnStructure)
               {
                  EvaluatePawns(Const.White, out sw0, out sw1);
                  EvaluatePawns(Const.Black, out sb0, out sb1);
                  pawnScoreStart += sw0 - sb0;
                  pawnScoreEnd += sw1 - sb1;
               }
               // store it
               pawnEvalTT.Put(board.bwPawnsHashValue, pawnScoreStart, pawnScoreEnd, PawnAttackBB, absRankNrOfLeastAdvancedPawnOnFile);
            }
         }
         else
         {
            InitPawnEvaluation();   // calculate least advanced pawns & PawnAttackBB's
            if (UsePawnStructure)
            {
               EvaluatePawns(Const.White, out sw0, out sw1);
               EvaluatePawns(Const.Black, out sb0, out sb1);
               pawnScoreStart += sw0 - sb0;
               pawnScoreEnd += sw1 - sb1;
            }
         }

         // pawn shield before a king, when it's on rank 0,1 & on file ABC or FGH
         if (UsePawnShieldStructure)
         {
            EvaluatePawnShield(Const.White, out sw0, out sw1);
            EvaluatePawnShield(Const.Black, out sb0, out sb1);
            pawnScoreStart += sw0 - sb0;
            pawnScoreEnd += sw1 - sb1;
         }

         globalScoreStart += pawnScoreStart;
         globalScoreEnd += pawnScoreEnd;

         // queens
         EvaluateQueen(Const.White, out sw0, out sw1);
         EvaluateQueen(Const.Black, out sb0, out sb1);
         globalScoreStart += sw0 - sb0;
         globalScoreEnd += sw1 - sb1;
         // rooks
         EvaluateRook(Const.White, out sw0, out sw1);
         EvaluateRook(Const.Black, out sb0, out sb1);
         globalScoreStart += sw0 - sb0;
         globalScoreEnd += sw1 - sb1;
         // bishops
         EvaluateBishop(Const.White, out sw0, out sw1);
         EvaluateBishop(Const.Black, out sb0, out sb1);
         globalScoreStart += sw0 - sb0;
         globalScoreEnd += sw1 - sb1;
         // knights
         EvaluateKnight(Const.White, out sw0, out sw1);
         EvaluateKnight(Const.Black, out sb0, out sb1);
         globalScoreStart += sw0 - sb0;
         globalScoreEnd += sw1 - sb1;


         int score = CalcStagedScore(globalScoreStart, globalScoreEnd);
         if (score == 0)
            score = 1;          // reserve 0 for true fraws

         // Return always : higher is better.
         if (board.colorToMove == Const.White)
            return score;
         else
            return -score;
      }


      public override int GetEvaluation(int alpha, int beta)
      {
         int score;
         int sw0, sw1, sb0, sb1;

         if (UseEvalTT)
         {

            if (board.IsPracticallyDrawn())
               return 0;                  // check for 50-move rule ,3x repetition & not enough material

            int index = evalTT.Get(board.HashValue);
            if (index > 0)
            {
               // found it. Now add the stuff which is not stored in evalTT
               globalScoreStart = evalTT.slots[index].scoreStart;
               globalScoreEnd = evalTT.slots[index].scoreEnd;
               EvaluateNotInEvalTT(Const.White, out sw0, out sw1);
               EvaluateNotInEvalTT(Const.Black, out sb0, out sb1);
               globalScoreStart += sw0 - sb0;
               globalScoreEnd += sw1 - sb1;

               CalculateGameStage();
               score = CalcStagedScore(globalScoreStart, globalScoreEnd);

               if (UseScoreNoise)
                  score += GetScoreNoise(ScoreNoise);
               
               if (score == 0)
                  score = 1;            // return 0 for a true draw

               // Return always : higher is better.
               if (board.colorToMove == Const.White)
                  return score;
               else
                  return -score;
            }
         }


         // First a quick evaluation.
         // This calculates game stage, material, static positional score, pawn stuff, pawn shield.
         // Excludes Mobility, KingBox1 attack, castling info, development
         score = GetFastEvaluation();

         if (score == 0)
            return 0;          // draw detected in GetFastEvaluation


 
#if MeasureLazyEvalDelta         
         if (board.colorToMove == Const.Black)
            score = -score;                            // undo the fast eval side switch     
         int lazyEvalScore = score;
#else
         if (UseLazyEvaluation)
         {
            // the returned score is not accurate, but is far outside the alpa-beta window,
            // so it is either ignored (<alpha) or produces a cutoff anyway (>beta)
            
            if (board.colorToMove == Const.Black)
               score = -score;                            // undo the fast eval side switch     
            if (score < alpha - LazyEvalDelta || score > beta + LazyEvalDelta)
            {
               if (UseScoreNoise)
                  score += GetScoreNoise(ScoreNoise);

               if (board.colorToMove == Const.White)
                  return score;
               else
                  return -score;
            }
             
         }
#endif

         // Full evaluation
         // scoreStart and scoreEnd have been initialized in LazyEval


         // Mobility
         if (UseMobilityBonus)
         {
            EvaluateMobility(Const.White, out sw0, out sw1);
            EvaluateMobility(Const.Black, out sb0, out sb1);
            globalScoreStart += sw0 - sb0;
            globalScoreEnd += sw1 - sb1;
            if (UseKingBox1DefendBonus)
            {
               globalScoreStart += totalKingBox1Defend[Const.White] - totalKingBox1Defend[Const.Black];
               globalScoreEnd += totalKingBox1Defend[Const.White] - totalKingBox1Defend[Const.Black];
            }
         }

         if (UseEvalTT)
         {
            evalTT.Put(board.HashValue, globalScoreStart, globalScoreEnd);
         }

         // the rest is stuff which is not found in EvalTT   

         EvaluateNotInEvalTT(Const.White, out sw0, out sw1);
         EvaluateNotInEvalTT(Const.Black, out sb0, out sb1);
         globalScoreStart += sw0 - sb0;
         globalScoreEnd += sw1 - sb1;

         score = CalcStagedScore(globalScoreStart, globalScoreEnd);

         if (UseScoreNoise)
            score += GetScoreNoise(ScoreNoise);

         if (score == 0)
            score = 1;            // return 0 for a true draw


#if MeasureLazyEvalDelta
         // lazy-eval delta test
         int lazyTrueScoreDifference = score - lazyEvalScore;
         if (lazyTrueScoreDifference < 0)
         {
            if (lazyTrueScoreDifference < maxNegLazyTrueScoreDifference)
               maxNegLazyTrueScoreDifference = lazyTrueScoreDifference;
         }
         else
         {
            if (lazyTrueScoreDifference > maxPosLazyTrueScoreDifference)
               maxPosLazyTrueScoreDifference = lazyTrueScoreDifference;
         }
#endif

         // Return always : higher is better.
         if (board.colorToMove == Const.White)
            return score;
         else
            return -score;
      }



 
      #region pawns

      private void InitPawnEvaluation()
      {
         // Generate an array for each color with the rank of the least advanced pawn on each file.
         // This number starts from the original colors side (white 0, black 7).
         for (int color = 0; color < Const.NrColors; color++)
         {
            for (int i = 0; i < 8; i++)
               absRankNrOfLeastAdvancedPawnOnFile[color, i] = absRankOfNoPawnOnFile;
            for (int i = 0; i < board.NrPieces[color, Const.PawnID]; i++)
            {
               int position = board.PiecePos[color, Const.PawnID, i];
               int file = position % 8;
               int rankNrFromStart;
               if (color == Const.White)
                  rankNrFromStart = position / 8;
               else
                  rankNrFromStart = 7 - position / 8;
               if (rankNrFromStart < absRankNrOfLeastAdvancedPawnOnFile[color, file])
                  absRankNrOfLeastAdvancedPawnOnFile[color, file] = rankNrFromStart;
            }
         }
         // calculate the pawn attack BB's
         ulong pawnBB = board.pieceBB[Const.White, Const.PawnID];
         PawnAttackBB[Const.White] = ((pawnBB & bitboard.FileBB0[0]) << 7) | ((pawnBB & bitboard.FileBB0[7]) << 9);
         pawnBB = board.pieceBB[Const.Black, Const.PawnID];
         PawnAttackBB[Const.Black] = ((pawnBB & bitboard.FileBB0[0]) >> 9) | ((pawnBB & bitboard.FileBB0[7]) >> 7);
      }


      public void EvaluatePawns(int thisColor, out int resultStart, out int resultEnd)
      {
         // pawn structure evaluation (doubled, isolated, backwards, passed);
         resultStart = 0;
         resultEnd = 0;
         int otherColor;
         if (thisColor == Const.White)
            otherColor = Const.Black;
         else
            otherColor = Const.White;

         for (int i = 0; i < board.NrPieces[thisColor, Const.PawnID]; i++)
         {
            int position = board.PiecePos[thisColor, Const.PawnID, i];
            int file = position % 8;
            int rankNrFromStart = position / 8;
            if (thisColor == Const.Black)
               rankNrFromStart = 7 - rankNrFromStart;
            int rankNrFromEnd = 7 - rankNrFromStart;      // used for passed pawns

            // check for doubled pawns. nb : 3 pawns on a file just generates 2*Doubled_Pawn_Penalty
            if (rankNrFromStart > absRankNrOfLeastAdvancedPawnOnFile[thisColor, file])
            {
               resultStart -= DoubledPawnPenalty[0];
               resultEnd -= DoubledPawnPenalty[1];
            }

            // Checks for  Isolated, Backward and passed pawns
            if (file == 0)
            {
               // check for isolated pawns ( no pawn on either sides file)
               if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 1] == absRankOfNoPawnOnFile)
               {
                  resultStart -= IsolatedPawnPenalty[0];
                  resultEnd -= IsolatedPawnPenalty[1];
               }
               // check for backward pawns (pawns on either sides file are more advanced)
               else if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 1] > rankNrFromStart)
               {
                  resultStart -= BackwardsPawnPenalty[0];
                  resultEnd -= BackwardsPawnPenalty[1];
               }
               // check for passed pawn
               if (rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 0]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 1])
               {
                  resultStart += rankNrFromStart * PassedPawnBonus[0];
                  resultEnd += rankNrFromStart * PassedPawnBonus[1];
               }
            }
            else if (file == 7)
            {
               // check for isolated pawns ( no pawn on either sides file)
               if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 6] == absRankOfNoPawnOnFile)
               {
                  resultStart -= IsolatedPawnPenalty[0];
                  resultEnd -= IsolatedPawnPenalty[1];
               }
               // check for backward pawns (pawns on either sides file are more advanced)
               else if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 6] > rankNrFromStart)
               {
                  resultStart -= BackwardsPawnPenalty[0];
                  resultEnd -= BackwardsPawnPenalty[1];
               }
               // check for passed pawn
               if (rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 7]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 6])
               {
                  resultStart += rankNrFromStart * PassedPawnBonus[0];
                  resultEnd += rankNrFromStart * PassedPawnBonus[1];
               }
            }
            else
            {
               // check for isolated pawns ( no pawn on either sides file)
               if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, file - 1] == absRankOfNoPawnOnFile
                  && absRankNrOfLeastAdvancedPawnOnFile[thisColor, file + 1] == absRankOfNoPawnOnFile)
               {
                  resultStart -= IsolatedPawnPenalty[0];
                  resultEnd -= IsolatedPawnPenalty[1];
               }
               // check for backward pawns (pawns on either sides file are more advanced)
               else if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, file - 1] > rankNrFromStart
                        && absRankNrOfLeastAdvancedPawnOnFile[thisColor, file + 1] > rankNrFromStart)
               {
                  resultStart -= BackwardsPawnPenalty[0];
                  resultEnd -= BackwardsPawnPenalty[1];
               }
               // check for passed pawn
               if (rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, file - 1]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, file]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, file + 1])
               {
                  resultStart += rankNrFromStart * PassedPawnBonus[0];
                  resultEnd += rankNrFromStart * PassedPawnBonus[1];
               }
            }
         }
      }


      private void EvaluatePawnShield(int color, out int resultStart, out int resultEnd)
      {
         // The maximum score = 30. the minimum = -10.
         // if the king is on rank 0 or 1 and at file ABC or FGH, evaluate the pawns right before the king
         resultStart = 0;
         resultEnd = 0;
         int position = board.PiecePos[color, Const.KingID, 0];
         // 
         int rankNrFromStart = position / 8;
         if (color == Const.Black)
            rankNrFromStart = 7 - rankNrFromStart;
         int file = position % 8;
         // only score if king is at rank 0,1 and file ABC or FGH
         if (rankNrFromStart > 1 || file == 3 || file == 4)
            return;
         int rankPlus1 = rankNrFromStart + 1;
         if (file <= 2)
         {
            // A-file
            int leastAdvPawnRank = absRankNrOfLeastAdvancedPawnOnFile[color, 0];
            if (leastAdvPawnRank == rankPlus1)
            {
               resultStart += PawnShield1Bonus[0];
               resultEnd += PawnShield1Bonus[1];
            }
            else if (leastAdvPawnRank == rankPlus1 + 1)
            {
               resultStart += PawnShield2Bonus[0];
               resultEnd += PawnShield2Bonus[1];
            }
            else
            {
               resultStart -= NoPawnShieldPenalty[0];
               resultEnd -= NoPawnShieldPenalty[1];
            }
            // B-file
            leastAdvPawnRank = absRankNrOfLeastAdvancedPawnOnFile[color, 1];
            if (leastAdvPawnRank == rankPlus1)
            {
               resultStart += PawnShield1Bonus[0];
               resultEnd += PawnShield1Bonus[1];
            }
            else if (leastAdvPawnRank == rankPlus1 + 1)
            {
               resultStart += PawnShield2Bonus[0];
               resultEnd += PawnShield2Bonus[1];
            }
            else
            {
               resultStart -= NoPawnShieldPenalty[0];
               resultEnd -= NoPawnShieldPenalty[1];
            }
            // C-file
            leastAdvPawnRank = absRankNrOfLeastAdvancedPawnOnFile[color, 2];
            if (leastAdvPawnRank == rankPlus1)
            {
               resultStart += PawnShield1Bonus[0];
               resultEnd += PawnShield1Bonus[1];
            }
            else if (leastAdvPawnRank == rankPlus1 + 1)
            {
               resultStart += PawnShield2Bonus[0];
               resultEnd += PawnShield2Bonus[1];
            }
         }
         else if (file >= 5)
         {
            // F-file
            int leastAdvPawnRank = absRankNrOfLeastAdvancedPawnOnFile[color, 5];
            if (leastAdvPawnRank == rankPlus1)
            {
               resultStart += PawnShield1Bonus[0];
               resultEnd += PawnShield1Bonus[1];
            }
            else if (leastAdvPawnRank == rankPlus1 + 1)
            {
               resultStart += PawnShield2Bonus[0];
               resultEnd += PawnShield2Bonus[1];
            }
            // G-file
            leastAdvPawnRank = absRankNrOfLeastAdvancedPawnOnFile[color, 6];
            if (leastAdvPawnRank == rankPlus1)
            {
               resultStart += PawnShield1Bonus[0];
               resultEnd += PawnShield1Bonus[1];
            }
            else if (leastAdvPawnRank == rankPlus1 + 1)
            {
               resultStart += PawnShield2Bonus[0];
               resultEnd += PawnShield2Bonus[1];
            }
            else
            {
               resultStart -= NoPawnShieldPenalty[0];
               resultEnd -= NoPawnShieldPenalty[1];
            }
            // H-file
            leastAdvPawnRank = absRankNrOfLeastAdvancedPawnOnFile[color, 7];
            if (leastAdvPawnRank == rankPlus1)
            {
               resultStart += PawnShield1Bonus[0];
               resultEnd += PawnShield1Bonus[1];
            }
            else if (leastAdvPawnRank == rankPlus1 + 1)
            {
               resultStart += PawnShield2Bonus[0];
               resultEnd += PawnShield2Bonus[1];
            }
            else
            {
               resultStart -= NoPawnShieldPenalty[0];
               resultEnd -= NoPawnShieldPenalty[1];
            }
         }
      }

      #endregion


      #region evaluate Queen, Rook, Bishop, Knight

      private void EvaluateQueen(int color, out int resultStart, out int resultEnd)
      {
         // sofar, nothing yet
         resultStart = 0;
         resultEnd = 0;
      }


      private void EvaluateRook(int color, out int resultStart, out int resultEnd)
      {
         // rook evaluation ( open file, semi open file, on 7th rank);
         resultStart = 0;
         resultEnd = 0;
         int otherColor;
         if (color == Const.White)
            otherColor = Const.Black;
         else
            otherColor = Const.White;
         int nrRooks = board.NrPieces[color, Const.RookID];
         for (int i = 0; i <nrRooks ; i++)
         {
            int position = board.PiecePos[color, Const.RookID, i];
            int file = position % 8;
            int rankNrFromStart = position / 8;
            if (color == Const.Black)
               rankNrFromStart = 7 - rankNrFromStart;
            // open & semi-open file
            if (absRankNrOfLeastAdvancedPawnOnFile[color, file] == absRankOfNoPawnOnFile)
            {
               // the colors own pawn is not present. So open or semi-open
               if (absRankNrOfLeastAdvancedPawnOnFile[otherColor, file] == absRankOfNoPawnOnFile)
               {
                  resultStart += RookOnOpenFileBonus[0];
                  resultEnd += RookOnOpenFileBonus[1];
               }
               else
               {
                  resultStart += RookOnSemiOpenFileBonus[0];
                  resultEnd += RookOnSemiOpenFileBonus[1];
               }
            }
            if (rankNrFromStart == 6)
            {
               resultStart += RookOnSeventhRankBonus[0];
               resultEnd += RookOnSeventhRankBonus[1];
            }
         }
         // 2 rooks on the same file (for speed : if >2 rooks are present (unlikely), only consider first 2 )
         if (nrRooks >= 2)
         {
            // 2 rooks on the same file
            if (board.PiecePos[color, Const.RookID, 0] % 8 == board.PiecePos[color, Const.RookID, 1] % 8)
            {
               resultStart += DoubledRookBonus[0];
               resultEnd += DoubledRookBonus[1];
            }
            // 2 rooks on the same rank (mostly middle,end game)
            if (board.PiecePos[color, Const.RookID, 0] / 8 == board.PiecePos[color, Const.RookID, 1] / 8)
            {
               resultStart += ConnectedRooksBonus[0];
               resultEnd += ConnectedRooksBonus[1];
            }
         }
      }


      private void EvaluateBishop(int color, out int resultStart, out int resultEnd)
      {
         resultStart = 0;
         resultEnd = 0;
         if (board.NrPieces[color, Const.BishopID] >= 2)
         {
            resultStart += TwoBishopBonus[0];
            resultEnd += TwoBishopBonus[1];
         }
         if (UseBishopOutpostBonus)
         {
            int nrBishops = board.NrPieces[color, Const.BishopID];
            for (int i = 0; i < nrBishops; i++)
            {
               int position = board.PiecePos[color, Const.BishopID, i];
               int outpostScore;
               if (color == Const.White)
                  outpostScore = BishopOutpostBonus[flip[position]];
               else
                  outpostScore = BishopOutpostBonus[position];
               if (outpostScore > 0)
               {
                  int outpostResult = 0;
                  int otherColor = color ^ 1;
                  // can it be driven away by an enemy pawn :
                  bool isOutpost = (bitboard.FilesLeftRightInFrontBB1[color, position] & board.pieceBB[otherColor, Const.PawnID]) == 0;
                  if (isOutpost)
                  {
                     // so, it's a true outpost
                     outpostResult += outpostScore;
                     // is it defended by 1 of my pawns ?
                     if ((bitboard.PawnAttackBB1[otherColor, position] & board.pieceBB[color, Const.PawnID]) != 0)
                     {
                        outpostResult += outpostScore / 2;
                        // can it be driven away by a minor enemy piece ?
                        if (board.NrPieces[otherColor, Const.KnightID] == 0)
                        {
                           int nrEnemyBishops = board.NrPieces[otherColor, Const.BishopID];
                           if (nrEnemyBishops == 0)
                              outpostResult += outpostScore;
                           else if (nrEnemyBishops == 1)
                           {
                              if (ColorOfSquare[position] != ColorOfSquare[board.PiecePos[otherColor, Const.BishopID, 0]])
                                 outpostResult += outpostScore;
                           }
                        }
                     }
                     resultStart += outpostResult;
                     resultEnd += outpostResult;
                  }
               }
            }
         }

      }


      private void EvaluateKnight(int color, out int resultStart, out int resultEnd)
      {
         resultStart = 0;
         resultEnd = 0;
         if (UseKnightOutpostBonus)
         {
            int nrKnights = board.NrPieces[color, Const.KnightID];
            for (int i = 0; i < nrKnights; i++)
            {
               int position = board.PiecePos[color, Const.KnightID, i];
               int outpostScore;
               if (color == Const.White)
                  outpostScore = KnightOutpostBonus[flip[position]];
               else
                  outpostScore = KnightOutpostBonus[position];
               if (outpostScore > 0)
               {
                  int outpostResult = 0;
                  int otherColor = color ^ 1;
                  // can it be driven away by an enemy pawn :
                  bool isOutpost = (bitboard.FilesLeftRightInFrontBB1[color, position] & board.pieceBB[otherColor, Const.PawnID]) == 0;
                  if (isOutpost)
                  {
                     // so, it's a true outpost
                     outpostResult += outpostScore;
                     // is it defended by 1 of my pawns ?
                     if ((bitboard.PawnAttackBB1[otherColor, position] & board.pieceBB[color, Const.PawnID]) != 0)
                     {
                        outpostResult += outpostScore / 2;
                        // can it be driven away by a minor enemy piece ?
                        if (board.NrPieces[otherColor, Const.KnightID] == 0)
                        {
                           int nrEnemyBishops = board.NrPieces[otherColor, Const.BishopID];
                           if (nrEnemyBishops == 0)
                              outpostResult += outpostScore;
                           else if (nrEnemyBishops == 1)
                           {
                              if (ColorOfSquare[position] != ColorOfSquare[board.PiecePos[otherColor, Const.BishopID, 0]])
                                 outpostResult += outpostScore;
                           }
                        }
                     }
                     resultStart += outpostResult;
                     resultEnd += outpostResult;
                  }
               }
            }
         }
      }

      #endregion


      #region evaluation of stuff  which do not depended solely on the board hash

      private void EvaluateNotInEvalTT(int color, out int resultStart, out int resultEnd)
      {
         // evaluation of things which do not depended solely on the board hash,
         // and so can not be gotten from the EvalTT.
         resultStart = 0;
         resultEnd = 0;
         int s0, s1;

         // castling
         EvaluateCastling(color, out s0, out s1);
         resultStart += s0;
         resultEnd += s1;

         // development
         EvaluateDevelopment(color, out s0, out s1);
         resultStart += s0;
         resultEnd += s1;
      }


      private void EvaluateCastling(int color, out int resultStart, out int resultEnd)
      {
         resultStart = 0;
         resultEnd = 0;
         // castling : give bonus if done so. give penealty if no longer possible
         if (UseCastleInfo)
         {
            if (board.hasCastled[color])
            {
               resultStart += HasCastledBonus[0];
               resultEnd += HasCastledBonus[1];
            }
            else
            {
               if (!board.canCastleKingSide[color])
               {
                  resultStart -= LostCastlingRightPenalty[0];
                  resultEnd -= LostCastlingRightPenalty[1];
               }
               if (!board.canCastleQueenSide[color])
               {
                  resultStart -= LostCastlingRightPenalty[0];
                  resultEnd -= LostCastlingRightPenalty[1];
               }
            }
         }

      }



      private void EvaluateDevelopment(int color, out int resultStart, out int resultEnd)
      {
         resultStart = 0;
         resultEnd = 0;
         // scale runs from 1.0 to 0.0 in the first 16 half-moves
         double scale = ( (double)nrOpeningMoves - board.halfMoveNr) / nrOpeningMoves;
         if (scale <= 0)
            return;

         // penalyze early queen movement
         // stimulate development of knight and bishops (knights a bit more then bishops)
         // penalyze blocking central pawns

         int result = 0;
         if (color == Const.White)
         {
            // the queen should not yet have moved
            if (board.SquareContents[3].pieceType != Const.QueenID)
               result -= QueenEarlyMovePenalty;    // ignore the color of the queen
            // count the number of developed knights
            int nrDevelopedKnights = 0;
            if (board.SquareContents[1].pieceType != Const.KnightID)
               nrDevelopedKnights++;
            if (board.SquareContents[6].pieceType != Const.KnightID)
               nrDevelopedKnights++;
            result += DevelopedKnights[nrDevelopedKnights];
            // count the number of developed bishops
            int nrDevelopedBishops = 0;
            if (board.SquareContents[2].pieceType != Const.BishopID)
               nrDevelopedBishops++;
            if (board.SquareContents[5].pieceType != Const.BishopID)
               nrDevelopedBishops++;
            result += DevelopedBishops[nrDevelopedBishops];
            // check the 2 squares directly before the 2 central pawns.
            // If the pawn has not moved and something is present ont that square, penalyze.
            if (board.SquareContents[11].pieceType == Const.PawnID
                 && board.SquareContents[19].pieceType != Const.EmptyID)
               result -= BlockedCentralPawnPenalty;
            if (board.SquareContents[12].pieceType == Const.PawnID
                 && board.SquareContents[20].pieceType != Const.EmptyID)
               result -= BlockedCentralPawnPenalty;
         }
         else
         {
            // the queen should not yet have moved
            if (board.SquareContents[59].pieceType != Const.QueenID)
               result -= QueenEarlyMovePenalty;    // ignore the color of the queen
            // count the number of developed knights
            int nrDevelopedKnights = 0;
            if (board.SquareContents[57].pieceType != Const.KnightID)
               nrDevelopedKnights++;
            if (board.SquareContents[62].pieceType != Const.KnightID)
               nrDevelopedKnights++;
            result += DevelopedKnights[nrDevelopedKnights];
            // count the number of developed bishops
            int nrDevelopedBishops = 0;
            if (board.SquareContents[58].pieceType != Const.BishopID)
               nrDevelopedBishops++;
            if (board.SquareContents[61].pieceType != Const.BishopID)
               nrDevelopedBishops++;
            result += DevelopedBishops[nrDevelopedBishops];
            // check the 2 squares directly before the 2 central pawns.
            // If the pawn has not moved and something is present ont that square, penalyze.
            if (board.SquareContents[51].pieceType == Const.PawnID
                 && board.SquareContents[43].pieceType != Const.EmptyID)
               result -= BlockedCentralPawnPenalty;
            if (board.SquareContents[52].pieceType == Const.PawnID
                 && board.SquareContents[44].pieceType != Const.EmptyID)
               result -= BlockedCentralPawnPenalty;

         }
         // scale these values gradually, to be 0 at the end of the opening
         resultStart = (int)(scale * result); ;
      }

      #endregion


      #region Mobility and KingBox1 attack

      private void EvaluateMobility(int color, out int resultStart, out int resultEnd)
      {
         resultStart = 0;
         resultEnd = 0;
         int otherColor;
         if (color == Const.White)
            otherColor = Const.Black;
         else
            otherColor = Const.White;
         // local copies
         ulong myPieces = board.pieces[color];
         ulong otherPieces = board.pieces[otherColor];
         ulong allPieces = board.allPiecesBB;

         // KingBox attacks
         int kingBox1AttackSum = 0;
         //kingBox1AttackSum[otherColor] = 0;
         nrKingBox1Attacks[otherColor] = 0;
         totalKingBox1Attack[otherColor] = 0;
         ulong otherKingBox1 = bitboard.Box1[board.PiecePos[otherColor, Const.KingID, 0]];

         // KingBox defends
         int kingBox1DefendSum = 0;
         //kingBox1DefendSum[color] = 0;
         nrKingBox1Defends[color] = 0;
         totalKingBox1Defend[color] = 0;
         ulong thisKingBox1 = bitboard.Box1[board.PiecePos[color, Const.KingID, 0]];


         // First : handle only major pieces
         ulong allMovesBB = 0;
         int nrMoves = 0;
         for (int i = Const.QueenID; i <= Const.KnightID; i++)
         {
            for (int j = 0; j < board.NrPieces[color, i]; j++)
            {
               int position = board.PiecePos[color, i, j];
               switch (i)
               {
                  case Const.QueenID:
                     allMovesBB = magicMoves.Qmagic(position, allPieces);
                     nrMoves = BitBoard.PopCount(allMovesBB & ~myPieces) - QueenMobilityOffset;
                     resultStart += nrMoves * QueenMobilityBonus[0] * MobilityMultiplier / 100;
                     resultEnd += nrMoves * QueenMobilityBonus[1] * MobilityMultiplier / 100;
                     // KingBox attack & defends
                     if (UseKingBox1AttackBonus)
                     {
#if KingBox1_MethodB
                        if ((allMovesBB & otherKingBox1) != 0) 
                        {
                           nrKingBox1Attacks[otherColor]++;
                           kingBox1AttackSum += QueenKingBox1Attack;
                        }
#else
                        int n = BitBoard.PopCount(allMovesBB & otherKingBox1);
                        nrKingBox1Attacks[otherColor] += n;
                        kingBox1AttackSum += n * QueenKingBox1Attack;
#endif
                     }
                     if (UseKingBox1DefendBonus)
                     {
                        int n = BitBoard.PopCount(allMovesBB & thisKingBox1);
#if KingBox1_MethodB
                        if (n > 0)
                           n = 1;
#endif
                        nrKingBox1Defends[color] += n;
                        kingBox1DefendSum += n * QueenKingBox1Attack;
                     }
                     break;
                  case Const.RookID:
                     allMovesBB = magicMoves.Rmagic(position, allPieces);
                     nrMoves = BitBoard.PopCount(allMovesBB & ~myPieces) - RookMobilityOffset;
                     resultStart += nrMoves * RookMobilityBonus[0] * MobilityMultiplier / 100;
                     resultEnd += nrMoves * RookMobilityBonus[1] * MobilityMultiplier / 100;
                     // KingBox attack & defends
                     if (UseKingBox1AttackBonus)
                     {
#if KingBox1_MethodB
                        if ((allMovesBB & otherKingBox1) != 0)
                        {
                           nrKingBox1Attacks[otherColor]++;
                           kingBox1AttackSum += RookKingBox1Attack;
                        }
#else
                        int n = BitBoard.PopCount(allMovesBB & otherKingBox1);
                        nrKingBox1Attacks[otherColor] += n;
                        kingBox1AttackSum += n * RookKingBox1Attack;
#endif
                     }
                     if (UseKingBox1DefendBonus)
                     {
                        int n = BitBoard.PopCount(allMovesBB & thisKingBox1);
#if KingBox1_MethodB
                        if (n > 0)
                           n = 1;
#endif
                        nrKingBox1Defends[color] += n;
                        kingBox1DefendSum += n * RookKingBox1Attack;
                     }
                     break;
                  case Const.BishopID:
                     allMovesBB = magicMoves.Bmagic(position, allPieces);
                     nrMoves = BitBoard.PopCount(allMovesBB & ~myPieces) - BishopMobilityOffset;
                     resultStart += nrMoves * BishopMobilityBonus[0] * MobilityMultiplier / 100;
                     resultEnd += nrMoves * BishopMobilityBonus[1] * MobilityMultiplier / 100;
                     // KingBox attack & defends
                     if (UseKingBox1AttackBonus)
                     {
#if KingBox1_MethodB
                        if ((allMovesBB & otherKingBox1) != 0)
                        {
                           nrKingBox1Attacks[otherColor]++;
                           kingBox1AttackSum += BishopKingBox1Attack;
                        }
#else
                        int n = BitBoard.PopCount(allMovesBB & otherKingBox1);
                        nrKingBox1Attacks[otherColor] += n;
                        kingBox1AttackSum += n * BishopKingBox1Attack;
#endif
                     }
                     if (UseKingBox1DefendBonus)
                     {
                        int n = BitBoard.PopCount(allMovesBB & thisKingBox1);
#if KingBox1_MethodB
                        if (n > 0)
                           n = 1;
#endif
                        nrKingBox1Defends[color] += n;
                        kingBox1DefendSum += n * BishopKingBox1Attack;
                     }
                     break;
                  case Const.KnightID:
                     allMovesBB = moveGenerator.EmptyBoardKnightMoves[position];
                     nrMoves = BitBoard.PopCount(allMovesBB & ~myPieces) - KnightMobilityOffset;
                     resultStart += nrMoves * KnightMobilityBonus[0] * MobilityMultiplier / 100;
                     resultEnd += nrMoves * KnightMobilityBonus[1] * MobilityMultiplier / 100;
                     // KingBox attack & defends
                     if (UseKingBox1AttackBonus)
                     {
#if KingBox1_MethodB
                        if ((allMovesBB & otherKingBox1) != 0)
                        {
                           nrKingBox1Attacks[otherColor]++;
                           kingBox1AttackSum += KnightKingBox1Attack;
                        }
#else
                        int n = BitBoard.PopCount(allMovesBB & otherKingBox1);
                        nrKingBox1Attacks[otherColor] += n;
                        kingBox1AttackSum += n * KnightKingBox1Attack;
#endif
                     }
                     if (UseKingBox1DefendBonus)
                     {
                        int n = BitBoard.PopCount(allMovesBB & thisKingBox1);
#if KingBox1_MethodB
                        if (n > 0)
                           n = 1;
#endif
                        nrKingBox1Defends[color] += n;
                        kingBox1DefendSum += n * KnightKingBox1Attack;
                     }
                     break;
               }
            }
         }
         if (UseKingBox1AttackBonus)
         {
            // first calculate the pawn attacks on the KingBox1
            int nn = BitBoard.PopCount(PawnAttackBB[color] & otherKingBox1);
            nrKingBox1Attacks[otherColor] += nn;
            kingBox1AttackSum += nn * PawnKingBox1Attack;
            // calculate the entire bonus
            int n = nrKingBox1Attacks[otherColor];
            if (n >= NrKingBox1AttacksScale.Length)
               totalKingBox1Attack[otherColor] = kingBox1AttackSum * KingBox1AttackMultiplier / 100;
            else
               totalKingBox1Attack[otherColor] = kingBox1AttackSum * NrKingBox1AttacksScale[n] * KingBox1AttackMultiplier
                         / MaxNrKingBox1AttacksScale / 100;
            resultStart += totalKingBox1Attack[otherColor];
            resultEnd += totalKingBox1Attack[otherColor];
         }
         if (UseKingBox1DefendBonus)
         {
            // calculate the entire bonus
            int n = nrKingBox1Defends[color];
            /*
            if (n >= NrKingBox1AttacksScale.Length)
               totalKingBox1Defend[color] = kingBox1DefendSum * KingBox1DefendMultiplier / 100;
            else
               totalKingBox1Defend[color] = kingBox1DefendSum * NrKingBox1AttacksScale[n] * KingBox1DefendMultiplier
                         / MaxNrKingBox1AttacksScale / 100;
             */
            if (n >= NrKingBox1AttacksScale.Length)
               totalKingBox1Defend[color] = kingBox1DefendSum;
            else
               totalKingBox1Defend[color] = kingBox1DefendSum * NrKingBox1AttacksScale[n] / MaxNrKingBox1AttacksScale;
            if (color == Const.Black)
            {
               // if color = black, it means both white and blacks attack & defends have been calculated.
               // now scale the totalKingBox1Defend, depending on how much the KingBox is attacked
               // the 50 is something like the maximum possible attack score
               totalKingBox1Defend[Const.White] = totalKingBox1Defend[Const.White] * KingBox1DefendMultiplier
                                                   * totalKingBox1Attack[Const.White] / 50 / 100;
               totalKingBox1Defend[Const.Black] = totalKingBox1Defend[Const.Black] * KingBox1DefendMultiplier
                                                   * totalKingBox1Attack[Const.Black] / 50 / 100;
               // don't add it to the score yet. do this later
             //  resultStart += totalKingBox1Defend[color];
            //   resultEnd += totalKingBox1Defend[color];
            }
         }
      }

      #endregion


      #region Show detailed evaluation

      public string[] ShowDetailedEvaluation()
      {
         int sw0, sw1, sb0, sb1, sw, sb;
         int sum = 0;
         string[] staticScoreStr = new string[Const.PawnID+1];
         List<string> result = new List<string>();
         CalculateGameStage();
         globalScoreStart = 0;
         globalScoreEnd = 0;

         // king positional score
         int whiteKingPos = flip[board.PiecePos[Const.White, Const.KingID, 0]];
         int blackKingPos = board.PiecePos[Const.Black, Const.KingID, 0];
         sw0 = king_midgame_pcsq[whiteKingPos];
         sw1 = king_endgame_pcsq[whiteKingPos];
         sb0 = king_midgame_pcsq[blackKingPos];
         sb1 = king_endgame_pcsq[blackKingPos];
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         staticScoreStr[Const.KingID] = "   , static: " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString();

         for (int i = 1; i <= Const.PawnID; i++)
         {
            sw = 0;
            for (int j = 0; j < board.NrPieces[0, i]; j++)
               sw += PieceSquareValues[0][i][board.PiecePos[0, i, j]];
            sb = 0;
            for (int j = 0; j < board.NrPieces[1, i]; j++)
               sb += PieceSquareValues[1][i][board.PiecePos[1, i, j]];
            sum += sw - sb;
            staticScoreStr[i] = "   , static: " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString();
         }

         // initialize least advanced pawn per file.
         // Calculate always, since it is also used for rook-files etc.
         InitPawnEvaluation();

         // pawn structure evaluation (doubled, isolated, backwards, passed);


         EvaluatePawns(Const.White, out sw0, out sw1);
         EvaluatePawns(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("PAWN structure: " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + staticScoreStr[Const.PawnID] + Environment.NewLine);
         result.AddRange(ShowDetailedPawnStructEvaluation(Const.White));
         result.AddRange(ShowDetailedPawnStructEvaluation(Const.Black));

         // knights
         EvaluateKnight(Const.White, out sw0, out sw1);
         EvaluateKnight(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("KNIGHT: " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + staticScoreStr[Const.KnightID] + Environment.NewLine);

         // bishops
         EvaluateBishop(Const.White, out sw0, out sw1);
         EvaluateBishop(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("BISHOP: " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + staticScoreStr[Const.BishopID] + Environment.NewLine);

         // rooks
         EvaluateRook(Const.White, out sw0, out sw1);
         EvaluateRook(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("ROOK  : " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + staticScoreStr[Const.RookID] + Environment.NewLine);

         // queens
         EvaluateQueen(Const.White, out sw0, out sw1);
         EvaluateQueen(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("QUEEN : " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + staticScoreStr[Const.QueenID] + Environment.NewLine);

         
         // king 
         result.Add(Environment.NewLine);
         EvaluateCastling(Const.White, out sw0, out sw1);
         EvaluateCastling(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("KING : " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + staticScoreStr[Const.KingID] + Environment.NewLine);
         //   result.Add("KING  : " + staticScoreStr[Const.KingID] + Environment.NewLine);

         // pawn shield before a king, when it's on rank 0,1 & on file ABC or FGH
         EvaluatePawnShield(Const.White, out sw0, out sw1);
         EvaluatePawnShield(Const.Black, out sb0, out sb1);
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("Pawn shield " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + Environment.NewLine);

         // mobility
         int TMsw0, TMsw1, TMsb0, TMsb1;       // the total mobility scores

         EvaluateMobility(Const.White, out TMsw0, out TMsw1);
         EvaluateMobility(Const.Black, out TMsb0, out TMsb1);


         // attack around other king
         if (UseKingBox1AttackBonus)
         {
            sw0 = totalKingBox1Attack[Const.Black];
            sw1 = totalKingBox1Attack[Const.Black];
            sb0 = totalKingBox1Attack[Const.White];
            sb1 = totalKingBox1Attack[Const.White];
            sw = CalcStagedScore(sw0, sw1);
            sb = CalcStagedScore(sb0, sb1);
            sum += sw - sb;
            result.Add("KingBox attack bonus " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + Environment.NewLine);
         }

         // defence around my king
         if (UseKingBox1DefendBonus)
         {
            sw0 = totalKingBox1Defend[Const.White];
            sw1 = totalKingBox1Defend[Const.White];
            sb0 = totalKingBox1Defend[Const.Black];
            sb1 = totalKingBox1Defend[Const.Black];
            sw = CalcStagedScore(sw0, sw1);
            sb = CalcStagedScore(sb0, sb1);
            sum += sw - sb;
            result.Add("KingBox defend bonus " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + Environment.NewLine);
         }


         // now the mobility (subtract KingBox1 attack & defence)
         /*
         sw0 = TMsw0 - totalKingBox1Attack[Const.Black] - totalKingBox1Defend[Const.White];
         sw1 = TMsw1 - totalKingBox1Attack[Const.Black] - totalKingBox1Defend[Const.White];
         sb0 = TMsb0 - totalKingBox1Attack[Const.White] - totalKingBox1Defend[Const.Black];
         sb1 = TMsb1 - totalKingBox1Attack[Const.White] - totalKingBox1Defend[Const.Black];
         */

         // now the mobility (subtract KingBox1 attacke)         
         sw0 = TMsw0 - totalKingBox1Attack[Const.Black];
         sw1 = TMsw1 - totalKingBox1Attack[Const.Black];
         sb0 = TMsb0 - totalKingBox1Attack[Const.White];
         sb1 = TMsb1 - totalKingBox1Attack[Const.White];
         


         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("MOBILITY " + sw.ToString() + "   " + sb.ToString() + "   " + (sw - sb).ToString() + Environment.NewLine);


         result.Add(Environment.NewLine);
         sw0 = board.StaticMaterialScore[Const.White];
         sw1 = board.StaticMaterialScore[Const.White];
         sb0 = board.StaticMaterialScore[Const.Black];
         sb1 = board.StaticMaterialScore[Const.Black];
         sw = CalcStagedScore(sw0, sw1);
         sb = CalcStagedScore(sb0, sb1);
         sum += sw - sb;
         result.Add("material    " + (sw - sb).ToString() + Environment.NewLine);
         result.Add("positional  " + (sum - (sw - sb)).ToString() + Environment.NewLine);

         result.Add("Total score " + sum.ToString() + Environment.NewLine);

         return result.ToArray();
      }

      
      public string[] ShowDetailedPawnStructEvaluation(int thisColor)
      {
         List<string> result = new List<string>();
         // pawn structure evaluation (doubled, isolated, backwards, passed);
         int[] doubledResult = {0,0};
         int[] isolatedResult = { 0, 0 };
         int[] backwardsResult = { 0, 0 };
         int[] passedResult = { 0, 0 };
         int otherColor;
         if (thisColor == Const.White)
            otherColor = Const.Black;
         else
            otherColor = Const.White;

         for (int i = 0; i < board.NrPieces[thisColor, Const.PawnID]; i++)
         {
            int position = board.PiecePos[thisColor, Const.PawnID, i];
            int file = position % 8;
            int rankNrFromStart = position / 8;
            if (thisColor == Const.Black)
               rankNrFromStart = 7 - rankNrFromStart;
            int rankNrFromEnd = 7 - rankNrFromStart;      // used for passed pawns
            // check for doubled pawns. nb : 3 pawns on a file just generates 2*Doubled_Pawn_Penalty
            if (rankNrFromStart > absRankNrOfLeastAdvancedPawnOnFile[thisColor, file])
            {
               doubledResult[0] -= DoubledPawnPenalty[0];
               doubledResult[1] -= DoubledPawnPenalty[1];
            }

            // Checks for  Isolated, Backward and passed pawns
            if (file == 0)
            {
               // check for isolated pawns ( no pawn on either sides file)
               if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 1] == absRankOfNoPawnOnFile)
               {
                  isolatedResult[0] -= IsolatedPawnPenalty[0];
                  isolatedResult[1] -= IsolatedPawnPenalty[1];
               }
               // check for backward pawns (pawns on either sides file are more advanced)
               else if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 1] > rankNrFromStart)
               {
                  backwardsResult[0] -= BackwardsPawnPenalty[0];
                  backwardsResult[1] -= BackwardsPawnPenalty[1];
               }
               // check for passed pawn
               if (rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 0]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 1])
               {
                  passedResult[0] += rankNrFromStart * PassedPawnBonus[0];
                  passedResult[1] += rankNrFromStart * PassedPawnBonus[1];
               }
            }
            else if (file == 7)
            {
               // check for isolated pawns ( no pawn on either sides file)
               if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 6] == absRankOfNoPawnOnFile)
               {
                  isolatedResult[0] -= IsolatedPawnPenalty[0];
                  isolatedResult[1] -= IsolatedPawnPenalty[1];
               }
               // check for backward pawns (pawns on either sides file are more advanced)
               else if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, 6] > rankNrFromStart)
               {
                  backwardsResult[0] -= BackwardsPawnPenalty[0];
                  backwardsResult[1] -= BackwardsPawnPenalty[1];
               }
               // check for passed pawn
               if (rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 7]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, 6])
               {
                  passedResult[0] += rankNrFromStart * PassedPawnBonus[0];
                  passedResult[1] += rankNrFromStart * PassedPawnBonus[1];
               }
            }
            else
            {
               // check for isolated pawns ( no pawn on either sides file)
               if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, file - 1] == absRankOfNoPawnOnFile
                  && absRankNrOfLeastAdvancedPawnOnFile[thisColor, file + 1] == absRankOfNoPawnOnFile)
               {
                  isolatedResult[0] -= IsolatedPawnPenalty[0];
                  isolatedResult[1] -= IsolatedPawnPenalty[1];
               }
               // check for backward pawns (pawns on either sides file are more advanced)
               else if (absRankNrOfLeastAdvancedPawnOnFile[thisColor, file - 1] > rankNrFromStart
                        && absRankNrOfLeastAdvancedPawnOnFile[thisColor, file + 1] > rankNrFromStart)
               {
                  backwardsResult[0] -= BackwardsPawnPenalty[0];
                  backwardsResult[1] -= BackwardsPawnPenalty[1];
               }
               // check for passed pawn
               if (rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, file - 1]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, file]
                  && rankNrFromEnd <= absRankNrOfLeastAdvancedPawnOnFile[otherColor, file + 1])
               {
                  passedResult[0] += rankNrFromStart * PassedPawnBonus[0];
                  passedResult[1] += rankNrFromStart * PassedPawnBonus[1];
               }
            }
         }
         if (thisColor == Const.White)
            result.Add("  White: ");
         else
            result.Add("  Black: ");
         result.Add("doubled=" + CalcStagedScore(doubledResult[0], doubledResult[1]).ToString()
                            + "  isolated=" + CalcStagedScore(isolatedResult[0], isolatedResult[1]).ToString()
                            + "  backwards=" + CalcStagedScore(backwardsResult[0], backwardsResult[1]).ToString()
                            + "  passed=" + CalcStagedScore(passedResult[0], passedResult[1]).ToString() 
                            + Environment.NewLine);
         return result.ToArray();
      }
      
      #endregion



   }
}
