using System;

namespace Valil.Chess.Engine
{
    public sealed partial class ChessEngine
    {
        private const int DOUBLED_PAWN_PENALTY = 10;
        private const int ISOLATED_PAWN_PENALTY = 20;
        private const int BACKWARDS_PAWN_PENALTY = 8;
        private const int PASSED_PAWN_BONUS = 20;
        private const int ROOK_SEMI_OPEN_FILE_BONUS = 10;
        private const int ROOK_OPEN_FILE_BONUS = 15;
        private const int ROOK_ON_SEVENTH_BONUS = 20;

        // the values of the pieces
        private static readonly int[] pieceValue = { 100, 300, 300, 500, 900, 0 };

        // the "PcSq" arrays are piece/square tables
        // they're values added to the material value of the piece based on the location of the piece
        private static readonly int[] pawnPcSq = 
{
	0,   0,   0,   0,   0,   0,   0,   0,
	5,  10,  15,  20,  20,  15,  10,   5,
	4,   8,  12,  16,  16,  12,   8,   4,
	3,   6,   9,  12,  12,   9,   6,   3,
	2,   4,   6,   8,   8,   6,   4,   2,
	1,   2,   3, -10, -10,   3,   2,   1,
	0,   0,   0, -40, -40,   0,   0,   0,
	0,   0,   0,   0,   0,   0,   0,   0
};
        private static readonly int[] knightPcSq = 
{
	-10, -10, -10, -10, -10, -10, -10, -10,
	-10,   0,   0,   0,   0,   0,   0, -10,
	-10,   0,   5,   5,   5,   5,   0, -10,
	-10,   0,   5,  10,  10,   5,   0, -10,
	-10,   0,   5,  10,  10,   5,   0, -10,
	-10,   0,   5,   5,   5,   5,   0, -10,
	-10,   0,   0,   0,   0,   0,   0, -10,
	-10, -30, -10, -10, -10, -10, -30, -10
};
        private static readonly int[] bishopPcSq = 
{
	-10, -10, -10, -10, -10, -10, -10, -10,
	-10,   0,   0,   0,   0,   0,   0, -10,
	-10,   0,   5,   5,   5,   5,   0, -10,
	-10,   0,   5,  10,  10,   5,   0, -10,
	-10,   0,   5,  10,  10,   5,   0, -10,
	-10,   0,   5,   5,   5,   5,   0, -10,
	-10,   0,   0,   0,   0,   0,   0, -10,
	-10, -10, -20, -10, -10, -20, -10, -10
};
        private static readonly int[] kingPcSq = 
{
	-40, -40, -40, -40, -40, -40, -40, -40,
	-40, -40, -40, -40, -40, -40, -40, -40,
	-40, -40, -40, -40, -40, -40, -40, -40,
	-40, -40, -40, -40, -40, -40, -40, -40,
	-40, -40, -40, -40, -40, -40, -40, -40,
	-40, -40, -40, -40, -40, -40, -40, -40,
	-20, -20, -20, -20, -20, -20, -20, -20,
	0,  20,  40, -20,   0, -20,  40,  20
};
        private static readonly int[] kingEndgamePcSq = 
{
	 0,  10,  20,  30,  30,  20,  10,   0,
	10,  20,  30,  40,  40,  30,  20,  10,
	20,  30,  40,  50,  50,  40,  30,  20,
	30,  40,  50,  60,  60,  50,  40,  30,
	30,  40,  50,  60,  60,  50,  40,  30,
	20,  30,  40,  50,  50,  40,  30,  20,
	10,  20,  30,  40,  40,  30,  20,  10,
	 0,  10,  20,  30,  30,  20,  10,   0
};

        // the flip array is used to calculate the piece/square values for DARK pieces
        // for example, the piece/square value of a LIGHT pawn is pawnPcsq[sq] and the value of a DARK pawn is pawnPcsq[flip[sq]]
        private static readonly int[] flip = 
{
	56,  57,  58,  59,  60,  61,  62,  63,
	48,  49,  50,  51,  52,  53,  54,  55,
	40,  41,  42,  43,  44,  45,  46,  47,
	32,  33,  34,  35,  36,  37,  38,  39,
	24,  25,  26,  27,  28,  29,  30,  31,
	16,  17,  18,  19,  20,  21,  22,  23,
	 8,   9,  10,  11,  12,  13,  14,  15,
	 0,   1,   2,   3,   4,   5,   6,   7
};

        // pawnRank[x, y] is the rank of the least advanced pawn of color x on file y - 1
        // there are "buffer files" on the left and right to avoid special-case logic later 
        // if there's no pawn on a rank, we pretend the pawn is impossibly far advanced (0 for LIGHT and 7 for DARK), 
        // this makes it easy to test for pawns on a rank and it simplifies some pawn evaluation code
        private int[,] pawnRank = new int[2, 10];

        private int[] pieceMat = new int[2];// the value of a side's pieces
        private int[] pawnMat = new int[2];// the value of a side's pawns

        /// <summary>
        /// Returns the board score.
        /// </summary>
        private int Evaluate()
        {
            int[] score = new int[2];// each side's score

            // this is the first pass: set up pawnRank, pieceMat, and pawnMat
            for (int i = 0; i < 10; i++)
            {
                pawnRank[LIGHT, i] = 0;
                pawnRank[DARK, i] = 7;
            }
            pieceMat[LIGHT] = 0;
            pieceMat[DARK] = 0;
            pawnMat[LIGHT] = 0;
            pawnMat[DARK] = 0;
            for (int i = 0; i < SQUARE_NO; i++)
            {
                if (color[i] == EMPTY) continue;

                if (piece[i] == PAWN)
                {
                    pawnMat[color[i]] += pieceValue[PAWN];
                    int file = (i & 7) + 1;// add 1 because of the extra file in the array

                    if (color[i] == LIGHT)
                    {
                        if (pawnRank[LIGHT, file] < (i >> 3))
                            pawnRank[LIGHT, file] = (i >> 3);
                    }
                    else
                    {
                        if (pawnRank[DARK, file] > (i >> 3))
                            pawnRank[DARK, file] = (i >> 3);
                    }
                }
                else
                {
                    pieceMat[color[i]] += pieceValue[piece[i]];
                }
            }

            // this is the second pass: evaluate each piece
            score[LIGHT] = pieceMat[LIGHT] + pawnMat[LIGHT];
            score[DARK] = pieceMat[DARK] + pawnMat[DARK];
            for (int i = 0; i < SQUARE_NO; ++i)
            {
                if (color[i] == EMPTY) continue;

                if (color[i] == LIGHT)
                {
                    switch (piece[i])
                    {
                        case PAWN:
                            score[LIGHT] += EvalLightPawn(i);

                            break;
                        case KNIGHT:
                            score[LIGHT] += knightPcSq[i];

                            break;
                        case BISHOP:
                            score[LIGHT] += bishopPcSq[i];

                            break;
                        case ROOK:
                            if (pawnRank[LIGHT, (i & 7) + 1] == 0)
                                score[LIGHT] += pawnRank[DARK, (i & 7) + 1] == 7 ? ROOK_OPEN_FILE_BONUS : ROOK_SEMI_OPEN_FILE_BONUS;

                            if ((i >> 3) == 1)
                                score[LIGHT] += ROOK_ON_SEVENTH_BONUS;

                            break;
                        case KING:
                            score[LIGHT] += pieceMat[DARK] <= 1200 ? kingEndgamePcSq[i] : EvalLightKing(i);

                            break;
                    }
                }
                else
                {
                    switch (piece[i])
                    {
                        case PAWN:
                            score[DARK] += EvalDarkPawn(i);

                            break;
                        case KNIGHT:
                            score[DARK] += knightPcSq[flip[i]];

                            break;
                        case BISHOP:
                            score[DARK] += bishopPcSq[flip[i]];

                            break;
                        case ROOK:
                            if (pawnRank[DARK, (i & 7) + 1] == 7)
                                score[DARK] += pawnRank[LIGHT, (i & 7) + 1] == 0 ? ROOK_OPEN_FILE_BONUS : ROOK_SEMI_OPEN_FILE_BONUS;

                            if ((i >> 3) == 6)
                                score[DARK] += ROOK_ON_SEVENTH_BONUS;

                            break;
                        case KING:
                            score[DARK] += pieceMat[LIGHT] <= 1200 ? kingEndgamePcSq[flip[i]] : EvalDarkKing(i);

                            break;
                    }
                }
            }

            // the score[] array is set, return the score relative to the side to move
            return side == LIGHT ? score[LIGHT] - score[DARK] : score[DARK] - score[LIGHT];
        }

        private int EvalLightPawn(int square)
        {
            int ret = pawnPcSq[square];// return value
            int file = (square & 7) + 1;// pawn's file

            // if there's a pawn behind this one, it's doubled
            if (pawnRank[LIGHT, file] > (square >> 3))
                ret -= DOUBLED_PAWN_PENALTY;

            // if there aren't any friendly pawns on either side of this one, it's isolated
            if (pawnRank[LIGHT, file - 1] == 0 && pawnRank[LIGHT, file + 1] == 0)
                ret -= ISOLATED_PAWN_PENALTY;
            // if it's not isolated, it might be backwards
            else if (pawnRank[LIGHT, file - 1] < (square >> 3) && pawnRank[LIGHT, file + 1] < (square >> 3))
                ret -= BACKWARDS_PAWN_PENALTY;

            // add a bonus if the pawn is passed
            if (pawnRank[DARK, file - 1] >= (square >> 3) && pawnRank[DARK, file] >= (square >> 3) && pawnRank[DARK, file + 1] >= (square >> 3))
                ret += (7 - (square >> 3)) * PASSED_PAWN_BONUS;

            return ret;
        }

        private int EvalDarkPawn(int square)
        {
            int ret = pawnPcSq[flip[square]];// return value
            int file = (square & 7) + 1;// pawn's file

            // if there's a pawn behind this one, it's doubled
            if (pawnRank[DARK, file] < (square >> 3))
                ret -= DOUBLED_PAWN_PENALTY;

            // if there aren't any friendly pawns on either side of this one, it's isolated
            if (pawnRank[DARK, file - 1] == 7 && pawnRank[DARK, file + 1] == 7)
                ret -= ISOLATED_PAWN_PENALTY;
            // if it's not isolated, it might be backwards
            else if (pawnRank[DARK, file - 1] > (square >> 3) && pawnRank[DARK, file + 1] > (square >> 3))
                ret -= BACKWARDS_PAWN_PENALTY;

            // add a bonus if the pawn is passed
            if (pawnRank[LIGHT, file - 1] <= (square >> 3) && pawnRank[LIGHT, file] <= (square >> 3) && pawnRank[LIGHT, file + 1] <= (square >> 3))
                ret += (square >> 3) * PASSED_PAWN_BONUS;

            return ret;
        }

        private int EvalLightKing(int square)
        {
            int ret = kingPcSq[square];// return value

            // if the king is castled, use a special function to evaluate the pawns on the appropriate side
            if ((square & 7) < 3)
            {
                ret += EvalLightKingPawn(1);
                ret += EvalLightKingPawn(2);
                ret += EvalLightKingPawn(3) / 2;// problems with pawns on the 'c' file are not as severe
            }
            else if ((square & 7) > 4)
            {
                ret += EvalLightKingPawn(8);
                ret += EvalLightKingPawn(7);
                ret += EvalLightKingPawn(6) / 2;// problems with pawns on the 'f' file are not as severe
            }
            // otherwise, just assess a penalty if there are open files near the king
            else
            {
                for (int i = (square & 7); i <= (square & 7) + 2; i++)
                {
                    if (pawnRank[LIGHT, i] == 0 && pawnRank[DARK, i] == 7)
                        ret -= 10;
                }
            }

            // scale the king safety value according to the opponent's material
            // the idea is that your king safety can only be bad if the opponent has enough pieces to attack you
            ret *= pieceMat[DARK];
            ret /= 3100;

            return ret;
        }

        private int EvalLightKingPawn(int file)
        {
            int ret = 0;// return value

            if (pawnRank[LIGHT, file] == 6)
                ret = 0;// pawn hasn't moved
            else if (pawnRank[LIGHT, file] == 5)
                ret -= 10;// pawn moved one square
            else if (pawnRank[LIGHT, file] != 0)
                ret -= 20;// pawn moved more than one square
            else
                ret -= 25;// no pawn on this file

            if (pawnRank[DARK, file] == 7)
                ret -= 15;// no enemy pawn
            else if (pawnRank[DARK, file] == 5)
                ret -= 10;// enemy pawn on the 3rd rank
            else if (pawnRank[DARK, file] == 4)
                ret -= 5;// enemy pawn on the 4th rank

            return ret;
        }

        private int EvalDarkKing(int square)
        {
            int ret = kingPcSq[flip[square]];// return value

            if ((square & 7) < 3)
            {
                ret += EvalDarkKingPawn(1);
                ret += EvalDarkKingPawn(2);
                ret += EvalDarkKingPawn(3) / 2;// problems with pawns on the 'c' file are not as severe
            }
            else if ((square & 7) > 4)
            {
                ret += EvalDarkKingPawn(8);
                ret += EvalDarkKingPawn(7);
                ret += EvalDarkKingPawn(6) / 2;// problems with pawns on the 'f' file are not as severe
            }
            // otherwise, just assess a penalty if there are open files near the king
            else
            {
                for (int i = (square & 7); i <= (square & 7) + 2; ++i)
                    if (pawnRank[LIGHT, i] == 0 && pawnRank[DARK, i] == 7)
                        ret -= 10;
            }

            // scale the king safety value according to the opponent's material
            // the idea is that your king safety can only be bad if the opponent has enough pieces to attack you
            ret *= pieceMat[LIGHT];
            ret /= 3100;

            return ret;
        }

        private int EvalDarkKingPawn(int file)
        {
            int ret = 0;// return value

            if (pawnRank[DARK, file] == 1)
                ret = 0;// pawn hasn't moved
            else if (pawnRank[DARK, file] == 2)
                ret -= 10;// pawn moved one square
            else if (pawnRank[DARK, file] != 7)
                ret -= 20;// pawn moved more than one square
            else
                ret -= 25;// no pawn on this file

            if (pawnRank[LIGHT, file] == 0)
                ret -= 15;// no enemy pawn
            else if (pawnRank[LIGHT, file] == 2)
                ret -= 10;// enemy pawn on the 6th rank
            else if (pawnRank[LIGHT, file] == 3)
                ret -= 5;// enemy pawn on the 5th rank

            return ret;
        }
    }
}