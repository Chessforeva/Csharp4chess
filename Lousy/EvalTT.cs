using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LousyChess
{

   /// <summary>
   /// A TranspositionTable for storing the evaluation.
   /// </summary>
   class EvalTT
   {

      private const int slotSize = 16;
      private const int defaultTTSizeInMB = 2;    // makes it ~ 2% faster

      [StructLayout(LayoutKind.Sequential)]
      public struct Slot
      {
         public ulong hashValue;                // hashValue == 0 : empty slot
         public int scoreStart;
         public int scoreEnd;
      }


      public Slot[] slots;
      private int maxNrSlots;


      public EvalTT()
      {
         SetTTSizeInKB(defaultTTSizeInMB);
      }


      public void SetTTSizeInKB(int nrMB)
      {
         maxNrSlots = nrMB * 1024 * 1024 / slotSize;
         slots = new Slot[maxNrSlots];
      }


      public void Clear()
      {
         for (int i = 0; i < maxNrSlots; i++)
            slots[i].hashValue = 0;
      }


      public void Put(ulong hashValue, int scoreStart, int scoreEnd)
      {
         // only use the lower 32 bits to find an index. The upper 32 bits adds to the hashValue check.
         int index = ((int)hashValue) % maxNrSlots;
         slots[index].hashValue = hashValue;
         slots[index].scoreStart = scoreStart;
         slots[index].scoreEnd = scoreEnd;
      }


      public int Get(ulong hashValue)
      {
         // only use the lower 32 bits to find an index. The upper 32 bits adds to the hashValue check.
         int index = ((int)hashValue) % maxNrSlots;
         // Use (index > 0) , not initialized(=0) gives an index of 0. Treat it as not found.
         if (index > 0 && slots[index].hashValue == hashValue)
            return index;
         else
            return -1;         // not found
      }


   }
}
