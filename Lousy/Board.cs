//#define HashDebug
//#define PawnHashDebug
//#define MoveToIsEmptyDebug

using System;
using System.Collections.Generic;
using System.Text;


namespace LousyChess
{

   public struct SquareContentInfo
   {
      public int pieceType;   // king, queen, rook, bishop, knight, pawn : 0..5
      public int pieceColor;  // White, Black : 0..1
      public int pieceIndex;  // index in PiecePos
   }


   public struct BoardState
   {
      // the state of the board. Used in MakeMove and UnMakeMove.
      public bool canCastleKingSide_White;
      public bool canCastleQueenSide_White;
      public bool canCastleKingSide_Black;
      public bool canCastleQueenSide_Black;
      public bool hasCastled_White;
      public bool hasCastled_Black;
      public int enPassantPosition;
      public int fiftyMoveNr;
      public int capturedPieceType;
      public int capturedPiecePosition;
      public int repeatedPosition_SearchOffset;
      public ulong hashValue;
      public ulong bwPawnsHashValue;
   }


   public class Board
   {

      // pointers to other classes
      public MagicMoves magicMoves;
      public TranspositionTable transpositionTable;
      public MoveGenerator moveGenerator;
      public Evaluator evaluator;
      public BitBoard bitboard;


      #region board state data

      // for all 2-position arrays :  index 0=white,  index 1=black

      // bitboards
      public ulong[] pieces = new ulong[Const.NrColors];   
      public ulong allPiecesBB;

      // the bitboards of each PieceType of each color
      public ulong[,] pieceBB = new ulong[Const.NrColors, Const.NrPieceTypes];

      // these hold the positions (0..63) of the pieces
      public int[, ,] PiecePos = new int[Const.NrColors, Const.NrPieceTypes, Const.MaxNrPiecesPerType];    // always filled from low to high

      // these hold the number of each pieceType
      public int[,] NrPieces = new int[Const.NrColors, Const.NrPieceTypes];

      // these hold the total nr of pieces (pawns + the rest) of each color
      public int[] TotalNrPieces = new int[Const.NrColors];

      // describes the contents of each square
      public SquareContentInfo[] SquareContents = new SquareContentInfo[Const.NrSquares];

      // state of the game info
      public int colorToMove;   // 0=white, 1=black
      public int enemyColor;
      public bool[] canCastleKingSide = new bool[Const.NrColors];
      public bool[] canCastleQueenSide = new bool[Const.NrColors];
      public bool[] hasCastled = new bool[Const.NrColors];
      public int enPassantPosition;    // If the last move was not a 2-step pawn move : -1
                                       // If it was, this value holds the position where it can be captured.
      public int halfMoveNr;           // starts at 0, and is incremented AFTER each move 
      int fiftyMoveNr;                 // incremented after each move. Reset on pawn-move or capture.
      int capturedPiecePosition;
      int capturedPieceType;

      int repeatedPosition_SearchOffset;       // the move-nr from which a 3x repeated position should be searched.

      // These stores the state of the board for deeper ply-nrs
      int maxNrStoredBoardStates = 100;      // will be increased if necessary
      int nrStoredBoardStates = 0;
      BoardState[] storedBoardStates;

      public ulong HashValue = 0;
      public ulong bwPawnsHashValue = 0;

      // The static material score (queen = 900, etc).
      // For both black and white holds : The larger the better.
      public int[] StaticMaterialScore = new int[Const.NrColors];

      // The static positional score (calculated from Evaluation.PieceSquareValues).
      // For both black and white holds : The larger the better.
      public int[] StaticPositionalScore = new int[Const.NrColors];

      #endregion

      // Just a 1 at the position bit, a 0 on all other bits
      private ulong[] Identity = new ulong[64];

      // The hashvalue of the board after _each_ move is made, also those in the AB_Search
      int nrHashValuesInHistory;
      ulong[] HashValueHistory;   // HashValueHistory[0] = Initial state


      public Board(bool isInitialBoard)
      {
         if (isInitialBoard)
         {
            CreateIdentity();
            //
            storedBoardStates = new BoardState[maxNrStoredBoardStates];
            HashValueHistory = new ulong[100];
         }
      }


      #region initialization

      private void CreateIdentity()
      {
         ulong q = 1;
         for (int i = 0; i < 64; i++)
         {
            Identity[i] = q;
            q <<= 1;
         }
      }


      #endregion


      #region Clone Board & Compare

      public Board Clone()
      {
         // use the empty creator : otherwise to many things are set.
         Board clone = new Board(false);
         clone.magicMoves = magicMoves;
         clone.transpositionTable = transpositionTable;
         clone.evaluator = evaluator;
         clone.ClearBoard(); 
         clone.moveGenerator = moveGenerator;
         //
         clone.pieces = new ulong[pieces.Length];
         for (int i = 0; i < pieces.Length; i++)
            clone.pieces[i] = pieces[i];
         clone.allPiecesBB = allPiecesBB;
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               clone.pieceBB[i, j] = pieceBB[i, j];
         // PiecePos
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               for (int k = 0; k < Const.MaxNrPiecesPerType; k++)
                  clone.PiecePos[i, j, k] = PiecePos[i, j, k];
         // nrPieces
         for (int i = 0; i < Const.NrColors; i++)
         {
            clone.TotalNrPieces[i] = TotalNrPieces[i];
            for (int j = 0; j < Const.NrPieceTypes; j++)
               clone.NrPieces[i, j] = NrPieces[i, j];
         }
         // SquareContents
         for (int i = 0; i < Const.NrSquares; i++)
            clone.SquareContents[i] = SquareContents[i];
         //
         // state of the game info
         clone.colorToMove = colorToMove;
         clone.enemyColor = enemyColor;
         for (int i = 0; i < Const.NrColors; i++)
         {
            clone.canCastleKingSide[i] = canCastleKingSide[i];
            clone.canCastleQueenSide[i] = canCastleQueenSide[i];
            clone.hasCastled[i] = hasCastled[i];
         }
         // ..
         clone.enPassantPosition = enPassantPosition;
         //
         clone.halfMoveNr = halfMoveNr; 
         clone.fiftyMoveNr = fiftyMoveNr;
         clone.capturedPiecePosition = capturedPiecePosition;
         clone.capturedPieceType = capturedPieceType;
         clone.repeatedPosition_SearchOffset = repeatedPosition_SearchOffset;
         //
         clone.maxNrStoredBoardStates = maxNrStoredBoardStates;
         clone.nrStoredBoardStates = nrStoredBoardStates;
         clone.storedBoardStates = new BoardState[storedBoardStates.Length];
         for (int i = 0; i < storedBoardStates.Length; i++)
            clone.storedBoardStates[i] = storedBoardStates[i];
         //
         clone.HashValue = HashValue;
         clone.bwPawnsHashValue = bwPawnsHashValue;
         for (int i = 0; i < Const.NrColors; i++)
         {
            clone.StaticMaterialScore[i] = StaticMaterialScore[i];
            clone.StaticPositionalScore[i] = StaticPositionalScore[i];
         }
         //
         clone.nrHashValuesInHistory = nrHashValuesInHistory;
         clone.HashValueHistory = new ulong[HashValueHistory.Length];
         for (int i = 0; i < HashValueHistory.Length; i++)
            clone.HashValueHistory[i] = HashValueHistory[i];
         //
         return clone;
      }


      public void LoadFrom(Board clone)
      {
         // load the entire contents from a clone
         magicMoves = clone.magicMoves;
         transpositionTable = clone.transpositionTable;
         evaluator = clone.evaluator;
         ClearBoard();
         moveGenerator = clone.moveGenerator;
         //
         pieces = new ulong[clone.pieces.Length];
         for (int i = 0; i < clone.pieces.Length; i++)
            pieces[i] = clone.pieces[i];
         allPiecesBB = clone.allPiecesBB;
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               pieceBB[i, j] = clone.pieceBB[i, j];
         // PiecePos
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               for (int k = 0; k < Const.MaxNrPiecesPerType; k++)
                  PiecePos[i, j, k] = clone.PiecePos[i, j, k];
         //..
         // nrPieces
         for (int i = 0; i < Const.NrColors; i++)
         {
            TotalNrPieces[i] = clone.TotalNrPieces[i];
            for (int j = 0; j < Const.NrPieceTypes; j++)
               NrPieces[i, j] = clone.NrPieces[i, j];
         }
         // SquareContents
         for (int i = 0; i < Const.NrSquares; i++)
            SquareContents[i] = clone.SquareContents[i];
         //
         // state of the game info
         colorToMove = clone.colorToMove;
         enemyColor = clone.enemyColor;
         for (int i = 0; i < Const.NrColors; i++)
         {
            canCastleKingSide[i] = clone.canCastleKingSide[i];
            canCastleQueenSide[i] = clone.canCastleQueenSide[i];
            hasCastled[i] = clone.hasCastled[i];
         }
         // ..
         enPassantPosition = clone.enPassantPosition;
         //
         halfMoveNr = clone.halfMoveNr;
         fiftyMoveNr = clone.fiftyMoveNr;
         capturedPiecePosition = clone.capturedPiecePosition;
         capturedPieceType = clone.capturedPieceType;
         repeatedPosition_SearchOffset = clone.repeatedPosition_SearchOffset;
         //
         maxNrStoredBoardStates = clone.maxNrStoredBoardStates;
         nrStoredBoardStates = clone.nrStoredBoardStates;
         storedBoardStates = new BoardState[clone.storedBoardStates.Length];
         for (int i = 0; i < clone.storedBoardStates.Length; i++)
            storedBoardStates[i] = clone.storedBoardStates[i];
         //
         HashValue = clone.HashValue;
         bwPawnsHashValue = clone.bwPawnsHashValue;
         for (int i = 0; i < Const.NrColors; i++)
         {
            StaticMaterialScore[i] = clone.StaticMaterialScore[i];
            StaticPositionalScore[i] = clone.StaticPositionalScore[i];
         }
         //
         nrHashValuesInHistory = clone.nrHashValuesInHistory;
         HashValueHistory = new ulong[clone.HashValueHistory.Length];
         for (int i = 0; i < clone.HashValueHistory.Length; i++)
            HashValueHistory[i] = clone.HashValueHistory[i];
      }


      public bool Compare(Board clone)
      {
         for (int i = 0; i < pieces.Length; i++)
            if (clone.pieces[i] != pieces[i])
               return false;
         if (clone.allPiecesBB != allPiecesBB)
               return false;
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               if (clone.pieceBB[i, j] != pieceBB[i, j])
                  return false;
         // PiecePos
         int nrPiecePosErrors = 0;
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               for (int k = 0; k < Const.MaxNrPiecesPerType; k++)
                  if (clone.PiecePos[i, j, k] != PiecePos[i, j, k])
                     nrPiecePosErrors++;
         if (nrPiecePosErrors > 0)
              return false;
         // nrPieces
         for (int i = 0; i < Const.NrColors; i++)
         {
            if (clone.TotalNrPieces[i] != TotalNrPieces[i])
               return false;
            for (int j = 0; j < Const.NrPieceTypes; j++)
               if (clone.NrPieces[i, j] != NrPieces[i, j])
                  return false;
         }
         // SquareContents
         for (int i = 0; i < Const.NrSquares; i++)
         {
            if (clone.SquareContents[i].pieceColor != SquareContents[i].pieceColor)
               return false;
            if (clone.SquareContents[i].pieceIndex != SquareContents[i].pieceIndex)
               return false;
            if (clone.SquareContents[i].pieceType != SquareContents[i].pieceType)
               return false;
         }
         //
         // state of the game info
         if (clone.colorToMove != colorToMove)
               return false;
         if (clone.enemyColor != enemyColor)
               return false;
         for (int i = 0; i < Const.NrColors; i++)
         {
            if (clone.canCastleKingSide[i] != canCastleKingSide[i])
               return false;
            if (clone.canCastleQueenSide[i] != canCastleQueenSide[i])
               return false;
            if (clone.hasCastled[i] != hasCastled[i])
               return false;
         }
         if (clone.enPassantPosition != enPassantPosition)
               return false;
         //
         if (clone.halfMoveNr != halfMoveNr)
               return false;
         if (clone.fiftyMoveNr != fiftyMoveNr)
               return false;
         if (clone.capturedPiecePosition != capturedPiecePosition)
               return false;
         if (clone.capturedPieceType != capturedPieceType)
               return false;
         if (clone.repeatedPosition_SearchOffset != repeatedPosition_SearchOffset)
               return false;
         //
         if (clone.maxNrStoredBoardStates != maxNrStoredBoardStates)
               return false;
         if (clone.nrStoredBoardStates != nrStoredBoardStates)
            return false;
         for (int i = 0; i < storedBoardStates.Length; i++)
         {
            if (!clone.storedBoardStates[i].Equals(storedBoardStates[i]) )
               return false;
         }
         //
         if (clone.HashValue != HashValue)
            return false;
         if (clone.bwPawnsHashValue != bwPawnsHashValue)
            return false;
         for (int i = 0; i < Const.NrColors; i++)
         {
            if (clone.StaticMaterialScore[i] != StaticMaterialScore[i])
               return false;
            if (clone.StaticPositionalScore[i] != StaticPositionalScore[i])
               return false;
         }

         //
         if (clone.nrHashValuesInHistory != nrHashValuesInHistory)
               return false;
         for (int i = 0; i < HashValueHistory.Length; i++)
            if (clone.HashValueHistory[i] != HashValueHistory[i])
               return false;
         //
         return true;
      }

      #endregion


      public void ToggleMoveColor()
      {
         if (colorToMove == Const.White)
         {
            colorToMove = Const.Black;
            enemyColor = Const.White;
         }
         else
         {
            colorToMove = Const.White;
            enemyColor = Const.Black;
         }
         HashValue ^= transpositionTable.ChangeMoveColor;
      }


      public ulong CalcHashValue()
      {
         ulong result = 0;
         // the pieces
         for (int i = 0; i < 64; i++)
         {
            int pieceType = SquareContents[i].pieceType;
            int color = SquareContents[i].pieceColor;
            if (pieceType != Const.EmptyID)
               result ^= transpositionTable.SquareHashValue[i, pieceType, color];
         }
         if (colorToMove == Const.Black)
            result ^= transpositionTable.ChangeMoveColor;
         // en-passant
         if (enPassantPosition != Const.InvalidID)
            result ^= transpositionTable.EPSquareValue[enPassantPosition];
         for (int i = 0; i < Const.NrColors; i++)
         {
            if (canCastleQueenSide[i])
               result ^= transpositionTable.CanCastleQSHashValue[i];
            if (canCastleKingSide[i])
               result ^= transpositionTable.CanCastleKSHashValue[i];
         }
         return result;
      }


      public ulong CalcBWPawnsHashValue()
      {
         ulong result = 0;
         for (int i = 0; i < Const.NrColors; i++)
         {
            for (int j=0; j<NrPieces[i,Const.PawnID]; j++)
               result ^= transpositionTable.SquareHashValue[PiecePos[i,Const.PawnID,j], Const.PawnID, i];
         }
         return result;
      }


      #region Setup board : empty & from EPD

      public void ClearBoard()
      {
         colorToMove = Const.White;
         enemyColor = Const.Black;
         // the bitboards
         pieces[Const.White] = 0;
         pieces[Const.Black] = 0;
         allPiecesBB = 0;
         for (int i = 0; i < Const.NrColors; i++)
            for (int j = 0; j < Const.NrPieceTypes; j++)
               pieceBB[i, j] = 0;
         // en passant
         enPassantPosition = Const.InvalidID;
         // the pieces
         for (int colorNr = 0; colorNr < Const.NrColors; colorNr++)
         {
            TotalNrPieces[colorNr] = 0;
            for (int pieceType = 0; pieceType < Const.NrPieceTypes; pieceType++)
            {
               NrPieces[colorNr, pieceType] = 0;
               for (int index = 0; index < Const.MaxNrPiecesPerType; index++)
                  PiecePos[colorNr, pieceType, index] = Const.InvalidID;
            }
            // Castle info
            canCastleKingSide[colorNr] = false;
            canCastleQueenSide[colorNr] = false;
            hasCastled[colorNr] = false;
            // the static scores
            StaticMaterialScore[colorNr] = 0;
            StaticPositionalScore[colorNr] = 0;
            //
         }
         // the squares
         for (int i = 0; i < Const.NrSquares; i++)
         {
            SquareContents[i].pieceType = Const.EmptyID;
            SquareContents[i].pieceColor = Const.InvalidID;
            SquareContents[i].pieceIndex = Const.InvalidID;
         }
         canCastleQueenSide[Const.White] = false;
         canCastleKingSide[Const.White] = false;
         canCastleQueenSide[Const.Black] = false;
         canCastleKingSide[Const.Black] = false;
         // hash
         HashValue = 0;
         bwPawnsHashValue = 0;
         // history
         nrHashValuesInHistory = 0;
         // miscillaneous
         nrStoredBoardStates = 0;
         repeatedPosition_SearchOffset = 0;
      }


      private void FEN_To_Board(string[] fenStrings)
      {
         // NB : starts from rank 8 !!, so use flip, to go from normal square-nr to flipped square nr
         int[] flip = {
                 	       56,  57,  58,  59,  60,  61,  62,  63,
	                      48,  49,  50,  51,  52,  53,  54,  55,
	                      40,  41,  42,  43,  44,  45,  46,  47,
	                      32,  33,  34,  35,  36,  37,  38,  39,
	                      24,  25,  26,  27,  28,  29,  30,  31,
	                      16,  17,  18,  19,  20,  21,  22,  23,
	                       8,   9,  10,  11,  12,  13,  14,  15,
	                       0,   1,   2,   3,   4,   5,   6,   7  };

         if (fenStrings.Length < 4)
            throw new ArgumentException("Not enough FEN strings");
         //
         ClearBoard();
         //
         // **** The positions ****
         string boardString = fenStrings[0];
         int squareNr = 0;
         int n = 0;
         while (n<boardString.Length)
         {
            char c = boardString[n];
            if (char.IsDigit(c))
               squareNr += (int)(c - '0');   // advance position
            else if (c == '/')
            {  // simply skip '/'
            }
            else if (char.IsLetter(c))
            {
               int color = char.IsUpper(c) ? Const.White : Const.Black;
               switch (Char.ToUpper(c))
               {
                  case 'P': AddNewPieceToBoard(color, Const.PawnID, flip[squareNr]); break;
                  case 'N': AddNewPieceToBoard(color, Const.KnightID, flip[squareNr]); break;
                  case 'B': AddNewPieceToBoard(color, Const.BishopID, flip[squareNr]); break;
                  case 'R': AddNewPieceToBoard(color, Const.RookID, flip[squareNr]); break;
                  case 'Q': AddNewPieceToBoard(color, Const.QueenID, flip[squareNr]); break;
                  case 'K': AddNewPieceToBoard(color, Const.KingID, flip[squareNr]); break;
                  default:
                     throw new ArgumentException("FEN_to_Board : invalid piece char");
               }
               squareNr++;
            }
            else
               throw new ArgumentException("FEN_to_Board : invalid char");
            n++;
         }
         if (squareNr != 64)
            throw new ArgumentException("FEN_to_Board : not all squares are assigned");
         //
         // **** Side to move ****
         string colorString = fenStrings[1];
         if (colorString.Length != 1)
            throw new ArgumentException("FEN_to_Board : 'b' or 'w' expected");
         if (colorString[0] == 'w')
         {
            colorToMove = Const.White;
            enemyColor = Const.Black;
         }
         else if (colorString[0] == 'b')
         {
            colorToMove = Const.Black;
            enemyColor = Const.White;
         }
         else
            throw new ArgumentException("FEN_to_Board : 'b' or 'w' expected");
         //
         // **** Castling ****
         string castlingString = fenStrings[Const.NrColors];
         canCastleQueenSide[Const.White] = false;
         canCastleKingSide[Const.White] = false;
         canCastleQueenSide[Const.Black] = false;
         canCastleKingSide[Const.Black] = false;
         n = 0;
         while (n < castlingString.Length)
         {
            switch (castlingString[n])
            {
               case '-': break;
               case 'q': canCastleQueenSide[Const.Black] = true; break;
               case 'k': canCastleKingSide[Const.Black] = true; break;
               case 'Q': canCastleQueenSide[Const.White] = true; break;
               case 'K': canCastleKingSide[Const.White] = true; break;
            }
            n++;
         }
         //
         // **** en-passant position ****
         string epString = fenStrings[3];
         if (epString.Length == 1 &&  epString[0] == '-')
            enPassantPosition = Const.InvalidID;
         else
         {
            if (epString.Length != 2)
               throw new ArgumentException("FEN_to_Board : wrong ep-string length");
            if (!EPD.IsFileChar(epString[0]))
               throw new ArgumentException("FEN_to_Board : 'a'-'h' expected");
            if (!EPD.IsRankChar(epString[1]))
               throw new ArgumentException("FEN_to_Board : '1'-'8' expected");
            enPassantPosition = (int)(epString[0] - 'a') + 8 * (int)(epString[1] - '1');
         }
         // 
         if (fenStrings.Length < 6)
         {
            // 50-move rule && full move nr were ommitted. Use defaults
            fiftyMoveNr = 0;
            halfMoveNr = 0;
         }
         else
         {
            if (!int.TryParse(fenStrings[4], out fiftyMoveNr))
               throw new ArgumentException("Invalid Fifty-Move nr");
            int fullMoveNr;
            if (!int.TryParse(fenStrings[5], out fullMoveNr))
               throw new ArgumentException("Invalid Full-Move nr");
            if (fullMoveNr < 1)
               throw new ArgumentException("Invalid Full-Move nr ( <1 ) ");
            halfMoveNr = (fullMoveNr - 1) * 2;
         }
         if (colorToMove == Const.Black)
            halfMoveNr++;
         //
         // initialize hashvalues
         HashValue = CalcHashValue();
         // bwPawnsHashValue is already been done by AddNewPieceToBoard
#if PawnHashDebug
         if (bwPawnsHashValue != CalcBWPawnsHashValue())
            throw new ApplicationException("Invalid PawnHashValue");
#endif
         // all done. Now also store the current state of the board. This also stores the HashValues.
         nrStoredBoardStates = 0;
         StoreBoardState();
         // history
         HashValueHistory[0] = HashValue;
         nrHashValuesInHistory++;
      }


      public void FEN_to_Board(string fenString)
      {
         // epd = fen + extensions , like 'bm' etc
         string[] fenStrings;
         string[] restStrings;
         EPD.SplitEPD_fen_rest(fenString, out fenStrings, out restStrings);
         // setup the board
         FEN_To_Board(fenStrings);
      }

 
      #endregion


      #region SAN & LAN string <-> Move


      public string MoveToLANString(Move move)
      {
         // e.g. e2e4  e1g1 (KS castle), e7e8q (promotion)
         string s = EPD.PositionToFileString(move.fromPosition) + EPD.PositionToFileString(move.toPosition);
         if (move.moveType >= Const.SpecialMoveID)
            switch (move.moveType)
            {
               case Const.PawnPromoteQueenID:
               case Const.PawnPromoteRookID:
               case Const.PawnPromoteBishopID:
               case Const.PawnPromoteKnightID:
                  s += EPD.PromotionPieceToString(move.moveType).ToLower();
                  break;
            }
         return s;
      }


      public string MoveToSANString(Move move)
      {
         int fromPosition = move.fromPosition;
         int toPosition = move.toPosition;
         // first generate all (pseudo-legal) moves :
         Move[] moves = moveGenerator.GenerateMoves(null);
         int nrMoves = moveGenerator.nrGeneratedMoves;
         // Check the list to see if the move is present
         int moveNr = -1;
         for (int i=0; i<nrMoves; i++)
            if (moves[i] == move)
            {
               moveNr = i;
               break;
            }
         if (moveNr == -1)
            return "???";

         // maybe it's a special move 

         if (move.moveType >= Const.SpecialMoveID)
         {
            // these are never ambiguous
            switch (move.moveType)
            {
               case Const.CastleQSID: return "O-O-O";
               case Const.CastleKSID: return "O-O";
               case Const.EnPassantCaptureID:
                  return EPD.PositionToFileString(fromPosition) + "x" +EPD.PositionToString(toPosition);
               case Const.PawnPromoteQueenID:
               case Const.PawnPromoteRookID:
               case Const.PawnPromoteBishopID:
               case Const.PawnPromoteKnightID:
                  string s = EPD.PositionToFileString(fromPosition);
                  if (move.captureInfo == Const.NoCaptureID)
                     s += EPD.PositionToRankString(toPosition);
                  else
                  {
                     s += "x" + EPD.PositionToString(toPosition);
                  }
                  return s + "=" + EPD.PromotionPieceToString(move.moveType);
               case Const.Pawn2StepID:
                  return EPD.PositionToString(toPosition);
               default:
                  throw new ArgumentException("Invalid Special MoveType");
            }
         }

         // maybe it's a pawn move :

         if (move.moveType == Const.PawnID)
         {
            // is never ambiguous
            if (move.captureInfo == Const.NoCaptureID)
               return EPD.PositionToString(toPosition);
            else
               return EPD.PositionToFileString(fromPosition) + "x" + EPD.PositionToString(toPosition);
         }

         // It's a major piece move.

         string ss = EPD.PieceTypeToString(move.moveType);
         // check whetter multiple moves of the same type end on the same square
         int nrAmbiguousMoves = 0;
         Move ambiguousMove = Move.NoMove();
         for (int i = 0; i < nrMoves; i++)
         {
            if (i == moveNr)
               continue;
            if (move.moveType == moves[i].moveType && toPosition == moves[i].toPosition)
            {
               nrAmbiguousMoves++;
               if (nrAmbiguousMoves > 1)
                  break;  // more then 1, always specify both file and rank
               // only need to store 1
               ambiguousMove = moves[i];
            }
         }
         if (nrAmbiguousMoves > 0)
         {
            // add either a FileChar, a RankChar or both
            if (nrAmbiguousMoves == 1)
            {
               // different files ?
               if (ambiguousMove.fromPosition %8 != move.fromPosition % 8)
                  ss += EPD.PositionToFileString(move.fromPosition);
               else
                  // no, so differnent ranks
                  ss += EPD.PositionToRankString(move.fromPosition);
            }
            else
               // more then 1 ambiguous move. Use both file and rank
               ss += EPD.PositionToString(move.fromPosition);
         }
         if (move.captureInfo != Const.NoCaptureID)
            ss += "x";
         return ss + EPD.PositionToString(toPosition);
      }


      private Move FindLANMoveOnBoard(string s)
      {
         // something like e2e4, e1g1 (KS castle), e7e8q (promotion)
         // first generate all (pseudo-legal) moves :
         int fromPos = EPD.StringToPosition(s.Substring(0, 2));
         int toPos = EPD.StringToPosition(s.Substring(2, 2));
         int promotionType = -1;
         if (s.Length == 5)
         {
            // it contains the promotion piece
            promotionType = EPD.CharToPromotionMoveType(char.ToUpper(s[4]));
         }
         Move[] moves = moveGenerator.GenerateMoves(null);
         int nrMoves = moveGenerator.nrGeneratedMoves;
         for (int i = 0; i < nrMoves; i++)
         {
            if (moves[i].fromPosition == fromPos && moves[i].toPosition == toPos)
            {
               if (promotionType == -1)
                  return moves[i];       // it's not a promotion
               else if (moves[i].moveType == promotionType)
                  return moves[i];
            }
         }
         // nothing found
         return Move.NoMove();
      }


      /// <summary>
      /// Generate all possible moves & compare them to the SAN string. Return the move if found.
      /// </summary>
      /// <param name="s">The SAN string.</param>
      /// <returns>The generated move which correspondents to the SAN-string, or NoMove if not found.</returns>
      private Move FindSANMoveOnBoard(string s)
      {
         // first generate all (pseudo-legal) moves :
         Move[] moves = moveGenerator.GenerateMoves(null);
         int nrMoves = moveGenerator.nrGeneratedMoves;
         // a3 Nb2 Bxc7 axb4 O-O O-O-O h8=Q . Can end on +, ! etc
         int nrChars = s.Length;

         // First do the castlings

         if (s == "O-O" || s == "0-0")
         {
            // king side castle
            for (int i = 0; i < nrMoves; i++)
               if (moves[i].moveType == Const.CastleKSID)
                  return moves[i];
            return Move.NoMove();
         }
         if (s == "O-O-O" || s == "0-0-0")
         {
            // queen side castle
            for (int i = 0; i < nrMoves; i++)
               if (moves[i].moveType == Const.CastleQSID)
                  return moves[i];
            return Move.NoMove();
         }

         // Test for non-capturing pawn moves :  a3 , a8=Q , a8Q : these all start with a1

         if (nrChars >= 2 && EPD.IsFileChar(s[0]) && EPD.IsRankChar(s[1]))
         {
            int toPos = EPD.StringToPosition(s.Substring(0, 2));
            // maybe it's a promotion
            char promotionChar = ' ';
            if (nrChars >= 3 && EPD.IsPieceChar(s[2]))
               promotionChar = s[2];
            else if (nrChars >= 4 && EPD.IsPieceChar(s[3]))
               promotionChar = s[3];
            if (promotionChar != ' ')
            {
               int moveType = Const.InvalidID;
               switch (promotionChar)
               {
                  case 'Q': moveType = Const.PawnPromoteQueenID; break;
                  case 'R': moveType = Const.PawnPromoteRookID; break;
                  case 'B': moveType = Const.PawnPromoteBishopID; break;
                  case 'N': moveType = Const.PawnPromoteKnightID; break;
               }
               for (int i = 0; i < nrMoves; i++)
                  if (moves[i].moveType == moveType
                     && moves[i].toPosition == toPos
                     && moves[i].captureInfo == Const.NoCaptureID
                     )
                     return moves[i];
               // nothing found :
               return Move.NoMove();
            }
            // it is not a promotion, but a normal move
            for (int i = 0; i < nrMoves; i++)
               if ((moves[i].moveType == Const.PawnID || moves[i].moveType == Const.Pawn2StepID)
                  && moves[i].toPosition == toPos
                  && moves[i].captureInfo == Const.NoCaptureID
                  )
                  return moves[i];
            // nothing found :
            return Move.NoMove();
         }

         // Test for a capturing pawn moves :  axb7 , axb8Q , axb8=Q : these start with a FileChar & 'x'

         if (EPD.IsFileChar(s[0]))
         {
            if (nrChars < 4 || s[1] != 'x')
               return Move.NoMove();
            int toPos = EPD.StringToPosition(s.Substring(2, 2));
            int fromFileNr = EPD.CharToFile(s[0]);
            // maybe it's a capturing promotion
            char promotionChar = ' ';
            if (nrChars >= 5 && EPD.IsPieceChar(s[4]))
               promotionChar = s[4];
            else if (nrChars >= 6 && EPD.IsPieceChar(s[5]))
               promotionChar = s[5];
            if (promotionChar != ' ')
            {
               int moveType = EPD.CharToPromotionMoveType(promotionChar);
               for (int i = 0; i < nrMoves; i++)
                  if (moves[i].moveType == moveType
                     && moves[i].fromPosition % 8 == fromFileNr
                     && moves[i].toPosition == toPos
                     && moves[i].captureInfo != Const.NoCaptureID
                     )
                     return moves[i];
               // nothing found :
               return Move.NoMove();
            }
            // it is not a promotion, but a normal capture
            for (int i = 0; i < nrMoves; i++)
               if ((moves[i].moveType == Const.PawnID || moves[i].moveType == Const.EnPassantCaptureID)
                  && moves[i].fromPosition % 8 == fromFileNr
                  && moves[i].toPosition == toPos
                  && moves[i].captureInfo != Const.NoCaptureID
                  )
                  return moves[i];
            // nothing found :
            return Move.NoMove();
         }

         // So, handled all the pawn moves, now the Piece moves
         // Ne4 , Ngf3 , N5e4 , Ng5f3 , Bxe6 , Bgxe6 , B1xe6 , Be2xe3

         int moveType2 = EPD.CharToPieceType(s[0]);
         // normal moves can be treated just as capturing moves
         bool isCapture = s.Contains("x");
         if (isCapture)
         {
            s = s.Replace("x", "");
            nrChars = s.Length;
         }         
         // now left with : Ne4 , Ngf3 , N5e4 , Ng5f3
         int fromRankNr2 = -1;
         int fromFileNr2 = -1;
         // First handle a few special cases where the FromFile and/or FromRank is specified
         if (nrChars >= 4 && EPD.IsRankChar(s[1]))
         {
            // N5e4 : has FromRank
            fromRankNr2 = EPD.CharToRank(s[1]) * 8;
            s = s.Remove(1, 1);   // remove the fromRankChar
            nrChars = s.Length;
         }
         // now left with Ne4 , Ngf3 , Ng5f3
         if (nrChars >= 4 && EPD.IsFileChar(s[2]))
         {
            // Ngf3 : has FromFile
            fromFileNr2 = EPD.CharToFile(s[1]);
            s = s.Remove(1, 1);   // remove the fromFileChar
            nrChars = s.Length;
         }
         // now left with Ne4 , Ng5f3
         if (nrChars >= 5 && EPD.IsFileChar(s[3]) && EPD.IsRankChar(s[4]))
         {
            // has both FromFile and FromRank
            fromFileNr2 = EPD.CharToFile(s[1]);
            fromRankNr2 = EPD.CharToRank(s[2]) * 8;
            s = s.Remove(1, 2);   // remove the fromFileChar * fromRankChar
            nrChars = s.Length;
         }
         // now left with Ne4
         int toPos2 = EPD.StringToPosition(s.Substring(1,2));
         // now loop over all the moves
         for (int i = 0; i < nrMoves; i++)
            if (moves[i].moveType == moveType2
               && moves[i].toPosition == toPos2
               && (fromFileNr2 < 0 || moves[i].fromPosition % 8 == fromFileNr2)
               && (fromRankNr2 < 0 || moves[i].fromPosition / 8 == fromRankNr2)
               && (isCapture == (moves[i].captureInfo != Const.NoCaptureID) )
               )
               return moves[i];

         // nothing found :
         return Move.NoMove();
      }


      public Move FindMoveOnBoard(string s)
      {
         Move move;
         // moveString is either a SAN or a e2e4 type string;
         if (EPD.StringIsLANMove(s))
            move = FindLANMoveOnBoard(s);
         else if (EPD.StringIsSanMove(s))
            move = FindSANMoveOnBoard(s);
         else
            return Move.NoMove();
         return move;
      }

      #endregion


      #region Add, remove, move piece

      public void AddNewPieceToBoard(int color, int pieceTypeNr, int position)
      {
         if (SquareContents[position].pieceType != Const.EmptyID)
            throw new ArgumentException("Square is not empty");
         int newIndex = NrPieces[color, pieceTypeNr];
         SquareContents[position].pieceType = pieceTypeNr;
         SquareContents[position].pieceColor = color;
         SquareContents[position].pieceIndex = newIndex;
         //
         TotalNrPieces[color]++;
         NrPieces[color, pieceTypeNr] = newIndex + 1;
         PiecePos[color, pieceTypeNr, newIndex] = position;
         //
         pieceBB[color, pieceTypeNr] |= Identity[position];
         pieces[color] |= Identity[position];
         allPiecesBB |= Identity[position];
         // update the static scores
         StaticMaterialScore[color] += evaluator.PieceValues[pieceTypeNr];
         StaticPositionalScore[color] += evaluator.PieceSquareValues[color][pieceTypeNr][position];
         // update hashes
         HashValue ^= transpositionTable.SquareHashValue[position, pieceTypeNr, color];
         if (pieceTypeNr == Const.PawnID)
            bwPawnsHashValue ^= transpositionTable.SquareHashValue[position, Const.PawnID, color];
#if PawnHashDebug
         if (bwPawnsHashValue != CalcBWPawnsHashValue())
            throw new ApplicationException("Invalid PawnHashValue");
#endif
      }


      public void RemovePieceFromBoard(int position)
      {
         int index = SquareContents[position].pieceIndex;
         int pieceTypeNr = SquareContents[position].pieceType;
         int color = SquareContents[position].pieceColor;
         // 
         // If a rook is captured, we might want to update the CastlingInfo !
         if (pieceTypeNr == Const.RookID)
         {
            if (colorToMove == Const.White && color == Const.Black)
            {
               // check black rooks
               if (index == 56)
                  ResetCanCastleQS(Const.Black);
               else if (index == 63)
                  ResetCanCastleKS(Const.Black);
            }
            else if (colorToMove == Const.Black && color == Const.White)
            {
               // check white rooks
               if (index == 0)
                  ResetCanCastleQS(Const.White);
               else if (index == 7)
                  ResetCanCastleKS(Const.White);
            }
         }
         //
         SquareContents[position].pieceType = Const.EmptyID;
         SquareContents[position].pieceColor = Const.InvalidID;
         SquareContents[position].pieceIndex = Const.InvalidID;
         //
         TotalNrPieces[color]--;
         int lastIndex = NrPieces[color, pieceTypeNr] - 1;
         NrPieces[color, pieceTypeNr] = NrPieces[color, pieceTypeNr] - 1;
         // copy the last piece to the removed piece slot
         if (index != lastIndex)
         {
            int pos = PiecePos[color, pieceTypeNr, lastIndex];
            PiecePos[color, pieceTypeNr, index] = pos;
            SquareContents[pos].pieceIndex = index;
         }
         PiecePos[color, pieceTypeNr, lastIndex] = Const.InvalidID;
         //
         pieceBB[color, pieceTypeNr] &= ~Identity[position];
         pieces[color] &= ~Identity[position];
         allPiecesBB &= ~Identity[position];
         // update the static scores
         StaticMaterialScore[color] -= evaluator.PieceValues[pieceTypeNr];
         StaticPositionalScore[color] -= evaluator.PieceSquareValues[color][pieceTypeNr][position];
         // update hashes
         HashValue ^= transpositionTable.SquareHashValue[position, pieceTypeNr, color];
         if (pieceTypeNr == Const.PawnID)
            bwPawnsHashValue ^= transpositionTable.SquareHashValue[position, Const.PawnID, color];
#if PawnHashDebug
         if (bwPawnsHashValue != CalcBWPawnsHashValue())
            throw new ApplicationException("Invalid PawnHashValue");
#endif
      }


      public void MovePieceOnBoard(int oldPosition, int newPosition)
      {
         // Capture must be handled by RemovePieceFromBoard
         int index = SquareContents[oldPosition].pieceIndex;
         int pieceTypeNr = SquareContents[oldPosition].pieceType;
         int color = SquareContents[oldPosition].pieceColor;
#if MoveToIsEmptyDebug
         if (SquareContents[newPosition].pieceType != Board.EmptyID)
            throw new ApplicationException("destination square is not empty");
#endif
         //
         SquareContents[oldPosition].pieceType = Const.EmptyID;
         SquareContents[oldPosition].pieceColor = Const.InvalidID;
         SquareContents[oldPosition].pieceIndex = Const.InvalidID;
         //
         SquareContents[newPosition].pieceType = pieceTypeNr;
         SquareContents[newPosition].pieceColor = color;
         SquareContents[newPosition].pieceIndex = index;
         //
         PiecePos[color, pieceTypeNr, index] = newPosition;
         //
         pieceBB[color, pieceTypeNr] &= ~Identity[oldPosition];
         pieces[color] &= ~Identity[oldPosition];
         allPiecesBB &= ~Identity[oldPosition];
         //
         pieceBB[color, pieceTypeNr] |= Identity[newPosition];
         pieces[color] |= Identity[newPosition];
         allPiecesBB |= Identity[newPosition];
         // update the static scores
         StaticPositionalScore[color] += evaluator.PieceSquareValues[color][pieceTypeNr][newPosition]
                                        - evaluator.PieceSquareValues[color][pieceTypeNr][oldPosition];
         // update hash
         HashValue ^= transpositionTable.SquareHashValue[oldPosition, pieceTypeNr, color];
         HashValue ^= transpositionTable.SquareHashValue[newPosition, pieceTypeNr, color];
         if (pieceTypeNr == Const.PawnID)
         {
            bwPawnsHashValue ^= transpositionTable.SquareHashValue[oldPosition, Const.PawnID, color];
            bwPawnsHashValue ^= transpositionTable.SquareHashValue[newPosition, Const.PawnID, color];
         }
#if PawnHashDebug
         if (bwPawnsHashValue != CalcBWPawnsHashValue())
            throw new ApplicationException("Invalid PawnHashValue");
#endif
      }

      #endregion


      #region Store BoardState for moving up and down the plies

      private void StoreBoardState()
      {
         nrStoredBoardStates++;
         if (nrStoredBoardStates == maxNrStoredBoardStates)
         {
            // increase storage
            int newMaxNrStoreBoardStates = 2 * maxNrStoredBoardStates;
            BoardState[] newStoredBoardStates = new BoardState[newMaxNrStoreBoardStates];
            for (int i = 0; i < maxNrStoredBoardStates; i++)
               newStoredBoardStates[i] = storedBoardStates[i];
            storedBoardStates = newStoredBoardStates;
            maxNrStoredBoardStates = newMaxNrStoreBoardStates;
         }
         //
         storedBoardStates[nrStoredBoardStates].enPassantPosition = enPassantPosition;
         storedBoardStates[nrStoredBoardStates].fiftyMoveNr = fiftyMoveNr;
         storedBoardStates[nrStoredBoardStates].canCastleQueenSide_White = canCastleQueenSide[Const.White];
         storedBoardStates[nrStoredBoardStates].canCastleKingSide_White = canCastleKingSide[Const.White];
         storedBoardStates[nrStoredBoardStates].hasCastled_White = hasCastled[Const.White];
         storedBoardStates[nrStoredBoardStates].canCastleQueenSide_Black = canCastleQueenSide[Const.Black];
         storedBoardStates[nrStoredBoardStates].canCastleKingSide_Black = canCastleKingSide[Const.Black];
         storedBoardStates[nrStoredBoardStates].hasCastled_Black = hasCastled[Const.Black];
         storedBoardStates[nrStoredBoardStates].capturedPieceType = capturedPieceType;
         storedBoardStates[nrStoredBoardStates].capturedPiecePosition = capturedPiecePosition;
         storedBoardStates[nrStoredBoardStates].repeatedPosition_SearchOffset = repeatedPosition_SearchOffset;
         storedBoardStates[nrStoredBoardStates].hashValue = HashValue;
         storedBoardStates[nrStoredBoardStates].bwPawnsHashValue = bwPawnsHashValue;
      }

      private void RestoreBoardState()
      {
         nrStoredBoardStates--;
         //
         enPassantPosition = storedBoardStates[nrStoredBoardStates].enPassantPosition;
         fiftyMoveNr = storedBoardStates[nrStoredBoardStates].fiftyMoveNr;
         canCastleQueenSide[Const.White] = storedBoardStates[nrStoredBoardStates].canCastleQueenSide_White;
         canCastleKingSide[Const.White] = storedBoardStates[nrStoredBoardStates].canCastleKingSide_White;
         hasCastled[Const.White] = storedBoardStates[nrStoredBoardStates].hasCastled_White;
         canCastleQueenSide[Const.Black] = storedBoardStates[nrStoredBoardStates].canCastleQueenSide_Black;
         canCastleKingSide[Const.Black] = storedBoardStates[nrStoredBoardStates].canCastleKingSide_Black;
         hasCastled[Const.Black] = storedBoardStates[nrStoredBoardStates].hasCastled_Black;
         capturedPieceType = storedBoardStates[nrStoredBoardStates].capturedPieceType;
         capturedPiecePosition = storedBoardStates[nrStoredBoardStates].capturedPiecePosition;
         repeatedPosition_SearchOffset = storedBoardStates[nrStoredBoardStates].repeatedPosition_SearchOffset;
         HashValue = storedBoardStates[nrStoredBoardStates].hashValue;
         bwPawnsHashValue = storedBoardStates[nrStoredBoardStates].bwPawnsHashValue;
      }

      #endregion


      #region IsInCheck , IsDrawn

      /// <summary>
      /// Check whetter the king of the color to move is under attack
      /// </summary>
      /// <returns>True if the ColorToMove is in check. False, if not.</returns>
      public bool IsInCheck()
      {
         // Use the attack symmetry:  Pretend our king is all piece types, 
         // and see if it can capture the corresponding enemy piece type.
         // This version speeds up the program by 10 %, in comparison to the IsInCheck_old version.
         ulong attackBB;
         int kingPos = PiecePos[colorToMove, Const.KingID, 0];
         // The sliding pieces
         // pretend the king is a bishop. Get all squares it could attack.
         attackBB = magicMoves.Bmagic(kingPos, allPiecesBB);
         // check whetter it includes an enemy Queen or Bishop
         if ((attackBB & (pieceBB[enemyColor, Const.QueenID] | pieceBB[enemyColor, Const.BishopID])) != 0)
            return true;
         // pretend the king is a rook. Get all squares it could attack.
         attackBB = magicMoves.Rmagic(kingPos, allPiecesBB);
         // check whetter it includes an enemy Queen or Rook
         if ((attackBB & (pieceBB[enemyColor, Const.QueenID] | pieceBB[enemyColor, Const.RookID])) != 0)
            return true;
         // The non-sliding pieces
         // pretend the king is a Knight. Get all squares it could attack. See if it includes an enemy Knight.
         if ((moveGenerator.EmptyBoardKnightMoves[kingPos] & pieceBB[enemyColor, Const.KnightID]) != 0)
            return true;
         // pretend the king is a Pawn. Get all squares it could attack. See if it includes an enemy pawn.
         // (the EmptyBoardPawnCatchMoves does _not_ include en-passant capturing, so ok).
         if ((bitboard.PawnAttackBB1[colorToMove, kingPos] & pieceBB[enemyColor, Const.PawnID]) != 0)
            return true;
         // pretend the king is a King. Get all squares it could attack. See if it includes the enemy King.
         if ((moveGenerator.EmptyBoardKingMoves[kingPos] & pieceBB[enemyColor, Const.KingID]) != 0)
            return true;
         //
         // Not in check !
         return false;
      }


      public bool IsDrawnBy50Moves()
      {
         return fiftyMoveNr >= 100;
      }


      public bool IsDrawnBy3xRepetition()
      {
         // check for 3x repetition
         int nrSame = 0;
         // repeatedPosition_SearchOffset is the last move a pawn-move was made. This position is irreversible.
         for (int i = repeatedPosition_SearchOffset; i < nrHashValuesInHistory; i++)
            if (HashValueHistory[i] == HashValue)
               nrSame++;
         // nrSame : officially it is 2 : 2 times before, so this is the 3rd time
         return nrSame >= 2;
      }


      public bool IsDrawnByMaterial()
      {
         // check for not enough material
         // Material draw, if both sides have no pawns and either 1 bishop, or 1 or 2 knights
         //
         // The fastest test is the TotalNrPieces :
         // nr pieces > 3 is king + 3 other pieces = never a draw
         if (TotalNrPieces[Const.White] > 3 || TotalNrPieces[Const.Black] > 3)
            return false;
         // if either side has pawns, it is not a draw
         if (NrPieces[Const.White, Const.PawnID] > 0 || NrPieces[Const.Black, Const.PawnID] > 0)
            return false;
         // if either side has a queen or a rook, it is not a draw
         if (NrPieces[Const.White, Const.QueenID] > 0 || NrPieces[Const.Black, Const.QueenID] > 0)
            return false;
         if (NrPieces[Const.White, Const.RookID] > 0 || NrPieces[Const.Black, Const.RookID] > 0)
            return false;
         // both sides have no pawns, no rooks, no queens.
         int nrWhiteBishops = NrPieces[Const.White, Const.BishopID];
         int nrBlackBishops = NrPieces[Const.Black, Const.BishopID];
         // if either side has more then 1 bishop, it is not a draw. (ignores 2 bishops of the same color)
         if (nrWhiteBishops > 1 || nrBlackBishops > 1)
            return false;
         // both sides have maximal 1 bishop
         int nrWhiteKnights = NrPieces[Const.White, Const.KnightID];
         int nrBlackKnights = NrPieces[Const.Black, Const.KnightID];
         bool whiteCantWin = nrWhiteBishops == 0 && nrWhiteKnights <= 2
                             || nrWhiteBishops == 1 && nrWhiteKnights == 0;
         bool blackCantWin = nrBlackBishops == 0 && nrBlackKnights <= 2
                             || nrBlackBishops == 1 && nrBlackKnights == 0;
         return whiteCantWin && blackCantWin;
      }


      public bool IsPracticallyDrawn()
      {
         // check for 50-rule
         if (fiftyMoveNr >= 100)
            return true;

         // check for 3x repetition
         int nrSame = 0;
         // repeatedPosition_SearchOffset is the last move a pawn-move was made. This position is irreversible.
         for (int i = repeatedPosition_SearchOffset; i < nrHashValuesInHistory; i++)
            if (HashValueHistory[i] == HashValue)
               nrSame++;
         // nrSame : officially it is 2 : 2 times before, so this is the 3rd time
         // but >= 1 works better, since if a repeated position could be forced, it can also happen a 2nd time.
         if (nrSame >= 1)
            return true;

         // check for not enough material
         if (IsDrawnByMaterial())
            return true;

         return false;
      }


      public bool HasZugZwang()
      {
         //return evaluator.GetGameStage() == Const.EndGame;   // this seems to restrictive
         // could introduce pawn mobility
         int nrPawns = NrPieces[colorToMove, Const.PawnID];
         int nrPieces = TotalNrPieces[colorToMove];
         return nrPieces - nrPawns == 1;  // zugzwang if no major/minor pieces left      
      }


      #endregion


      #region Make / Unmake move


      private void ResetCanCastleQS(int castleColor)
      {
         // first check, because we want to remove the CanCastle hashValue only once
         if (canCastleQueenSide[castleColor])
         {
            canCastleQueenSide[castleColor] = false;
            HashValue ^= transpositionTable.CanCastleQSHashValue[castleColor];
         }
      }

      private void ResetCanCastleKS(int castleColor)
      {
         // first check, because we want to remove the CanCastle hashValue only once
         if (canCastleKingSide[castleColor])
         {
            canCastleKingSide[castleColor] = false;
            HashValue ^= transpositionTable.CanCastleKSHashValue[castleColor];
         }
      }

      public bool MakeMove(Move move)
      {
         bool moveIsLegal = true;   // this will be checked if a castle is done and by the IsInCheck test
         // increment the HalfMoveNr
         halfMoveNr++;
         // increment the fiftyMoveNr. It will be reset later in this method if it was a capture or a pawn-move.
         fiftyMoveNr++;
         //         
         capturedPieceType = Const.InvalidID;
         capturedPiecePosition = Const.InvalidID;
#if HashDebug
         if (HashValue != CalcHashValue())
            throw new ApplicationException("hash differs");
#endif
         // maybe remove the en-passant from the hash 
         if (enPassantPosition != Const.InvalidID)
            HashValue ^= transpositionTable.EPSquareValue[enPassantPosition];
         //
         if (move.moveType < Const.SpecialMoveID)
         {
            // a normal move of a piece
            enPassantPosition = Const.InvalidID;    // normal move : always reset the enpassant position
            // Handle castling info
            if (canCastleQueenSide[colorToMove] || canCastleKingSide[colorToMove])
            {
               // check for a king move
               if (move.moveType == Const.KingID)
               {
                  ResetCanCastleQS(colorToMove);
                  ResetCanCastleKS(colorToMove);
               }
               // check for a castle move
               if (move.moveType == Const.RookID)
               {
                  int offset = colorToMove * 56;
                  // check if the QS rook will move. Offset points to the queen-side rooks.
                  if (move.fromPosition == offset)
                     ResetCanCastleQS(colorToMove);
                  // check if the KS rook will move.  Offset+7 points to the king-side rooks.
                  if (move.fromPosition == offset + 7)
                     ResetCanCastleKS(colorToMove);
               }
            }
            // update the fiftyMoveNr
            if (move.moveType == Const.PawnID)
            {
               fiftyMoveNr = 0;                 // pawn move : reset the fiftyMoveNr
               // if a pawn is moved, the previous position can never be seen again : so
               repeatedPosition_SearchOffset = nrHashValuesInHistory;
               //repeatedPosition_SearchOffset = halfMoveNr - 1;
            }
            // is it a capture ? Yes : remove the captured piece
            if (SquareContents[move.toPosition].pieceType != Const.EmptyID)
            {
               // save state
               capturedPiecePosition = move.toPosition;
               capturedPieceType = SquareContents[move.toPosition].pieceType;
               //
               RemovePieceFromBoard(move.toPosition);
               fiftyMoveNr = 0;   // capture : reset the fiftyMoveNr
            }
            // now make the move
            MovePieceOnBoard(move.fromPosition, move.toPosition);
         }
         else
         {
            // Specials
            // it is a Castle, enpassant capture, 2-stap pawn move or pawn promotion
            int castleRankOffset = colorToMove * 56;
            switch (move.moveType)
            {
               case Const.CastleQSID:
                  // Maybe the castle was illegal. Do it anyway. 
                  // Illegal castle will immediately be undone in SearchMove.
                  if (moveIsLegal)
                     moveIsLegal = moveGenerator.CastleIsLegal(Const.CastleQSID);
                  MovePieceOnBoard(castleRankOffset + 4, castleRankOffset + 2);  // King
                  MovePieceOnBoard(castleRankOffset + 0, castleRankOffset + 3);  // Rook
                  hasCastled[colorToMove] = true;
                  ResetCanCastleQS(colorToMove);
                  ResetCanCastleKS(colorToMove);
                  break;
               case Const.CastleKSID:
                  // Maybe the castle was illegal. Do it anyway. 
                  // Illegal castle will immediately be undone in SearchMove.
                  if (moveIsLegal)
                     moveIsLegal = moveGenerator.CastleIsLegal(Const.CastleKSID);
                  MovePieceOnBoard(castleRankOffset + 4, castleRankOffset + 6);  // King
                  MovePieceOnBoard(castleRankOffset + 7, castleRankOffset + 5);  // Rook
                  hasCastled[colorToMove] = true;
                  ResetCanCastleQS(colorToMove);
                  ResetCanCastleKS(colorToMove);
                  break;
               case Const.EnPassantCaptureID:
                  capturedPieceType = Const.PawnID;
                  if (colorToMove == Const.White)
                     capturedPiecePosition = enPassantPosition - 8;  // captured pawn is black
                  else
                     capturedPiecePosition = enPassantPosition + 8;  // captured pawn is white
                  RemovePieceFromBoard(capturedPiecePosition);
                  MovePieceOnBoard(move.fromPosition, move.toPosition);
                  break;
               case Const.PawnPromoteQueenID:
               case Const.PawnPromoteRookID:
               case Const.PawnPromoteBishopID:
               case Const.PawnPromoteKnightID:
                  if (SquareContents[move.toPosition].pieceType != Const.EmptyID)
                  {
                     // save state
                     capturedPiecePosition = move.toPosition;
                     capturedPieceType = SquareContents[move.toPosition].pieceType;
                     RemovePieceFromBoard(move.toPosition);          // it was a capture
                  }
                  RemovePieceFromBoard(move.fromPosition);
                  AddNewPieceToBoard(colorToMove, Const.QueenID + move.moveType - Const.PawnPromoteQueenID, move.toPosition);
                  fiftyMoveNr = 0;                 // pawn move : reset the fiftyMoveNr
                  break;
               case Const.Pawn2StepID:
                  MovePieceOnBoard(move.fromPosition, move.toPosition);
                  // point enPassantPosition to the jumped over square
                  enPassantPosition = (move.fromPosition + move.toPosition) / 2;
                  fiftyMoveNr = 0;                 // pawn move : reset the fiftyMoveNr
                  // update hashValue
                  HashValue ^= transpositionTable.EPSquareValue[enPassantPosition];
                  break;
               case Const.NullMoveID:
                  // just do nothing
                  break;
               default:
                  throw new Exception("Invalid special move nr");
            }
            // always reset the enPassant pasition, unless it set by the Pawn 2Step move
            if (move.moveType != Const.Pawn2StepID)
               enPassantPosition = Const.InvalidID;
         }
         // Finally check if this move was legal. If not, it will immediately be undone in SearchMove.
         // Do it with an IF statement, since also castling could have set moveIsLegal to false.
         if (IsInCheck())
            moveIsLegal = false;
         //
         ToggleMoveColor();
         // Store the state exactly as it was AFTER this move was made. Also stores HashValue.
         StoreBoardState();
         //
         // Remember the hash-value of this board
         nrHashValuesInHistory++;
         if (nrHashValuesInHistory == HashValueHistory.Length)
         {
            // increase size
            ulong[] newArray = new ulong[HashValueHistory.Length + 100];
            Array.Copy(HashValueHistory, newArray, HashValueHistory.Length);
            HashValueHistory = newArray;
         }
         HashValueHistory[nrHashValuesInHistory] = HashValue;
         //
         return moveIsLegal;
      }


      public void UnMakeMove(Move move)
      {
         int prevColorToMove = colorToMove;
         // This is the color which made the move :
         ToggleMoveColor();
         // decrement the HalfMoveNr 
         halfMoveNr--;
         //
         if (move.moveType < Const.SpecialMoveID)
         {
            // un-make the move
            MovePieceOnBoard(move.toPosition, move.fromPosition);
         }
         else
         {
            // Unmake specials
            // it is a Castle, enpassant capture, 2-stap pawn move or pawn promotion
            int castleRankOffset = colorToMove * 56;
            switch (move.moveType)
            {
               case Const.CastleQSID:
                  MovePieceOnBoard(castleRankOffset + 2, castleRankOffset + 4);  // King
                  MovePieceOnBoard(castleRankOffset + 3, castleRankOffset + 0);  // Rook
                  break;
               case Const.CastleKSID:
                  MovePieceOnBoard(castleRankOffset + 6, castleRankOffset + 4);  // King
                  MovePieceOnBoard(castleRankOffset + 5, castleRankOffset + 7);  // Rook
                  break;
               case Const.EnPassantCaptureID:
                  MovePieceOnBoard(move.toPosition, move.fromPosition);
                  break;
               case Const.PawnPromoteQueenID:
               case Const.PawnPromoteRookID:
               case Const.PawnPromoteBishopID:
               case Const.PawnPromoteKnightID:
                  RemovePieceFromBoard(move.toPosition);     // the promoted piece
                  AddNewPieceToBoard(colorToMove, Const.PawnID, move.fromPosition);
                  break;
               case Const.Pawn2StepID:
                  MovePieceOnBoard(move.toPosition, move.fromPosition);
                  int oldEPPos = (move.toPosition + move.fromPosition)/2;
                  break;
               case Const.NullMoveID:
                  break;
               default:
                  throw new Exception("Invalid special move nr");
            }
         }
         // was it a capture ? Yes : restore the captured piece
         if (capturedPieceType != Const.InvalidID)
            AddNewPieceToBoard(prevColorToMove, capturedPieceType, capturedPiecePosition);
         // Last restore the BoardState as it was just after the move. This also restores the HashValue.
         RestoreBoardState();
#if HashDebug
         if (HashValue != CalcHashValue())
               throw new ApplicationException("hash differs");
#endif
         nrHashValuesInHistory--;
      }

      #endregion


    }
}
