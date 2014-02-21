// 21-2 crafty 5 seconds : 122 ok, 225 fail

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LousyChess
{
   public class EPD
   {

      #region static char/string and rank/file/position stuff

      public static bool IsFileChar(char c)
      {
         return char.IsLower(c) && c >= 'a' && c <= 'h';
      }

      public static bool IsRankChar(char c)
      {
         return char.IsDigit(c) && c >= '1' && c <= '8';
      }

      public static bool IsPieceChar(char c)
      {
         return c == 'K' || c == 'Q' || c == 'R' || c == 'B' || c == 'N';
      }


      public static string PositionToFileString(int position)
      {
         return ((char)((position % 8) + 'a')).ToString();
      }

      public static string PositionToRankString(int position)
      {
         return ((char)((position / 8) + '1')).ToString();
      }

      public static string PositionToString(int position)
      {
         return PositionToFileString(position) + PositionToRankString(position);
      }

      public static string PieceTypeToString(int pieceType)
      {
         switch (pieceType)
         {
            case Const.KingID: return "K";
            case Const.QueenID: return "Q";
            case Const.RookID: return "R";
            case Const.BishopID: return "B";
            case Const.KnightID: return "N";
            case Const.PawnID: return "";
            default: throw new ArgumentException("invalid PieceType : " + pieceType.ToString());
         }
      }

      public static int StringToPosition(string s)
      {
         if (s.Length != 2)
            throw new ArgumentException("invalid PositionString : " + s);
         return CharToFile(s[0]) + 8*CharToRank(s[1]);
      }

      public static int CharToFile(char fileChar)
      {
         if (fileChar < 'a' || fileChar > 'h')
            throw new ArgumentException("invalid fileChar : " + fileChar);
         return (int)(fileChar - 'a');
      }

      public static int CharToRank(char rankChar)
      {
         if (rankChar < '1' || rankChar > '8')
            throw new ArgumentException("invalid rankChar : " + rankChar);
         return (int)(rankChar - '1');
      }


      public static int CharToPieceType(char pieceChar)
      {
         switch (pieceChar)
         {
            case 'K': return Const.KingID;
            case 'Q': return Const.QueenID;
            case 'R': return Const.RookID;
            case 'B': return Const.BishopID;
            case 'N': return Const.KnightID;
            default: throw new ArgumentException("invalid PieceChar : " + pieceChar);
         }
      }

      public static string PromotionPieceToString(int moveType)
      {
         switch (moveType)
         {
            case Const.PawnPromoteQueenID: return "Q";
            case Const.PawnPromoteRookID: return "R";
            case Const.PawnPromoteBishopID: return "B";
            case Const.PawnPromoteKnightID: return "N";
            default: throw new ArgumentException("invalid moveType : " + moveType.ToString());
         }
      }

      public static int CharToPromotionMoveType(char pieceChar)
      {
         switch (pieceChar)
         {
            case 'Q': return Const.PawnPromoteQueenID;
            case 'R': return Const.PawnPromoteRookID;
            case 'B': return Const.PawnPromoteBishopID;
            case 'N': return Const.PawnPromoteKnightID;
            default: throw new ArgumentException("invalid PieceChar : " + pieceChar);
         }
      }


      #endregion



      /// <summary>
      /// Splits an EPD string in 2 string arrays. One with the FEN pieces and one with the res.
      /// </summary>
      /// <param name="epd"></param>
      /// <param name="fenPieces"></param>
      /// <param name="restPieces"></param>
      public static void SplitEPD_fen_rest(string epd, out string[] fenStrings, out string[] restStrings)
      {
         epd = epd.Trim();   // remove leading and trailing spaces
         // discard everything after the first ';'
         int n = epd.IndexOf(';');
         if (n >= 0)
            epd = epd.Substring(0, n);
         // split the string at spaces; don't return empty entries due to multiple spaces.
         char[] charSeparators = new char[] {' '};
         string[] epdStrings = epd.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
         int nrEpdStrings = epdStrings.Length;
         // the first 4 are : board, colorToMove, castling, ep-position
         // nr 5 should be the 50-move nr
         // nr 6 should be the full-move nr
         // But 5 & 6 are sometimes ommited
         int nrFenStrings = Math.Min(4, nrEpdStrings);             // expect at least 4
         // check if piece 5 exists and starts with a digit
         if (nrEpdStrings > 4 && epdStrings[4].Length>0 && char.IsDigit(epdStrings[4][0]) )
            nrFenStrings++;
         // check if piece 6 exists and starts with a digit
         if (nrEpdStrings > 5 && epdStrings[5].Length>0 && char.IsDigit(epdStrings[5][0]) )
            nrFenStrings++;
         fenStrings = new string[nrFenStrings];
         for (int i=0; i<nrFenStrings; i++)
            fenStrings[i] = epdStrings[i];
         // 
         restStrings = new string[nrEpdStrings - nrFenStrings];
         for (int i=nrFenStrings; i<nrEpdStrings; i++)
            restStrings[i-nrFenStrings] = epdStrings[i];
      }


      public static bool StringIsSanMove(string s)
      {
         // a3 Nb2 Bxc7 axb4 O-O O-O-O h8=Q . Can end on +, ! etc
         // nb : the string can always be followed by : +  =Q  ! ? etc
         int nrChars = s.Length;
         if (nrChars < 2)
            return false;                  // must be at least 2 chars
         // test for O-O  O-O-O
         if (s.StartsWith("O-O") || s.StartsWith("0-0"))
            return true;
         // test for a3
         if (nrChars >= 2 && IsFileChar(s[0]) && IsRankChar(s[1])  )
            return true;
         // Now there must be at least 3 chars
         if (nrChars < 3)
            return false;
         if (IsPieceChar(s[0]))
         {
            // starts with K,Q,R,B,N
            return true;
         }
         // test for axb4
         if ( nrChars >= 4 && IsFileChar(s[0]) && s[1] == 'x' && IsFileChar(s[2]) && IsRankChar(s[3]) )
            return true;
         return false;
      }

      public static bool StringIsLANMove(string s)
      {
         // a 4 char string as : e2e4, or e7e8q
         return s.Length >= 4 &&  IsFileChar(s[0]) && IsRankChar(s[1]) && IsFileChar(s[2]) && IsRankChar(s[3]);
      }

      /// <summary>
      /// Extracts the best-move strings from the RestStrings and removes them.
      /// </summary>
      /// <param name="restPieces"></param>
      /// <returns></returns>
      public static string[] ExtractBestMoveStrings(string[] restStrings)
      {
         int bmIndex = -1;
         // find the index of the  "bm" string
         for (int i = 0; i < restStrings.Length; i++)
         {
            if (restStrings[i].ToLower() == "bm")
            {
               bmIndex = i;
               break;
            }
         }
         if (bmIndex == -1)
            return new string[0];    // nothing found
         //
         // now find valid moves : either e2e4 or a SAN format move
         int nrBMStrings = 0;
         // each restPieces string has a length of at least 1
         for (int i = bmIndex + 1; i < restStrings.Length; i++)
         {
            if (StringIsLANMove(restStrings[i]) || StringIsSanMove(restStrings[i]) )
               nrBMStrings++;
            else
               break;
         }
         // maybe nothing was found :
         if (nrBMStrings == 0)
            return new string[0];
         // at least 1 was found
         string[] result = new string[nrBMStrings];
         for (int i=0; i<nrBMStrings; i++)
            result[i] = restStrings[bmIndex + 1 + i];
         // remove the 'bm' string and the moves
         string[] newRestStrings = new string[restStrings.Length - nrBMStrings - 1];
         // before the best moves
         for (int i=0; i<bmIndex; i++)
            newRestStrings[i] = restStrings[i];
         // after the best moves
         for (int i=0; i<restStrings.Length-nrBMStrings - bmIndex -1; i++)
            newRestStrings[bmIndex+i] = restStrings[bmIndex+i + nrBMStrings + 1];
         restStrings = newRestStrings;
         return result;
      }

      public static string GetEPDString(string fileName, int epdNr)
      {
         if (!File.Exists(fileName))
            return "File not found";
         int lineNr = 0;
         using (StreamReader sr = new StreamReader(fileName))
         {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
               lineNr++;
               if (lineNr == epdNr)
                  return line;
            }
         }
         return "End of file exceeded";
      }


      public static int NrEPDsInFile(string fileName)
      {
         if (!File.Exists(fileName))
            return 0;
         int lineNr = 0;
         using (StreamReader sr = new StreamReader(fileName))
         {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
               if (line.Trim() == "")
                  continue;
               lineNr++;
            }
         }
         return lineNr;
      }


   }
}
