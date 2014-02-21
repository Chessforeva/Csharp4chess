//#define UseMagicLSB  // = set is 0.4% slower

using System;
using System.Collections.Generic;
using System.Text;

namespace LousyChess
{

   /// <summary>
   /// Miscelleaneous low-level bitboard related stuff
   /// </summary>
   public class BitBoard
   {

      // Direction constants : North is pointing from White to Black
      public const int North = 0;
      public const int East  = 1;
      public const int South = 2;
      public const int West  = 3;
      public const int NorthEast = 4;
      public const int SouthEast = 5;
      public const int SouthWest = 6;
      public const int NorthWest = 7;

      public ulong All1sBB;    // a bitboard with a 1 at each position

      // Just a 1 at the position bit, a 0 on all other bits
      // { for normal bitboard : 64 bit : starting from A1..H1, B2..H2 ,,,,,,, A8..H8 ]
      public ulong[] Identity = new ulong[Const.NrSquares];

      // a matrix of bitboards. For each square 8 bitboards, with the squares which can be reached 
      // from this square. 8 directions :
      // 0=N, 1=E, 2=S, 3=W   , 4=NE, 5=SE, 6=SW, 7=NW
      public ulong[][] Ray;

      // A matrix with FromSquares and ToSquares. The contents of each element is the Direction (see before)
      // how one should travel in a straight line from the FromSquare to the ToSquare. 
      // If it is not possible in a straight line, the element is -1;
      public int[,] Direction = new int[Const.NrSquares, Const.NrSquares];

      // An array with bitboards, representing a rectangle around each square
      public ulong[] Box1;    // a 3x3 rectangle with 1's. 0's inside and outside
      public ulong[] Box2;    // a 5x5 rectangle with 1's. 0's inside and outside

      // An array with bitboards, with 1's on the respective file and 0's otherwise
      public ulong[] FileBB1 = new ulong[8];
      // An array with bitboards, with 0's on the respective file and 1's otherwise
      public ulong[] FileBB0 = new ulong[8];
      // An array with bitboards, with 1's on the respective rank and 0's otherwise
      public ulong[] RankBB1 = new ulong[8];
      // An array with bitboards, with 0's on the respective rank and 1's otherwise
      public ulong[] RankBB0 = new ulong[8];

      // bitmasks with 1's on all ranks which are in front of this rank
      public ulong[,] AllRanksInFrontBB1 = new ulong[2, 8];
      // bitmasks with 1's on the file in front of this square. 
      public ulong[,] FileInFrontBB1 = new ulong[2, 64];
      // bitmasks with 1's in front of this square, on files left and right of the square. 
      public ulong[,] FilesLeftRightInFrontBB1 = new ulong[2, 64];

      public ulong[,] PawnAttackBB1 = new ulong[2, 64];   // 0=white, 1 = black


      public BitBoard()
      {
         CreateIdentity();
         Initialize_msb_lsb();
         InitializeRays();
         InitializeDirections();    // NB. Rays must have been initialized
         CreateBoxes();
         CreateFileRankBBs();
         CreatePawnAttackBBs();
      }

      #region Identity

      private void CreateIdentity()
      {
         ulong q = 1;
         for (int i = 0; i < Const.NrSquares; i++)
         {
            Identity[i] = q;
            All1sBB |= q;             // the BB with all 1's
            q <<= 1;
         }
      }

      #endregion


      #region Initialize Rays and Direction

      private void InitializeRays()
      {
         // A matrix of bitboards. For each square 8 bitboards, with the squares which can be reached 
         // from this square. 8 directions :
         // 0=N, 1=E, 2=S, 3=W   , 4=NE, 5=SE, 6=SW, 7=NW
         // So, North is increasing rank, East is increasing rile etc.
         Ray = new ulong[Const.NrSquares][];
         for (int squareNr = 0; squareNr < Const.NrSquares; squareNr++)
         {
            Ray[squareNr] = new ulong[8];
            for (int i=0; i<8; i++)
               Ray[squareNr][i] = 0;

            // North
            int rank = squareNr / 8;
            int file = squareNr % 8;
            while (true)
            {
               rank++;
               if (rank == 8)
                  break;
               Ray[squareNr][North] |= Identity[8 * rank + file];
            }
            // East
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               file++;
               if (file == 8)
                  break;
               Ray[squareNr][East] |= Identity[8 * rank + file];
            }
            // South
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               rank--;
               if (rank == -1)
                  break;
               Ray[squareNr][South] |= Identity[8 * rank + file];
            }
            // West
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               file--;
               if (file == -1)
                  break;
               Ray[squareNr][West] |= Identity[8 * rank + file];
            }
            // NorthEast
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               rank++;
               file++;
               if (rank == 8 || file == 8)
                  break;
               Ray[squareNr][NorthEast] |= Identity[8 * rank + file];
            }
            // SouthEast
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               rank--;
               file++;
               if (rank == -1 || file == 8)
                  break;
               Ray[squareNr][SouthEast] |= Identity[8 * rank + file];
            }
            // SouthWest
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               rank--;
               file--;
               if (rank == -1 || file == -1)
                  break;
               Ray[squareNr][SouthWest] |= Identity[8 * rank + file];
            }
            // NorthWest
            rank = squareNr / 8;
            file = squareNr % 8;
            while (true)
            {
               rank++;
               file--;
               if (rank == 8 || file == -1)
                  break;
               Ray[squareNr][NorthWest] |= Identity[8 * rank + file];
            }
         }

      }


      private void InitializeDirections()
      {
         // A matrix with FromSquares and ToSquares. The contents of each element is the Direction (see below)
         // how one should travel in a straight line from the FromSquare to the ToSquare. 
         // If it is not possible in a straight line, the element is -1;
         for (int fromSquare = 0; fromSquare < Const.NrSquares; fromSquare++)
            for (int toSquare = 0; toSquare < Const.NrSquares; toSquare++)
            {
               Direction[fromSquare, toSquare] = -1;
               ulong toBB = Identity[toSquare];
               for (int i = 0; i < 8; i++)
                  if ( (Ray[fromSquare][i] & toBB) != 0)
                  {
                     Direction[fromSquare, toSquare] = i;
                     break;
                  }
            }
      }

      #endregion


      #region least & most significant bit in a BitBoard : LSB()  &  MSB()

      int[] msb = new int[65536];
      int[] lsb = new int[65536];

      private void Initialize_msb_lsb()
      {
         // from Crafty (Robert Hyatt) : init.c
         msb[0] = 16;
         lsb[0] = 16;
         for (int i = 1; i < 65536; i++)
         {
            lsb[i] = 16;
            for (int j = 0; j < 16; j++)
               if ((i & (1 << j)) != 0)
               {
                  msb[i] = j;
                  if (lsb[i] == 16)
                     lsb[i] = j;
               }
         }
      }


      /// <summary>
      /// Returns the leading bit in a non-zero bitboard (= most significant bit). 
      /// </summary>
      /// <param name="bb">The bitboard (MUST be != 0).</param>
      /// <returns>The most significant 1 bit.</returns>
      /// <remarks>
      /// Algorithm by Robert Hyatt (Crafty, boolean.c).
      /// </remarks>
      public int MSB(ulong bb)
      {
         // take 8 ns on Core2Duo@3GHz
         if (bb >> 48 != 0)
            return (msb[bb >> 48] + 48);
         if (((bb >> 32) & 65535) != 0)
            return (msb[(bb >> 32) & 65535] + 32);
         if (((bb >> 16) & 65535) != 0)
            return (msb[(bb >> 16) & 65535] + 16);
         return (msb[bb & 65535]);
      }


      /// <summary>
      /// Returns the leading bit in a non-zero bitboard (= most significant bit)
      /// and removes it from the bitboard.
      /// </summary>
      /// <param name="bb">The bitboard (MUST be != 0).</param>
      /// <returns>The most significant 1 bit.</returns>
      /// <remarks>
      /// Algorithm by Robert Hyatt (Crafty, boolean.c).
      /// </remarks>
      public int MSB_andReset(ref ulong bb)
      {
         int result;
         if (bb >> 48 != 0)
            result = msb[bb >> 48] + 48;
         else if (((bb >> 32) & 65535) != 0)
            result = msb[(bb >> 32) & 65535] + 32;
         else if (((bb >> 16) & 65535) != 0)
            result = msb[(bb >> 16) & 65535] + 16;
         else 
            result = msb[bb & 65535];
         bb &= ~Identity[result];
         return result;
      }


#if !UseMagicLSB

      /// <summary>
      /// Returns the trailing bit in a non-zero bitboard (= least significant bit).
      /// </summary>
      /// <param name="bb">The bitboard (MUST be != 0).</param>
      /// <returns>The least significant 1 bit.</returns>
      /// <remarks>
      /// Algorithm by Robert Hyatt (Crafty, boolean.c).
      /// </remarks>
      public int LSB
         (ulong bb)
      {
         // take 8 ns on Core2Duo@3GHz
         if ((bb & 65535) != 0)
            return (lsb[bb & 65535]);
         if (((bb >> 16) & 65535) != 0)
            return (lsb[(bb >> 16) & 65535] + 16);
         if (((bb >> 32) & 65535) != 0)
            return (lsb[(bb >> 32) & 65535] + 32);
         return (lsb[bb >> 48] + 48);
      }


      /// <summary>
      /// Returns the trailing bit in a non-zero bitboard (= least significant bit),
      /// and removes it from the bitboard.
      /// </summary>
      /// <param name="bb">The bitboard (MUST be != 0).</param>
      /// <returns>The least significant 1 bit.</returns>
      /// <remarks>
      /// Algorithm by Robert Hyatt (Crafty, boolean.c).
      /// </remarks>
      public int LSB_andReset(ref ulong bb)
      {
         int result;
         if ((bb & 65535) != 0)
            result = lsb[bb & 65535];
         else if (((bb >> 16) & 65535) != 0)
            result = lsb[(bb >> 16) & 65535] + 16;
         else if (((bb >> 32) & 65535) != 0)
            result = lsb[(bb >> 32) & 65535] + 32;
         else
            result = lsb[bb >> 48] + 48;
         //    bb &= ~Identity[result];
         bb &= bb - 1;    // fast way to reset least significant bit
         return result;
      }

#endif


      #endregion


      #region LSB, using magic

#if UseMagicLSB

      // http://www.pradu.us/old/Nov27_2008/Buzz/research/magic/Bitboards.pdf

      const long LSB_MAGIC = 0x07EDD5E59A4E28C2;

      /*
      int[] LSB_Database = new int[64];

      void InitializeLSBMagic()
      {
         // populate the magic LSB_Database
         ulong bit = 1;
         int i = 0;
         do
         {
            LSB_Database[(bit * LSB_MAGIC) >> 58] = i;
            i++;
            bit <<= 1;
         } while (bit != 0);
      }
      */

      // = LSB_Database, hardcoded
      static int[] LSB_Database =
                       {
                          63, 00, 58, 01, 59, 47, 53, 02,
                          60, 39, 48, 27, 54, 33, 42, 03,
                          61, 51, 37, 40, 49, 18, 28, 20,
                          55, 30, 34, 11, 43, 14, 22, 04,
                          62, 57, 46, 52, 38, 26, 32, 41,
                          50, 36, 17, 19, 29, 10, 13, 21,
                          56, 45, 25, 31, 35, 16, 09, 12,
                          44, 24, 15, 08, 23, 07, 06, 05
                     };


      /// <summary>
      /// Returns the trailing bit in a non-zero bitboard (= least significant bit).
      /// </summary>
      /// <param name="bb">The bitboard (MUST be != 0).</param>
      /// <returns>The least significant 1 bit.</returns>
      public int LSB(ulong bb)
      {
     //    long X = (long)bb;
         //    return LSB_Database[(ulong)((X & -X) * LSB_MAGIC) >> 58];
         return LSB_Database[((bb & (0-bb)) * LSB_MAGIC) >> 58];
      }


      /// <summary>
      /// Returns the trailing bit in a non-zero bitboard (= least significant bit),
      /// and removes it from the bitboard.
      /// </summary>
      /// <param name="bb">The bitboard (MUST be != 0).</param>
      /// <returns>The least significant 1 bit.</returns>
      public int LSB_andReset(ref ulong bb)
      {
        // long X = (long)bb;
         // return LSB_Database[(ulong)((X & -X) * LSB_MAGIC) >> 58];
         int N = LSB_Database[((bb & (0-bb)) * LSB_MAGIC) >> 58];
         bb &= bb - 1;          // fast way to reset least significant bit
         return N;
      }

#endif

      #endregion



      #region alternative LSB, slightly faster on 32 bits

      static int[] LSB_Database2 =
                       {
                         63, 30,  3, 32, 59, 14, 11, 33,
                         60, 24, 50,  9, 55, 19, 21, 34,
                         61, 29,  2, 53, 51, 23, 41, 18,
                         56, 28,  1, 43, 46, 27,  0, 35,
                         62, 31, 58,  4,  5, 49, 54,  6,
                         15, 52, 12, 40,  7, 42, 45, 16,
                         25, 57, 48, 13, 10, 39,  8, 44,
                         20, 47, 38, 22, 17, 37, 36, 26
                       };



      public int LSB2(ulong bb)
      {
         // returns the index of the least sigificant bit (0..63)
         // from : http://chessprogramming.wikispaces.com/BitScan
         // author : Matt Taylor (32-bit friendly implementation)
         // assuming bitboard != 0   !!!
         bb ^= (bb - 1);
         uint folded = (uint)(bb ^ (bb >> 32));
         return LSB_Database2[folded * 0x78291ACF >> 26];
      }

      public int LSB_andReset2(ref ulong bb)
      {
         // returns the index of the least sigificant bit (0..63)
         // from : http://chessprogramming.wikispaces.com/BitScan
         // author : Matt Taylor (32-bit friendly implementation)
         // assuming bitboard != 0   !!!
         ulong bb2 = bb;
         bb2 ^= (bb2 - 1);
         bb &= bb - 1;          // fast way to reset least significant bit
         uint folded = (uint)(bb2 ^ (bb2 >> 32));
         return LSB_Database2[folded * 0x78291ACF >> 26];
      }

      #endregion


      #region PopCount

      public static int PopCount(ulong bb)
      {
         // Returns the number of 1 bits in the bitboard bb.
         // from : http://chessprogramming.wikispaces.com/Population+Count
         const ulong k1 = 0x5555555555555555;
         const ulong k2 = 0x3333333333333333;
         const ulong k4 = 0x0f0f0f0f0f0f0f0f;
         const ulong kf = 0x0101010101010101;
         bb = bb - ((bb >> 1) & k1);          //put count of each 2 bits into those 2 bits
         bb = (bb & k2) + ((bb >> 2) & k2);   //put count of each 4 bits into those 4 bits
         bb = (bb + (bb >> 4)) & k4;          //put count of each 8 bits into those 8 bits
         bb = (bb * kf) >> 56;                //returns 8 most significant bits of x + (x<<8) + (x<<16) + (x<<24) + ...
         return (int)bb;
      }

      #endregion

      
      #region rectangularMasks

      private ulong[] CalculateRectangularMasks(int delta)
      {
         // Calculates rectangular bitmasks as bitboards around each square.
         // The center position is not set.
         // Delta=1, 1 'layer' around each square. Delta=2, 2 'layers' around each square, etc.
         ulong[] result = new ulong[64];
         for (int i = 0; i < 64; i++)
         {
            ulong bitmask = 0;
            int file0 = i % 8;
            int rank0 = i / 8;
            for (int m = -delta; m <= delta; m++)
            {
               int y = rank0 + m;
               if (y < 0 || y > 7)
                  continue;
               for (int n = -delta; n <= delta; n++)
               {
                  if (m == 0 && n == 0)
                     continue;               // exclude center
                  int x = file0 + n;
                  if (x < 0 || x > 7)
                     continue;
                  bitmask |= Identity[y * 8 + x];
               }
            }
            result[i] = bitmask;
         }
         return result;
      }


      private void CreateBoxes()
      {
         Box1 = CalculateRectangularMasks(1);
         Box2 = CalculateRectangularMasks(2);
         for (int i = 0; i < 64; i++)
            Box2[i] &= ~Box1[i];         // set inside to 0;
      }

      #endregion


      private void CreateFileRankBBs()
      {
         // the files
         for (int i = 0; i < 8; i++)
         {
            FileBB1[i] = 0;
            FileBB0[i] = All1sBB;
         }
         for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
               int squareNr = x + y * 8;
               FileBB1[x] |= Identity[squareNr];
               FileBB0[x] &= ~Identity[squareNr];
            }
         // the ranks
         for (int i = 0; i < 8; i++)
         {
            RankBB1[i] = 0;
            RankBB0[i] = All1sBB;
         }
         for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
               int squareNr = x + y * 8;
               RankBB1[y] |= Identity[squareNr];
               RankBB0[y] &= ~Identity[squareNr];
            }
         // AllRanksInFrontBB1
         for (int y = 0; y < 8; y++)
         {
            AllRanksInFrontBB1[0, y] = 0;
            AllRanksInFrontBB1[1, y] = 0;
            // for white :
            for (int yy = y + 1; yy < 8; yy++)
               AllRanksInFrontBB1[0, y] |= RankBB1[yy];
            // for black :
            for (int yy = y - 1; yy >= 0; yy--)
               AllRanksInFrontBB1[1, y] |= RankBB1[yy];
         }
         // FileInFrontBB1
         for (int color = 0; color < 2; color++)
            for (int i = 0; i < 64; i++)
            {
               int x = i % 8;
               int y = i / 8;
               FileInFrontBB1[color, i] = FileBB1[x] & AllRanksInFrontBB1[color, y];
            }
         // FilesLeftRightInFrontBB1
         for (int color = 0; color < 2; color++)
            for (int i = 0; i < 64; i++)
            {
               int x = i % 8;
               int y = i / 8;
               FileInFrontBB1[color, i] = 0;
               if (x > 0)
                  FileInFrontBB1[color, i] |= FileBB1[x - 1] & AllRanksInFrontBB1[color, y];
               if (x < 7)
                  FileInFrontBB1[color, i] |= FileBB1[x + 1] & AllRanksInFrontBB1[color, y];
            }
      }


      private void CreatePawnAttackBBs()
      {
         // NB : en passant is NOT included !!
         // Include captures from rank 1 (white) and rank 8 (black) since these are used in Board.IsInCheck
         //
         // for white
         for (int i = 0; i < Const.NrSquares; i++)
         {
            PawnAttackBB1[Const.White, i] = 0;
            if (i <= 55)
            {
               int x = i % 8;
               if (x > 0)
                  PawnAttackBB1[Const.White, i] |= Identity[i + 7];    // left
               if (x < 7)
                  PawnAttackBB1[Const.White, i] |= Identity[i + 9];    // right
            }
         }
         // for black
         for (int i = 0; i < Const.NrSquares; i++)
         {
            PawnAttackBB1[Const.Black, i] = 0;
            if (i >= 8)
            {
               int x = i % 8;
               if (x > 0)
                  PawnAttackBB1[Const.Black, i] |= Identity[i - 9];    // left
               if (x < 7)
                  PawnAttackBB1[Const.Black, i] |= Identity[i - 7];    // right
            }
         }
      }


   }
}
