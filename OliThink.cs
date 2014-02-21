/* OliThink5 Java(c) Oliver Brausch 28.Jan.2010, ob112@web.de, http://home.arcor.de/dreamlike */
/* port to C# by Chessforeva.blogspot.com */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

/*
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.security.AccessControlException;
import java.util.HashMap;
import java.util.Random;
import java.util.StringTokenizer;
 */

namespace OliThink
{
    public class OliThink
	{

	const String VER = "5.3.0 Java";

	const int PAWN = 1;
	const int KNIGHT = 2;
	const int KING = 3;
	const int ENP = 4;
	const int BISHOP = 5;
	const int ROOK = 6;
	const int QUEEN = 7;

	const int CNODES = 0xFFFF;
	const int HEUR = 9900000;
    private static int[] pval = { 0, 100, 290, 0, 100, 310, 500, 950 };
    private static int[] capval = { 0, HEUR + 1, HEUR + 2, 0, HEUR + 1, HEUR + 2, HEUR + 3, HEUR + 4 };
    private static int[] pawnrun = { 0, 0, 1, 8, 16, 32, 64, 128 };

	private int FROM(int x) { return ((x) & 63); }
	private int TO(int x) { return (((x) >> 6) & 63); }
	private int PROM(int x) { return (((x) >> 12) & 7); }
	private int PIECE(int x) { return (((x) >> 15) & 7); }
	private int ONMV(int x) { return (((x) >> 18) & 1); }
	private int CAP(int x) { return (((x) >> 19) & 7); }

	private int _TO(int x) { return ((x) << 6); }
	private int _PROM(int x) { return ((x) << 12); }
	private int _PIECE(int x) { return ((x) << 15); }
	private int _ONMV(int x) { return ((x) << 18); }
	private int _CAP(int x) { return ((x) << 19); }
	private int PREMOVE(int f, int p, int c) { return ((f) | _ONMV(c) | _PIECE(p)); }

	private long RATT1(int f) { return rays[((f) << 7) | key000(BOARD(), f)]; }
	private long RATT2(int f) { return rays[((f) << 7) | key090(BOARD(), f) | 0x2000]; }
	private long BATT3(int f) { return rays[((f) << 7) | key045(BOARD(), f) | 0x4000]; }
	private long BATT4(int f) { return rays[((f) << 7) | key135(BOARD(), f) | 0x6000]; }
	private long RXRAY1(int f) { return rays[((f) << 7) | key000(BOARD(), f) | 0x8000]; }
	private long RXRAY2(int f) { return rays[((f) << 7) | key090(BOARD(), f) | 0xA000]; }
	private long BXRAY3(int f) { return rays[((f) << 7) | key045(BOARD(), f) | 0xC000]; }
	private long BXRAY4(int f) { return rays[((f) << 7) | key135(BOARD(), f) | 0xE000]; }

	private long ROCC1(int f) { return (RATT1(f) & BOARD()); }
	private long ROCC2(int f) { return (RATT2(f) & BOARD()); }
	private long BOCC3(int f) { return (BATT3(f) & BOARD()); }
	private long BOCC4(int f) { return (BATT4(f) & BOARD()); }
	private long RMOVE1(int f) { return (RATT1(f) & (~BOARD())); }
	private long RMOVE2(int f) { return (RATT2(f) & (~BOARD())); }
	private long BMOVE3(int f) { return (BATT3(f) & (~BOARD())); }
	private long BMOVE4(int f) { return (BATT4(f) & (~BOARD())); }
	private long RCAP1(int f, int c) { return (RATT1(f) & colorb[(c)^1]); }
	private long RCAP2(int f, int c) { return (RATT2(f) & colorb[(c)^1]); }
	private long BCAP3(int f, int c) { return (BATT3(f) & colorb[(c)^1]); }
	private long BCAP4(int f, int c) { return (BATT4(f) & colorb[(c)^1]); }
	private long ROCC(int f) { return (ROCC1(f) | ROCC2(f)); }
	private long BOCC(int f) { return (BOCC3(f) | BOCC4(f)); }
	private long RMOVE(int f) { return (RMOVE1(f) | RMOVE2(f)); }
	private long BMOVE(int f) { return (BMOVE3(f) | BMOVE4(f)); }
	private long RCAP(int f, int c) { return (ROCC(f) & colorb[(c)^1]); }
	private long BCAP(int f, int c) { return (BOCC(f) & colorb[(c)^1]); }

	private long SHORTMOVE(long x) { return ((x) & ((x)^BOARD())); }
	private long SHORTOCC(long x) { return ((x) & BOARD()); }
	private long SHORTCAP(long x, int c) { return ((x) & colorb[(c)^1]); }

	private long NMOVE(int x) { return (SHORTMOVE(nmoves[x])); }
	private long KMOVE(int x) { return (SHORTMOVE(kmoves[x])); }
	private long PMOVE(int x, int c) { return (pmoves[(c)][(x)] & (~BOARD())); }
	private long NOCC(int x) { return (SHORTOCC(nmoves[x])); }
	private long KOCC(int x) { return (SHORTOCC(kmoves[x])); }
	private long POCC(int x, int c) { return (pcaps[(c)][(x)] & BOARD()); }
	private long NCAP(int x, int c) { return (SHORTCAP(nmoves[x], (c))); }
	private long KCAP(int x, int c) { return (SHORTCAP(kmoves[x], (c))); }
	private long PCAP(int x, int c) { return (pcaps[(c)][(x)] & colorb[(c)^1]); }
	private long PCA3(int x, int c) { return (pcaps[(c)][(x) | 64] & (colorb[(c)^1] | ((BIT[ENPASS()]) & (c == 1 ? 0xFF0000L : 0xFF0000000000L)))); }
	private long PCA4(int x, int c) { return (pcaps[(c)][(x) | 128] & (colorb[(c)^1] | ((BIT[ENPASS()]) & (c == 1? 0xFF0000L : 0xFF0000000000L)))); }

	private bool RANK(int x, int y) { return (((x) & 0x38) == (y)); }
	private bool TEST(int f, long b) { return (BIT[f] & (b)) != 0; }
	private int ENPASS() { return (flags & 63); }
	private int CASTLE() { return (flags & 960); }
	private int COUNT() { return (count & 0x3FF); }

    const long HSIZEB = 0x200000;
    const long HMASKB = HSIZEB - 1;
	const long HINVB = 0xFFFFFF & (~HMASKB);

    const long HSIZEP = 0x400000;
    const long HMASKP = HSIZEP - 1;
	const long HINVP = 0xFFFFFF & (~HMASKP);

	private long[] hashDB = new long[HSIZEB];
	private long[] hashDP = new long[HSIZEP];
	private long hashb = 0L;
	private long[] hstack = new long[0x800];
	private long[] mstack = new long[0x800];

	private long[] hashxor = new long[4096];
	private long[] rays = new long[0x10000];
	private long[][] pmoves = { new long[64], new long[64] };
	private long[][] pcaps = { new long[192], new long[192] };
	private long[] nmoves = new long[64];
	private long[] kmoves = new long[64];
    private static int[] _knight = { -17, -10, 6, 15, 17, 10, -6, -15 };
    private static int[] _king = { -9, -1, 7, 8, 9, 1, -7, -8 };
	private long[] BIT = new long[64];
	private byte[] LSB = new byte[0x10000];
	private byte[] BITC = new byte[0x10000] ;      
	private int[] crevoke = new int[64];
	private int[] nmobil = new int[64];
	private int[] kmobil = new int[64];
	private int[][] pawnprg = { new int[64], new int[64] };
	private long[][] pawnfree = { new long[64], new long[64] };
	private long[][] pawnfile = { new long[64], new long[64] };
	private long[][] pawnhelp = { new long[64], new long[64] };
	private int[][] movelist = new int[64][];       // int[64][256]
	private int[] movenum = new int[64];
	private int[][] pv = new int[64][];            // int[64][64]
	private int[] pvlength = new int[64];
	private int[] value = new int[64];
	private int iter;
	private String pieceChar = "*PNK.BRQ";
	private long searchtime, maxtime, starttime;
	private bool sabort, noabort;
	private int sd = 32;
    public String bestmove = "";
	private int count, flags, mat, onmove, engine =-1;
	private int[] kingpos = new int[2];
	private long[] pieceb = new long[8];
	private long[] colorb = new long[2];
	private String[] irbuf = new String[20];
	private long BOARD() { return (colorb[0] | colorb[1]); }
	private long RQU() { return (pieceb[QUEEN] | pieceb[ROOK]); }
	private long BQU() { return (pieceb[QUEEN] | pieceb[BISHOP]); }

    private static int[] nullvar = { 13, 43, 149, 519, 1809, 6311, 22027 };
    private long[] bmask45 = new long[64];
    private long[] bmask135 = new long[64];
    private int[] killer = new int[128];
    private int[] history = new int[0x1000];

    private int time = 30000;
	private int mps = 0;
	private int baze = 5;
	private int inc = 0;
	private bool post = true;

	private long getLowestBit(long bb) {
		return bb & (-(long)bb);
	}

	private int _getpiece(char s, int[] c) {
		int i;
		for (i = 1; i < 8; i++) 
			if (pieceChar[i] == s) { c[0] = 0; return i; } 
			else if (pieceChar[i] == s-32) { c[0] = 1; return i; }
		return 0;
	}

	private bool book;
	private void _parse_fen(String fen) {
		char s, mv = 'w';
		String pos = "", cas = "", enps = "";
		int c, i, halfm = 0, fullm = 1, col = 0, row = 7;
		for (i = 0; i < 8; i++) pieceb[i] = 0L;
		colorb[0] = colorb[1] = hashb = 0L;
		mat = i = c = 0;
		book = false;
        String[] st = fen.Split(' ');
		if (st.Length>0) pos = st[0];
		if (st.Length>1) mv = st[1][0];
		if (st.Length>2) cas = st[2];
		if (st.Length>3) enps = st[3];
		if (st.Length>4) halfm = Convert.ToInt32(st[4]);
		if (st.Length>5) fullm = Convert.ToInt32(st[5]);

		for (i = 0; i < pos.Length; i++) {
			s = pos[i];
			if (s == '/') {
				row--;
				col = 0;
			} else if (s >= '1' && s <= '8') {
				col += s - '0';
			} else {
				int[] cp = new int[]{c};
				int p = _getpiece(s, cp);
				c = cp[0];
				if (p == KING) kingpos[c] = row*8 + col;
				else mat += c == 1 ? -pval[p] : pval[p];
				hashb ^= hashxor[col | row << 3 | i << 6 | (c == 1 ? 512 : 0)];
				pieceb[p] |= BIT[row*8 + col];
				colorb[c] |= BIT[row*8 + (col++)];
			}
		}
		onmove = mv == 'b' ? 1 : 0;
		flags = i = 0;
		for (i = 0; i < cas.Length; i++) {
			s = cas[i];
			if (s == 'K') flags |= (int)BIT[6];
			if (s == 'k') flags |= (int)BIT[7];
			if (s == 'Q') flags |= (int)BIT[8];
			if (s == 'q') flags |= (int)BIT[9];
		}
		if (enps[0] >= 'a' && enps[0] <= 'h' && enps[1] >= '1' && enps[1] <= '8') flags |= 8*(enps[1] - '1') + enps[0] - 'a'; 
		count = (fullm - 1)*2 + onmove + (halfm << 10);
		for (i = 0; i < COUNT(); i++) hstack[i] = 0L;
	}

	private String sfen = "rnbqkbnr/pppppppp/////PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	
	const int BKSIZE = 1;  // We ignore book here porting to C#.
                           // Not very optimal, may be developed later. 
	private int[] bkmove = new int[BKSIZE*32];
	private int[] bkflag = new int[BKSIZE];
	private int[] bkcount = new int[3];

	private void _readbook(String bk) {
        book = false;
        printf("NO BOOK");
		_parse_fen(sfen);
		engine = 1;
		sd = 32;
	}

	private int LOW16(long x) { return (int)((x) & 0xFFFF); }
	private int LOW32(long x) { return (int)((x) & 0xFFFFFFFFL); }
	private long L32(long x) { return ((x) & 0xFFFFFFFFL); }
	private long r_x = 30903, r_y = 30903, r_z = 30903, r_w = 30903, r_carry = 0;
	private long _rand_32() {
		r_x = L32(r_x * 69069 + 1);
		r_y ^= L32(r_y << 13);
		r_y ^= L32(r_y >> 17);
		r_y ^= L32(r_y << 5);
		r_y = L32(r_y);
		long t = L32((r_w << 1)) + r_z + r_carry;
		r_carry = (L32(r_z >> 2) + L32(r_w >> 3) + L32(r_carry >> 2)) >> 30;
		r_z = r_w;
		r_w = L32(t);
		return L32(r_x + r_y + r_w);
	}

	private long _rand_64() { long c = _rand_32(); return _rand_32() | (c << 32); }

	private long getTime() {
		return DateTime.UtcNow.Millisecond;
	}

	private byte getLsb(long bm) {
		int n = (int) LOW32(bm);
		if (n != 0) {
			if (LOW16(n) != 0) return LSB[LOW16(n)];
			else return (byte)(16 | LSB[LOW16(n >> 16)]);
		} else {
			n = (int)(bm >> 32);
			if (LOW16(n) != 0) return (byte)(32 | LSB[LOW16(n)]);
			else return (byte)(48 | LSB[LOW16(n >> 16)]);
		}
	}

	private byte _slow_lsb(long bm) {
		int k = -1;
		while (bm != 0) { k++; if ((bm & 1) != 0) break; bm >>= 1; }
		return (byte)k;
	}

	private byte _bitcnt(long bit) {
		byte c = 0;
		while (bit != 0) { bit &= (bit - 1); c++; }
		return c;
	}

	private byte bitcnt (long n) {

		byte a1 = (BITC[LOW16(n)]);
		long g2 = n >> 16;
		byte a2 = (BITC[LOW16(g2)]);
		long g3 = n >> 32;
		byte a3 = (BITC[LOW16(g3)]);
		long g4 = n >> 48;
		byte a4 = (BITC[LOW16(g4)]);
		
	     return (byte)(BITC[LOW16(n)]
	         +  BITC[LOW16(n >> 16)]
	         +  BITC[LOW16(n >> 32)]
	         +  BITC[LOW16(n >> 48)]);
	}

	private int identPiece(int f) {
		if (TEST(f, pieceb[PAWN])) return PAWN;
		if (TEST(f, pieceb[KNIGHT])) return KNIGHT;
		if (TEST(f, pieceb[BISHOP])) return BISHOP;
		if (TEST(f, pieceb[ROOK])) return ROOK;
		if (TEST(f, pieceb[QUEEN])) return QUEEN;
		if (TEST(f, pieceb[KING])) return KING;
		return ENP;
	}

	private int key000(long b, int f) {
		return (int) ((b >> (f & 56)) & 0x7E);
	}

	private int key090(long b, int f) {
		int h;
		b = b >> (f&7);
		h = (int)((b & 0x1010101) | ((b >> 31) & 0x2020202));
		h = (h & 0x303) | ((h >> 14) & 0xC0C);
		return (h & 0xE) | ((h >> 4) & 0x70);
	}

	private int keyDiag(long _b) {
	   int h = (int)(_b | _b >> 32);
	   h |= h >> 16;
	   h |= h >>  8;
	   return h & 0x7E;
	}

	private int key045(long b, int f) {
	   return keyDiag(b & bmask45[f]);
	}

	private int key135(long b, int f) {
	   return keyDiag(b & bmask135[f]);
	}

	private bool DUALATT(int x, int y, int c) { return (battacked(x, c) || battacked(y, c)); }
	private bool battacked(int f, int c) {
		if ((PCAP(f, c) & pieceb[PAWN]) != 0) return true;
		if ((NCAP(f, c) & pieceb[KNIGHT]) != 0) return true;
		if ((KCAP(f, c) & pieceb[KING]) != 0) return true;
		if ((RCAP1(f, c) & RQU()) != 0) return true; 
		if ((RCAP2(f, c) & RQU()) != 0) return true; 
		if ((BCAP3(f, c) & BQU()) != 0) return true;
		if ((BCAP4(f, c) & BQU()) != 0) return true;
		return false;
	}

	private long reach(int f, int c) {
		return (NCAP(f, c) & pieceb[KNIGHT])
			| (RCAP1(f, c) & RQU())
			| (RCAP2(f, c) & RQU())
			| (BCAP3(f, c) & BQU())
			| (BCAP4(f, c) & BQU());
	}

	private long  attacked(int f, int c) {
		return (PCAP(f, c) & pieceb[PAWN]) | reach(f, c);
	}

	private void _init_pawns(long[] moves, long[] caps, long[] freep, long[] filep, long[] helpp, int c) {
		int i, j;
		for (i = 0; i < 64; i++) {
			int rank = i/8;
			int file = i&7;
			int m = i + (c == 1 ? -8 : 8);
			pawnprg[c][i] = pawnrun[c == 1 ? 7-rank : rank];
			for (j = 0; j < 64; j++) {
				int jrank = j/8;
				int jfile = j&7;
				int dfile = (jfile - file)*(jfile - file);
				if (dfile > 1) continue;
				if ((c == 1 && jrank < rank) || (c == 0 && jrank > rank)) {//The not touched half of the pawn
					if (dfile == 0) filep[i] |= BIT[j];
					freep[i] |= BIT[j];
				} else if (dfile != 0 && (jrank - rank)*(jrank - rank) <= 1) {
					helpp[i] |= BIT[j];
				}
			}
			if (m < 0 || m > 63) continue;
			moves[i] |= BIT[m];
			if (file > 0) {
				m = i + (c == 1 ? -9 : 7);
				if (m < 0 || m > 63) continue;
				caps[i] |= BIT[m];
				caps[i + 64*(2 - c)] |= BIT[m];
			}
			if (file < 7) {
				m = i + (c == 1 ? -7 : 9);
				if (m < 0 || m > 63) continue;
				caps[i] |= BIT[m];
				caps[i + 64*(c + 1)] |= BIT[m];
			}
		}
	}

	private void _init_shorts(long[] moves, int[] m) {
		int i, j, n;
		for (i = 0; i < 64; i++) {
			for (j = 0; j < 8; j++) {
				n = i + m[j];
				if (n < 64 && n >= 0 && ((n & 7)-(i & 7))*((n & 7)-(i & 7)) <= 4) {
					moves[i] |= BIT[n];
				}
			}
		}
	}

	private long _occ_free_board(int bc, int del, long free) {
		long low, perm = free;
		int i;
		for (i = 0; i < bc; i++) {
			low = getLowestBit(free);
			free &= (~low);
			if (!TEST(i, del)) perm &= (~low);
		}
		return perm;
	}
	
	private void _init_rays1() {
		int i, f, iperm, bc, index; 
		long board, mmask, occ, move, xray;
		for (f = 0; f < 64; f++) {
			mmask = _rook0(f, 0L, 0) | BIT[f];
			iperm = 1 << (bc = bitcnt(mmask));
			for (i = 0; i < iperm; i++) {
				board = _occ_free_board(bc, i, mmask);
				move = _rook0(f, board, 1);
				occ = _rook0(f, board, 2);
				xray = _rook0(f, board, 3);
				index = key000(board, f);
				rays[(f << 7) + index] = occ | move;
				rays[(f << 7) + index + 0x8000] = xray;
			}
		}
	}

	private void _init_rays2() {
		int i, f, iperm, bc, index; 
		long board, mmask, occ, move, xray;
		for (f = 0; f < 64; f++) {
			mmask = _rook90(f, 0L, 0) | BIT[f];
			iperm = 1 << (bc = bitcnt(mmask));
			for (i = 0; i < iperm; i++) {
				board = _occ_free_board(bc, i, mmask);
				move = _rook90(f, board, 1);
				occ = _rook90(f, board, 2);
				xray = _rook90(f, board, 3);
				index = key090(board, f);
				rays[(f << 7) + index + 0x2000] = occ | move;
				rays[(f << 7) + index + 0x8000 + 0x2000] = xray;
			}
		}
	}

	private void _init_rays3() {
		int i, f, iperm, bc, index; 
		long board, mmask, occ, move, xray;
		for (f = 0; f < 64; f++) {
			mmask = _bishop45(f, 0L, 0) | BIT[f];
			iperm = 1 << (bc = bitcnt(mmask));
			for (i = 0; i < iperm; i++) {
				board = _occ_free_board(bc, i, mmask);
				move = _bishop45(f, board, 1);
				occ = _bishop45(f, board, 2);
				xray = _bishop45(f, board, 3);
				index = key045(board, f);
				rays[(f << 7) + index + 0x4000] = occ | move;
				rays[(f << 7) + index + 0x8000 + 0x4000] = xray;
			}
		}
	}

	private void _init_rays4() {
		int i, f, iperm, bc, index; 
		long board, mmask, occ, move, xray;
		for (f = 0; f < 64; f++) {
			mmask = _bishop135(f, 0L, 0) | BIT[f];
			iperm = 1 << (bc = bitcnt(mmask));
			for (i = 0; i < iperm; i++) {
				board = _occ_free_board(bc, i, mmask);
				move = _bishop135(f, board, 1);
				occ = _bishop135(f, board, 2);
				xray = _bishop135(f, board, 3);
				index = key135(board, f);
				rays[(f << 7) + index + 0x6000] = occ | move;
				rays[(f << 7) + index + 0x8000 + 0x6000] = xray;
			}
		}
	}

	private long _rook0(int f, long board, int t) {
		long free = 0L, occ = 0L, xray = 0L;
		int i, b;
		for (b = 0, i = f+1; i < 64 && i%8 != 0; i++) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		for (b = 0, i = f-1; i >= 0 && i%8 != 7; i--) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		return (t < 2) ? free : (t == 2 ? occ : xray);
	}

	private long _rook90(int f, long board, int t) {
		long free = 0L, occ = 0L, xray = 0L;
		int i, b;
		for (b = 0, i = f-8; i >= 0; i-=8) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		for (b = 0, i = f+8; i < 64; i+=8) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		return (t < 2) ? free : (t == 2 ? occ : xray);
	}

	private long _bishop45(int f, long board, int t) {
		long free = 0L, occ = 0L, xray = 0L;
		int i, b;
		for (b = 0, i = f+9; i < 64 && (i%8 != 0); i+=9) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		for (b = 0, i = f-9; i >= 0 && (i%8 != 7); i-=9) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		return (t < 2) ? free : (t == 2 ? occ : xray);
	}

	private long _bishop135(int f, long board, int t) {
		long free = 0L, occ = 0L, xray = 0L;
		int i, b;
		for (b = 0, i = f-7; i >= 0 && (i%8 != 0); i-=7) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		for (b = 0, i = f+7; i < 64 && (i%8 != 7); i+=7) {
			if (TEST(i, board)) { 
				if (b != 0) { xray |= BIT[i]; break; } else { occ |= BIT[i]; b = 1; } 
				} 
			if (b == 0) free |= BIT[i];
		}
		return (t < 2) ? free : (t == 2 ? occ : xray);
	}
	
	private String displaym(int m) {
        String s = "" + ((char)('a' + FROM(m) % 8))
				+ ((char)('1' + FROM(m) / 8))
				+ ((char)('a' + TO(m) % 8))
				+ ((char)('1' + TO(m) / 8));
		if (PROM(m) != 0) s+= ("" + (char)(pieceChar[PROM(m)]+32));
        return s;
	}

    private void printf(String s)
    {
        // do nothing for this silverlight
        //System.Diagnostics.Debug.WriteLine(s);
    }

	private void errprintf(String s) { printf(s); }

	private String displaypv() {
		int i;
        String s = "";
		for (i = 0; i < pvlength[0]; i++) {
			s+=(displaym(pv[0][i])+" ");
		}
       return s;
	}

	private int isDraw(long hp, int nrep) {
		if (count > 0xFFF) { //fifty > 3
			int i, c = 0, n = COUNT() - (count >> 10);
			if (count >= 0x400*100) return 2; //100 plies
			for (i = COUNT() - 2; i >= n; i--) 
				if (hstack[i] == hp && ++c == nrep) return 1; 
		} else if ((pieceb[PAWN] | RQU()) == 0) { //Check for mating material
			if (_bitcnt(colorb[0]) <= 2 && _bitcnt(colorb[1]) <= 2) return 3;
		}
		return 0;
	}

	private long pinnedPieces(int f, int oc) {
		long pin = 0L;
		long b = ((RXRAY1(f) | RXRAY2(f)) & colorb[oc]) & RQU();
		while (b != 0) {
			int t = getLsb(b);
			b ^= BIT[t];
			pin |= RCAP(t, oc) & ROCC(f);
		}
		b = ((BXRAY3(f) | BXRAY4(f)) & colorb[oc]) & BQU();
		while (b != 0) {
			int t = getLsb(b);
			b ^= BIT[t];
			pin |= BCAP(t, oc) & BOCC(f);
		}
		return pin;
	}

	private byte getDir(int f, int t) {
		if (((f ^ t) & 56) == 0) return 8;
		if (((f ^ t) & 7) == 0) return 16;
		return (byte)(((f - t) % 7) != 0 ? 32 : 64);
	}

	/* move is both makeMove and unmakeMove, only for unmakeMove the globalflags have to be restored (counter, castle, enpass...) */
	private void move(int m, int c) {
		int f = FROM(m);
		int t = TO(m);
		int p = PIECE(m);
		int a = CAP(m);

		colorb[c] ^= BIT[f];
		pieceb[p] ^= BIT[f];

		colorb[c] ^= BIT[t];
		pieceb[p] ^= BIT[t];
		hashb ^= hashxor[(f) | (p) << 6 | (c) << 9];
		hashb ^= hashxor[(t) | (p) << 6 | (c) << 9];

		flags &= 960;
		count += 0x401;
		if (a != 0) {
			if (a == ENP) { // Enpassant Capture
				t = (t&7) | (f&56);
				a = PAWN;
			} else if (a == ROOK && CASTLE() != 0) { //Revoke castling rights.
				flags &= crevoke[t];
			}
			pieceb[a] ^= BIT[t];
			colorb[c^1] ^= BIT[t];
			hashb ^= hashxor[(t) | (a) << 6 | (c^1) << 9];
			count &= 0x3FF; //Reset Fifty Counter
			mat += c == 1 ? -pval[a] : +pval[a];
		}
		if (p == PAWN) {
			if (((f^t)&8) == 0) flags |= f^24; //Enpassant
			else if ((t&56) == 0 || (t&56) == 56) {
				pieceb[PAWN] ^= BIT[t];
				pieceb[PROM(m)] ^= BIT[t];
				hashb ^= hashxor[(t) | (PAWN) << 6 | (c) << 9];
				hashb ^= hashxor[(t) | (PROM(m)) << 6 | (c) << 9];
				mat += c == 1 ? pval[PAWN] - pval[PROM(m)] : -pval[PAWN] + pval[PROM(m)];
			}
			count &= 0x3FF; //Reset Fifty Counter
		} else if (p == KING) {
			if (kingpos[c] == f) kingpos[c] = t; else kingpos[c] = f;
			flags &= ~(320 << c); // Lose castling rights
			if (((f^t)&3) == 2) { // Castle
				if (t == 6) { f = 7; t = 5; }
				else if (t == 2) { f = 0; t = 3; }
				else if (t == 62) { f = 63; t = 61; }
				else { f = 56; t = 59; }
				colorb[c] ^= BIT[f];
				pieceb[ROOK] ^= BIT[f];
				colorb[c] ^= BIT[t];
				pieceb[ROOK] ^= BIT[t];
				hashb ^= hashxor[(f) | (ROOK) << 6 | (c) << 9];
				hashb ^= hashxor[(t) | (ROOK) << 6 | (c) << 9];
			}
		} else if (p == ROOK && CASTLE() != 0) {
			flags &= crevoke[f];
		}
	}

	private void doMove(int m, int c) {
        mstack[COUNT()] = count | (flags << 17) | (((long)(mat + 0x4000)) << 27) | (((long)m) << 42);
        move(m, c);
	}

	private void undoMove(int m, int c) {
	        long u = mstack[COUNT() - 1];
	        move(m, c);
	        count = (int)(u & 0x1FFFF);
	        flags = (int)((u >> 17) & 0x3FF);
	        mat = (int)(((u >> 27) & 0x7FFF) - 0x4000);
	}

	private void registerCaps(int m, long bc, int[] mlist, int[] mn) {
		while (bc != 0) {
			int t = getLsb(bc);
			bc ^= BIT[t];
			mlist[mn[0]++] = m | _TO(t) | _CAP(identPiece(t));
		}
	}

	private void registerMoves(int m, long bc, long bm, int[] mlist, int[] mn) {
		while (bc != 0) {
			int t = getLsb(bc);
			bc ^= BIT[t];
			mlist[mn[0]++] = m | _TO(t) | _CAP(identPiece(t));
		}
		while (bm != 0) {
			int t = getLsb(bm);
			bm ^= BIT[t];
			mlist[mn[0]++] = m | _TO(t);
		}
	}

	private void registerProms(int f, int c, long bc, long bm, int[] mlist, int[] mn) {
		while (bc != 0) {
			int t = getLsb(bc);
			bc ^= BIT[t];
			int m = f | _ONMV(c) | _PIECE(PAWN) | _TO(t) | _CAP(identPiece(t));
			mlist[mn[0]++] = m | _PROM(QUEEN);
			mlist[mn[0]++] = m | _PROM(KNIGHT);
			mlist[mn[0]++] = m | _PROM(ROOK);
			mlist[mn[0]++] = m | _PROM(BISHOP);
		}
		while (bm != 0) {
			int t = getLsb(bm);
			bm ^= BIT[t];
			int m = f | _ONMV(c) | _PIECE(PAWN) | _TO(t);
			mlist[mn[0]++] = m | _PROM(QUEEN);
			mlist[mn[0]++] = m | _PROM(KNIGHT);
			mlist[mn[0]++] = m | _PROM(ROOK);
			mlist[mn[0]++] = m | _PROM(BISHOP);
		}
	}

	private void registerKing(int m, long bc, long bm, int[] mlist, int[] mn, int c) {
		while (bc != 0) {
			int t = getLsb(bc);
			bc ^= BIT[t];
			if (battacked(t, c)) continue;
			mlist[mn[0]++] = m | _TO(t) | _CAP(identPiece(t));
		}
		while (bm != 0) {
			int t = getLsb(bm);
			bm ^= BIT[t];
			if (battacked(t, c)) continue;
			mlist[mn[0]++] = m | _TO(t);
		}
	}

	private int generateCheckEsc(long ch, long apin, int c, int k, int[] ml, int[] mn) {
		long cc, fl;
		int d, bf = _bitcnt(ch);
		colorb[c] ^= BIT[k];
		registerKing(PREMOVE(k, KING, c), KCAP(k, c), KMOVE(k), ml, mn, c);
		colorb[c] ^= BIT[k];
		if (bf > 1) return bf; //Doublecheck
		bf = getLsb(ch);

		cc = attacked(bf, c^1) & apin;  //Can we capture the checker?
		while (cc != 0) {
			int cf = getLsb(cc);
			cc ^= BIT[cf];
			int p = identPiece(cf);
			if (p == PAWN && RANK(cf, c != 0 ? 0x08 : 0x30)) {
				registerProms(cf, c, ch, 0L, ml, mn);
			} else {
				registerMoves(PREMOVE(cf, p, c), ch, 0L, ml, mn);
			}
		}
		if (ENPASS() != 0 && (ch & pieceb[PAWN]) != 0) { //Enpassant capture of attacking Pawn
			cc = PCAP(ENPASS(), c^1) & pieceb[PAWN] & apin;
			while (cc != 0) {
				int cf = getLsb(cc);
				cc ^= BIT[cf];
				registerMoves(PREMOVE(cf, PAWN, c), BIT[ENPASS()], 0L, ml, mn);
			}
		}
		if ((ch & (nmoves[k] | kmoves[k])) != 0) return 1; //We can't move anything between!

		d = getDir(bf, k);
		if ((d & 8) != 0) fl = RMOVE1(bf) & RMOVE1(k);
		else if ((d & 16) != 0) fl = RMOVE2(bf) & RMOVE2(k);
		else if ((d & 32) != 0) fl = BMOVE3(bf) & BMOVE3(k);
		else fl = BMOVE4(bf) & BMOVE4(k);

		while (fl != 0) {
			int f = getLsb(fl);
			fl ^= BIT[f];
			cc = reach(f, c^1) & apin;
			while (cc != 0) {
				int cf = getLsb(cc);
				cc ^= BIT[cf];
				int p = identPiece(cf);
				registerMoves(PREMOVE(cf, p, c), 0L, BIT[f], ml, mn);
			}
			bf = c != 0 ? f+8 : f-8;
			if (bf < 0 || bf > 63) continue;
			if ((BIT[bf] & pieceb[PAWN] & colorb[c] & apin) != 0) {
				if (RANK(bf, c != 0 ? 0x08 : 0x30))
					registerProms(bf, c, 0L, BIT[f], ml, mn);
				else
					registerMoves(PREMOVE(bf, PAWN, c), 0L, BIT[f], ml, mn);
			}
			if (RANK(f, c != 0 ? 0x20 : 0x18) && (BOARD() & BIT[bf]) == 0 && (BIT[c != 0 ? f+16 : f-16] & pieceb[PAWN] & colorb[c] & apin) != 0)
				registerMoves(PREMOVE(c != 0 ? f+16 : f-16, PAWN, c), 0L, BIT[f], ml, mn);
		}
		return 1;
	}

	private int generateMoves(long ch, int c, int ply) {
		int t, f = kingpos[c];
		long m, b, a, cb = colorb[c];
		long pin = pinnedPieces(f, c^1);
		int[] ml = movelist[ply];
		int[] mn = new int[]{0};

		if (ch != 0) {
			int ret = generateCheckEsc(ch, ~pin, c, f, ml, mn);
			movenum[ply] = mn[0];
			return ret;
		}
		registerKing(PREMOVE(f, KING, c), KCAP(f, c), KMOVE(f), ml, mn, c);

		cb = colorb[c] & (~pin);
		b = pieceb[PAWN] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			m = PMOVE(f, c);
			a = PCAP(f, c);
			if (m != 0 && RANK(f, c != 0 ? 0x30 : 0x08)) m |= PMOVE(c != 0 ? f-8 : f+8, c);
			if (RANK(f, c != 0 ? 0x08 : 0x30)) {
				registerProms(f, c, a, m, ml, mn);
			} else {
				if (ENPASS() != 0 && (BIT[ENPASS()] & pcaps[(c)][(f)]) != 0) {
					long hh;
					int clbd = ENPASS()^8;
					colorb[c] ^= BIT[clbd];
					hh = ROCC1(f);
					if ((hh & BIT[kingpos[c]]) == 0 || (hh & colorb[c^1] & RQU()) == 0) {
						a = a | BIT[ENPASS()];
					}
					colorb[c] ^= BIT[clbd];
				}
				registerMoves(PREMOVE(f, PAWN, c), a, m, ml, mn);
			}
		}

		b = pin & pieceb[PAWN]; 
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			t = getDir(f, kingpos[c]);
			if ((t & 8) != 0) continue;
			m = a = 0L;
			if ((t & 16) != 0) {
				m = PMOVE(f, c);         
				if (m != 0 && RANK(f, c != 0 ? 0x30 : 0x08)) m |= PMOVE(c != 0 ? f-8 : f+8, c);
			} else if ((t & 32) != 0) {
				a = PCA3(f, c);
			} else {
				a = PCA4(f, c);
			}
			if (RANK(f, c != 0 ? 0x08 : 0x30)) {
				registerProms(f, c, a, m, ml, mn);
			} else {
				registerMoves(PREMOVE(f, PAWN, c), a, m, ml, mn);
			}
		}

		b = pieceb[KNIGHT] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerMoves(PREMOVE(f, KNIGHT, c), NCAP(f, c), NMOVE(f), ml, mn);
		}

		b = pieceb[ROOK] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerMoves(PREMOVE(f, ROOK, c), RCAP(f, c), RMOVE(f), ml, mn);
			if (CASTLE() != 0 && ch == 0) {
				if (c != 0) {
					if ((flags & 128) != 0 && (f == 63) && (RMOVE1(63) & BIT[61]) != 0)
						if (!DUALATT(61, 62, c)) registerMoves(PREMOVE(60, KING, c), 0L, BIT[62], ml, mn);
					if ((flags & 512) != 0 && (f == 56) && (RMOVE1(56) & BIT[59]) != 0)
						if (!DUALATT(59, 58, c)) registerMoves(PREMOVE(60, KING, c), 0L, BIT[58], ml, mn);
				} else {
					if ((flags & 64) != 0 && (f == 7) && (RMOVE1(7) & BIT[5]) != 0)
						if (!DUALATT(5, 6, c)) registerMoves(PREMOVE(4, KING, c), 0L, BIT[6], ml, mn);
					if ((flags & 256) != 0 && (f == 0) && (RMOVE1(0) & BIT[3]) != 0)
						if (!DUALATT(3, 2, c)) registerMoves(PREMOVE(4, KING, c), 0L, BIT[2], ml, mn);
				}
			}
		}

		b = pieceb[BISHOP] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerMoves(PREMOVE(f, BISHOP, c), BCAP(f, c), BMOVE(f), ml, mn);
		}

		b = pieceb[QUEEN] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerMoves(PREMOVE(f, QUEEN, c), RCAP(f, c) | BCAP(f,c), RMOVE(f) | BMOVE(f), ml, mn);
		}

		b = pin & (pieceb[ROOK] | pieceb[BISHOP] | pieceb[QUEEN]); 
		while (b != 0) {
			int p;
			f = getLsb(b);
			b ^= BIT[f];
			p = identPiece(f);
			t = p | getDir(f, kingpos[c]);
			if ((t & 10) == 10) registerMoves(PREMOVE(f, p, c), RCAP1(f, c), RMOVE1(f), ml, mn);
			if ((t & 18) == 18) registerMoves(PREMOVE(f, p, c), RCAP2(f, c), RMOVE2(f), ml, mn);
			if ((t & 33) == 33) registerMoves(PREMOVE(f, p, c), BCAP3(f, c), BMOVE3(f), ml, mn);
			if ((t & 65) == 65) registerMoves(PREMOVE(f, p, c), BCAP4(f, c), BMOVE4(f), ml, mn);
		}
		movenum[ply] = mn[0];
		return 0;
	}

	private int generateCaps(long ch, int c, int ply) {
		int t, f = kingpos[c];
		long m, b, a, cb = colorb[c];
		long pin = pinnedPieces(f, c^1);
		int[] ml = movelist[ply];
		int[] mn = new int[]{0};

		if (ch != 0) {
			int ret = generateCheckEsc(ch, ~pin, c, f, ml, mn);
			movenum[ply] = mn[0];
			return ret;
		}
		registerKing(PREMOVE(f, KING, c), KCAP(f, c), 0L, ml, mn, c);

		cb = colorb[c] & (~pin);

		b = pieceb[PAWN] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			a = PCAP(f, c);
			if (RANK(f, c != 0 ? 0x08 : 0x30)) {
				registerMoves(PREMOVE(f, PAWN, c) | _PROM(QUEEN), a, PMOVE(f, c), ml, mn);
			} else {
				if (ENPASS() != 0 && (BIT[ENPASS()] & pcaps[(c)][(f)]) != 0) {
					long hh;
					int clbd = ENPASS()^8;
					colorb[c] ^= BIT[clbd];
					hh = ROCC1(f);
					if ((hh & BIT[kingpos[c]]) == 0 || (hh & colorb[c^1] & RQU()) == 0) {
						a = a | BIT[ENPASS()];
					}
					colorb[c] ^= BIT[clbd];
				}
				registerCaps(PREMOVE(f, PAWN, c), a, ml, mn);
			}
		}

		b = pin & pieceb[PAWN]; 
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			t = getDir(f, kingpos[c]);
			if ((t & 8) != 0) continue;
			m = a = 0L;
			if ((t & 16) != 0) {
				m = PMOVE(f, c);         
			} else if ((t & 32) != 0) {
				a = PCA3(f, c);
			} else {
				a = PCA4(f, c);
			}
			if (RANK(f, c != 0 ? 0x08 : 0x30)) {
				registerMoves(PREMOVE(f, PAWN, c) | _PROM(QUEEN), a, m, ml, mn);
			} else {
				registerCaps(PREMOVE(f, PAWN, c), a, ml, mn);
			}
		}

		b = pieceb[KNIGHT] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerCaps(PREMOVE(f, KNIGHT, c), NCAP(f, c), ml, mn);
		}

		b = pieceb[BISHOP] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerCaps(PREMOVE(f, BISHOP, c), BCAP(f, c), ml, mn);
		}

		b = pieceb[ROOK] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerCaps(PREMOVE(f, ROOK, c), RCAP(f, c), ml, mn);
		}

		b = pieceb[QUEEN] & cb;
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			registerCaps(PREMOVE(f, QUEEN, c), RCAP(f, c) | BCAP(f,c), ml, mn);
		}

		b = pin & (pieceb[ROOK] | pieceb[BISHOP] | pieceb[QUEEN]); 
		while (b != 0) {
			int p;
			f = getLsb(b);
			b ^= BIT[f];
			p = identPiece(f);
			t = p | getDir(f, kingpos[c]);
			if ((t & 10) == 10) registerCaps(PREMOVE(f, p, c), RCAP1(f, c), ml, mn);
			if ((t & 18) == 18) registerCaps(PREMOVE(f, p, c), RCAP2(f, c), ml, mn);
			if ((t & 33) == 33) registerCaps(PREMOVE(f, p, c), BCAP3(f, c), ml, mn);
			if ((t & 65) == 65) registerCaps(PREMOVE(f, p, c), BCAP4(f, c), ml, mn);
		}
		movenum[ply] = mn[0];
		return 0;
	}

	private int swap(int m) //SEE Stuff
	{
	  int[] s_list = new int[32];
	  int f = FROM(m), t = TO(m), onmv = ONMV(m);
	  int a_piece = pval[CAP(m)], piece = PIECE(m), c = onmv^1, nc = 1;
	  long attacks, temp = 0, colstore0 = colorb[0], colstore1 = colorb[1];

	  attacks = attacked(t, 0) | attacked(t, 1);
	  s_list[0] = a_piece;
	  a_piece = pval[piece];
	  colorb[onmv] ^= BIT[f];
	  if ((piece & 4) != 0 || piece == 1) {
		int d = getDir(f, t);
	    if (d == 32 || d == 64) attacks |= BOCC(t) & BQU();
		if (d == 8 || d == 16) attacks |= ROCC(t) & RQU();
	  }
	  attacks &= BOARD();

	  while (attacks != 0) {
	      if ((temp = pieceb[PAWN] & colorb[c] & attacks) != 0) piece = PAWN;
	      else if ((temp = pieceb[KNIGHT] & colorb[c] & attacks) != 0) piece = KNIGHT;
	      else if ((temp = pieceb[BISHOP] & colorb[c] & attacks) != 0) piece = BISHOP;
	      else if ((temp = pieceb[ROOK] & colorb[c] & attacks) != 0) piece = ROOK;
	      else if ((temp = pieceb[QUEEN] & colorb[c] & attacks) != 0) piece = QUEEN;
	      else if ((temp = pieceb[KING] & colorb[c] & attacks) != 0) piece = KING;
	      else break;
	 
		temp &= -(long)temp;
		colorb[c] ^= temp;
		if ((piece & 4) != 0 || piece == 1) {
		  if ((piece & 1) != 0) attacks |= BOCC(t) & BQU();
		  if ((piece & 2) != 0) attacks |= ROCC(t) & RQU();
		}
		attacks &= BOARD();

		s_list[nc] = -s_list[nc - 1] + a_piece;
		a_piece = pval[piece];
		nc++;
		c ^= 1;
	  }

	  while ((--nc) != 0)
	    if (s_list[nc] > -s_list[nc - 1])
	      s_list[nc - 1] = -s_list[nc];

	  colorb[0] = colstore0;
	  colorb[1] = colstore1;
	  return s_list[0];
	}

	/* In quiesce the moves are ordered just for the value of the captured piece */
	private int qpick(int[] ml, int mn, int s) {
		int m;
		int i, t, pi = 0, vmax = -HEUR;
		for (i = s; i < mn; i++) {
			m = ml[i];
			t = capval[CAP(m)];
			if (t > vmax) {
				vmax = t;
				pi = i;
			}
		}
		m = ml[pi];
		if (pi != s) ml[pi] = ml[s];
		return m;
	}

	/* In normal search some basic move ordering heuristics are used */
	private int spick(int[] ml, int mn, int s, int ply) {
		int m; int cap;
		int i, t, pi = 0, vmax = -HEUR;
		for (i = s; i < mn; i++) {
			m = ml[i];
			cap = CAP(m);
			if (cap != 0) {
				t = capval[cap];
				if (t > vmax) {
					vmax = t;
					pi = i;
				}
			}	
			if (vmax < HEUR && m == killer[ply]) {
				vmax = HEUR;
				pi = i;
			}
			if (vmax < history[m & 0xFFF]) {
				vmax = history[m & 0xFFF];
				pi = i;
			}
		}
		m = ml[pi];
		if (pi != s) ml[pi] = ml[s];
		return m;
	}

	/* The evaluation for Color c. It's only mobility stuff. Pinned pieces are still awarded for limiting opposite's king */
	private int evalc(int c, int[] sf) {
		int f;
		int mn = 0, katt = 0;
		int oc = c^1;
		long ocb = colorb[oc];
		long m, b, a, cb;
		long kn = kmoves[kingpos[oc]];
		long pin = pinnedPieces(kingpos[c], oc);

		b = pieceb[PAWN] & colorb[c];
		while (b != 0) {
			int ppos = 0;
			f = getLsb(b);
			b ^= BIT[f];
			ppos = pawnprg[c][f];
			m = PMOVE(f, c);
			a = POCC(f, c);
			if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			if ((BIT[f] & pin) != 0) {
				if ((getDir(f, kingpos[c]) & 16) == 0) m = 0;
			} else {
				ppos += _bitcnt(a & pieceb[PAWN] & colorb[c]) << 2;
			}
			if (m != 0) ppos += 8; else ppos -= 8;
			if ((pawnfile[c][f] & pieceb[PAWN] & ocb) == 0) { //Free file?
				if ((pawnfree[c][f] & pieceb[PAWN] & ocb) == 0) ppos *= 2; //Free run?
				if ((pawnhelp[c][f] & pieceb[PAWN] & colorb[c]) == 0) ppos -= 33; //Hanging backpawn?
			}

			mn += ppos;
		}

		cb = colorb[c] & (~pin);
		b = pieceb[KNIGHT] & cb;
		while (b != 0) {
			sf[0] += 1;
			f = getLsb(b);
			b ^= BIT[f];
			a = nmoves[f];
			if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			mn += nmobil[f];
		}

		b = pieceb[KNIGHT] & pin;
		while (b != 0) {
			sf[0] += 1;
			f = getLsb(b);
			b ^= BIT[f];
			a = nmoves[f];
			if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
		}

		colorb[oc] ^= BIT[kingpos[oc]]; //Opposite King doesn't block mobility at all
		b = pieceb[QUEEN] & cb;
		while (b != 0) {
			sf[0] += 4;
			f = getLsb(b);
			b ^= BIT[f];
			a = RATT1(f) | RATT2(f) | BATT3(f) | BATT4(f);
			if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			mn += bitcnt(a);
		}

		colorb[oc] ^= RQU() & ocb; //Opposite Queen & Rook doesn't block mobility for bishop
		b = pieceb[BISHOP] & cb;
		while (b != 0) {
			sf[0] += 1;
			f = getLsb(b);
			b ^= BIT[f];
			a = BATT3(f) | BATT4(f);
			if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			mn += bitcnt(a) << 3;
		}

		colorb[oc] ^= pieceb[ROOK] & ocb; //Opposite Queen doesn't block mobility for rook.
		colorb[c] ^= pieceb[ROOK] & cb; //Own non-pinned Rook doesn't block mobility for rook.
		b = pieceb[ROOK] & cb;
		while (b != 0) {
			sf[0] += 2;
			f = getLsb(b);
			b ^= BIT[f];
			a = RATT1(f) | RATT2(f);
			if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			mn += bitcnt(a) << 2;
		}

		colorb[c] ^= pieceb[ROOK] & cb; // Back
		b = pin & (pieceb[ROOK] | pieceb[BISHOP] | pieceb[QUEEN]); 
		while (b != 0) {
			f = getLsb(b);
			b ^= BIT[f];
			int p = identPiece(f);
			if (p == BISHOP) {
				sf[0] += 1; 
				a = BATT3(f) | BATT4(f);
				if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			} else if (p == ROOK) {
				sf[0] += 2; 
				a = RATT1(f) | RATT2(f);
				if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			} else {
				sf[0] += 4;
				a = RATT1(f) | RATT2(f) | BATT3(f) | BATT4(f);
				if ((a & kn) != 0) katt += _bitcnt(a & kn) << 4;
			}
			int t = p | getDir(f, kingpos[c]);
			if ((t & 10) == 10) mn += _bitcnt(RATT1(f));
			if ((t & 18) == 18) mn += _bitcnt(RATT2(f));
			if ((t & 33) == 33) mn += _bitcnt(BATT3(f));
			if ((t & 65) == 65) mn += _bitcnt(BATT4(f));
		}

		colorb[oc] ^= pieceb[QUEEN] & ocb; //Back
		colorb[oc] ^= BIT[kingpos[oc]]; //Back
		if (sf[0] == 1 && (pieceb[PAWN] & colorb[c]) == 0) mn =- 200; //No mating material
		if (sf[0] < 7) katt = katt * sf[0] / 7; //Reduce the bonus for attacking king squares
	    if (sf[0] < 2) sf[0] = 2;
		return mn + katt;
	}

	private int eval1 = 0;
	private int eval(int c) {
		int sf0 = 0, sf1 = 0;
		int[] sfp = new int[]{sf0};
		int ev0 = evalc(0, sfp);
		sf0 = sfp[0];
		sfp[0] = sf1;
		int ev1 = evalc(1, sfp);
		sf1 = sfp[0];
		eval1++;

		if (sf1 < 6) ev0 += kmobil[kingpos[0]]*(6-sf1);
		if (sf0 < 6) ev1 += kmobil[kingpos[1]]*(6-sf0);

		return (c != 0 ? (ev1 - ev0) : (ev0 - ev1));
	}

	private long nodes;
	private long qnodes;
	private int quiesce(long ch, int c, int ply, int alpha, int beta) {
		int i, w, best = -32000;
		int cmat = c == 1 ? -mat: mat;
		if (ply == 63) return eval(c) + cmat;
		
		if (ch == 0) do {
			if (cmat - 200 >= beta) return beta;
			if (cmat + 200 <= alpha) break;
			best = eval(c) + cmat;
			if (best > alpha) {
				alpha = best;
				if (best >= beta) return beta;
			}
		} while(false);

		generateCaps(ch, c, ply);
		if (ch != 0 && movenum[ply] == 0) return -32000 + ply;

		for (i = 0; i < movenum[ply]; i++) {
			int m = qpick(movelist[ply], movenum[ply], i);
			if (ch == 0 && PROM(m) == 0 && pval[PIECE(m)] > pval[CAP(m)] && swap(m) < 0) continue;

			doMove(m, c);
			qnodes++;

			w = -quiesce(attacked(kingpos[c^1], c^1), c^1, ply+1, -beta, -alpha);

			undoMove(m, c);

			if (w > best) {
				best = w;
				if (w > alpha) {
					alpha = w;
					if (w >= beta) return beta;
				}
			}
		}
		return best >= alpha ? best : eval(c) + cmat;
	}

	private int retPVMove(int c, int ply) {
        int i;
        generateMoves(attacked(kingpos[c], c), c, 0);
        for (i = 0; i < movenum[0]; i++) {
                int m = movelist[0][i];
                if (m == pv[0][ply]) return m;
        }
        return 0;
	}

	private int nullvariance(int delta) {
	      int r = 0;
	      if (delta >= 4) for (r = 1; r <= nullvar.Length; r++) if (delta < nullvar[r - 1]) break;
	      return r;
	}

	private long HASHP(int c) { return (hashb ^ hashxor[flags | 1024 | (c << 11)]); }
	private long HASHB(int c, int d) { return ((hashb ^ hashxor[flags | 1024]) ^ hashxor[c | (d << 1) | 2048]); }
	private int search(long ch, int c, int d, int ply, int alpha, int beta, int pvnode, int isnull) {
		int i, j, n, w, asave, first, best;
		int hsave, hmove;
		long hb, hp, he;
		
		pvlength[ply] = ply;
		if (ply == 63) return eval(c) + (c != 0 ? -mat: mat);
		if ((++nodes & CNODES) == 0) {
			long consumed = getTime() - starttime;
            if (consumed > maxtime || (consumed > searchtime && !noabort)) sabort = true;
 		}
		if (sabort) return 0;

		hp = HASHP(c);
		if (ply != 0 && isDraw(hp, 1) != 0) return 0;

		if (d == 0) return quiesce(ch, c, ply, alpha, beta);
		hstack[COUNT()] = hp;

		hb = HASHB(c, d);
		
		he = hashDB[(int)(hb & HMASKB)];
		if (((he^hb) & HINVB) == 0) {
			w = (int)LOW16(he) - 32768;
			if ((he & 0x10000) != 0) {
				isnull = 0;
				if (w <= alpha) return alpha;
			} else {
				if (w >= beta) return beta;
			}
		} else {
                w = c != 0 ? -mat : mat;
        }

		if (pvnode == 0 && ch == 0 && isnull != 0 && d > 1 && bitcnt(colorb[c] & (~pieceb[PAWN]) & (~pinnedPieces(kingpos[c], c^1))) > 2) {
			int flagstore = flags;
			int R = (10 + d + nullvariance(w - beta))/4;
			if (R > d) R = d;
			flags &= 960;
			count += 0x401;
			w = -search(0L, c^1, d-R, ply+1, -beta, -alpha, 0, 0); //Null Move Search
			flags = flagstore;
			count -= 0x401;
			if (!sabort && w >= beta) {
				hashDB[(int)(hb & HMASKB)] = (hb & HINVB) | (w + 32768); 
				return beta;
			}
		}

		hsave = hmove = 0;
		if (ply > 0) {
			he = hashDP[(int)(hp & HMASKP)];
			if (((he^hp) & HINVP) == 0) hsave = hmove = (int)(he & HMASKP);
	
			if (d >= 4 && hmove == 0) { // Simple version of Internal Iterative Deepening
				w = search(ch, c, d-3, ply, alpha, beta, pvnode, 0);
				he = hashDP[(int)(hp & HMASKP)];
				if (((he^hp) & HINVP) == 0) hsave = hmove = (int)(he & HMASKP);
			}
		} else {
			hmove = retPVMove(c, ply);
		}

		n = i = -1;
		best = pvnode != 0 ? alpha : -32001;
		asave = alpha;
		first = 1;
		while (++i != n) {
			int m;
			long nch;
			int ext = 0;
			if (hmove != 0) { 
				m = hmove;
				hmove = 0;
				i--;
			} else {
				if (n == -1) {
					generateMoves(ch, c, ply);
					n = movenum[ply];
					if (n == 0) return ch != 0 ? -32000+ply : 0;
				}
				m = spick(movelist[ply], n, i, ply);
				if (hsave != 0 && m == hsave) continue;
			}
			doMove(m, c);

			nch = attacked(kingpos[c^1], c^1);
			if (nch != 0) ext++; // Check Extension
            else if (d >= 3 && i >= 4 && pvnode == 0) { //LMR
                if (CAP(m) != 0 || PROM(m) != 0); //Don't reduce Captures and Promotions
                else if (PIECE(m) == PAWN && (pawnfree[c][TO(m)] & pieceb[PAWN] & colorb[c^1]) == 0); //Don't reduce free pawns
				else ext--;
			}

			if (first != 0 && pvnode != 0) {
				w = -search(nch, c^1, d-1+ext, ply+1, -beta, -alpha, 1, 1);
				if (ply == 0) noabort = (iter > 1 && w < value[iter-1] - 40);
			} else {
				w = -search(nch, c^1, d-1+ext, ply+1, -alpha-1, -alpha, 0, 1);
				if (w > alpha && ext < 0) w = -search(nch, c^1, d-1, ply+1, -alpha-1, -alpha, 0, 1);
				if (w > alpha && w < beta && pvnode != 0) w = -search(nch, c^1, d-1+ext, ply+1, -beta, -alpha, 1, 1);
			}
			undoMove(m, c);

			if (!sabort && w > best) {
				if (w > alpha) {
					hashDP[(int)(hp & HMASKP)] = (hp & HINVP) | m;
					alpha = w;
				}
				if (w >= beta) {
					if (CAP(m) == 0) {
						killer[ply] = m;
						history[m & 0xFFF]++;
					}
					hashDB[(int)(hb & HMASKB)] = (hb & HINVB) | (w + 32768); 
					return beta;
				}
				if (pvnode != 0 && w >= alpha) {
					pv[ply][ply] = m;
					for (j = ply +1; j < pvlength[ply +1]; j++) pv[ply][j] = pv[ply +1][j];
					pvlength[ply] = pvlength[ply +1];
					if (ply == 0 && iter > 1 && w > value[iter-1] - 20) noabort = false;
					if (w == 31999 - ply) return w;
				}
				best = w;
			}
			first = 0;
		}
		if (!sabort && (pvnode != 0 || asave == alpha)) hashDB[(int)(hb & HMASKB)] = (hb & HINVB) | 0x10000 | (best + 32768);
		return alpha;
	}

	private int execMove(int m) {
		int i, c;
		doMove(m, onmove);
		onmove ^= 1; 
		c = onmove;
		if (book) for (i = 0; i < BKSIZE; i++) {
			if (bkflag[i] < 2 && (bkmove[i*32 + COUNT() - 1] != m || bkmove[i*32 + COUNT()] == 0)) {
				bkcount[bkflag[i]]--;
				bkflag[i] = 2;
			}
		}
		hstack[COUNT()] = HASHP(c);
		for (i = 0; i < 127; i++) killer[i] = killer[i+1];
		for (i = 0; i < 0x1000; i++) history[i] = 0;
		i = generateMoves(attacked(kingpos[c], c), c, 0);

		if (movenum[0] == 0) {
			if (i == 0) {
				printf("1/2-1/2 {Stalemate}\n"); return 4;
			} else {
				printf(c == 1 ? "1-0 {White mates}\n" : "0-1 {Black mates}\n"); return 5 + c;
			}
		}
		switch (isDraw(HASHP(c), 2)) {
			case 1: printf("1/2-1/2 {Draw by Repetition}\n"); return 1;
			case 2: printf("1/2-1/2 {Draw by Fifty Move Rule}\n"); return 2;
			case 3: printf("1/2-1/2 {Insufficient material}\n"); return 3;
		}
		return 0;
	}

	private bool ISRANK(int c) { return (c >= '1' && c <= '8'); }
	private bool ISFILE(int c) { return (c >= 'a' && c <= 'h'); }
	
	private bool ismove(int m, int to, int from, int piece, int prom, int h) {
        if (TO(m) != to) return false;
        if (from < 0 && PIECE(m) != piece) return false;
        if (from >= 0 && FROM(m) != from) return false;
        if (ISFILE(h) && (FROM(m) & 7) != h - 'a') return false;
        if (ISRANK(h) && (FROM(m) & 56) != 8*(h - '1')) return false;
        if (prom != 0&& PROM(m) != prom) return false;
        return true;
	}
	
	private int parseMove(String s, int c, int p) {
		int i, to, from = -1, piece = PAWN, prom = 0;
	    char h = (char)0, c1, c2;
		int[] ip = new int[1];
		if (s.StartsWith("O-O-O")) s = c != 0 ? "Kc8" : "Kc1";
		else if (s.StartsWith("O-O")) s = c != 0 ? "Kg8" : "Kg1";
		int sp = 0;

		if (s[sp] >= 'A' && s[sp] <= 'Z') if ((piece = _getpiece(s[sp++], ip)) < 1) return -1;
		if (s[sp] == 'x') sp++;
		if (ISRANK(s[sp])) {h = s[sp++]; if (s[sp] == 'x') sp++; }
		if (!ISFILE(s[sp])) return -1;
		c1 = s[sp++];
		if (s[sp] == 'x') sp++;
		if (ISFILE(s[sp])) {h = c1; c1 = s[sp++];}
		c2 = s[sp++];
		if (!ISRANK(c2)) return -1;
		if (s.Length > sp) {
			if (s[sp] == '=') prom = _getpiece(s[sp + 1], ip);
			else if (s[sp] == '+');
			else { // Algebraic Notation
				from = c1 - 'a' + 8*(c2 - '1');
				c1 = s[sp++]; 
				c2 = s[sp++];
				if (!ISFILE(c1) || !ISRANK(c2)) return -1;
				if (s.Length > sp) prom = _getpiece(s[sp], ip);
			}
		}
		to = c1 - 'a' + 8*(c2 - '1');
        if (p != 0) {
            if (ismove(p, to, from, piece, prom, h)) return p;
            return 0;
        }
		generateMoves(attacked(kingpos[c], c), c, 0);
        for (i = 0; i < movenum[0]; i++) if (ismove(movelist[0][i], to, from, piece, prom, h)) return movelist[0][i];

        return 0;
	}

	private int parseMoveNExec(String s, int c, int[] m) {
        m[0] = parseMove(s, c, 0);
        if (m[0] == -1) printf("UNKNOWN COMMAND: " + s);
        else if (m[0] == 0) errprintf("Illegal move: " + s);
        else return execMove(m[0]);
        return -1;
	}

	private void undo() {
	        int cnt = COUNT() - 1;
	        onmove ^= 1;
	        undoMove((int)(mstack[cnt] >> 42), onmove);
	}

	private int calc(int sd, int tm) {
			int i, j, t1 = 0, m2go = 32;
			long ch = attacked(kingpos[onmove], onmove);
			eval1 = iter = value[0] = 0;
			sabort = false;
			qnodes = nodes = 0L;
			if (mps > 0) m2go = 1 + mps - ((COUNT()/2) % mps);

			searchtime = (tm*10L)/m2go + inc*1000L;
			maxtime = inc != 0 ? tm*3L : tm*2L;

			starttime = getTime();
			Random rand = new Random((int)starttime);
			if (book) {
				if (bkcount[onmove] == 0) book = false;
				else {
					j = rand.Next(bkcount[onmove]);
					for (i = 0; i < BKSIZE; i++) {
						if (bkflag[i] == onmove && j == t1++) { pv[0][0] = bkmove[i*32 + COUNT()]; break; }
					}
				}
			}
			if (!book) for (iter = 1; iter <= sd; iter++) {
				noabort = false;
				value[iter] = search(ch, onmove, iter, 0, -32000, 32000, 1, 0);
				t1 = (int)(getTime() - starttime);
				if (sabort && pvlength[0] == 0 && (iter--) != 0) break;
				if (post && pvlength[0] > 0) {
                    printf(iter.ToString() + " " + value[iter].ToString() + " " +
                        (t1 / 10).ToString() + " " + (nodes + qnodes).ToString() + " " +
					displaypv()); 
				}
				if (iter >= 32000-value[iter] || sabort || t1 > searchtime/2) break;
			}

            bestmove = displaym(pv[0][0]);

            printf((COUNT() / 2 + 1).ToString() + "." + (onmove > 0 ? "... " : " ") + bestmove);

			if (post) printf("\nkibitz W: " + value[iter > sd ? sd : iter].ToString() 
					+ " Nodes: " + nodes.ToString()
                    + " QNodes: " + qnodes.ToString()
                    + " Evals: " + eval1.ToString()
                    + " cs: " + (t1 / 10).ToString()
                    + " knps: " + ((nodes + qnodes) / (t1 + 1)).ToString()); 
			return execMove(pv[0][0]);
	}


	private int protV2(String buf) {
		if (buf.StartsWith("xboard")) printf("feature setboard=1 myname=\"OliThink " + VER + "\" colors=0 analyze=0 done=1\n");
		else if (buf.StartsWith("quit")) return -2;
		else if (buf.StartsWith("new")) return -3;
		else if (buf.StartsWith("remove")) return -4;
		else if (buf.StartsWith("force")) engine = -1;
		else if (buf.StartsWith("go")) engine = onmove;
		else if (buf.StartsWith("setboard")) _parse_fen(buf.Substring(9));
		else if (buf.StartsWith("undo")) undo();
		else if (buf.StartsWith("sd")) sd = Convert.ToInt32(buf.Substring(3));
		else if (buf.StartsWith("time")) time = Convert.ToInt32(buf.Substring(5));
		else if (buf.StartsWith("level")) {
			String[] st = buf.Substring(6).Split(' ');
			mps = Convert.ToInt32(st[0]);
			baze = Convert.ToInt32(st[0]);
			inc = Convert.ToInt32(st[0]);
		}
		else if (buf.StartsWith("post")) post = true;
		else if (buf.StartsWith("nopost")) post = false;
		else if (buf.StartsWith("result"));//result 0-1 {Black mates}
		else if (buf.StartsWith("otim"));//otim <optime>
		else if (buf.StartsWith("draw"));//draw offer
		else if (buf.StartsWith("st"));
		else if (buf.StartsWith("bk"));
		else if (buf.StartsWith("hint"));
		else if (buf.StartsWith("computer"));
		else if (buf.StartsWith("protover"));//protover 2
		else if (buf.StartsWith("accepted"));//accepted <feature>
		else if (buf.StartsWith("random"));
		else if (buf.StartsWith("rating"));//ICS: rating <myrat> <oprat>
		else if (buf.StartsWith("name"));//ICS: name <opname>
        else return -1;
        return 0;
}

	
	public int do_inp(String buf) {
            int i;
			int c = onmove;
			int[] m = new int[1];
			int ex = protV2(buf);
            if (ex == -1) return parseMoveNExec(buf, c, m);
            if (buf.StartsWith("go"))
             {
                // clear cache, sometimes got strange results
                for (i = 0; i < HSIZEB; i++) hashDB[i] = 0L;
                for (i = 0; i < HSIZEP; i++) hashDP[i] = 0L;

                engine = onmove;
                ex = calc(sd, time);
             }
            return ex;
	}

	/**
	 * Call this to initialize variables
	 */
	public void MainInit() {
		int i;
        for (i = 0; i < 64; i++) { movelist[i] = new int[256]; pv[i] = new int[64]; }
				
		for (i = 0; i < 0x10000; i++) LSB[i] = _slow_lsb(i);
		for (i = 0; i < 0x10000; i++) BITC[i] = _bitcnt(i);
		for (i = 0; i < 4096; i++) hashxor[i] = _rand_64();
		for (i = 0; i < HSIZEB; i++) hashDB[i] = 0L;
		for (i = 0; i < HSIZEP; i++) hashDP[i] = 0L;
		for (i = 0; i < 64; i++) BIT[i] = 1L << i;
		for (i = 0; i < 64; i++) pmoves[0][i] = pawnfree[0][i] = pawnfile[0][i] = pawnhelp[0][i] = 0L;
		for (i = 0; i < 192; i++) pcaps[0][i] = 0L;
		for (i = 0; i < 64; i++) pmoves[1][i] = pawnfree[1][i] = pawnfile[1][i] = pawnhelp[1][i] = 0L;
		for (i = 0; i < 192; i++) pcaps[1][i] = 0L;
		for (i = 0; i < 64; i++) bmask45[i] = _bishop45(i, 0L, 0) | BIT[i];
		for (i = 0; i < 64; i++) bmask135[i] = _bishop135(i, 0L, 0) | BIT[i];
		for (i = 0; i < 64; i++) crevoke[i] = 0x3FF;
		for (i = 0; i < 64; i++) kmoves[i] = nmoves[i] = 0L;
		crevoke[7] ^= (int)BIT[6];
        crevoke[63] ^= (int)BIT[7];
        crevoke[0] ^= (int)BIT[8];
        crevoke[56] ^= (int)BIT[9];

		_init_rays1();
		_init_rays2();
		_init_rays3();
		_init_rays4();
		_init_shorts(nmoves, _knight);
		_init_shorts(kmoves, _king);
		_init_pawns(pmoves[0], pcaps[0], pawnfree[0], pawnfile[0], pawnhelp[0], 0);
		_init_pawns(pmoves[1], pcaps[1], pawnfree[1], pawnfile[1], pawnhelp[1], 1);
		_readbook("");
			
		for (i = 0; i < 64; i++) nmobil[i] = (bitcnt(nmoves[i])-1)*6;
		for (i = 0; i < 64; i++) kmobil[i] = (bitcnt(nmoves[i])/2)*2;
		
		sd = 5;

		//time = 10000;
        //do_inp("setboard 7k/Q7/2P2K2/8/8/8/8/8 w - - 0 1");	// # in 1 move
        //do_inp("setboard r3kr2/pbq5/2pRB1p1/8/4QP2/2P3P1/PP6/2K5 w q - 0 36");	// # in 3 moves
        //do_inp("go");

	}

    }
}
