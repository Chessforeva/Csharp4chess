/*
    CuckooChess - A java chess program.
    Copyright (C) 2011  Peter Österlund, peterosterlund2@gmail.com

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/* Java code has been partially ported to C# by http://chessforeva.blogspot.com
    Just because this is extremely strong java chess engine.
    2012.feb.
 
   There is no complete UCI interface, logs of SearchTrees, file-operations etc.
   Anyway, chess engine is working ok.
 */



using System;
using System.Diagnostics;

namespace Cuckoo
{
    public class Cuckoo
    {
        // This calls lots of constructors needed for variables definition
        BitBoard defBitBoard = new BitBoard();
        Evaluate defEvaluate = new Evaluate();

        public static ComputerPlayer CuckComp;
        public static HumanPlayer CuckHumn;
        public static Game CuckGM;
        public static Book CuckBK;

        public Cuckoo()
        {
            // Or just run samples:
            //Sample();

         }

        public static string simplyCalculateMove(string sFEN)
        {
            bool w2m = sFEN.Contains(" w ");
            CuckComp = new ComputerPlayer();
            CuckHumn = new HumanPlayer();
            if(w2m) CuckGM = new Game(CuckComp, CuckHumn);
            else CuckGM = new Game(CuckHumn, CuckComp);
            CuckGM.handleCommand("setpos " + sFEN);
            Position pos = CuckGM.getPos();

            CuckComp.maxTimeMillis = 1 * 100;
            CuckComp.maxTimeMillis = 6 * 100;
            CuckComp.maxDepth = 8;

            string CurrentPositionFEN = TextIO.toFEN(pos);

            string cmd = CuckComp.getCommand(new Position(pos),
                    CuckGM.haveDrawOffer(), CuckGM.getHistory());

            string a = cmd;
            if ((a.Length > 1) && (("NBRQK").IndexOf(a[0]) >= 0)) a = a.Substring(1);
            if (a[0]=='O') a=(a.Length==3 ? (w2m?"e1g1":"e8g8") : (w2m?"e1c1":"e8c8"));
            else a = ((a.Length > 4) ? a.Substring(0, 2) + a.Substring(3) : "");
            return a;
        }

        private void Sample()
        {
            CuckComp = new ComputerPlayer();
            CuckHumn = new HumanPlayer();
            CuckBK = new Book(false);
            CuckGM = new Game(CuckHumn, CuckComp);
            Position pos = CuckGM.getPos();

            // e4(102) d4(31) ...
            string CurrentBookMoves = CuckBK.getAllBookMoves(pos);

            // Nb1-a3;Nb1-c3;...;a2-a3;a2-a4;...
            string CurrentValidMoves = TextIO.AllMovesTostring(pos, true);

            // RNB...w KQkq... 
            string CurrentPositionFEN = TextIO.toFEN(pos);

            // Display board to console
            TextIO.DispBoard(pos);

            // Swap & move
            CuckGM.whitePlayer = CuckComp;
            CuckGM.blackPlayer = CuckHumn;
            //CuckComp.bookEnabled = false;
            CuckComp.maxTimeMillis = 1 * 100;
            CuckComp.maxTimeMillis = 6 * 100;
            //CuckComp.maxDepth = 6;

            // Ng1-f3
            string CommandFromComp = CuckComp.getCommand(new Position(pos),
                                CuckGM.haveDrawOffer(), CuckGM.getHistory());        
         }


    }





    // Helping classes


    public class Defs
    {
    public static ulong ulongN1 = 0xFFFFFFFFFFFFFFFF;
    }

    public class SystemHelper
    {
        public static long currentTimeMillis()
        {
            return DateTime.UtcNow.Millisecond;
        }

        public static void println(string s)
        {
            /* display nothing */
            // System.Diagnostics.Debug.WriteLine(s);
        }
        public static void printf(string s) { println(s); }
	}
   
    public class RuntimeException: System.Exception
    {
        public RuntimeException()
        {  SystemHelper.println("RuntimeException"); }
    }
    public class NumberFormatException : System.Exception
    {
        public NumberFormatException()
        {  SystemHelper.println("NumberFormatException"); }
    }
    public class IOException : System.Exception
    {
        public IOException()
        {  SystemHelper.println("IOException"); }
    }
    public class NoSuchAlgorithmException : System.Exception
    {
        public NoSuchAlgorithmException()
        {  SystemHelper.println("NoSuchAlgorithmException"); }
    }
    public class UnsupportedOperationException : System.Exception
    {
        public UnsupportedOperationException()
        { SystemHelper.println("UnsupportedOperationException"); }
    }
    public class UnsupportedSHA1OperationException : System.Exception
    {   public UnsupportedSHA1OperationException()
        { SystemHelper.println("Unsupported SHA-1 OperationException"); }
    }
    public class ChessParseError : System.Exception
    {
        public ChessParseError()
        { SystemHelper.println("ChessParseError"); }
    }

    public static class BITS
    {
        private static bool bitcntinit = false;
        private static ulong[] BIT;
        private static byte[] LSB;
        private static byte[] BITC;

        private static int LOW16(ulong x) { return (int)((x) & 0xFFFF); }
        private static int LOW32(ulong x) { return (int)((x) & 0xFFFFFFFFL); }
        private static ulong L32(ulong x) { return ((x) & 0xFFFFFFFFL); }

        public static ulong Neg(ulong n) { return ((~n) + 1); }

        private static byte _bitcnt(ulong bit)
        {
            byte c = 0;
            while (bit != 0) { bit &= (bit - 1); c++; }
            return c;
        }

        public static byte bitCount(ulong n)
        {
            if (!bitcntinit)
            {
                BIT = new ulong[64];
                LSB = new byte[0x10000];
                BITC = new byte[0x10000];
                for (ulong i = 0; i < 0x10000; i++) BITC[i] = _bitcnt(i);
                bitcntinit = true;
            }
            byte a1 = (BITC[LOW16(n)]);
            ulong g2 = n >> 16;
            byte a2 = (BITC[LOW16(g2)]);
            ulong g3 = n >> 32;
            byte a3 = (BITC[LOW16(g3)]);
            ulong g4 = n >> 48;
            byte a4 = (BITC[LOW16(g4)]);

            return (byte)(BITC[LOW16(n)]
                + BITC[LOW16(n >> 16)]
                + BITC[LOW16(n >> 32)]
                + BITC[LOW16(n >> 48)]);
        }

    }

}
