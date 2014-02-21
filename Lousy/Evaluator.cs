using System;
using System.Collections.Generic;
using System.Text;

namespace LousyChess
{

   // The basic class from which all Evaluators should descend
   // The descending class MUST call SetStaticMaterialScore and SetStaticPositionalScore.

   public abstract class Evaluator
   {
      // pointers to other classes
      public Board board;

      public const int MateValue = 1000000;
      public const int FutureMate = MateValue - 10000;    // assuming no more then 10000 plies

      private const int InitialSeed = 12345;
      private Random rng = new Random(InitialSeed);
      

      // king, queen, rook, bishop, knight, pawn
      // The King and Pawn values are fixed
      public int[] PieceValues = { 50000, 0, 0, 0, 0, 100 };

      // Holds static scores for each color, PieceType and position
      // first index = color. second index is piece-type, 3rd index is square-nr
      public int[][][] PieceSquareValues;

      public const int A1 = 0, B1 = 1, C1 = 2, D1 = 3, E1 = 4, F1 = 5, G1 = 6, H1 = 7;
      public const int A2 = 8, B2 = 9, C2 = 10, D2 = 11, E2 = 12, F2 = 13, G2 = 14, H2 = 15;
      public const int A3 = 16, B3 = 17, C3 = 18, D3 = 19, E3 = 20, F3 = 21, G3 = 22, H3 = 23;
      public const int A4 = 24, B4 = 25, C4 = 26, D4 = 27, E4 = 28, F4 = 29, G4 = 30, H4 = 31;
      public const int A5 = 32, B5 = 33, C5 = 34, D5 = 35, E5 = 36, F5 = 37, G5 = 38, H5 = 39;
      public const int A6 = 40, B6 = 41, C6 = 42, D6 = 43, E6 = 44, F6 = 45, G6 = 46, H6 = 47;
      public const int A7 = 48, B7 = 49, C7 = 50, D7 = 51, E7 = 52, F7 = 53, G7 = 54, H7 = 55;
      public const int A8 = 56, B8 = 57, C8 = 58, D8 = 59, E8 = 60, F8 = 61, G8 = 62, H8 = 63;


      /* The flip array is used to calculate the piece/square
         values for WHITE pieces. The piece/square value of a
         Black pawn is pawn_pcsq[sq] and the value of a WHITE
         pawn is pawn_pcsq[flip[sq]] */
      public int[] flip = {
	 56,  57,  58,  59,  60,  61,  62,  63,
	 48,  49,  50,  51,  52,  53,  54,  55,
	 40,  41,  42,  43,  44,  45,  46,  47,
	 32,  33,  34,  35,  36,  37,  38,  39,
	 24,  25,  26,  27,  28,  29,  30,  31,
	 16,  17,  18,  19,  20,  21,  22,  23,
	  8,   9,  10,  11,  12,  13,  14,  15,
	  0,   1,   2,   3,   4,   5,   6,   7  };


       public int[] ColorOfSquare = {
       1, 0, 1, 0, 1, 0, 1, 0,
       0, 1, 0, 1, 0, 1, 0, 1,
       1, 0, 1, 0, 1, 0, 1, 0,
       0, 1, 0, 1, 0, 1, 0, 1,
       1, 0, 1, 0, 1, 0, 1, 0,
       0, 1, 0, 1, 0, 1, 0, 1,
       1, 0, 1, 0, 1, 0, 1, 0,
       0, 1, 0, 1, 0, 1, 0, 1
      };


      public Evaluator()
      {
      }


      #region setup static material and positional score values. MUST be called by descending class.

      protected void SetStaticMaterialScore(int queenValue, int rookValue, int bishopValue, int knightValue)
      {
         // Setup the static positional scores, which hold for every stage of the game.
         // This MUST be called (once) by the descending class.
         //
         // The king value is fixed at 50000
         PieceValues[Const.QueenID] = queenValue;
         PieceValues[Const.RookID] = rookValue;
         PieceValues[Const.BishopID] = bishopValue;
         PieceValues[Const.KnightID] = knightValue;
         // The pawn value is fixed at 100
      }


      protected void SetStaticPositionalScore(int[] king_pcsq,int[] queen_pcsq,int[] rook_pcsq
                                           ,int[] bishop_pcsq,int[] knight_pcsq,int[] pawn_pcsq )
      {
         // Setup the static positional scores, which hold for every stage of the game.
         // This MUST be called (once) by the descending class.
         PieceSquareValues = new int[Const.NrColors][][];
         PieceSquareValues[Const.White] = new int[Const.NrPieceTypes][];
         PieceSquareValues[Const.Black] = new int[Const.NrPieceTypes][];
         // the original tables are for black
         PieceSquareValues[Const.Black][Const.KingID] = king_pcsq;
         PieceSquareValues[Const.Black][Const.QueenID] = queen_pcsq;
         PieceSquareValues[Const.Black][Const.RookID] = rook_pcsq;
         PieceSquareValues[Const.Black][Const.BishopID] = bishop_pcsq;
         PieceSquareValues[Const.Black][Const.KnightID] = knight_pcsq;
         PieceSquareValues[Const.Black][Const.PawnID] = pawn_pcsq;
         // now generate the flipped tables for white;
         for (int i = 0; i < Const.NrPieceTypes; i++)
         {
            PieceSquareValues[Const.White][i] = new int[Const.NrSquares];
            for (int j = 0; j < Const.NrSquares; j++)
               PieceSquareValues[Const.White][i][j] = PieceSquareValues[Const.Black][i][flip[j]];
         }
      }

      #endregion


      public virtual int GetFastEvaluation()
      {
         int sum = 0;
         sum += board.StaticMaterialScore[Const.White] - board.StaticMaterialScore[Const.Black];
         // StaticPositionalScore does not contain game-stage related stuff, like the king !
         sum += board.StaticPositionalScore[Const.White] - board.StaticPositionalScore[Const.Black];
         // Return always : higher is better.
         if (board.colorToMove == Const.White)
            return sum;
         else
            return -sum;
      }


      public abstract int GetEvaluation(int alpha, int beta);

      public int GetScoreNoise(int max)
      {
         return rng.Next(-max, max+1);
      }

      public void ReseedRandom(int seed)
      {
         rng = new Random(seed);
      }

      public void ResetRandom()
      {
         rng = new Random(InitialSeed);
      }

   }
}
