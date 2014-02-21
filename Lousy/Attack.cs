using System;
using System.Collections.Generic;
using System.Text;

namespace LousyChess
{
   public class Attack
   {

      public Board board = null;
      public BitBoard bitBoard = null;
      public MagicMoves magicMoves = null;
      public Evaluator evaluator = null;
      public MoveGenerator moveGenerator = null;

      public Attack()
      {
      }


      /// <summary>
      /// Returns a bitboard with all squares which are attacked by the specified color.
      /// These squares include empty squares, own pieces and enemy pieces.
      /// </summary>
      /// <param name="movingColor">The color which attacks.</param>
      /// <returns>The bitboard with all squares which are attacked/defended by the movingColor</returns>
      public ulong GetAttackedSquaresBitBoard(int movingColor)
      {
         // Returns a bitboard with all squares which can be attacked by the movingColor
         ulong result = 0;
         // local copies
         ulong _allPieces = board.allPiecesBB;
         // by king
         result |= moveGenerator.EmptyBoardKingMoves[board.PiecePos[movingColor, Const.KingID, 0]];
         // by queens
         for (int queenNr = 0; queenNr < board.NrPieces[movingColor, Const.QueenID]; queenNr++)
         {
            int position = board.PiecePos[movingColor, Const.QueenID, queenNr];
            result |= magicMoves.Qmagic(position, _allPieces);
         }
         // by rooks
         for (int rookNr = 0; rookNr < board.NrPieces[movingColor, Const.RookID]; rookNr++)
         {
            int position = board.PiecePos[movingColor, Const.RookID, rookNr];
            result |= magicMoves.Rmagic(position, _allPieces);
         }
         // by bishops
         for (int bishopNr = 0; bishopNr < board.NrPieces[movingColor, Const.BishopID]; bishopNr++)
         {
            int position = board.PiecePos[movingColor, Const.BishopID, bishopNr];
            result |= magicMoves.Bmagic(position, _allPieces);
         }
         // by knights
         for (int knightNr = 0; knightNr < board.NrPieces[movingColor, Const.KnightID]; knightNr++)
            result |= moveGenerator.EmptyBoardKnightMoves[board.PiecePos[movingColor, Const.KnightID, knightNr]];
         // by pawns : excluding en-passant
         for (int pawnNr = 0; pawnNr < board.NrPieces[movingColor, Const.PawnID]; pawnNr++)
            result |= bitBoard.PawnAttackBB1[movingColor, board.PiecePos[movingColor, Const.PawnID, pawnNr]];
         //
         return result;
      }


      /// <summary>
      /// Gets a bitboard with the position of all pieces of the specified color, 
      /// which attack/defends the specified square.
      /// </summary>
      /// <param name="squareNr">The square which should be attacked/defended.</param>
      /// <param name="color">The color which attacks/defends the square.</param>
      /// <returns>A bitboard with all the attacking/defending pieces.</returns>
      public ulong GetAttackingDefendingPiecesBitBoard(int color, int squareNr)
      {
         // Use the attack symmetry:  Pretend square is all piece types, 
         // and see if it can capture the corresponding enemy piece type.
         ulong result = 0;
         // The sliding pieces
         // pretend the square is a bishop. Get all squares it could attack.
         ulong attackBB = magicMoves.Bmagic(squareNr, board.allPiecesBB);
         // check whetter it includes an enemy Queen or Bishop
         result |= attackBB & (board.pieceBB[color, Const.QueenID] | board.pieceBB[color, Const.BishopID]);
         // pretend the square is a rook. Get all squares it could attack.
         attackBB = magicMoves.Rmagic(squareNr, board.allPiecesBB);
         // check whetter it includes an enemy Queen or Rook
         result |= attackBB & (board.pieceBB[color, Const.QueenID] | board.pieceBB[color, Const.RookID]);
         // The non-sliding pieces
         // pretend the square is a Knight. Get all squares it could attack. See if it includes an enemy Knight.
         result |= moveGenerator.EmptyBoardKnightMoves[squareNr] & board.pieceBB[color, Const.KnightID];
         // pretend the square is a Pawn. Get all squares it could attack. See if it includes an enemy pawn.
         // (the EmptyBoardPawnCatchMoves does _not_ include en-passant capturing, so ok).
         result |= bitBoard.PawnAttackBB1[color ^ 1, squareNr] & board.pieceBB[color, Const.PawnID];
         // pretend the square is a King. Get all squares it could attack. See if it includes the enemy King.
         result |= moveGenerator.EmptyBoardKingMoves[squareNr] & board.pieceBB[color, Const.KingID];
         //
         return result;
      }


      /// <summary>
      /// Gets a bitboard with the position of all pieces of both colors, which attack/defends the specified square.
      /// </summary>
      /// <param name="squareNr">The square which should be attacked/defended.</param>
      /// <returns>A bitboard with all the attacking/defending pieces.</returns>
      public ulong GetAttackingDefendingPiecesBitBoard(int squareNr)
      {
         // Use the attack symmetry:  Pretend square is all piece types, 
         // and see if it can capture the corresponding enemy piece type.
         ulong result = 0;
         // The sliding pieces
         // pretend the square is a bishop. Get all squares it could attack.
         ulong attackBB = magicMoves.Bmagic(squareNr, board.allPiecesBB);
         // check whetter it includes an enemy Queen or Bishop
         result |= attackBB & 
                     (  board.pieceBB[Const.White, Const.QueenID] | board.pieceBB[Const.White, Const.BishopID]
                      | board.pieceBB[Const.Black, Const.QueenID] | board.pieceBB[Const.Black, Const.BishopID] );
         // pretend the square is a rook. Get all squares it could attack.
         attackBB = magicMoves.Rmagic(squareNr, board.allPiecesBB);
         // check whetter it includes an enemy Queen or Rook
         result |= attackBB &
                     (  board.pieceBB[Const.White, Const.QueenID] | board.pieceBB[Const.White, Const.RookID]
                      | board.pieceBB[Const.Black, Const.QueenID] | board.pieceBB[Const.Black, Const.RookID]);
         // The non-sliding pieces
         // pretend the square is a Knight. Get all squares it could attack. See if it includes an enemy Knight.
         result |= moveGenerator.EmptyBoardKnightMoves[squareNr] &
                    (board.pieceBB[Const.White, Const.KnightID] | board.pieceBB[Const.Black, Const.KnightID]);
         // pretend the square is a Pawn. Get all squares it could attack. See if it includes an enemy pawn.
         // (the EmptyBoardPawnCatchMoves does _not_ include en-passant capturing, so ok).
         result |= bitBoard.PawnAttackBB1[Const.White, squareNr] & board.pieceBB[Const.Black, Const.PawnID];
         result |= bitBoard.PawnAttackBB1[Const.Black, squareNr] & board.pieceBB[Const.White, Const.PawnID];
         // pretend the square is a King. Get all squares it could attack. See if it includes the enemy King.
         result |= moveGenerator.EmptyBoardKingMoves[squareNr] & 
                    ( board.pieceBB[Const.White, Const.KingID] | board.pieceBB[Const.Black, Const.KingID]);
         //
         return result;
      }



      public int XRay(int attackerSquare, int attackedSquare)
      {
         // Returns the position (or -1, if none) of the next sliding attacker, 
         // hiding behind the attackerSquare, aiming at the attackedSquare.
         int color = board.SquareContents[attackerSquare].pieceColor;      // the currently attacking color
         // find the direction from the attackedSquare to previous attacker
         int direction = bitBoard.Direction[attackedSquare, attackerSquare];
         if (direction == -1)
            return -1;             // this should not happen
         // Find the ray (file, rank or diagonal) , which stretches beyond the attackerSquare 
         // to the edge of the board in the direction of attackedSquare to the attacker.
         ulong ray = bitBoard.Ray[attackerSquare][direction];
         int pieceType;      // the PieceType, next to the Queen, which can make this move.
         if (direction < 4)
            pieceType = Const.RookID;        // 0,1,2,3 : the rank and file directions
         else
            pieceType = Const.BishopID;      // 4,5,6,7 : the diagonal directions  
         // Find the bitboard with possible attackers of the correct type on the ray.
         ulong sliderBB = ray & (board.pieceBB[color, Const.QueenID] | board.pieceBB[color, pieceType]);
         if (sliderBB == 0)
            return -1;         // no slider behind attackerSquare         
         else if (attackerSquare < attackedSquare)
            return bitBoard.MSB(sliderBB);           // ray is below the attacked square. The first is at the MSB.
         else
            return bitBoard.LSB(sliderBB);           // ray is beyond the attacked square. The first is at the LSB.
      }


      public int SEE(int moveType, int fromSquare, int toSquare)
      {
         // quick try :
         /*
         if (moveType != Const.EnPassantCaptureID)
         {
            int quickSEE = evaluator.PieceValues[board.SquareContents[toSquare].pieceType]
                          - evaluator.PieceValues[board.SquareContents[fromSquare].pieceType];
            if (quickSEE > 0)
               return quickSEE;        // this is a winning capture, like a pawn x bishop
         }
         */

         // Based on (=copied from) Crafty, swap.c
         int[] swap_list = new int[32];
         // The attackers bitboard contains the pieces of both white and black which can capture
         // on the attacked square
         ulong attackersBB = GetAttackingDefendingPiecesBitBoard(toSquare);
         // Add the value of the piece on the attacked square
         if (moveType == Const.EnPassantCaptureID)
            swap_list[0] = evaluator.PieceValues[Const.PawnID];
         else
            swap_list[0] = evaluator.PieceValues[board.SquareContents[toSquare].pieceType];
         // the type & value of the primary attacker
         int attackerPieceType = board.SquareContents[fromSquare].pieceType;
         int lastAttackerValue = evaluator.PieceValues[attackerPieceType];
         // remove the original attacker from the attackersBB
         attackersBB &= ~bitBoard.Identity[fromSquare];
         // if a sliding piece is hiding behind the original attacker, add it to the attackers bitboard
         if (attackerPieceType != Const.KingID && attackerPieceType != Const.KnightID)
         {
            int sq = XRay(fromSquare, toSquare);
            if (sq >= 0)
               attackersBB |= bitBoard.Identity[sq];    // add the hiding piece
         }

         // Repeatedly pick out the least valuable type of each color, add it's value to the swap_list 
         // and remove it from the attackersBB. Do this until one color has no more attacks.

         // NB : if en-passant, the toSquare is empty, so get the defending color from the attacking color.
         int color = board.SquareContents[fromSquare].pieceColor ^ 1;      // start with the defenders
         ulong temp;
         int n = 1;      // the index in the swap_list
         while (true)
         {
            bool foundAttacker = false;
            for (int pieceType = Const.PawnID; pieceType >= Const.KingID; pieceType--)
            {
               if ((temp =  board.pieceBB[color, pieceType] & attackersBB) != 0)
               {
                  foundAttacker = true;  // found the least valuable piece of the current color which can attack 
                  int square = bitBoard.LSB_andReset(ref temp);
                  // if a sliding piece is hiding behind the original attacker, add it to the attackers bitboard
                  if (pieceType != Const.KingID && pieceType != Const.KnightID)
                  {
                     int sq = XRay(square, toSquare);
                     if (sq >= 0)
                        attackersBB |= bitBoard.Identity[sq];    // add the hiding piece
                  }
                  // remove this attacker from the attackersBB
                  attackersBB &= ~bitBoard.Identity[square];
                  // append the differential score to the swap_list
                  swap_list[n] = -swap_list[n - 1] + lastAttackerValue;
                  lastAttackerValue = evaluator.PieceValues[pieceType];    // the value of the last attacker
                  n++;
                  color ^= 1;      // flip the color
                  break;          
               }
            }
            if (!foundAttacker)
               break;
         }
         // so, now all possible captures, ranked from least valuable to most valuable is ready
         // 'Bubble' the best outcome to the front of the array
         n--;
         while (n > 0)
         {
            swap_list[n - 1] = -Math.Max(-swap_list[n - 1], swap_list[n]);
            n--;
         }
         return swap_list[0];
      }



   }
}
