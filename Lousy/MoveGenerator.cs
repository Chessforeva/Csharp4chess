//TODO:  don't know yet how to handle promotions in seeScore

using System;
using System.Collections.Generic;
using System.Text;

namespace LousyChess
{
   
   #region Move struct

   public struct Move
   {

      public int fromPosition;
      public int toPosition;
      public int moveType;

      // bits 0..2  : captured piece type;
      // bits 3..5 : capturing piece type
      public int captureInfo;     // Constants.NoCaptureID = no capture

      // NB : the seeScore is not stored in the compressed version.
      //      the seeScore is not used in the equals operator etc.
      //      the seeScore is not used in the hash-value.
      public int seeScore;

      public static Move NoMove()
      {
         // This returns a Move, which indicates no move has been made.
         Move result = new Move();
         result.moveType = Const.NoMoveID;
         return result;
      }

      public static Move NullMove()
      {
         // This returns a Move, which indicates no move has been made.
         Move result = new Move();
         result.moveType = Const.NullMoveID;
         result.captureInfo = Const.NoCaptureID;
         return result;
      }

      public Move(int compressedMove)
      {
         // NB : the seeScore is not stored in the TT !!

         // bits 0..5 from position
         fromPosition = compressedMove & Const.PositionBitMask;
         compressedMove = compressedMove >> Const.NrPositionBits;
         // bits 0..5 to position
         toPosition = compressedMove & Const.PositionBitMask;
         compressedMove = compressedMove >> Const.NrPositionBits;
         // bits 12..17 : MoveType (see Board)
         moveType = compressedMove & Const.MoveTypeBitMask;
         compressedMove = compressedMove >> Const.NrMoveTypeBits;
         // bits 18..20 : captured piece type  (7 = no capture)
         // bits 21..23 : capturing piece type  (7 = no capture)
         captureInfo = compressedMove & Const.CaptureInfoBitMask;
         seeScore = 0;
      }

      public int Compress()
      {
         // NB : the seeScore is not stored in the TT !!

         // compress the move to 1 int, for storing in the TranspositionTable
         int result = 0;
         // bits 0..5 from position
         result = fromPosition & Const.PositionBitMask;
         // bits 6..11 : to position
         result += (toPosition & Const.PositionBitMask) << Const.NrPositionBits;
         // bits 12..17 : MoveType (see Board)
         result += (moveType & Const.MoveTypeBitMask) << (2 * Const.NrPositionBits);
         // bits 18..20 : captured piece type  (7 = no capture)
         // bits 21..23 : capturing piece type  (7 = no capture)
         result += (captureInfo & Const.CaptureInfoBitMask) << (2 * Const.NrPositionBits + Const.NrMoveTypeBits);
         return result;
      }


      #region operator overloads

      public static bool operator ==(Move m1, Move m2)
      {
         return m1.fromPosition == m2.fromPosition && m1.toPosition == m2.toPosition
                && m1.moveType == m2.moveType && m1.captureInfo == m2.captureInfo;
      }

      public static bool operator !=(Move m1, Move m2)
      {
         return m1.fromPosition != m2.fromPosition || m1.toPosition != m2.toPosition
                || m1.moveType != m2.moveType || m1.captureInfo != m2.captureInfo;
      }

      public override bool Equals(object obj)
      {
         if ( !(obj is Move) )
            return false;
         return this == (Move)obj;
      }

      public override int GetHashCode()
      {
         return fromPosition.GetHashCode() ^ toPosition.GetHashCode() ^ moveType.GetHashCode() ^ captureInfo.GetHashCode();
      }

      #endregion


      #region Move to String

      private string PositionToString(int pos)
      {
         int x = pos % 8;
         int y = pos / 8;
         return ((char)((int)'a' + x)).ToString() + ((char)((int)'1' + y)).ToString();
      }

      private string PieceTypeToString(int pieceType)
      {
         string[] PieceStr = { "K", "Q", "R", "B", "N", ""};
         return PieceStr[pieceType];
      }

      public string ToString(int colorToMove)
      {
         string s;
         string separator = "-";
         if (captureInfo != Const.NoCaptureID)
            separator = "x" + PieceTypeToString(captureInfo & Const.PieceTypeBitMask);
         //
         if (moveType < Const.SpecialMoveID)
         {
            s = PieceTypeToString(moveType) + PositionToString(fromPosition) + separator + PositionToString(toPosition);
         }
         else
         {
            switch (moveType)
            {
               case Const.CastleQSID: s = "0-0-0"; break;
               case Const.CastleKSID: s = "0-0"; break;
               case Const.EnPassantCaptureID:
                  s = PositionToString(fromPosition) + separator + PositionToString(toPosition)+" e.p.";
                  break;
               case Const.PawnPromoteQueenID:
                  s = PositionToString(fromPosition) + separator + PositionToString(toPosition) + "=Q";
                  break;
               case Const.PawnPromoteRookID:
                  s = PositionToString(fromPosition) + separator + PositionToString(toPosition) + "=R";
                  break;
               case Const.PawnPromoteBishopID:
                  s = PositionToString(fromPosition) + separator + PositionToString(toPosition) + "=B";
                  break;
               case Const.PawnPromoteKnightID:
                  s = PositionToString(fromPosition) + separator + PositionToString(toPosition) + "=N";
                  break;
               case Const.Pawn2StepID:
                  s = PositionToString(fromPosition) + separator + PositionToString(toPosition);
                  break;
               default:
                  s = "????";
                  break;
            }
         }
         //
         if (colorToMove == -1)
            return s;               // -1 : no padding etc
         if (colorToMove % 2 == 0)
            s = s + "   ";
         else
            s =  ".. " + s;
         s = s.PadLeft(10);
         return s;
      }

      public override string ToString()
      {
         return ToString(-1);
      }

      public string ToLANString()
      {
         string result = EPD.PositionToString(fromPosition)
                         + EPD.PositionToString(toPosition);
         if (moveType >= Const.PawnPromoteQueenID && moveType <= Const.PawnPromoteKnightID)
            result += EPD.PromotionPieceToString(moveType);
         return result;
      }

      #endregion

   }

   #endregion


   public class MoveGenerator
   {

      // pointers to other classes
      public Board board;
      public MagicMoves magicMoves;
      public BitBoard bitboard;
      public Attack attack;

      public int nrGeneratedMoves;

      public bool noNegativeSEECaptures = true;



      #region private data

      int maxNrMoves = 100;      // will be increased automatically, if needed in AddMove
      private Move[] moveList;

      // Just a 1 at the position bit, a 0 on all other bits
      // { for normal bitboard : 64 bit : starting from A1..H1, B2..H2 ,,,,,,, A8..H8 ]
      ulong[] Identity = new ulong[Const.NrSquares];

      int[] KingDirections = { -9, -8, -7, -1, +1, +7, +8, +9 };
      int[] KingXDirections = { -1, 0, 1, -1, +1, -1, 0, +1 };
      int[] KingYDirections = { -1, -1, -1, 0, 0, +1, +1, +1 };
      //int[] QueenDirections = { -8, -1, +1, +8  ,  - 9, -7, +7, +9 };
      //int[] BishopDirections = { -9, -7, +7, +9 };
      //int[] RookDirections = { -8, -1, +1, +8 };
      int[] KnightDirections = { -17, -15, -10, -6, +17, +15, +10, +6 };
      int[] KnightXDirections = { -1, +1, -2, +2, +1, -1, +2, -2 };
      int[] KnightYDirections = { -2, -2, -1, -1, +2, +2, +1, +1 };

      int[] PawnDelta = { 8, -8 };              // 1 pawn-step
      int[] PawnFrom2StepRank = { 1, 6 };       // rank from which a 2-step is possible
      int[] PawnPromotionToRank = { 7, 0 };     // rank on which pawn is promoted
      int[] PawnPromotionFromRank = { 6, 1 };   // rank from which a pawn gets promoted

      public ulong[] EmptyBoardKingMoves = new ulong[Const.NrSquares];
      public ulong[] EmptyBoardKnightMoves = new ulong[Const.NrSquares];

      ulong[,] EmptyBoardPawn1StepMoves = new ulong[Const.NrColors, Const.NrSquares];
      ulong[,] EmptyBoardPawn2StepMoves = new ulong[Const.NrColors, Const.NrSquares];

      // castle stuff
      ulong[] CastleEmptySquaresQS = new ulong[Const.NrColors];  // squares between king & rook
      ulong[] CastleEmptySquaresKS = new ulong[Const.NrColors];
      ulong[] CastleUnattackedSquaresQS = new ulong[Const.NrColors]; // king square + 2 squares next to it
      ulong[] CastleUnattackedSquaresKS = new ulong[Const.NrColors];
      int[] CastleRookPositionQS = new int[Const.NrColors];
      int[] CastleRookPositionKS = new int[Const.NrColors];


      #endregion


      public MoveGenerator()
      {
         CreateIdentity();
         moveList = new Move[maxNrMoves];
         //
         GenerateEmptyBoardKingMoves();
         GenerateEmptyBoardKnightMoves();
         GenerateEmptyBoardPawn1StepMoves();
         GenerateEmptyBoardPawn2StepMoves();
         //
         GenerateCastleSquares();
      }


      #region  some helper bitboards : Identity, Castle

      private void CreateIdentity()
      {
         ulong q = 1;
         for (int i = 0; i < Const.NrSquares; i++)
         {
            Identity[i] = q;
            q <<= 1;
         }
      }


      private void GenerateCastleSquares()
      {
         // white 
         // these square must be empty
         CastleEmptySquaresQS[Const.White] = Identity[1] | Identity[2] | Identity[3];
         CastleEmptySquaresKS[Const.White] = Identity[5] | Identity[6];
         // these square may not be under attack
         CastleUnattackedSquaresQS[Const.White] = Identity[2] | Identity[3] | Identity[4];
         CastleUnattackedSquaresKS[Const.White] = Identity[4] | Identity[5] | Identity[6];
         //
         // black 
         // these square must be empty
         CastleEmptySquaresQS[Const.Black] = Identity[57] | Identity[58] | Identity[59];
         CastleEmptySquaresKS[Const.Black] = Identity[61] | Identity[62];
         // these square may not be under attack
         CastleUnattackedSquaresQS[Const.Black] = Identity[58] | Identity[59] | Identity[60];
         CastleUnattackedSquaresKS[Const.Black] = Identity[60] | Identity[61] | Identity[62];
         //
         // The squares where a rook of the right color must be present for castling
         CastleRookPositionQS[Const.White] = 0;
         CastleRookPositionKS[Const.White] = 7;
         CastleRookPositionQS[Const.Black] = 56;
         CastleRookPositionKS[Const.Black] = 63;
      }

      #endregion
 

      #region Generate moves for non-sliding pieces on an empty board


      private void GenerateEmptyBoardKnightMoves()
      {
         for (int i = 0; i < Const.NrSquares; i++)
         {
            EmptyBoardKnightMoves[i] = 0;
            for (int j = 0; j < 8; j++)
            {
               int x = i % 8 + KnightXDirections[j];
               int y = i / 8 + KnightYDirections[j];
               if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
               {
                  int position = x + y * 8;
                  EmptyBoardKnightMoves[i] |= Identity[position];
               }
            }
         }
      }


      private void GenerateEmptyBoardKingMoves()
      {
         for (int i = 0; i < Const.NrSquares; i++)
         {
            EmptyBoardKingMoves[i] = 0;
            for (int j = 0; j < 8; j++)
            {
               int x = i % 8 + KingXDirections[j];
               int y = i / 8 + KingYDirections[j];
               if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
               {
                  int position = x + y * 8;
                  EmptyBoardKingMoves[i] |= Identity[position];
               }
            }
         }
      }

      private void GenerateEmptyBoardPawn1StepMoves()
      {
         // white
         for (int i = 0; i < Const.NrSquares; i++)
         {
            if (i >= 8 && i <= 55)
               EmptyBoardPawn1StepMoves[Const.White, i] = Identity[i + 8];
            else
               EmptyBoardPawn1StepMoves[Const.White, i] = 0;
         }
         // black
         for (int i = 0; i < Const.NrSquares; i++)
         {
            if (i >= 8 && i <= 55)
               EmptyBoardPawn1StepMoves[Const.Black, i] = Identity[i - 8];
            else
               EmptyBoardPawn1StepMoves[Const.Black, i] = 0;
         }

      }

      private void GenerateEmptyBoardPawn2StepMoves()
      {
         // white
         for (int i = 0; i < Const.NrSquares; i++)
         {
            if (i >= 8 && i <= 15)
               EmptyBoardPawn2StepMoves[Const.White, i] = Identity[i + 16];
            else
               EmptyBoardPawn2StepMoves[Const.White, i] = 0;
         }
         // black
         for (int i = 0; i < Const.NrSquares; i++)
         {
            if (i >= 48 && i <= 55)
               EmptyBoardPawn2StepMoves[Const.Black, i] = Identity[i - 16];
            else
               EmptyBoardPawn2StepMoves[Const.Black, i] = 0;
         }
      }



      #endregion


      #region Generate ALL moves for each piece type


      private void AddMove(int moveType, int fromPos, int toPos)
      {
         if (nrGeneratedMoves == maxNrMoves)
         {
            // optionally increase the moveList
            int newMaxNrMoves = maxNrMoves * 2;
            Move[] moveList2 = new Move[newMaxNrMoves];
            for (int i = 0; i < maxNrMoves; i++)
               moveList2[i] = moveList[i];
            maxNrMoves = newMaxNrMoves;
            moveList = moveList2;
         }
         //
         moveList[nrGeneratedMoves].moveType = moveType;
         moveList[nrGeneratedMoves].fromPosition = fromPos;
         moveList[nrGeneratedMoves].toPosition = toPos;
         // don't know yet how to handle promotions in seeScore
         moveList[nrGeneratedMoves].seeScore = 0;            // will be filled later
         // store the possible captured piece :
         if (moveType < Const.SpecialMoveID)
         {
            // this can be EmptyID or some PieceType
            int capturedPieceType = board.SquareContents[toPos].pieceType;
            //
            if (capturedPieceType == Const.EmptyID)
            {
               moveList[nrGeneratedMoves].captureInfo = Const.NoCaptureID;
            }
            else
            {
               // yes, it is a capture. 
               // Store the captured piece type in bits 0..2, and the capturing piece type in bits 3..5
               moveList[nrGeneratedMoves].captureInfo =
                  capturedPieceType + (board.SquareContents[fromPos].pieceType << Const.NrPieceTypeBits);
               // also store the SEE score
               moveList[nrGeneratedMoves].seeScore = attack.SEE(moveType, fromPos, toPos);
            }
         }
         else
         {
            switch (moveType)
            {
               case Const.EnPassantCaptureID:
                  moveList[nrGeneratedMoves].captureInfo = Const.PawnID + (Const.PawnID << Const.NrPieceTypeBits);
                  moveList[nrGeneratedMoves].seeScore = attack.SEE(moveType, fromPos, toPos);
                  break;
               case Const.PawnPromoteQueenID:
               case Const.PawnPromoteRookID:
               case Const.PawnPromoteBishopID:
               case Const.PawnPromoteKnightID:
                  // this can be EmptyID or some PieceType
                  // this can be EmptyID or some PieceType
                  int capturedPieceType = board.SquareContents[toPos].pieceType;
                  //
                  if (capturedPieceType == Const.EmptyID)
                     moveList[nrGeneratedMoves].captureInfo = Const.NoCaptureID;
                  else
                  {
                     // yes, it is a capture. 
                     // Store the captured piece type in bits 0..2,  and the capturing piece type in bits 3..5
                     moveList[nrGeneratedMoves].captureInfo =
                        capturedPieceType + (board.SquareContents[fromPos].pieceType << Const.NrPieceTypeBits);
                  }
                  break;
               default:
                  moveList[nrGeneratedMoves].captureInfo = Const.NoCaptureID;
                  break;
            }
         }
         nrGeneratedMoves++;
      }


      private void GenerateKingMoves()
      {
         // local copies :
         ulong myPieces = board.pieces[board.colorToMove];
         //
         int position = board.PiecePos[board.colorToMove, Const.KingID, 0];
         ulong kingMoves = EmptyBoardKingMoves[position] & ~myPieces;
         //
         while (kingMoves != 0)
         {
            int bitNr = bitboard.LSB_andReset(ref kingMoves);
            AddMove(Const.KingID, position, bitNr);
         }
      }


      public bool CastleIsLegal(int castleType)
      {
         // checks if the squares between the king and rook are empty and
         // the king square and the 2 next to it are not attacked.
         // It is called by MakeMove AFTER the castle is made, to see if it was a legal move.
         //
         // local copies :
         int colorToMove = board.colorToMove;
         int enemyColor = board.enemyColor;
         //
         ulong attackedBitBoard = attack.GetAttackedSquaresBitBoard(enemyColor);
         if (castleType == Const.CastleQSID)
         {
            // QueenSide
            return  (CastleUnattackedSquaresQS[colorToMove] & attackedBitBoard) == 0;
         }
         else if (castleType == Const.CastleKSID)
         {
            return (CastleUnattackedSquaresKS[colorToMove] & attackedBitBoard) == 0;
         }
         else
            throw new Exception("Invalid CastleType");
      }


      private void GenerateCastleMoves()
      {
         // The check to see if the castle is legal, i.e. the squares are not under attack
         // is performed in MakeMove.
         // local copies :
         int colorToMove = board.colorToMove;
         int castleRankOffset = colorToMove * 56;    // first square-nr of King rank
         //
         if (board.canCastleQueenSide[colorToMove] || board.canCastleKingSide[colorToMove])
         {
            // QueenSide
            int rookSquare = CastleRookPositionQS[colorToMove];
            if (board.canCastleQueenSide[colorToMove]
               && (CastleEmptySquaresQS[colorToMove] & board.allPiecesBB) == 0
               && board.SquareContents[rookSquare].pieceType == Const.RookID
               && board.SquareContents[rookSquare].pieceColor == colorToMove
               )
               AddMove(Const.CastleQSID, castleRankOffset + 4, castleRankOffset + 2);    // King moves
            // KingSide
            rookSquare = CastleRookPositionKS[colorToMove];
            if (board.canCastleKingSide[colorToMove]
               && (CastleEmptySquaresKS[colorToMove] & board.allPiecesBB) == 0
               && board.SquareContents[rookSquare].pieceType == Const.RookID
               && board.SquareContents[rookSquare].pieceColor == colorToMove
               )
               AddMove(Const.CastleKSID, castleRankOffset + 4, castleRankOffset + 6);    // King moves
         }
      }



      private void GenerateQueenMoves()
      {
         // local copies :
         int  colorToMove = board.colorToMove;
         ulong myPieces = board.pieces[colorToMove];
         //
         for (int queenNr = 0; queenNr < board.NrPieces[colorToMove, Const.QueenID]; queenNr++)
         {
            int position = board.PiecePos[colorToMove, Const.QueenID, queenNr];
            ulong queenMoves = magicMoves.Qmagic(position, board.allPiecesBB) & ~myPieces;
            while (queenMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref queenMoves);
               AddMove(Const.QueenID, position, bitNr);
            }
         }
      }


      private void GenerateRookMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong myPieces = board.pieces[colorToMove];
         //
         for (int rookNr = 0; rookNr < board.NrPieces[colorToMove, Const.RookID]; rookNr++)
         {
            int position = board.PiecePos[colorToMove, Const.RookID, rookNr];
            ulong rookMoves = magicMoves.Rmagic(position, board.allPiecesBB) & ~myPieces;
            //
            while (rookMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref rookMoves);
               AddMove(Const.RookID, position, bitNr);
            }
         }
      }


      private void GenerateBishopMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong myPieces = board.pieces[colorToMove];
         //
         for (int bishopNr = 0; bishopNr < board.NrPieces[colorToMove, Const.BishopID]; bishopNr++)
         {
            int position = board.PiecePos[colorToMove, Const.BishopID, bishopNr];
            ulong bishopMoves = magicMoves.Bmagic(position, board.allPiecesBB) & ~myPieces;
            //
            while (bishopMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref bishopMoves);
               AddMove(Const.BishopID, position, bitNr);
            }
         }
      }


      private void GenerateKnightMoves()
      {
         // local copies
         int colorToMove = board.colorToMove;
         ulong myPieces = board.pieces[colorToMove];
         //
         for (int knightNr = 0; knightNr < board.NrPieces[colorToMove, Const.KnightID]; knightNr++)
         {
            int position = board.PiecePos[colorToMove, Const.KnightID, knightNr];
            ulong knightMoves = EmptyBoardKnightMoves[position] & ~myPieces;
            //
            while (knightMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref knightMoves);
               AddMove(Const.KnightID, position, bitNr);
            }
         }
      }


      private void AddPawnMove(int fromPos, int ToPos)
      {
         // local copies :
         int colorToMove = board.colorToMove;
         //
         // add a pawn move, which can both be a normal pawn move, or a pawn promotion
         if (ToPos / 8 == PawnPromotionToRank[colorToMove])
         {
            AddMove(Const.PawnPromoteQueenID, fromPos, ToPos);
            AddMove(Const.PawnPromoteRookID, fromPos, ToPos);
            AddMove(Const.PawnPromoteBishopID, fromPos, ToPos);
            AddMove(Const.PawnPromoteKnightID, fromPos, ToPos);
         }
         else
            AddMove(Const.PawnID, fromPos, ToPos);
      }


      private void GeneratePawnMoves()
      {
         //
         // local copies :
         int colorToMove = board.colorToMove;
         ulong allPieces = board.allPiecesBB;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         for (int pawnNr = 0; pawnNr < board.NrPieces[colorToMove, Const.PawnID]; pawnNr++)
         {
            int position = board.PiecePos[colorToMove, Const.PawnID, pawnNr];
            int fromRank = position / 8;
            ulong Pawn1StepMove = EmptyBoardPawn1StepMoves[colorToMove, position];
            // Pawn1StepMove has only 1 bit set if it is on a legal rank, and 0 bits otherwise
            // Check if a 1 step move is possible (rank != 0 && 7)
            // and check if the new fields is not blocked
            if (Pawn1StepMove != 0 && (Pawn1StepMove & allPieces) == 0)
            {
               // normal pawn move or pawn promotion 
               AddPawnMove(position, position + PawnDelta[colorToMove]);
               // 1 step move is possible, so also try 2-step move
               if (fromRank == PawnFrom2StepRank[colorToMove])   // on 2nd or 7th rank
               {
                  ulong Pawn2StepMove = EmptyBoardPawn2StepMoves[colorToMove, position];
                  // check if it is not blocked
                  if ((Pawn2StepMove & allPieces) == 0)
                     AddMove(Const.Pawn2StepID, position, position + 2 * PawnDelta[colorToMove]);
               }
            }
            // capture move ?
            if ( (bitboard.PawnAttackBB1[colorToMove, position] & enemyPieces) != 0)
            {
               // left and/or right
               int x = position % 8;
               if (colorToMove == 0)
               {
                  // white , capture left
                  if (x > 0 && (Identity[position + 7] & enemyPieces) != 0)
                     AddPawnMove(position, position + 7);
                  // capture right
                  if (x < 7 && (Identity[position + 9] & enemyPieces) != 0)
                     AddPawnMove(position, position + 9);
               }
               else
               {
                  // black , capture left
                  if (x > 0 && (Identity[position - 9] & enemyPieces) != 0)
                     AddPawnMove(position, position - 9);
                  // capture right
                  if (x < 7 && (Identity[position - 7] & enemyPieces) != 0)
                     AddPawnMove(position, position - 7);
               }
            }
            // enpassant capture ?
            if (board.enPassantPosition >= 0)
            {
               if ((bitboard.PawnAttackBB1[colorToMove, position] & Identity[board.enPassantPosition]) != 0)
                  AddMove(Const.EnPassantCaptureID, position, board.enPassantPosition);
            }
         }
      }


      public Move[] GenerateMoves(Move[] initialMoveStorage)
      {
         moveList = initialMoveStorage;
         if (moveList == null || moveList.Length < 50)
            moveList = new Move[50];
         maxNrMoves = moveList.Length;
         nrGeneratedMoves = 0;
         //
         // Now generate pseudo legal moves. No check for getting into Check !
         nrGeneratedMoves = 0;

         GenerateKingMoves();
         GenerateCastleMoves();
         GenerateQueenMoves();
         GenerateRookMoves();
         GenerateBishopMoves();
         GenerateKnightMoves();
         GeneratePawnMoves();
         // Returns a pointer to the moveList. 
         // This may be a different one then the initial, if the initial was to short.
         return moveList;
      }


      #endregion


      #region Generate Quiescence moves : captures, pawn promotions
      
      private void AddQuiescenceMove(int moveType, int fromPos, int toPos)
      {
         if (nrGeneratedMoves == maxNrMoves)
         {
            // optionally increase the moveList
            int newMaxNrMoves = maxNrMoves * 2;
            Move[] moveList2 = new Move[newMaxNrMoves];
            for (int i = 0; i < maxNrMoves; i++)
               moveList2[i] = moveList[i];
            maxNrMoves = newMaxNrMoves;
            moveList = moveList2;
         }
         //
         moveList[nrGeneratedMoves].moveType = moveType;
         moveList[nrGeneratedMoves].fromPosition = fromPos;
         moveList[nrGeneratedMoves].toPosition = toPos;
         moveList[nrGeneratedMoves].seeScore = 0;             // filled in later
         //
         // store the possible captured piece :
         if (moveType < Const.SpecialMoveID)
         {
            // this can be EmptyID or some PieceType
            int capturedPieceType = board.SquareContents[toPos].pieceType;
            //
            if (capturedPieceType == Const.EmptyID)
               moveList[nrGeneratedMoves].captureInfo = Const.NoCaptureID;
            else
            {
               // yes, it is a capture. 
               // Store the captured piece type in bits 0..2, and the capturing piece type in bits 3..5
               moveList[nrGeneratedMoves].captureInfo =
                  capturedPieceType + (board.SquareContents[fromPos].pieceType << Const.NrPieceTypeBits);
               // also store the SEE-score
               moveList[nrGeneratedMoves].seeScore = attack.SEE(moveType, fromPos, toPos);
            }
         }
         else
         {
            switch (moveType)
            {
               case Const.EnPassantCaptureID:
                  moveList[nrGeneratedMoves].captureInfo = Const.PawnID + (Const.PawnID << Const.NrPieceTypeBits);
                  moveList[nrGeneratedMoves].seeScore = attack.SEE(moveType, fromPos, toPos);
                  break;
               case Const.PawnPromoteQueenID:
               case Const.PawnPromoteRookID:
               case Const.PawnPromoteBishopID:
               case Const.PawnPromoteKnightID:
                  // this can be EmptyID or some PieceType
                  // this can be EmptyID or some PieceType
                  int capturedPieceType = board.SquareContents[toPos].pieceType;
                  //
                  if (capturedPieceType == Const.EmptyID)
                     moveList[nrGeneratedMoves].captureInfo = Const.NoCaptureID;
                  else
                  {
                     // yes, it is a capture. 
                     // Store the captured piece type in bits 0..2,  and the capturing piece type in bits 3..5
                     moveList[nrGeneratedMoves].captureInfo =
                        capturedPieceType + (board.SquareContents[fromPos].pieceType << Const.NrPieceTypeBits);
                  }
                  break;
               default:
                  moveList[nrGeneratedMoves].captureInfo = Const.NoCaptureID;
                  break;
            }
         }
         // maybe abort here, if captures with negative SEE is excluded
         if (noNegativeSEECaptures && !(moveType >= Const.SpecialMoveID))
         {
            // moveType>=Const.SpecialMoveID : always do special stuff. 
            // This also means that en-passant captures are never filtered out !
            // Also : SEE has problems with promotions or so 
            if (moveList[nrGeneratedMoves].seeScore < 0)
               return;
         }
         nrGeneratedMoves++;
      }


      private void GenerateCapturingKingMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         int position = board.PiecePos[colorToMove, Const.KingID, 0];
         ulong kingMoves = EmptyBoardKingMoves[position] & enemyPieces;
         //
         while (kingMoves != 0)
         {
            int bitNr = bitboard.LSB_andReset(ref kingMoves);
            AddQuiescenceMove(Const.KingID, position, bitNr);
         }
      }


      private void GenerateCapturingQueenMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         for (int queenNr = 0; queenNr < board.NrPieces[colorToMove, Const.QueenID]; queenNr++)
         {
            int position = board.PiecePos[colorToMove, Const.QueenID, queenNr];
            ulong queenMoves = magicMoves.Qmagic(position, board.allPiecesBB) & enemyPieces;
            while (queenMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref queenMoves);
               AddQuiescenceMove(Const.QueenID, position, bitNr);
            }
         }
      }


      private void GenerateCapturingRookMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         for (int rookNr = 0; rookNr < board.NrPieces[colorToMove, Const.RookID]; rookNr++)
         {
            int position = board.PiecePos[colorToMove, Const.RookID, rookNr];
            ulong rookMoves = magicMoves.Rmagic(position, board.allPiecesBB) & enemyPieces;
            //
            while (rookMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref rookMoves);
               AddQuiescenceMove(Const.RookID, position, bitNr);
            }
         }
      }


      private void GenerateCapturingBishopMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         for (int bishopNr = 0; bishopNr < board.NrPieces[colorToMove, Const.BishopID]; bishopNr++)
         {
            int position = board.PiecePos[colorToMove, Const.BishopID, bishopNr];
            ulong bishopMoves = magicMoves.Bmagic(position, board.allPiecesBB) & enemyPieces;
            //
            while (bishopMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref bishopMoves);
               AddQuiescenceMove(Const.BishopID, position, bitNr);
            }
         }
      }


      private void GenerateCapturingKnightMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         for (int knightNr = 0; knightNr < board.NrPieces[colorToMove, Const.KnightID]; knightNr++)
         {
            int position = board.PiecePos[colorToMove, Const.KnightID, knightNr];
            ulong knightMoves = EmptyBoardKnightMoves[position] & enemyPieces;
            //
            while (knightMoves != 0)
            {
               int bitNr = bitboard.LSB_andReset(ref knightMoves);
               AddQuiescenceMove(Const.KnightID, position, bitNr);
            }
         }
      }


      private void GenerateCapturingPawnMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong enemyPieces = board.pieces[board.enemyColor];
         //
         for (int pawnNr = 0; pawnNr < board.NrPieces[colorToMove, Const.PawnID]; pawnNr++)
         {
            int position = board.PiecePos[colorToMove, Const.PawnID, pawnNr];
            //
            // Capture ?
            if ((bitboard.PawnAttackBB1[colorToMove, position] & enemyPieces) != 0)
            {
               // left and/or right
               int x = position % 8;
               if (colorToMove == 0)
               {
                  // white , capture left
                  if (x > 0 && (Identity[position + 7] & enemyPieces) != 0)
                     AddQuiescenceMove(Const.PawnID, position, position + 7);
                  // capture right
                  if (x < 7 && (Identity[position + 9] & enemyPieces) != 0)
                     AddQuiescenceMove(Const.PawnID, position, position + 9);
               }
               else
               {
                  // black , capture left
                  if (x > 0 && (Identity[position - 9] & enemyPieces) != 0)
                     AddQuiescenceMove(Const.PawnID, position, position - 9);
                  // capture right
                  if (x < 7 && (Identity[position - 7] & enemyPieces) != 0)
                     AddQuiescenceMove(Const.PawnID, position, position - 7);
               }
            }
            // enpassant capture ?
            if (board.enPassantPosition >= 0)
            {
               if ((bitboard.PawnAttackBB1[colorToMove, position] & Identity[board.enPassantPosition]) != 0)
                  AddQuiescenceMove(Const.EnPassantCaptureID, position, board.enPassantPosition);
            }
         }
      }


      private void GeneratePromotingPawnMoves()
      {
         // local copies :
         int colorToMove = board.colorToMove;
         ulong allPieces = board.allPiecesBB;
         //
         for (int pawnNr = 0; pawnNr < board.NrPieces[colorToMove, Const.PawnID]; pawnNr++)
         {
            int position = board.PiecePos[colorToMove, Const.PawnID, pawnNr];
            int fromRank = position / 8;
            if (fromRank == PawnPromotionFromRank[colorToMove])
            {
               ulong Pawn1StepMove = EmptyBoardPawn1StepMoves[colorToMove, position];
               // Pawn1StepMove has only 1 bit set if it is on a legal rank, and 0 bits otherwise
               // Check if a 1 step move is possible (rank != 0 && 7)
               // and check if the new fields is not blocked
               if (Pawn1StepMove != 0 && (Pawn1StepMove & allPieces) == 0)
               {
                  // pawn promotion 
                  int ToPos = position + PawnDelta[colorToMove];
                  // Use only promotion to queen.
                  // The other onces are only extremely seldom useful (like avoiding a stalemate).
                  AddQuiescenceMove(Const.PawnPromoteQueenID, position, ToPos);
         //       AddQuiescenceMove(Const.PawnPromoteRookID, position, ToPos);
         //       AddQuiescenceMove(Const.PawnPromoteBishopID, position, ToPos);
         //       AddQuiescenceMove(Const.PawnPromoteKnightID, position, ToPos);
               }
            }
         }
      }


      public Move[] GenerateQuiescenceMoves(Move[] initialMoveStorage)
      {
         // This generates only captures and pawn promotions
         moveList = initialMoveStorage;
         if (moveList == null || moveList.Length < 50)
            moveList = new Move[50];
         maxNrMoves = moveList.Length;
         nrGeneratedMoves = 0;
         //
         // Now generate pseudo legal moves. No check for getting into Check !
         nrGeneratedMoves = 0;
         GenerateCapturingKingMoves();
         GenerateCapturingQueenMoves();
         GenerateCapturingRookMoves();
         GenerateCapturingBishopMoves();
         GenerateCapturingKnightMoves();
         GenerateCapturingPawnMoves();
         GeneratePromotingPawnMoves();
         // Returns a pointer to the moveList. 
         // This may be a different one then the initial, if the initial was to short.
         return moveList;
      }


      #endregion


 
   }
}
