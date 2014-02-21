using System;

namespace Valil.Chess.Engine
{
    public sealed partial class ChessEngine
    {
        /// <summary>
        /// Generates pseudo-legal moves for the current position.
        /// </summary>
        private void GeneratePlyMoves()
        {
            // initialize the ply move index
            moveIndexList[ply + 1] = moveIndexList[ply];

            // scan the board to find friendly pieces and then determines what squares they attack
            // when a piece/square combination is found, AddPlyMove is called to put the move in the move list

            for (int i = 0; i < SQUARE_NO; i++)
            {
                if (color[i] == side)
                {
                    if (piece[i] == PAWN)
                    {
                        // a pawn can move to:
                        // the front and side squares if they are occupied by opposite pieces,
                        // the front square,
                        // the second front square if it moves for the first time
                        if (side == LIGHT)
                        {
                            if ((i & 7) != 0 && color[i - 9] == DARK) AddPlyMove(i, i - 9, 17);

                            if ((i & 7) != 7 && color[i - 7] == DARK) AddPlyMove(i, i - 7, 17);

                            if (color[i - 8] == EMPTY)
                            {
                                AddPlyMove(i, i - 8, 16);

                                if (i >= 48 && color[i - 16] == EMPTY) AddPlyMove(i, i - 16, 24);
                            }
                        }
                        else
                        {
                            if ((i & 7) != 0 && color[i + 7] == LIGHT) AddPlyMove(i, i + 7, 17);

                            if ((i & 7) != 7 && color[i + 9] == LIGHT) AddPlyMove(i, i + 9, 17);

                            if (color[i + 8] == EMPTY)
                            {
                                AddPlyMove(i, i + 8, 16);

                                if (i <= 15 && color[i + 16] == EMPTY) AddPlyMove(i, i + 16, 24);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < offsets[piece[i]]; j++)
                        {
                            for (int k = i; ; )
                            {
                                // get the next square where the piece can move
                                k = mailbox[mailbox64[k] + offset[piece[i], j]];

                                // if the next square is outside the board, stop the iteration
                                if (k == -1) break;

                                // if the next square is not empty, stop the iteration
                                if (color[k] != EMPTY)
                                {
                                    // if the next square contains an opposite piece, add the move
                                    if (color[k] == xside) AddPlyMove(i, k, 1);

                                    break;
                                }

                                // the next square is empty, add the move
                                AddPlyMove(i, k, 0);

                                // if the piece cannot slide, stop after the first iteration
                                if (!slide[piece[i]]) break;
                            }
                        }
                    }
                }
            }

            // generate castle moves
            if (side == LIGHT)
            {
                if ((castle & 1) != 0) AddPlyMove(E1, G1, 2);

                if ((castle & 2) != 0) AddPlyMove(E1, C1, 2);

            }
            else
            {
                if ((castle & 4) != 0) AddPlyMove(E8, G8, 2);

                if ((castle & 8) != 0) AddPlyMove(E8, C8, 2);
            }

            // generate en passant moves
            if (ep != -1)
            {
                if (side == LIGHT)
                {
                    if ((ep & 7) != 0 && color[ep + 7] == LIGHT && piece[ep + 7] == PAWN)
                        AddPlyMove(ep + 7, ep, 21);

                    if ((ep & 7) != 7 && color[ep + 9] == LIGHT && piece[ep + 9] == PAWN)
                        AddPlyMove(ep + 9, ep, 21);
                }
                else
                {
                    if ((ep & 7) != 0 && color[ep - 9] == DARK && piece[ep - 9] == PAWN)
                        AddPlyMove(ep - 9, ep, 21);

                    if ((ep & 7) != 7 && color[ep - 7] == DARK && piece[ep - 7] == PAWN)
                        AddPlyMove(ep - 7, ep, 21);
                }
            }
        }

        /// <summary>
        /// Generate only capture and promotion moves.
        /// Used by quiescence search.
        /// </summary>
        private void GeneratePlyCaptureMoves()
        {
            moveIndexList[ply + 1] = moveIndexList[ply];

            for (int i = 0; i < SQUARE_NO; i++)
            {
                if (color[i] == side)
                {
                    if (piece[i] == PAWN)
                    {
                        // a pawn capture the opposite pieces from the front and side squares
                        if (side == LIGHT)
                        {
                            if ((i & 7) != 0 && color[i - 9] == DARK) AddPlyMove(i, i - 9, 17);

                            if ((i & 7) != 7 && color[i - 7] == DARK) AddPlyMove(i, i - 7, 17);

                            if (i <= 15 && color[i - 8] == EMPTY) AddPlyMove(i, i - 8, 16);
                        }
                        if (side == DARK)
                        {
                            if ((i & 7) != 0 && color[i + 7] == LIGHT) AddPlyMove(i, i + 7, 17);

                            if ((i & 7) != 7 && color[i + 9] == LIGHT) AddPlyMove(i, i + 9, 17);

                            if (i >= 48 && color[i + 8] == EMPTY) AddPlyMove(i, i + 8, 16);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < offsets[piece[i]]; ++j)
                        {
                            for (int k = i; ; )
                            {
                                // get the next square where the piece can move
                                k = mailbox[mailbox64[k] + offset[piece[i], j]];

                                // if the next square is outside the board, stop the iteration
                                if (k == -1) break;

                                // if the next square is not empty, stop the iteration
                                if (color[k] != EMPTY)
                                {
                                    // if the next square contains an opposite piece, add the move
                                    if (color[k] == xside) AddPlyMove(i, k, 1);

                                    break;
                                }

                                // if the piece cannot slide, stop after the first iteration
                                if (!slide[piece[i]]) break;
                            }
                        }
                    }
                }
            }

            // generate en passant captures
            if (ep != -1)
            {
                if (side == LIGHT)
                {
                    if ((ep & 7) != 0 && color[ep + 7] == LIGHT && piece[ep + 7] == PAWN)
                        AddPlyMove(ep + 7, ep, 21);

                    if ((ep & 7) != 7 && color[ep + 9] == LIGHT && piece[ep + 9] == PAWN)
                        AddPlyMove(ep + 9, ep, 21);
                }
                else
                {
                    if ((ep & 7) != 0 && color[ep - 9] == DARK && piece[ep - 9] == PAWN)
                        AddPlyMove(ep - 9, ep, 21);

                    if ((ep & 7) != 7 && color[ep - 7] == DARK && piece[ep - 7] == PAWN)
                        AddPlyMove(ep - 7, ep, 21);
                }
            }
        }

        /// <summary>
        /// Adds a move to the move list, unless it's a pawn promotion that needs to be handled by AddPlyPromotionMove.
        /// </summary>
        private void AddPlyMove(int from, int to, int bits)
        {
            // if this is a promotion move, call AddPlyPromotionMove
            if ((bits & 16) != 0)
            {
                if (side == LIGHT)
                {
                    if (to <= H8)
                    {
                        AddPlyPromotionMove(from, to, bits);
                        return;
                    }
                }
                else
                {
                    if (to >= A1)
                    {
                        AddPlyPromotionMove(from, to, bits);
                        return;
                    }
                }
            }

            int moveIndex = moveIndexList[ply + 1]++;

            moveList[moveIndex].move.from = (byte)from;
            moveList[moveIndex].move.to = (byte)to;
            moveList[moveIndex].move.promote = 0;
            moveList[moveIndex].move.bits = (byte)bits;

            // assign a score to the move for alpha-beta move ordering
            // if the move is a capture, it uses MVV/LVA (Most Valuable Victim/Least Valuable Attacker), otherwise it uses the move's history heuristic value
            // note that 1,000,000 is added to a capture move's score, so it always gets ordered above a "normal" move
            moveList[moveIndex].score = color[to] != EMPTY ? 1000000 + (piece[to] * 10) - piece[from] : heuristicHistory[from, to];
        }

        /// <summary>
        /// Adds promotion moves to the move list.
        /// </summary>
        private void AddPlyPromotionMove(int from, int to, int bits)
        {
            int moveIndex;

            // add 4 moves to the move list, one for each possible promotion piece
            for (int i = KNIGHT; i <= QUEEN; i++)
            {
                moveIndex = moveIndexList[ply + 1]++;
                moveList[moveIndex].move.from = (byte)from;
                moveList[moveIndex].move.to = (byte)to;
                moveList[moveIndex].move.promote = (byte)i;
                moveList[moveIndex].move.bits = (byte)(bits | 32);
                moveList[moveIndex].score = 1000000 + (i * 10);
            }
        }
    }
}