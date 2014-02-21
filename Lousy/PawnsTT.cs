using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LousyChess
{

   /// <summary>
   /// A TranspositionTable for storing the difference between the white and black pawn evaluation scores.
   /// The hash is composed of both black and white Pawn positions.
   /// </summary>
   /// <remarks>
   /// Initial board : 40 moves @ 1 second = 93% hits.  40 moves @8/0 : 10% faster
   /// </remarks>
   class PawnsTT
   {

      private const int slotSize = 20 + 2 * 8 * 4;   // = 80 bytes
      private const int defaultTTSizeInKB = 500;     // ~6250 entries

      [StructLayout(LayoutKind.Sequential)]
      public struct Slot
      {
         // try to keep the most used stuff on 4-byte boundaries
         public ulong hashValue;                // hashValue == 0 : empty slot
         public int pawnDiffScoreStart;         // white minus black pawns score @ start of game
         public int pawnDiffScoreEnd;           // white minus black pawns score @ start of endgame
         public int[,] LeastAdvancedPawns;      // = new int[2, 8];
         public ulong WhitePawnAttackBB;               // The white pawn attack bitboards
         public ulong BlackPawnAttackBB;               // The black pawn attack bitboards
      }


      public Slot[] slots;
      private int maxNrSlots;


      public PawnsTT()
      {
         SetTTSizeInKB(defaultTTSizeInKB);
      }


      public void SetTTSizeInKB(int nrKB)
      {
         maxNrSlots = nrKB * 1024 / slotSize;
         slots = new Slot[maxNrSlots];
      }


      public void Clear()
      {
         for (int i = 0; i < maxNrSlots; i++)
         {
            slots[i].hashValue = 0;
            slots[i].LeastAdvancedPawns = null;
         }
      }


      public void Put(ulong hashValue, int pawnDiffScoreStart, int pawnDiffScoreEnd, ulong[] PawnAttackBBs, int[,] LeastAdvancedPawns)
      {
         // only use the lower 32 bits to find an index. The upper 32 bits adds to the hashValue check.
         int index = ((int)hashValue) % maxNrSlots;
         // LeastAdvancedPawns : make a local copy
         int[,] advPawns = new int[2, 8];
         for (int i = 0; i < 2; i++)
            for (int j = 0; j < 8; j++)
               advPawns[i, j] = LeastAdvancedPawns[i, j];
         ulong[] pawnAttackBB = new ulong[2];

         slots[index].hashValue = hashValue;
         slots[index].pawnDiffScoreStart = pawnDiffScoreStart;
         slots[index].pawnDiffScoreEnd = pawnDiffScoreEnd;
         slots[index].LeastAdvancedPawns = advPawns;
         slots[index].WhitePawnAttackBB = PawnAttackBBs[0];
         slots[index].BlackPawnAttackBB = PawnAttackBBs[1];
      }


      public int GetIndex(ulong hashValue)
      {
         // only use the lower 32 bits to find an index. The upper 32 bits adds to the hashValue check.
         int index = ((int)hashValue) % maxNrSlots;
         if (slots[index].hashValue == hashValue)
            return index;
         else
            return -1;
      }


   }
}
