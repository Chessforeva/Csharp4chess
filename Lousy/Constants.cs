using System;
using System.Collections.Generic;
using System.Text;

namespace LousyChess
{
   public class Const
   {

      // for matrix declarations
      public const int NrColors = 2;
      public const int NrPieceTypes = 6;
      public const int MaxNrPiecesPerType = 10;
      public const int NrSquares = 64;

      public const int White = 0;
      public const int Black = 1;

      // MoveTypes (must be <= 63, due to move compacting !) :
      // The first 6 are also PieceType ID's
      public const int KingID = 0;
      public const int QueenID = 1;
      public const int RookID = 2;
      public const int BishopID = 3;
      public const int KnightID = 4;
      public const int PawnID = 5;
      //
      public const int SpecialMoveID = 10;
      public const int CastleQSID = SpecialMoveID + 1;
      public const int CastleKSID = SpecialMoveID + 2;
      public const int Pawn2StepID = SpecialMoveID + 3;
      // from here on, the material score is changed. Used in Futility pruning
      public const int EnPassantCaptureID = SpecialMoveID + 4;
      public const int PawnPromoteQueenID = SpecialMoveID + 5;
      public const int PawnPromoteRookID = SpecialMoveID + 6;
      public const int PawnPromoteBishopID = SpecialMoveID + 7;
      public const int PawnPromoteKnightID = SpecialMoveID + 8;
      public const int NullMoveID = SpecialMoveID + 9;
      public const int NoMoveID = SpecialMoveID + 10;
      
      // miscellaneous
      public const int InvalidID = -1;
      public const int EmptyID = -1;


      // The maximum bit sizes of various things and their bitmasks

      // a position is square 0..63
      public const int NrPositionBits = 6;
      public const int PositionBitMask = 63;

      // a MoveType = 0..63
      public const int NrMoveTypeBits = 6;
      public const int MoveTypeBitMask = 63;

      // a Piece = 0..5
      public const int NrPieceTypeBits = 3;
      public const int PieceTypeBitMask = 7;

      // The capture info consists of 2x3 bits. 2 PieceTypes. 
      // bits 0..2  : captured piece type;
      // bits 3..5 : capturing piece type
      public const int NrCaptureInfoBits = 6;
      public const int CaptureInfoBitMask = 63;

      // The value for no capture : lowest 6 bits set, 2x3 bits for captured/capturing piece.
      // This is possible, since the largest PieceType = 5
      public const int NoCaptureID = 63;

      // Game stage constants
      public const int Opening  = 0;
      public const int MidGame  = 1;
      public const int EndGame  = 2;
   }
}
