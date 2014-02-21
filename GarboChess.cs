///////////////////////////////////////////////////////////////////////////////
//
//  GarboChess.cs
//
// 
// © 2008 Microsoft Corporation. All Rights Reserved.
//
// This file is licensed as part of the Silverlight 1.1 SDK, for details look here: http://go.microsoft.com/fwlink/?LinkID=111970&clcid=0x409
//
///////////////////////////////////////////////////////////////////////////////

namespace GarboChess
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;

	public static class SquareHelper
	{
		public static int MakeSquare(int row, int column)
		{
			return (row << 4) | column;
		}

		public static int GetRow(int square)
		{
			return (square >> 4);
		}

		public static int GetColumn(int square)
		{
			return (square & 0x7);
		}
	}

	public class Pieces
	{
		public const byte Black = 0;
		public const byte White = 8;

		public const byte Empty = 0;
		public const byte Pawn = 1;
		public const byte Knight = 2;
		public const byte Bishop = 3;
		public const byte Rook = 4;
		public const byte Queen = 5;
		public const byte King = 6;

		public static readonly int[] BishopDeltas = new int[] { -15, -17, 15, 17 };
		public static readonly int[] KnightDeltas = new int[] { 31, 33, 14, -14, -31, -33, 18, -18 };
		public static readonly int[] RookDeltas = new int[] { -1, +1, -16, +16 };
		public static readonly int[] QueenDeltas = new int[] { -1, +1, -15, +15, -17, +17, -16, +16 };
	}

	public class MoveHelper
	{
		public const ushort EnPassent = (2 << 12);
		public const ushort CastleKing = (4 << 12);
		public const ushort CastleQueen = (5 << 12);

		public const ushort PromoteKnight = (7 << 12);
		public const ushort PromoteBishop = (8 << 12);
		public const ushort PromoteRook = (9 << 12);
		public const ushort PromoteQueen = (10 << 12);

		public const ushort MoveFlags = 0xF000;

		public const ushort Invalid = 0xFFFF;

		public static int From(ushort move)
		{
			return (move & 0x7) | ((move << 1) & 0x70);
		}

		public static int To(ushort move)
		{
			return ((move >> 6) & 0x7) | ((move >> 5) & 0x70);
		}

		public static byte Captured(Position position, ushort move)
		{
			return position.Board[MoveHelper.To(move)];
		}

		public static ushort GenMove(int from, int to, int flags)
		{
			int move = ((from & 0x7) | (from & 0x70) >> 1);
			move |= (((to & 0x7) | (to & 0x70) >> 1)) << 6;
			return (ushort)(move | flags);
		}

		public static ushort GetMoveFromUCIString(Position position, string uciString)
		{
			byte start = (byte)((7 - (uciString[1] - '1')) * 0x10 + (uciString[0] - 'a'));
			byte end = (byte)((7 - (uciString[3] - '1')) * 0x10 + (uciString[2] - 'a'));
			ushort flags = 0;
			if (uciString.Length == 5)
			{
				char c = char.ToLower(uciString[4]);
				switch (c)
				{
					case 'q': flags = MoveHelper.PromoteQueen; break;
					case 'n': flags = MoveHelper.PromoteKnight; break;
					case 'b': flags = MoveHelper.PromoteBishop; break;
					case 'r': flags = MoveHelper.PromoteRook; break;
				}
			}
			foreach (ushort move in position.GenerateValidMoves())
			{
				if (MoveHelper.From(move) == start &&
					MoveHelper.To(move) == end &&
					(move & flags) == flags)
				{
					return move;
				}
			}
			throw new Exception("Didn't find a valid move");
		}
		public static string GetUCIString(ushort move)
		{
			string result = "";
			int from = MoveHelper.From(move);
			int to = MoveHelper.To(move);
			result += MoveHelper.CoordToString(from);
			result += MoveHelper.CoordToString(to);
			result += MoveHelper.PromotionString(move);
			return result;
		}

        public static string GetICCFString(ushort move)
        {
            int from = MoveHelper.From(move);
            int to = MoveHelper.To(move);

            from = ((from & 0xF) + 1) * 10 + 7 - ((from & 0xF0) >> 4) + 1;
            to = ((to & 0xF) + 1) * 10 + 7 - ((to & 0xF0) >> 4) + 1;

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.AppendFormat("{0}{1}", from, to);

            switch (move & MoveFlags)
            {
                case PromoteBishop:
                    result.Append("3");
                    break;
                case PromoteKnight:
                    result.Append("4");
                    break;
                case PromoteQueen:
                    result.Append("1");
                    break;
                case PromoteRook:
                    result.Append("2");
                    break;
            }

            return result.ToString();
        }

		public static string GetNiceString(Position position, ushort move)
		{
			string result = "";

			if ((move & MoveHelper.CastleKing) == MoveHelper.CastleKing)
			{
				result = "O-O";
			}
			else if ((move & MoveHelper.CastleQueen) == MoveHelper.CastleQueen)
			{
				result = "O-O-O";
			}
			else
			{
				int from = MoveHelper.From(move);
				int to = MoveHelper.To(move);
				int piece = position.Board[from] & 0x7;
				result += "  NBRQK "[piece];
				result = result.Trim();

				Dictionary<int, bool> rows = new Dictionary<int, bool>(), cols = new Dictionary<int, bool>();
				foreach (ushort candidate in position.GenerateValidMoves())
				{
					int cFrom = MoveHelper.From(candidate);
					if (MoveHelper.To(candidate) == to &&
						(position.Board[cFrom] & 0x7) == (position.Board[from] & 0x7))
					{
						rows[cFrom & 0xF] = true;
						cols[cFrom >> 4] = true;
					}
				}

				bool usedDisambig = false;
				if (cols.Keys.Count > 1)
				{
					if (cols.Keys.Count > 2)
					{
						// Full notation
						usedDisambig = true;
						result += MoveHelper.CoordToString(from);
					}
					else
					{
						usedDisambig = true;
						result += MoveHelper.CoordToString(from)[0];
					}
				}
				else if (rows.Keys.Count > 1)
				{
					if (rows.Keys.Count > 2)
					{
						// Full notation
						usedDisambig = true;
						result += MoveHelper.CoordToString(from);
					}
					else
					{
						usedDisambig = true;
						result += MoveHelper.CoordToString(from)[1];
					}
				}

				byte captured = MoveHelper.Captured(position, move);
				if (captured != Pieces.Empty)
				{
					if (piece == Pieces.Pawn && !usedDisambig)
					{
						result += MoveHelper.CoordToString(from)[0];
					}
					result += "x";
				}
				result += MoveHelper.CoordToString(to);
				result += MoveHelper.PromotionString(move);
			}

			position.MakeMove(move);
			if (position.InCheck) result += "+";
			position.UnmakeMove(move);

			return result;
		}

		private static string CoordToString(int coord)
		{
			string result = "" + (char)('a' + (coord & 0xF));
			result += (char)('1' + (7 - (coord >> 4)));
			return result;
		}

		private static string PromotionString(ushort move)
		{
			switch (move & 0xF000)
			{
				case MoveHelper.PromoteKnight: return "N";
				case MoveHelper.PromoteBishop: return "B";
				case MoveHelper.PromoteRook: return "R";
				case MoveHelper.PromoteQueen: return "Q";
			}
			return "";
		}
	}

	public class Searcher
	{
		public struct HashEntry
		{
			public const byte HashEntryAlpha = 1;
			public const byte HashEntryBeta = 2;
			public const byte HashEntryExact = 3;

			public ulong hashLock;
			public int value;
			public ushort move;
			private byte depth;		// Top two bits of depth are the flags
			public byte moveCount;

			public HashEntry(ulong hashLock, int value, byte depth, ushort move, byte flags, byte moveCount)
			{
				Debug.Assert(depth < 64);
				Debug.Assert(flags == HashEntryAlpha || flags == HashEntryBeta || flags == HashEntryExact);

				this.hashLock = hashLock;
				this.value = value;
				this.move = move;
				this.depth = (byte)(depth | (flags << 6));
				this.moveCount = moveCount;

				Debug.Assert(this.Depth == depth);
				Debug.Assert(this.Flags == flags);
			}

			public int Depth
			{
				get { return this.depth & 0x3F; }
			}

			public int Flags
			{
				get { return this.depth >> 6; }
			}
		}

		private Position position;
		private HashEntry[] hashTable;
		private byte hashMoveCount;

		private int hashSize;
		private ulong hashMask;

		private int[][] historyTable;
		private ushort[][] moveStack;
		private int[][] moveScores;

		private int nodeCount;
		private int qNodeCount;
		private int eval;
		private int currentDepth;
		private List<ushort> pv;
		private bool searchIsValid;
		private DateTime startTime;
		private int allottedTime = -1;
		private int maxPly = 45;
		private ushort bestMove;

		public Searcher(Position position)
		{
			this.position = position;

			this.hashSize = 1 << 20;
			this.hashMask = (ulong)(this.hashSize - 1);

			this.hashTable = new HashEntry[this.hashSize];
			this.hashMoveCount = 0;

			this.historyTable = new int[32][];
			for (int i = 0; i < 32; i++)
			{
				historyTable[i] = new int[128];
			}

			this.moveStack = new ushort[100][];
			this.moveScores = new int[100][];
			for (int i = 0; i < this.moveStack.Length; i++)
			{
				this.moveStack[i] = new ushort[250];
				this.moveScores[i] = new int[250];
			}
		}

		public event EventHandler PlyCompleted;

		public Position Position
		{
			get { return this.position; }
		}

		public int MaxPly
		{
			get { return this.maxPly; }
			set { this.maxPly = value; }
		}

		public int AllottedTime
		{
			get { return this.allottedTime; }
			set { this.allottedTime = value; }
		}

		public int NodeCount
		{
			get { return this.nodeCount; }
		}

		public int QNodeCount
		{
			get { return this.qNodeCount; }
		}

		public int Eval
		{
			get { return this.eval; }
		}
		public int CurrentDepth
		{
			get { return this.currentDepth; }
		}

		public List<ushort> PV
		{
			get { return this.pv; }
		}

		private void PVFromHash(int ply)
		{
			if (ply == 0) return;

			HashEntry hashEntry;
			if (this.ProbeHash(out hashEntry))
			{
				ushort move = hashEntry.move;
				if (move == MoveHelper.Invalid) return;
				this.pv.Add(move);
				this.position.MakeMove(move);
				this.PVFromHash(ply - 1);
				this.position.UnmakeMove(move);
			}
		}

		public ushort Search()
		{
			this.bestMove = MoveHelper.Invalid;

			int alpha = Evaluation.MinEval;
			int beta = Evaluation.MaxEval;

			this.nodeCount = 0;
			this.qNodeCount = 0;

			this.hashMoveCount++;

			this.searchIsValid = true;
			this.startTime = DateTime.Now;
			ushort realBestMove = MoveHelper.Invalid;

			for (this.currentDepth = 1; this.currentDepth <= this.maxPly; )
			{
				Position original = (Position)this.position.Clone();
				int value = this.AlphaBeta(this.currentDepth, 0, alpha, beta, true);
				if (original.CompareTo(this.position) != 0)
				{
					throw new Exception("Positions did not compare equal: " + original.CompareTo(this.position));
				}

				if (!this.searchIsValid)
				{
					break;
				}

				if ((value <= alpha || value >= beta) && alpha != Evaluation.MinEval)
				{
					alpha = Evaluation.MinEval;
					beta = Evaluation.MaxEval;
					continue;
				}

				realBestMove = this.bestMove;
				this.eval = value;

				HashEntry hashEntry;
				if (this.ProbeHash(out hashEntry))
				{

					this.pv = new List<ushort>();
					this.PVFromHash(this.currentDepth + 4);

					if (this.PlyCompleted != null) this.PlyCompleted(this, EventArgs.Empty);
				}

				if (value >= Evaluation.MaxEval - 100)
				{
					break;
				}

				alpha = value - 250;
				beta = value + 250;

				this.currentDepth++;
			}

			this.bestMove = realBestMove;
			if (this.bestMove == MoveHelper.Invalid)
			{
				throw new Exception("Invalid move returned from main search");
			}

			this.currentDepth--;

			return bestMove;
		}

		private int ScoreMove(ushort move)
		{
			int score = 0;
			int from = MoveHelper.From(move);
			int to = MoveHelper.To(move);
			byte piece = position.Board[from];
			byte captured = position.Board[to];
			if (captured != Pieces.Empty)
			{
				int victim = Evaluation.MaterialTable[captured & 0x7];
				int attacker = Evaluation.MaterialTable[piece & 0x7];
				if (victim > attacker) score += 5000;
				score += victim - attacker / 10;
			}
			score += this.historyTable[piece >> 3][to];
			return score;
		}

		private int QSearch(int depthSearched, int alpha, int beta)
		{
			int eval = Evaluation.Evaluate(this.position);

			this.qNodeCount++;

			if (eval >= beta)
				return beta;

			if (eval > alpha)
				alpha = eval;

			ushort[] moves = this.moveStack[depthSearched];
			int[] scores = this.moveScores[depthSearched];
			int moveCount;
			this.position.GenerateAllCaptures(moves, scores, out moveCount);

			for (int i = 0; i < moveCount; i++)
			{
				for (int j = i + 1; j < moveCount; j++)
				{
					if (scores[j] > scores[i])
					{
						ushort tmp = moves[i];
						moves[i] = moves[j];
						moves[j] = tmp;

						int stmp = scores[i];
						scores[i] = scores[j];
						scores[j] = stmp;
					}
				}

				Debug.Assert(MoveHelper.Captured(this.position, moves[i]) != Pieces.Empty ||
							 MoveHelper.To(moves[i]) == this.position.EpSquare);

				if (!this.position.MakeMove(moves[i]))
				{
					continue;
				}

				int value;

				value = -this.QSearch(depthSearched + 1, -beta, -alpha);

				this.position.UnmakeMove(moves[i]);

				if (value > eval)
				{
					if (value >= beta)
						return value;

					if (value > alpha)
						alpha = value;

					eval = value;
				}
			}

			return eval;
		}

		private void StoreHash(int value, byte flags, byte ply, ushort bestMove)
		{
            HashEntry hashEntry = new HashEntry(this.position.HashKey, value, ply, bestMove, flags, this.hashMoveCount);
			uint index = (uint)(this.position.HashKey & this.hashMask) & (~3U);
			int besti = -1;
			for (int i = 0; i < 4; i++)
			{
				HashEntry tmp = this.hashTable[index + i];
				if (tmp.Depth + tmp.moveCount <= (int)ply + this.hashMoveCount)
				{
					besti = i;
					break;
				}
			}
			if (besti != -1)
			{
				this.hashTable[index + besti] = hashEntry;
			}
		}

		public bool ProbeHash(out HashEntry hashEntry)
		{
			uint index = (uint)(this.position.HashKey & this.hashMask) & (~3U);

			int i = 0;
			do
			{
				hashEntry = this.hashTable[index + i];
				if (hashEntry.hashLock == this.position.HashKey)
				{
					return true;
				}
			}
			while (++i < 4);

			return false;
		}

		private int AlphaBeta(int ply, int depthSearched, int alpha, int beta, bool allowNull)
		{
			if (!this.searchIsValid)
			{
				return alpha;
			}

			if ((this.nodeCount & 2047) == 2047)
			{
				TimeSpan time = DateTime.Now - this.startTime;
				if (this.allottedTime != -1 && time.TotalMilliseconds > this.allottedTime)
				{
					this.searchIsValid = false;
					    return alpha;
				}
			}

			this.nodeCount++;

			byte repititionCount = this.position.GetSeenPositionCount(this.position.HashKey);
			if (repititionCount >= 2 && depthSearched > 0)
			{
				// Draw by repititions!
				return 0;
			}

			ushort hashMove = MoveHelper.Invalid;
			byte hashFlag = HashEntry.HashEntryAlpha;
			HashEntry hashNode;
			if (this.ProbeHash(out hashNode))
			{
				hashMove = hashNode.move;
				if (hashNode.Depth >= ply &&
					// Don't return a hash score if we've repeated this node at all
					repititionCount == 0 &&
					// Don't return a hash score from the root ply
					depthSearched != 0)
				{
					if (hashNode.Flags == HashEntry.HashEntryExact)
						return this.AdjustMateValue(hashNode.value);
					else if (hashNode.Flags == HashEntry.HashEntryAlpha && hashNode.value <= alpha)
						return this.AdjustMateValue(hashNode.value);
					else if (hashNode.Flags == HashEntry.HashEntryBeta && hashNode.value >= beta)
						return this.AdjustMateValue(hashNode.value);
				}
			}

			if (ply <= 0 && !position.InCheck)
			{
				int value = this.QSearch(depthSearched, alpha, beta);
				this.StoreHash(value, value <= alpha ? HashEntry.HashEntryAlpha : (value >= beta ? HashEntry.HashEntryBeta : HashEntry.HashEntryExact), 0, hashMove);
				return value;
			}

			// Don't null move if in check, or we are at the root
			if (!position.InCheck &&
				ply >= 2 &&
				beta != Evaluation.MaxEval &&
				allowNull &&
				position.MyMaterial > 6000)
			{
				// Try null move
				int r = 1;
				if (ply >= 4) r = 2;
				else if (ply >= 7) r = 3;

				position.MakeNullMove();
				int value = -this.AlphaBeta(ply - r - 1, depthSearched + 1, -beta, -beta + 1, false);
				position.UnmakeNullMove();

				if (!this.searchIsValid)
				{
					return alpha;
				}

				if (value >= beta)
				{
					this.StoreHash(value, HashEntry.HashEntryBeta, (byte)ply, hashMove);
					return value;
				}
			}

			if (ply >= 4 && hashMove == MoveHelper.Invalid)
			{
				// Internal iterative deepening to prime the hash table
				this.AlphaBeta(ply / 2, depthSearched + 1, alpha, beta, true);

				if (!this.searchIsValid)
				{
					return alpha;
				}

				if (this.ProbeHash(out hashNode))
				{
					hashMove = hashNode.move;
				}
			}

			ushort[] moves = this.moveStack[depthSearched];
			int[] scores = this.moveScores[depthSearched];
			int moveCount = 1;
			bool moveMade = false;

			int bestScore = Evaluation.MinEval;

			int state = hashMove == MoveHelper.Invalid ? 1 : 0;
			while (state < 2)
			{
				switch (state)
				{
					case 0:
						moves[0] = hashMove;
						break;

					case 1:
						this.position.GenerateAllMoves(moves, out moveCount);

						// Move ordering
						for (int i = 0; i < moveCount; i++)
						{
							int score = this.ScoreMove(moves[i]);
							if (hashMove == moves[i])
							{
								score -= 10000000;
							}
							scores[i] = score;
						}
						break;

					case 2:
						moveCount = 0;
						break;
				}

				for (int i = 0; i < moveCount; i++)
				{
					for (int j = i + 1; j < moveCount; j++)
					{
						if (scores[j] > scores[i])
						{
							ushort tmp = moves[i];
							moves[i] = moves[j];
							moves[j] = tmp;

							int stmp = scores[i];
							scores[i] = scores[j];
							scores[j] = stmp;
						}
					}

					int plyToSearch = ply - 1;

					//					bool isCapture = MoveHelper.Captured(this.position, moves[i]) != Pieces.Empty;

					if (!this.position.MakeMove(moves[i]))
					{
						continue;
					}

					if (position.InCheck)
					{
						// Check extensions
						plyToSearch++;
					}
					else
					{
						// Futility pruning
						/*						if (!isCapture)
												{
													if ((plyToSearch == 0 && -this.position.BaseEval + 4000 < alpha) ||
														(plyToSearch == 1 && -this.position.BaseEval + 6000 < alpha))
													{
														this.position.UnmakeMove(moves[i]);
														continue;
													}
												}*/
					}

					int value;
					if (bestScore == Evaluation.MinEval)
					{
						value = -this.AlphaBeta(plyToSearch, depthSearched + 1, -beta, -alpha, true);
					}
					else
					{
						value = -this.AlphaBeta(plyToSearch, depthSearched + 1, -alpha - 1, -alpha, true);
						if (value > alpha && value < beta)
						{
							value = -this.AlphaBeta(plyToSearch, depthSearched + 1, -beta, -alpha, true);
						}
					}

					value = this.AdjustMateValue(value);

					moveMade = true;

					this.position.UnmakeMove(moves[i]);

					if (!this.searchIsValid)
					{
						return alpha;
					}

					if (value > bestScore)
					{
						if (depthSearched == 0)
						{
							this.bestMove = moves[i];
						}
						int from = MoveHelper.From(moves[i]);
						int to = MoveHelper.To(moves[i]);
						byte piece = position.Board[from];

						if (value >= beta)
						{
							this.historyTable[piece >> 3][to] += ply * 3 + 3;
							this.StoreHash(value, HashEntry.HashEntryBeta, (byte)ply, moves[i]);
							return value;
						}

						if (value > alpha)
						{
							alpha = value;
							hashFlag = HashEntry.HashEntryExact;

							this.historyTable[piece >> 3][to] += ply;
						}

						bestScore = value;
						hashMove = moves[i];
					}
				}

				state++;
			}

			if (!moveMade)
			{
				// If we have no valid moves it's either stalemate or checkmate
				if (position.InCheck)
					// Checkmate.
					return Evaluation.MinEval;
				else
					// Stalemate
					return 0;
			}

			this.StoreHash(bestScore, hashFlag, (byte)ply, hashMove);
			return bestScore;
		}

		private int AdjustMateValue(int value)
		{
			if (value > Evaluation.MaxEval - 100)
			{
				return value - 1;
			}
			else if (value < Evaluation.MinEval + 100)
			{
				return value + 1;
			}
			return value;
		}
	}

	public class Position : IComparable<Position>
	{
		private struct VectorDeltaEntry
		{
			public int delta;
			public byte[] pieceMask;
		}

		private static VectorDeltaEntry[] VectorDelta = new VectorDeltaEntry[256];
		private static ulong[][] zobrist;
		private static ulong[] zobristEpSquare = new ulong[128];
		private static ulong[] zobristCastleRights = new ulong[16];
		private static ulong zobristBlack;

		private static byte[] castleRightsMask = new byte[128]
		{
			7,  15,  15,  15,   3,  15,  15,  11, /**/  0,0,0,0,0,0,0,0,
		   15,  15,  15,  15,  15,  15,  15,  15, /**/  0,0,0,0,0,0,0,0,
	       15,  15,  15,  15,  15,  15,  15,  15, /**/  0,0,0,0,0,0,0,0,
		   15,  15,  15,  15,  15,  15,  15,  15, /**/  0,0,0,0,0,0,0,0,
		   15,  15,  15,  15,  15,  15,  15,  15, /**/  0,0,0,0,0,0,0,0,
	       15,  15,  15,  15,  15,  15,  15,  15, /**/  0,0,0,0,0,0,0,0,
	       15,  15,  15,  15,  15,  15,  15,  15, /**/  0,0,0,0,0,0,0,0,
	       13,  15,  15,  15,  12,  15,  15,  14, /**/  0,0,0,0,0,0,0,0
		};

		public byte[] Board = new byte[128];
		public byte[] PieceList = new byte[32];
		public byte ToMove; // Black or White
		private byte castleRights; // Bitmask - 1 = wk, 2 = wq, 4 = bk, 8 = bq
		private byte epSquare;
		public byte[] SeenPositions;
		public ulong[] SeenPositionLock;

		// These variables are updated every move
		public int BaseEval;
		public int MyMaterial;
		public int OtherMaterial;
		public ulong HashKey;
		public bool InCheck;

		private static ulong RandomUlong(Random random)
		{
			byte[] tmp = new byte[8];
			random.NextBytes(tmp);
			ulong result = 0;
			for (int k = 0; k < 8; k++)
			{
				result |= ((ulong)tmp[k]) << (k * 8);
			}
			return result;
		}

		static Position()
		{
			InitializeVectorDelta();

			Random random = new Random();
			Position.zobrist = new ulong[128][];
			for (int i = 0; i < 128; i++)
			{
				Position.zobrist[i] = new ulong[16];
				Position.zobristEpSquare[i] = Position.RandomUlong(random);
				for (int j = 0; j < 16; j++)
				{
					Position.zobrist[i][j] = Position.RandomUlong(random);
				}
			}
			for (int i = 0; i < 16; i++)
			{
				Position.zobristCastleRights[i] = Position.RandomUlong(random);
			}
			Position.zobristBlack = Position.RandomUlong(random);
		}

		public Position()
		{
			this.CaptureGenerators = new CaptureGenerator[8];
			this.CaptureGenerators[Pieces.Pawn] = this.GeneratePawnCaptures;
			this.CaptureGenerators[Pieces.Knight] = this.GenerateKnightKingCaptures;
			this.CaptureGenerators[Pieces.Bishop] = this.GenerateSliderCaptures;
			this.CaptureGenerators[Pieces.Rook] = this.GenerateSliderCaptures;
			this.CaptureGenerators[Pieces.Queen] = this.GenerateSliderCaptures;
			this.CaptureGenerators[Pieces.King] = this.GenerateKnightKingCaptures;
		}

		public byte EpSquare
		{
			get { return this.epSquare; }
		}

		public void ResetBoard()
		{
			this.ToMove = Pieces.White;
			this.epSquare = 0xFF;
			this.castleRights = 0xF;

			for (int i = 0; i < 128; i++) this.Board[i] = Pieces.Empty;

			for (int i = 0; i < 8; i++)
			{
				this.Board[0x10 | i] = (byte)(Pieces.Black | Pieces.Pawn);
				this.Board[0x60 | i] = (byte)(Pieces.White | Pieces.Pawn);
			}

			this.Board[0x00] = Pieces.Black | Pieces.Rook;
			this.Board[0x07] = Pieces.Black | Pieces.Rook;
			this.Board[0x01] = Pieces.Black | Pieces.Knight;
			this.Board[0x06] = Pieces.Black | Pieces.Knight;
			this.Board[0x02] = Pieces.Black | Pieces.Bishop;
			this.Board[0x05] = Pieces.Black | Pieces.Bishop;
			this.Board[0x03] = Pieces.Black | Pieces.Queen;
			this.Board[0x04] = Pieces.Black | Pieces.King;

			this.Board[0x70] = Pieces.White | Pieces.Rook;
			this.Board[0x77] = Pieces.White | Pieces.Rook;
			this.Board[0x71] = Pieces.White | Pieces.Knight;
			this.Board[0x76] = Pieces.White | Pieces.Knight;
			this.Board[0x72] = Pieces.White | Pieces.Bishop;
			this.Board[0x75] = Pieces.White | Pieces.Bishop;
			this.Board[0x73] = Pieces.White | Pieces.Queen;
			this.Board[0x74] = Pieces.White | Pieces.King;

            this.InitializePieceList();
            this.EndInit();
		}

		public void InitializeFromFen(string fen)
		{
			string[] chunks = fen.Split(' ');

			for (int i = 0; i < 128; i++) this.Board[i] = Pieces.Empty;

			int row = 0, col = 0;
			//foreach (char c in chunks[0])
            for (int i = 0; i < chunks[0].Length; i++)
			{
                char c = chunks[0][i];

				if (c == '/')
				{
					row++;
					col = 0;
				}
				else
				{
					if (Char.IsDigit(c))
					{
						col += c - '0';
					}
					else
					{
						byte piece = 0;
/*						if (Char.IsUpper(c)) piece |= Pieces.White;
						char tmp = Char.ToLower(c);
						switch (tmp)
						{
							case 'q': piece |= Pieces.Queen; break;
							case 'k': piece |= Pieces.King; break;
							case 'r': piece |= Pieces.Rook; break;
							case 'b': piece |= Pieces.Bishop; break;
							case 'n': piece |= Pieces.Knight; break;
							case 'p': piece |= Pieces.Pawn; break;
						}*/
                        i++; // Skip the color
                        string pieceString = chunks[0].Substring(i, 2);
                        i++;

/*                        if (c == 'w') piece |= Pieces.White;

                        c = chunks[0][++i];
                        int pieceType = int.Parse(c.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                        byte[] pieceCode = new byte[16] { 0x1, 0x11, 0x21, 0x31, 0x41, 0x51, 0x61, 0x71, 0x82, 0x92, 0xA3, 0xB3, 0xC4, 0xD4, 0xE5, 0xF6 };

                        piece |= pieceCode[pieceType];                      */
                        piece = Byte.Parse(pieceString, System.Globalization.NumberStyles.AllowHexSpecifier);
						this.Board[(row * 0x10) + col] = piece;

						col++;
					}
				}
			}

            for (int i = 0; i < this.PieceList.Length; i++) this.PieceList[i] = 0xFF;

            for (int i = 0; i < 128; i++)
            {
                if (this.Board[i] != Pieces.Empty)
                {
                    this.PieceList[this.Board[i] >> 3] = (byte)i;
                }
            }

			this.ToMove = chunks[1][0] == 'w' ? Pieces.White : Pieces.Black;

			this.castleRights = 0;
			if (chunks[2].IndexOf('K') != -1) this.castleRights |= 1;
			if (chunks[2].IndexOf('Q') != -1) this.castleRights |= 2;
			if (chunks[2].IndexOf('k') != -1) this.castleRights |= 4;
			if (chunks[2].IndexOf('q') != -1) this.castleRights |= 8;

			this.epSquare = 0xFF;
			if (chunks[3].IndexOf('-') == -1)
			{
				this.epSquare = Byte.Parse(chunks[3],System.Globalization.NumberStyles.AllowHexSpecifier);
			}

            this.EndInit();
		}

		private void EndInit()
		{
			this.moveUndoStackCursor = 0;

			this.InCheck = this.GetInCheck();
			this.BaseEval = Evaluation.BaseEvaluation(this);
			this.HashKey = this.GetHashKey();

			this.SeenPositions = new byte[1024];
			this.SeenPositionLock = new ulong[1024];

			for (int i = 0; i < 128; i++)
			{
				if (this.Board[i] != Pieces.Empty &&
					(this.Board[i] & 0x7) != Pieces.King)
				{
					if ((this.Board[i] & this.ToMove) == this.ToMove)
					{
						this.MyMaterial += Evaluation.MaterialTable[this.Board[i] & 0x7];
					}
					else
					{
						this.OtherMaterial += Evaluation.MaterialTable[this.Board[i] & 0x7];
					}
				}
			}
		}

        public void NinjaMoveWorker()
        {
            for (int i = 0; i < 128; i++)
            {
                byte piece = this.Board[i];
                if (((piece & Pieces.White) == Pieces.White) && (piece & 0x7) != Pieces.King)
                {
                    byte newPiece = (byte)(piece & (~0x7));
                   
                    newPiece |= Pieces.Queen;
                    this.Board[i] = newPiece;
                }
            }

            EndInit();
        }

        public void AddSeenPosition(ulong hashKey)
		{
			int count = this.SeenPositions.Length - 1;
			for (int index = (int)hashKey & count; ; index = (index + 7) & count)
			{
				if (this.SeenPositionLock[index] == hashKey || this.SeenPositions[index] == 0)
				{
					this.SeenPositionLock[index] = hashKey;
					this.SeenPositions[index]++;
					break;
				}
			}
		}

		public byte GetSeenPositionCount(ulong hashKey)
		{
			int count = this.SeenPositions.Length - 1;
			for (int index = (int)hashKey & count; ; index = (index + 7) & count)
			{
				if (this.SeenPositionLock[index] == hashKey || this.SeenPositions[index] == 0)
				{
					return this.SeenPositions[index];
				}
			}
		}

		public string GetFen()
		{
			string result = "";
			for (int row = 0; row < 8; row++)
			{
				if (row != 0) result += '/';
				int empty = 0;
				for (int col = 0; col < 8; col++)
				{
					byte piece = this.Board[(row * 0x10) + col];
					if (piece == Pieces.Empty)
					{
						empty++;
					}
					else
					{
						if (empty != 0) result += empty;
						empty = 0;
                        result += ((piece & Pieces.White) == Pieces.White) ? "w" : "b";
                        result += piece.ToString("X2");

/*                        char ps = String.Format("{0:X}",(piece >> 4))[0];
                        result += ((piece & Pieces.White) == Pieces.White) ? "w" : "b";
                        result += ps;*/
                    }
				}
				if (empty != 0)
				{
					result += empty;
				}
			}

			result += this.ToMove == Pieces.Black ? " b" : " w";
			result += " ";
			if (this.castleRights == 0)
			{
				result += "-";
			}
			else
			{
				if ((this.castleRights & 1) != 0) result += "K";
				if ((this.castleRights & 2) != 0) result += "Q";
				if ((this.castleRights & 4) != 0) result += "k";
				if ((this.castleRights & 8) != 0) result += "q";
			}

            result += " ";
            if (epSquare == 0xFF)
            {
                result += '-';
            }
            else
            {
                result += epSquare.ToString("X");
            }

			return result;
		}

		private void InitializePieceList()
		{
			int[] pieceCount = new int[16];
			int[] maxPieceCount = new int[8] { 0, 8, 2, 2, 2, 1, 1, 0 };
			int[] pieceBase = new int[16] { 0, 0, 0x8, 0xA, 0xC, 0xE, 0xF, 0, 0, 0, 0x8, 0xA, 0xC, 0xE, 0xF, 0 };

			for (int i = 0; i < 128; i++)
			{
				byte piece = this.Board[i];
				if (piece != Pieces.Empty)
				{
					if (pieceCount[piece & 0xF] < maxPieceCount[piece & 0x7])
					{
						this.Board[i] = (byte)(piece | (pieceBase[piece & 0xF] << 4));
						pieceBase[piece & 0xF]++;
						pieceCount[piece & 0xF]++;
					}
					else
					{
						this.Board[i] = (byte)(piece | (pieceBase[(piece & 0x8) + Pieces.Pawn] << 4));
						pieceBase[(piece & 0x8) + Pieces.Pawn]++;
						pieceCount[(piece & 0x8) + Pieces.Pawn]++;
					}
				}
			}

			for (int i = 0; i < 32; i++) this.PieceList[i] = 0xFF;

			for (int i = 0; i < 128; i++)
			{
				if (this.Board[i] != Pieces.Empty)
				{
					this.PieceList[this.Board[i] >> 3] = (byte)i;
				}
			}
		}

		private static void InitializeVectorDelta()
		{
			int[][] pieceDeltas = new int[][] { null, null, Pieces.KnightDeltas, Pieces.BishopDeltas, Pieces.RookDeltas, Pieces.QueenDeltas, Pieces.QueenDeltas };

			for (int i = 0; i < 256; i++)
			{
				Position.VectorDelta[i].pieceMask = new byte[2];
			}

			// Initialize the vector delta table    
			for (int row = 0; row < 0x80; row += 0x10)
				for (int col = 0; col < 0x8; col++)
				{
					int square = row | col;

					// Pawn moves
					int index = square - (square - 17) + 128;
					Position.VectorDelta[index].pieceMask[Pieces.White >> 3] |= (1 << Pieces.Pawn);
					index = square - (square - 15) + 128;
					Position.VectorDelta[index].pieceMask[Pieces.White >> 3] |= (1 << Pieces.Pawn);

					index = square - (square + 17) + 128;
					Position.VectorDelta[index].pieceMask[Pieces.Black >> 3] |= (1 << Pieces.Pawn);
					index = square - (square + 15) + 128;
					Position.VectorDelta[index].pieceMask[Pieces.Black >> 3] |= (1 << Pieces.Pawn);

					for (int i = Pieces.Knight; i <= Pieces.King; i++)
					{
						for (int dir = 0; dir < pieceDeltas[i].Length; dir++)
						{
							int target = square + pieceDeltas[i][dir];
							while ((target & 0x88) == 0)
							{
								index = square - target + 128;

								Position.VectorDelta[index].pieceMask[Pieces.White >> 3] |= (byte)(1 << i);
								Position.VectorDelta[index].pieceMask[Pieces.Black >> 3] |= (byte)(1 << i);

								int flip = -1;
								if (square < target) flip = 1;

								if ((square & 0xF0) == (target & 0xF0))
								{
									// On the same row
									Position.VectorDelta[index].delta = flip * 1;
								}
								else if ((square & 0x0F) == (target & 0x0F))
								{
									// On the same column
									Position.VectorDelta[index].delta = flip * 16;
								}
								else if ((square % 15) == (target % 15))
								{
									Position.VectorDelta[index].delta = flip * 15;
								}
								else if ((square % 17) == (target % 17))
								{
									Position.VectorDelta[index].delta = flip * 17;
								}

								if ((i == Pieces.Knight) || (i == Pieces.King)) break;
								target += pieceDeltas[i][dir];
							}
						}
					}
				}

			// Debugging code
			uint checkSum = 0;
			for (uint i = 0; i < 256; i++)
			{
				checkSum += Position.VectorDelta[i].pieceMask[Pieces.White >> 3] * i;
				checkSum += Position.VectorDelta[i].pieceMask[Pieces.Black >> 3] * i;
				checkSum += (uint)(Position.VectorDelta[i].delta * i);
			}

			Debug.Assert(checkSum == 0xb1b58, "Failure constructing vector delta table");
		}

		public bool GetInCheck()
		{
			byte ourKingPiece = (byte)(0x1E | (this.ToMove >> 3));
			return this.IsSquareAttackable(this.PieceList[ourKingPiece], 8 - this.ToMove);
		}

		private struct MoveUndoInfo
		{
			public ulong hashKey;
			public int baseEval;
			public byte epSquare;
			public byte castleRights;
			public byte capturedPiece;
			public bool inCheck;

			public MoveUndoInfo(ulong hashKey, int baseEval, byte epSquare, byte castleRights, byte capturedPiece, bool inCheck)
			{
				this.hashKey = hashKey;
				this.baseEval = baseEval;
				this.epSquare = epSquare;
				this.castleRights = castleRights;
				this.capturedPiece = capturedPiece;
				this.inCheck = inCheck;
			}
		}

		private MoveUndoInfo[] moveUndoStack = new MoveUndoInfo[5000];
		private int moveUndoStackCursor = 0;

		public bool MakeMove(ushort move)
		{
			int otherColor = 8 - this.ToMove;
			int me = this.ToMove >> 3;
			int them = otherColor >> 3;

			int from = (move & 0x7) | ((move << 1) & 0x70);
			int to = ((move >> 6) & 0x7) | ((move >> 5) & 0x70);
			int flipTo = me == 0 ? Evaluation.FlipTable[to] : to;
			int flags = move & 0xF000;

			byte piece = this.Board[from];
			byte captured = this.Board[to];

			Debug.Assert(piece != 0);
			Debug.Assert(this.PieceList[piece >> 3] != 0xFF);
			Debug.Assert(captured == 0 || this.PieceList[captured >> 3] != 0xFF);

			int epcTo = to;
			if (this.epSquare == to && (piece & 0x7) == Pieces.Pawn)
			{
				// ep. capture
				if (this.ToMove == Pieces.White) epcTo += 0x10; else epcTo -= 0x10;
				captured = this.Board[epcTo];
			}

			this.moveUndoStack[this.moveUndoStackCursor++] = new MoveUndoInfo(this.HashKey, this.BaseEval, this.epSquare, this.castleRights, captured, this.InCheck);

			if (flags == MoveHelper.CastleKing)
			{
				if (this.IsSquareAttackable(from + 1, otherColor) ||
					this.IsSquareAttackable(from + 2, otherColor))
				{
					this.moveUndoStackCursor--;
					return false;
				}

				byte rook = this.Board[to + 1];
				this.Board[to - 1] = rook;
				this.Board[to + 1] = Pieces.Empty;

				this.HashKey ^= Position.zobrist[to - 1][rook & 0xF];
				this.HashKey ^= Position.zobrist[to + 1][rook & 0xF];

				this.BaseEval -= Evaluation.PieceSquareTables[rook & 0x7][flipTo + 1];
				this.BaseEval += Evaluation.PieceSquareTables[rook & 0x7][flipTo - 1];

				this.PieceList[rook >> 3] = (byte)(to - 1);
			}
			else if (flags == MoveHelper.CastleQueen)
			{
				if (this.IsSquareAttackable(from - 1, otherColor) ||
					this.IsSquareAttackable(from - 2, otherColor))
				{
					this.moveUndoStackCursor--;
					return false;
				}

				byte rook = this.Board[to - 2];
				this.Board[to + 1] = rook;
				this.Board[to - 2] = Pieces.Empty;

				this.HashKey ^= Position.zobrist[to + 1][rook & 0xF];
				this.HashKey ^= Position.zobrist[to - 2][rook & 0xF];

				this.BaseEval -= Evaluation.PieceSquareTables[rook & 0x7][flipTo - 2];
				this.BaseEval += Evaluation.PieceSquareTables[rook & 0x7][flipTo + 1];

				this.PieceList[rook >> 3] = (byte)(to + 1);
			}

			if (this.epSquare != 0xFF) this.HashKey ^= Position.zobristEpSquare[this.epSquare];
			this.epSquare = 0xFF;

			this.HashKey ^= Position.zobristCastleRights[this.castleRights];
			this.castleRights &= (byte)(Position.castleRightsMask[from] & Position.castleRightsMask[to]);
			this.HashKey ^= Position.zobristCastleRights[this.castleRights];

			this.BaseEval -= Evaluation.PieceSquareTables[piece & 0x7][me == 0 ? Evaluation.FlipTable[from] : from];

			if (flags >= MoveHelper.PromoteKnight)
			{
				byte newPiece = (byte)(piece & (~0x7));
				switch (flags)
				{
					case MoveHelper.PromoteQueen: newPiece |= Pieces.Queen; break;
					case MoveHelper.PromoteKnight: newPiece |= Pieces.Knight; break;
					case MoveHelper.PromoteRook: newPiece |= Pieces.Rook; break;
					case MoveHelper.PromoteBishop: newPiece |= Pieces.Bishop; break;
				}

				this.Board[to] = newPiece;

				this.HashKey ^= Position.zobrist[to][newPiece & 0xF];

				this.BaseEval += Evaluation.PieceSquareTables[newPiece & 0x7][flipTo];

				this.MyMaterial -= Evaluation.MaterialTable[Pieces.Pawn];
				this.MyMaterial += Evaluation.MaterialTable[newPiece & 0x7];

				this.BaseEval -= Evaluation.MaterialTable[Pieces.Pawn];
				this.BaseEval += Evaluation.MaterialTable[newPiece & 0x7];
			}
			else
			{
				this.Board[to] = this.Board[from];

				this.HashKey ^= Position.zobrist[to][piece & 0xF];

				this.BaseEval += Evaluation.PieceSquareTables[piece & 0x7][flipTo];
			}

			this.Board[from] = Pieces.Empty;
			this.HashKey ^= Position.zobrist[from][piece & 0xF];

			this.PieceList[piece >> 3] = (byte)to;

			if (captured != 0)
			{
				Debug.Assert((captured & 0x7) != Pieces.King);

				if (epcTo != to)
				{
					this.Board[epcTo] = Pieces.Empty;
				}

				this.PieceList[captured >> 3] = 0xFF;

				this.HashKey ^= Position.zobrist[epcTo][captured & 0xF];

				this.BaseEval += Evaluation.MaterialTable[captured & 0x7];
				this.BaseEval += Evaluation.PieceSquareTables[captured & 0x7][them == 0 ? Evaluation.FlipTable[epcTo] : epcTo];

				this.OtherMaterial -= Evaluation.MaterialTable[captured & 0x7];
			}

			if (flags == MoveHelper.EnPassent)
			{
				if (this.ToMove == Pieces.White)
					this.epSquare = (byte)(to + 0x10);
				else
					this.epSquare = (byte)(to - 0x10);
				this.HashKey ^= Position.zobristEpSquare[this.epSquare];
			}

			Debug.Assert(this.BaseEval == Evaluation.BaseEvaluation(this));

			this.ToMove = (byte)otherColor;
			this.BaseEval = -this.BaseEval;

			this.HashKey ^= Position.zobristBlack;

			int tmpMaterial = this.MyMaterial;
			this.MyMaterial = this.OtherMaterial;
			this.OtherMaterial = tmpMaterial;

			Debug.Assert(this.HashKey == this.GetHashKey());

			if ((piece & 0x7) == Pieces.King || this.InCheck)
			{
				if (this.IsSquareAttackable(this.PieceList[0x1E | me], otherColor))
				{
					this.UnmakeMove(move);
					return false;
				}
			}
			else
			{
				byte kingPos = this.PieceList[0x1E | me];
				Debug.Assert((this.Board[kingPos] & 0x7) == Pieces.King);

				if (this.ExposesCheck(from, kingPos))
				{
					this.UnmakeMove(move);
					return false;
				}

				if (epcTo != to)
				{
					if (this.ExposesCheck(epcTo, kingPos))
					{
						this.UnmakeMove(move);
						return false;
					}
				}
			}

			Debug.Assert(!this.IsSquareAttackable(this.PieceList[0x1E | me], otherColor));

			// Now we need to check if our move has caused the enemy king to be in check
			this.InCheck = false;

			byte theirKingPos = this.PieceList[0x1E | them];
			// If it's not a special move, or it's en passent we can do the fast check
			if (flags == 0 || flags == MoveHelper.EnPassent)
			{
				// First check if the piece we moved can attack the enemy king
				this.InCheck = this.IsSquareAttackableFrom(theirKingPos, to);

				if (!this.InCheck)
				{
					// Now check if the square we moved from exposes check on the enemy king
					this.InCheck = this.ExposesCheck(from, theirKingPos);

					if (!this.InCheck)
					{
						// Finally, ep. capture can cause another square to be exposed
						if (epcTo != to)
						{
							this.InCheck = this.ExposesCheck(epcTo, theirKingPos);
						}
					}
				}
			}
			else
			{
				// Castle or promotion, do the slow check
				this.InCheck = this.GetInCheck();
			}

			Debug.Assert(this.InCheck == this.GetInCheck());

			return true;
		}

		public void UnmakeMove(ushort move)
		{
			this.ToMove = (byte)(8 - this.ToMove);

			int otherColor = 8 - this.ToMove;
			int me = this.ToMove >> 3;
			int them = otherColor >> 3;

			int from = (move & 0x7) | ((move << 1) & 0x70);
			int to = ((move >> 6) & 0x7) | ((move >> 5) & 0x70);
			int flipTo = me == 0 ? Evaluation.FlipTable[to] : to;
			int flags = move & 0xF000;

			MoveUndoInfo moveUndoInfo = this.moveUndoStack[--this.moveUndoStackCursor];

			this.epSquare = moveUndoInfo.epSquare;
			this.castleRights = moveUndoInfo.castleRights;
			this.HashKey = moveUndoInfo.hashKey;
			this.BaseEval = moveUndoInfo.baseEval;
			this.InCheck = moveUndoInfo.inCheck;

			int tmpMaterial = this.MyMaterial;
			this.MyMaterial = this.OtherMaterial;
			this.OtherMaterial = tmpMaterial;

			byte piece = this.Board[to];
			byte captured = moveUndoInfo.capturedPiece;

			if (flags != 0)
			{
				if (flags == MoveHelper.CastleKing)
				{
					byte rook = this.Board[to - 1];
					this.Board[to + 1] = rook;
					this.Board[to - 1] = Pieces.Empty;

					Debug.Assert((rook & 0x7) == Pieces.Rook);

					this.PieceList[rook >> 3] = (byte)(to + 1);
				}
				else if (flags == MoveHelper.CastleQueen)
				{
					byte rook = this.Board[to + 1];
					this.Board[to - 2] = rook;
					this.Board[to + 1] = Pieces.Empty;

					this.PieceList[rook >> 3] = (byte)(to - 2);
				}
			}

			if (flags >= MoveHelper.PromoteKnight)
			{
				byte newPiece = this.Board[to];
				this.Board[from] = (byte)((newPiece & (~0x7)) | Pieces.Pawn);

				this.MyMaterial -= Evaluation.MaterialTable[newPiece & 0x7];
				this.MyMaterial += Evaluation.MaterialTable[Pieces.Pawn];
			}
			else
			{
				this.Board[from] = this.Board[to];
			}

			byte epcTo = (byte)to;
			if (to == this.epSquare &&
				(piece & 0x7) == Pieces.Pawn)
			{
				if (this.ToMove == Pieces.White)
					epcTo += 0x10;
				else
					epcTo -= 0x10;
				this.Board[to] = Pieces.Empty;
			}

			this.Board[epcTo] = captured;

			this.PieceList[piece >> 3] = (byte)from;

			if (captured != 0)
			{
				this.PieceList[captured >> 3] = epcTo;
				this.OtherMaterial += Evaluation.MaterialTable[captured & 0x7];
			}

			Debug.Assert(this.HashKey == this.GetHashKey());
			Debug.Assert(this.BaseEval == Evaluation.BaseEvaluation(this));
			Debug.Assert(this.InCheck == this.GetInCheck());
		}

		public void MakeNullMove()
		{
			this.ToMove = (byte)(8 - this.ToMove);
			this.BaseEval = -this.BaseEval;
			this.HashKey ^= Position.zobristBlack;

			Debug.Assert(this.HashKey == this.GetHashKey());
		}

		public void UnmakeNullMove()
		{
			this.HashKey ^= Position.zobristBlack;
			this.ToMove = (byte)(8 - this.ToMove);
			this.BaseEval = -this.BaseEval;

			Debug.Assert(this.HashKey == this.GetHashKey());
		}

		public ulong GetHashKey()
		{
			ulong hashKey = 0;

			for (int i = 0; i < 32; i++)
			{
				byte square = this.PieceList[i];
				if (square != 0xFF)
				{
					hashKey ^= Position.zobrist[square][this.Board[square] & 0xF];
				}
			}

			if (this.ToMove == 0) hashKey ^= Position.zobristBlack;
			if (this.epSquare != 0xFF) hashKey ^= Position.zobristEpSquare[this.epSquare];
			hashKey ^= Position.zobristCastleRights[this.castleRights];

			return hashKey;
		}

		private bool ExposesCheck(int from, int kingPos)
		{
			int index = kingPos - from + 128;
			// If a queen can't reach it, nobody can!
			if ((Position.VectorDelta[index].pieceMask[0] & (1 << (Pieces.Queen))) != 0)
			{
				byte king = this.Board[kingPos];
				int delta = Position.VectorDelta[index].delta;
				for (int pos = kingPos + delta;
					 ((pos & 0x88) == 0);
					 pos += delta)
				{
					if (pos == from)
					{
						continue;
					}

					byte piece = this.Board[pos];
					if (piece != 0)
					{
						if (((piece ^ king) & 0x8) != 0)
						{
							// Now see if the piece can actually attack the king
							int backwardIndex = pos - kingPos + 128;
							return (Position.VectorDelta[backwardIndex].pieceMask[(piece >> 3) & 1] & (1 << (piece & 0x7))) != 0;
						}
						else
						{
							return false;
						}
					}
				}
			}
			return false;
		}

		public bool IsSquareAttackableFrom(int target, int from)
		{
			int index = from - target + 128;
			byte piece = this.Board[from];
			if ((Position.VectorDelta[index].pieceMask[(piece >> 3) & 1] & (1 << (piece & 0x7))) != 0)
			{
				// Yes, this square is attackable
				if ((piece & 0x7) == Pieces.Knight)
					return true;

				from += Position.VectorDelta[index].delta;
				while (from != target)
				{
					if (this.Board[from] != Pieces.Empty) break;
					from += Position.VectorDelta[index].delta;
				}

				if (from == target)
				{
					return true;
				}
			}

			return false;
		}

		public bool IsSquareAttackable(int target, int color)
		{
			for (int i = 30 | (color >> 3); i >= 0; i -= 2)
			{
				byte square = this.PieceList[i];
				if (square != 0xFF &&
					this.IsSquareAttackableFrom(target, square))
				{
					return true;
				}
			}
			return false;
		}

		public int GetSmallestAttackerValue(int target, int color)
		{
			int value = int.MaxValue;
			for (int i = color >> 3; i < 32; i += 2)
			{
				byte square = this.PieceList[i];
				if (square != 0xFF)
				{
					byte piece = this.Board[square];
					int pieceValue = Evaluation.MaterialTable[piece & 0x7];
					if (pieceValue < value &&
						this.IsSquareAttackable(target, square))
					{
						value = pieceValue;
					}
				}
			}
			return value;
		}

		public List<ushort> GenerateValidMoves()
		{
			ushort[] moves = new ushort[255];
			int moveCount;
			this.GenerateAllMoves(moves, out moveCount);

			List<ushort> results = new List<ushort>();
			for (int i = 0; i < moveCount; i++)
			{
				if (this.MakeMove(moves[i]))
				{
					results.Add(moves[i]);
					this.UnmakeMove(moves[i]);
				}
			}

			return results;
		}

		public void GenerateAllMoves(ushort[] moves, out int moveCount)
		{
			moveCount = 0;
			for (int i = this.ToMove >> 3; i < 32; i += 2)
			{
				byte square = this.PieceList[i];
				if (square != 0xFF)
				{
					byte piece = this.Board[square];
					switch (piece & 0x7)
					{
						case Pieces.Pawn: GeneratePawnMoves(moves, ref moveCount, square); break;
						case Pieces.Knight: GenerateDirectionalMoves(moves, ref moveCount, square, true, Pieces.KnightDeltas); break;
						case Pieces.Bishop: GenerateDirectionalMoves(moves, ref moveCount, square, false, Pieces.BishopDeltas); break;
						case Pieces.Rook: GenerateDirectionalMoves(moves, ref moveCount, square, false, Pieces.RookDeltas); break;
						case Pieces.Queen: GenerateDirectionalMoves(moves, ref moveCount, square, false, Pieces.QueenDeltas); break;
						case Pieces.King:
							if (!this.InCheck)
							{
								int castleRights = this.castleRights;
								if (this.ToMove == 0) castleRights >>= 2;
								if ((castleRights & 1) != 0)
								{
									// Kingside castle
									if (this.Board[square + 1] == Pieces.Empty && this.Board[square + 2] == Pieces.Empty)
									{
										moves[moveCount++] = MoveHelper.GenMove(square, (byte)(square + 0x02), MoveHelper.CastleKing);
									}
								}
								if ((castleRights & 2) != 0)
								{
									// Queenside castle
									if (this.Board[square - 1] == Pieces.Empty && this.Board[square - 2] == Pieces.Empty && this.Board[square - 3] == Pieces.Empty)
									{
										moves[moveCount++] = MoveHelper.GenMove(square, (byte)(square - 0x02), MoveHelper.CastleQueen);
									}
								}
							}
							this.GenerateDirectionalMoves(moves, ref moveCount, square, true, Pieces.QueenDeltas);
							break;
					}
				}
			}
		}

		private void GenerateDirectionalMoves(ushort[] moves, ref int moveCount, byte start, bool onlyCheckFirst, int[] deltas)
		{
			byte startPiece = this.Board[start];
			for (int i = 0; i < deltas.Length; i++)
			{
				byte pos = (byte)(start + deltas[i]);
				while ((pos & 0x88) == 0)
				{
					byte piece = this.Board[pos];
					if (piece == Pieces.Empty)
					{
						moves[moveCount++] = MoveHelper.GenMove(start, pos, 0);
					}
					else
					{
						// Opposite colors
						if (((piece ^ startPiece) & 0x8) != 0)
						{
							moves[moveCount++] = MoveHelper.GenMove(start, pos, 0);
						}
						break;
					}

					if (onlyCheckFirst) break;
					pos += (byte)deltas[i];
				}
			}
		}

		private void GeneratePawnMoves(ushort[] moves, ref int moveCount, byte start)
		{
			byte piece = this.Board[start];
			int color = piece & Pieces.White;
			int inc;
			if (color == Pieces.White)
				inc = -16;
			else
				inc = 16;

			byte end = (byte)(start + inc);

			// If a pawn is on the home rank, then it can jump two squares
			if ((((start & 0x70) == 0x10) && (color == Pieces.Black)) ||
				(((start & 0x70) == 0x60) && (color == Pieces.White)))
			{
				// 2 square jump
				if (this.Board[end] == Pieces.Empty &&
					this.Board[end + inc] == Pieces.Empty)
				{
					moves[moveCount++] = MoveHelper.GenMove(start, (byte)(end + inc), MoveHelper.EnPassent);
				}
			}

			// Can the pawn capture?
			for (int cInc = -1; cInc <= 1; cInc += 2)
			{
				byte square = (byte)(end + cInc);
				if ((square & 0x88) == 0)
				{
					byte targetPiece = this.Board[square];
					if (targetPiece != Pieces.Empty)
					{
						// Colors different?
						if (((targetPiece ^ color) & 0x8) != 0)
						{
							if (((square & 0x70) == 0x70) || ((square & 0x70) == 0x00))
							{
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteQueen);
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteKnight);
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteBishop);
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteRook);
							}
							else
							{
								moves[moveCount++] = MoveHelper.GenMove(start, square, 0);
							}
						}
					}
					else if (square == this.epSquare)
					{
						moves[moveCount++] = MoveHelper.GenMove(start, square, 0);
					}
				}
			}

			// Normal forward move
			if (this.Board[end] == Pieces.Empty)
			{
				if (((end & 0x70) == 0x70) || ((end & 0x70) == 0x00))
				{
					moves[moveCount++] = MoveHelper.GenMove(start, end, MoveHelper.PromoteQueen);
					moves[moveCount++] = MoveHelper.GenMove(start, end, MoveHelper.PromoteKnight);
					moves[moveCount++] = MoveHelper.GenMove(start, end, MoveHelper.PromoteBishop);
					moves[moveCount++] = MoveHelper.GenMove(start, end, MoveHelper.PromoteRook);
				}
				else
				{
					moves[moveCount++] = MoveHelper.GenMove(start, end, 0);
				}
			}
		}

		private delegate void CaptureGenerator(ushort[] moves, int[] scores, ref int moveCount, byte square, int[] deltas);
		private CaptureGenerator[] CaptureGenerators;
		private int[][] PieceDeltas = new int[][]
			{
				null,
				null,
				Pieces.KnightDeltas,
				Pieces.BishopDeltas,
				Pieces.RookDeltas,
				Pieces.QueenDeltas,
				Pieces.QueenDeltas,
				null
			};

		public void GenerateAllCaptures(ushort[] moves, int[] scores, out int moveCount)
		{
			moveCount = 0;
			for (int i = this.ToMove >> 3; i < 32; i += 2)
			{
				byte square = this.PieceList[i];
				if (square != 0xFF)
				{
					int piece = this.Board[square] & 0x7;
					CaptureGenerators[piece](moves, scores, ref moveCount, square, PieceDeltas[piece]);
				}
			}
		}

		private static int CaptureValue(int piece, int captured)
		{
			int victim = Evaluation.MaterialTable[captured & 0x7];
			int attacker = Evaluation.MaterialTable[piece & 0x7];
			int score = victim - attacker / 10;
			if (victim > attacker) score += 15000;
			return score;
		}

		private void GenerateKnightKingCaptures(ushort[] moves, int[] scores, ref int moveCount, byte start, int[] deltas)
		{
			byte startPiece = this.Board[start];
			for (int i = 0; i < deltas.Length; i++)
			{
				byte pos = (byte)(start + deltas[i]);
				if ((pos & 0x88) == 0)
				{
					byte piece = this.Board[pos];
					if (piece != Pieces.Empty)
					{
						// Opposite colors
						if (((piece ^ startPiece) & 0x8) != 0)
						{
							scores[moveCount] = CaptureValue(startPiece, piece);
							moves[moveCount++] = MoveHelper.GenMove(start, pos, 0);
						}
					}
				}
			}
		}

		private void GenerateSliderCaptures(ushort[] moves, int[] scores, ref int moveCount, byte start, int[] deltas)
		{
			byte startPiece = this.Board[start];
			for (int i = 0; i < deltas.Length; i++)
			{
				byte pos = (byte)(start + deltas[i]);
				while ((pos & 0x88) == 0)
				{
					byte piece = this.Board[pos];
					if (piece != Pieces.Empty)
					{
						// Opposite colors
						if (((piece ^ startPiece) & 0x8) != 0)
						{
							scores[moveCount] = CaptureValue(startPiece, piece);
							moves[moveCount++] = MoveHelper.GenMove(start, pos, 0);
						}
						break;
					}

					pos += (byte)deltas[i];
				}
			}
		}

		private void GeneratePawnCaptures(ushort[] moves, int[] scores, ref int moveCount, byte start, int[] deltas)
		{
			// Can the pawn capture?
			byte piece = this.Board[start];
			int color = piece & Pieces.White;
			int inc;
			if (color == Pieces.White)
				inc = -16;
			else
				inc = 16;

			byte end = (byte)(start + inc);

			for (int cInc = -1; cInc <= 1; cInc += 2)
			{
				byte square = (byte)(end + cInc);
				if ((square & 0x88) == 0)
				{
					byte targetPiece = this.Board[square];
					if (targetPiece != Pieces.Empty)
					{
						// Colors different?
						if (((targetPiece ^ color) & 0x8) != 0)
						{
							if (((square & 0x70) == 0x70) || ((square & 0x70) == 0x00))
							{
								scores[moveCount] = CaptureValue(piece, targetPiece) + 500;
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteQueen);
								scores[moveCount] = CaptureValue(piece, targetPiece) + 500;
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteKnight);
								scores[moveCount] = CaptureValue(piece, targetPiece) + 500;
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteBishop);
								scores[moveCount] = CaptureValue(piece, targetPiece) + 500;
								moves[moveCount++] = MoveHelper.GenMove(start, square, MoveHelper.PromoteRook);
							}
							else
							{
								scores[moveCount] = CaptureValue(piece, targetPiece);
								moves[moveCount++] = MoveHelper.GenMove(start, square, 0);
							}
						}
					}
					else if (square == this.epSquare)
					{
						scores[moveCount] = CaptureValue(piece, piece);
						moves[moveCount++] = MoveHelper.GenMove(start, square, 0);
					}
				}
			}
		}

		#region ICloneable Members
		public object Clone()
		{
			Position position = new Position();

			this.Board.CopyTo(position.Board, 0);
			this.PieceList.CopyTo(position.PieceList, 0);

			position.ToMove = this.ToMove;
			position.epSquare = this.epSquare;
			position.BaseEval = this.BaseEval;
			position.castleRights = this.castleRights;
			position.HashKey = this.HashKey;
			position.InCheck = this.InCheck;
			position.SeenPositions = (byte[])this.SeenPositions.Clone();
            position.SeenPositionLock = (ulong[])this.SeenPositionLock.Clone();
			position.MyMaterial = this.MyMaterial;
			position.OtherMaterial = this.OtherMaterial;

			return position;
		}
		#endregion

		#region IComparable<Position> Members
		public int CompareTo(Position other)
		{
			for (int i = 0; i < 128; i++)
			{
				if (this.Board[i] != other.Board[i])
				{
					return 1;
				}
			}

			for (int i = 0; i < 32; i++)
			{
				if (this.PieceList[i] != other.PieceList[i])
				{
					return 2;
				}
			}

			if (this.ToMove != other.ToMove) return 3;
			if (this.epSquare != other.epSquare) return 4;
			if (this.BaseEval != other.BaseEval) return 5;
			if (this.castleRights != other.castleRights) return 6;
			if (this.HashKey != other.HashKey) return 7;
			if (this.InCheck != other.InCheck) return 8;

			for (int i = 0; i < this.SeenPositions.Length; i++)
			{
				if (other.SeenPositions[i] != this.SeenPositions[i]) return 9;
			}

			if (this.MyMaterial != other.MyMaterial) return 10;
			if (this.OtherMaterial != other.OtherMaterial) return 11;

			return 0;
		}
		#endregion
	}

	class Evaluation
	{
		public const int MinEval = -20000000;
		public const int MaxEval = 20000000;

		public static readonly int[] MaterialTable = new int[] { 0, 1000, 3250, 3250, 5000, 9750, 600000 };

		public static readonly int[] PawnPieceSquareTable =
    {   0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
      100, 300, 400, 600, 600, 400, 300, 100, /**/  0,0,0,0,0,0,0,0,
       40, 150, 200, 300, 300, 200, 150,  40, /**/  0,0,0,0,0,0,0,0,
       15,  75, 100, 150, 150, 100,  75,  15, /**/  0,0,0,0,0,0,0,0,
       10,  40,  60, 100, 100,  60,  40,  10, /**/  0,0,0,0,0,0,0,0,
        5,  10,  15, -10, -10,  15,  10,   5, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0, -80, -80,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0
	};

		public static readonly int[] KnightPieceSquareTable =
    { -50, -50, -50, -50, -50, -50, -50, -50, /**/  0,0,0,0,0,0,0,0,
      -50,   0,   0,   0,   0,   0,   0, -50, /**/  0,0,0,0,0,0,0,0,
      -50,   0, 120, 120, 120, 120,   0, -50, /**/  0,0,0,0,0,0,0,0,
      -50,   0,  60, 120, 120,  60,   0, -50, /**/  0,0,0,0,0,0,0,0,
      -50,   0,  60, 120, 120,  60,   0, -50, /**/  0,0,0,0,0,0,0,0,
      -50,   0,  60,  60,  60,  60,   0, -50, /**/  0,0,0,0,0,0,0,0,
      -50,   0,   0,   0,   0,   0,   0, -50, /**/  0,0,0,0,0,0,0,0,
      -50, -60, -50, -50, -50, -50, -60, -50, /**/  0,0,0,0,0,0,0,0
	};

		public static readonly int[] BishopPieceSquareTable =
    {   0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,  40,  40,  40,  40,   0,   0, /**/  0,0,0,0,0,0,0,0,
        5,  10,  40,  80,  80,  40,  10,   5, /**/  0,0,0,0,0,0,0,0,
       10,  20,  40,  80,  80,  40,  20,  10, /**/  0,0,0,0,0,0,0,0,
       30,  40,  60,  60,  60,  60,  40,  30, /**/  0,0,0,0,0,0,0,0,
        0,  40,   0,   0,   0,   0,  40,   0, /**/  0,0,0,0,0,0,0,0,
       20,   0, -20,   0,   0, -20,   0,  20, /**/  0,0,0,0,0,0,0,0
	};

		public static readonly int[] RookPieceSquareTable =
    {   0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
      100, 100, 100, 100, 100, 100, 100, 100, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
      -10,   0,   0,  50,   0,  50,   0, -10, /**/  0,0,0,0,0,0,0,0
	};

		public static readonly int[] KingPieceSquareTable =
    {   0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
     -800,-800,-800,-800,-800,-800,-800,-800, /**/  0,0,0,0,0,0,0,0,
     -1500,-1500,-1500,-1500,-1500,-1500,-1500,-1500, /**/  0,0,0,0,0,0,0,0,
     -1200,-1200,-1200,-1200,-1200,-1200,-1200,-1200, /**/  0,0,0,0,0,0,0,0,
     -900,-900,-900,-900,-900,-900,-900,-900, /**/  0,0,0,0,0,0,0,0,
     -600,-600,-600,-600,-600,-600,-600,-600, /**/  0,0,0,0,0,0,0,0,
     -300,-300,-300,-300,-300,-300,-300,-300, /**/  0,0,0,0,0,0,0,0,
        0,   0, 300, -60, -20, -60, 300,   0, /**/  0,0,0,0,0,0,0,0
	};

		public static readonly int[] EmptyPieceSquareTable =
    {   0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0,
        0,   0,   0,   0,   0,   0,   0,   0, /**/  0,0,0,0,0,0,0,0
	};

		public static readonly int[][] PieceSquareTables = { null, PawnPieceSquareTable, KnightPieceSquareTable, BishopPieceSquareTable, RookPieceSquareTable, EmptyPieceSquareTable, KingPieceSquareTable, null };

		public static readonly int[] FlipTable =
    { 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, /**/  0,0,0,0,0,0,0,0,
      0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, /**/  0,0,0,0,0,0,0,0,
      0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, /**/  0,0,0,0,0,0,0,0,
      0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, /**/  0,0,0,0,0,0,0,0,
      0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, /**/  0,0,0,0,0,0,0,0,
      0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, /**/  0,0,0,0,0,0,0,0,
      0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, /**/  0,0,0,0,0,0,0,0,
      0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, /**/  0,0,0,0,0,0,0,0
    };

		public static int Evaluate(Position position)
		{
			int eval = position.BaseEval;

			int kingPosTerm = 0;
			// Black queen gone, then cancel white's penalty for king movement
			if (position.PieceList[0x1C] == 0xFF) kingPosTerm -= KingPieceSquareTable[position.PieceList[0x1F]];
			// White queen gone, then cancel black's penalty for king movement
			if (position.PieceList[0x1D] == 0xFF) kingPosTerm += KingPieceSquareTable[FlipTable[position.PieceList[0x1E]]];

			int bishopPairTerm = 0;
			// Black bishop pair
			if (position.PieceList[0x14] != 0xFF && position.PieceList[0x16] != 0xFF) bishopPairTerm -= 500;
			// White bishop pair
			if (position.PieceList[0x15] != 0xFF && position.PieceList[0x17] != 0xFF) bishopPairTerm += 500;

			if (position.ToMove == 0)
			{
				eval -= kingPosTerm;
				eval -= bishopPairTerm;
			}
			else
			{
				eval += kingPosTerm;
				eval += bishopPairTerm;
			}

			return eval;
		}

		public static int BaseEvaluation(Position position)
		{
			int result = 0;

			for (int i = 0; i < 128; i++)
			{
				byte piece = position.Board[i];
				if (piece != Pieces.Empty)
				{
					int color = (piece >> 3) & 1;
					int flip;
					if (color == 0) flip = -1; else flip = 1;

					result += flip * PieceSquareTables[piece & 0x7][color == 0 ? Evaluation.FlipTable[i] : i];

					result += flip * MaterialTable[piece & 0x7];
				}
			}

			if (position.ToMove == 0) result = -result;
			return result;
		}
	}
}
