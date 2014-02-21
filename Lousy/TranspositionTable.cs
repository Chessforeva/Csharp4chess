// todo: tweak settings

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LousyChess
{


   public class TranspositionTable
   {

      // this must go
      public int nrExactEntries = 0;
      public int nrUpperEntries = 0;
      public int nrLowerEntries = 0;
      public int nrDeepStores = 0;
      public int nrFreshStores = 0;
      public int nrCacheFull = 0;

      private int nrFilledDeepSlots = 0;

      public int NrDeepHits = 0;
      public int NrFreshHits = 0;
      public int NrMisses = 0;

      public const int exactBound = 0;
      public const int upperBound = 1;
      public const int lowerBound = 2;

      public ulong[, ,] SquareHashValue = new ulong[Const.NrSquares, Const.NrPieceTypes, Const.NrColors];      // square, piecetype, color
      public ulong[] EPSquareValue = new ulong[Const.NrSquares];        // only used if there is an enPassant square
      public ulong[] CanCastleQSHashValue = new ulong[Const.NrColors];
      public ulong[] CanCastleKSHashValue = new ulong[Const.NrColors];
      public ulong ChangeMoveColor;

      private const int defaultTTSizeInMB = 24;   // 24 = 1 million entries

      [StructLayout(LayoutKind.Sequential)]
      public struct Slot
      {
         // try to keep the most used stuff on 4-byte boundaries
         public ulong hashValue;      // hashValue == 0 : empty slot
         public int score;
         public int compressedMove;
         public short flags;
         public short moveNr;
         public short depth;
      }

      public Slot[] slots;
      private int maxNrSlots;

      private int maxNrMoveAge = 1;             // allow overwriting older moves 


      public TranspositionTable()
      {
         SetTTSizeInMB(defaultTTSizeInMB);
         MakeZobristNrs(12345);
      }


      public void SetTTSizeInMB(int nrMB)
      {
#if SILVERLIGHT
         int SlotSize = 24;
#else
         Slot testSlot = new Slot();
         int SlotSize = Marshal.SizeOf(testSlot);   // 24 bytes
#endif
         maxNrSlots = nrMB * 1024 * 1024 / SlotSize;
         if (maxNrSlots % 2 == 1)
            maxNrSlots++;                  // always an even nr of slots, for the 2-level cache
         slots = new Slot[maxNrSlots];
      }


      public void MakeZobristNrs(int seedValue)
      {
         // generate the Zobrist random numbers (64 bits)
         Random rnd;
         if (seedValue == 0)
            rnd = new Random();
         else
            rnd = new Random(seedValue);
         // squares, pieces, color
         for (int i = 0; i < Const.NrSquares; i++)
            for (int j=0; j<6; j++)
               for (int k=0; k<2; k++)
                  SquareHashValue[i,j,k] = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
         // toggle color to move
         ChangeMoveColor = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
         // en-passant squares
         for (int i=0; i<64; i++)
            EPSquareValue[i] = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
         // Can castle info
         for (int i = 0; i < 2; i++)
         {
            CanCastleQSHashValue[i] = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
            CanCastleKSHashValue[i] = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
         }
      }

      public void Clear()
      {
         for (int i = 0; i < maxNrSlots; i++)
         {
            slots[i].hashValue = 0;
            slots[i].depth = -1;      // easy way to detect unused slot
         }
         //
         nrExactEntries = 0;
         nrLowerEntries = 0;
         nrUpperEntries = 0;
         nrDeepStores = 0;
         nrFreshStores = 0;
         nrCacheFull = 0;
         //
         NrDeepHits = 0;
         NrFreshHits = 0;
         NrMisses = 0;
         //
         nrFilledDeepSlots = 0;
      }


      public string[] ShowStatistics()
      {
         List<string> result = new List<string>();
         result.Add(Environment.NewLine);
         result.Add("nr Deep stores   " + nrDeepStores.ToString() + Environment.NewLine);
         result.Add("nr Fresh stores  " + nrFreshStores.ToString() + Environment.NewLine);
         result.Add("nr Exact entries " + nrExactEntries.ToString() + Environment.NewLine);
         result.Add("nr Upper entries " + nrLowerEntries.ToString() + Environment.NewLine);
         result.Add("nr Lower entries " + nrUpperEntries.ToString() + Environment.NewLine);
         //
         result.Add(Environment.NewLine);
         result.Add("nr Deep hits  " + NrDeepHits.ToString() + Environment.NewLine);
         result.Add("nr Fresh hits " + NrFreshHits.ToString() + Environment.NewLine);
         result.Add("nr misses     " + NrMisses.ToString() + Environment.NewLine);
         return result.ToArray();
      }

      public int GetTTFullPerMill()
      {
         // return the percentage permil the deep TT is full (for UCI statistics).
         return (1000 * nrFilledDeepSlots) / (maxNrSlots / 2);
      }
 
      // 2-level cache : Deep and Fresh
      // Put : if the depth >= stored depth in Deep, the Deep value is copied to Fresh and the 
      //       new entry is stored in Deep.
      //       If the depth < stored depth in Deep, it is always stored in Fresh
      // Get : First check Deep. If not found, check Fresh.
      // Technical : the Deep values are stored in the even slots, the Fresh in the oddd slots.

      public void Put(ulong hashValue, int score, int bound, int depth, int moveNr, int compressedMove)
      {
         bool canStore = false;
         // only use the lower 32 bits to find an index. The upper 32 bits adds to the hashValue check.
         int index = ((int)hashValue) % maxNrSlots;
         if (index % 2 == 1)
            index--;                // first check the Deep slots (even)
         if (slots[index].hashValue == 0)
            nrFilledDeepSlots++;              // use this for cache full statistics for UCI
         // Can store 'deep' if the slot is empty, the new depth >= stored depth, or the stored value is to old.
         canStore =    moveNr - slots[index].moveNr >= maxNrMoveAge    // slot is to old 
                    || slots[index].depth <= depth;      // same or better depth (NB: TT cleared, depth -> -1 = empty)
         if (canStore)
         {
            nrDeepStores++;
            slots[index + 1] = slots[index];  // copy the Deep slot to the Fresh slot
         }
         else
         {
            nrFreshStores++;
            index++;  // store it in the fresh slot
         }
         //
         if (bound == exactBound)
            nrExactEntries++;
         else if (bound == lowerBound)
            nrLowerEntries++;
         else
            nrUpperEntries++;
         //  
         slots[index].hashValue = hashValue;
         slots[index].score = score;
         slots[index].compressedMove = compressedMove;
         slots[index].flags = (short)bound;
         slots[index].moveNr = (short)moveNr;
         slots[index].depth = (short)depth;
      }


      public int GetIndex(ulong hashValue, int requiredDepth)
      {
         // only use the lower 32 bits to find an index. The upper 32 bits adds to the hashValue check.
         int index = ((int)hashValue) % maxNrSlots;
         if (index % 2 == 1)
            index--;            
         // first check the Deep entry
         if (slots[index].hashValue == hashValue && slots[index].depth >= requiredDepth)
         {
            NrDeepHits++;
            return index;
         }
         else
         {
            // Not found, try the Fresh slot
            index++;
            if (slots[index].hashValue == hashValue && slots[index].depth >= requiredDepth)
            {
               NrFreshHits++;
               return index;
            }
         }
         NrMisses++;
         return -1;
      }


   }
}
