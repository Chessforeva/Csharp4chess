using System;
using System.ComponentModel;
using System.Collections.Generic;
//using Valil.Chess.Model.Properties;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements a chess game.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Mean history length.
        /// </summary>
        public const int MeanHistoryLength = 50;
        /// <summary>
        /// Maximum possible moves number.
        /// </summary>
        public const int MaxPossibleMoves = 50;

        /// <summary>
        /// Current board configuration.
        /// </summary>
        private Board currentBoard;
        /// <summary>
        /// Current board index.
        /// </summary>
        private int currentBoardIndex;
        /// <summary>
        /// Move history.
        /// </summary>
        private List<Move> moveHistory;
        /// <summary>
        /// Status of the game.
        /// </summary>
        private GameStatus status;
        /// <summary>
        /// List of the possible moves of the current board.
        /// </summary>
        private List<Move> possibleMoves;
        /// <summary>
        /// Contains pairs consisting of a board hash and the frequency of that board in the history of the game.
        /// </summary>
        private Dictionary<int, int> historyHashes;

        /// <summary>
        /// Promotion delegate.
        /// </summary>
        private PromotionHandler promote;

        /// <summary>
        /// Moving event.
        /// </summary>
        public event EventHandler<CancelMoveEventArgs> Moving;
        /// <summary>
        /// Moved event.
        /// </summary>
        public event EventHandler<MoveEventArgs> Moved;
        /// <summary>
        /// Going forward event.
        /// </summary>
        public event EventHandler<CancelMoveEventArgs> GoingForward;
        /// <summary>
        /// Gone forward event.
        /// </summary>
        public event EventHandler<MoveEventArgs> GoneForward;
        /// <summary>
        /// Going back event.
        /// </summary>
        public event EventHandler<CancelMoveEventArgs> GoingBack;
        /// <summary>
        /// Gone back event.
        /// </summary>
        public event EventHandler<MoveEventArgs> GoneBack;
        /// <summary>
        /// Modifying event.
        /// </summary>
        public event EventHandler<CancelEventArgs> Modifying;
        /// <summary>
        /// Modified event.
        /// </summary>
        public event EventHandler Modified;
        /// <summary>
        /// Loading event.
        /// </summary>
        public event EventHandler<CancelEventArgs> Loading;
        /// <summary>
        /// Board configuration loaded event.
        /// </summary>
        public event EventHandler BoardConfigurationLoaded;
        /// <summary>
        /// Game board configuration loaded event.
        /// </summary>
        public event EventHandler GameBoardConfigurationLoaded;
        /// <summary>
        /// Game move section loaded event.
        /// </summary>
        public event EventHandler GameMoveSectionLoaded;

        /// <summary>
        /// Raises the Moving event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMoving(CancelMoveEventArgs e)
        {
            if (Moving != null) { Moving(this, e); }
        }
        /// <summary>
        /// Raises the Moved event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMoved(MoveEventArgs e)
        {
            if (Moved != null) { Moved(this, e); }
        }
        /// <summary>
        /// Raises the GoingForward event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGoingForward(CancelMoveEventArgs e)
        {
            if (GoingForward != null) { GoingForward(this, e); }
        }
        /// <summary>
        /// Raises the GoneForward event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGoneForward(MoveEventArgs e)
        {
            if (GoneForward != null) { GoneForward(this, e); }
        }
        /// <summary>
        /// Raises the GoingBack event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGoingBack(CancelMoveEventArgs e)
        {
            if (GoingBack != null) { GoingBack(this, e); }
        }
        /// <summary>
        /// Raises the GoneBack event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGoneBack(MoveEventArgs e)
        {
            if (GoneBack != null) { GoneBack(this, e); }
        }

        /// <summary>
        /// Raises the Modifying event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnModifying(CancelEventArgs e)
        {
            if (Modifying != null) { Modifying(this, e); }
        }
        /// <summary>
        /// Raises the Modified event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnModified(EventArgs e)
        {
            if (Modified != null) { Modified(this, e); }
        }

        /// <summary>
        /// Raises the Loading event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnLoading(CancelEventArgs e)
        {
            if (Loading != null) { Loading(this, e); }
        }
        /// <summary>
        /// Raises the BoardConfigurationLoaded event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnBoardConfigurationLoaded(EventArgs e)
        {
            if (BoardConfigurationLoaded != null) { BoardConfigurationLoaded(this, e); }
        }
        /// <summary>
        /// Raises the GameBoardConfigurationLoaded event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnGameBoardConfigurationLoaded(EventArgs e)
        {
            if (GameBoardConfigurationLoaded != null) { GameBoardConfigurationLoaded(this, e); }
        }
        /// <summary>
        /// Raises the GameMoveSectionLoaded event.
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnGameMoveSectionLoaded(EventArgs e)
        {
            if (GameMoveSectionLoaded != null) { GameMoveSectionLoaded(this, e); }
        }

        /// <summary>
        /// Promotion delegate.
        /// </summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //[Browsable(false)]
        public PromotionHandler Promote
        {
            get { return promote; }
            set { promote = value; }
        }

        /// <summary>
        /// True if the current board configuration is the last in history, false otherwise.
        /// </summary>
        public bool IsLast
        {
            get { return currentBoardIndex == moveHistory.Count; }
        }

        /// <summary>
        /// True if the current board configuration is the first in history, false otherwise.
        /// </summary>
        public bool IsFirst
        {
            get { return currentBoardIndex == 0; }
        }

        /// <summary>
        /// True if the game is ended, false otherwise.
        /// </summary>
        public bool IsEnded
        {
            get { return status != GameStatus.Normal && status != GameStatus.Check; }
        }

        /// <summary>
        /// True if the game is initialized, false otherwise.
        /// </summary>
        public bool IsInitialized
        {
            get { return currentBoard != null; }
        }

        /// <summary>
        /// Gets or sets the current board configuration.
        /// </summary>
        public Board CurrentBoard
        {
            get { return currentBoard; }
            internal set
            {
                moveHistory.Clear();
                historyHashes.Clear();

                currentBoard = value;
                currentBoardIndex = 0;

                GenerateMoves();
                AddHistoryHash();
                SetStatus();
            }
        }

        /// <summary>
        /// Gets the list of possible moves.
        /// </summary>
        internal List<Move> PossibleMoves
        {
            get { return possibleMoves; }
        }

        /// <summary>
        /// Gets the game status.
        /// </summary>
        public GameStatus Status
        {
            get { return status; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game()
        {
            moveHistory = new List<Move>(MeanHistoryLength);
            possibleMoves = new List<Move>(MaxPossibleMoves);
            historyHashes = new Dictionary<int, int>(MeanHistoryLength);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">The container for the component.</param>
        public Game(IContainer container)
            : this()
        {
            // Do nothing................
            //container.Add(this);
        }

        /// <summary>
        /// Gets the valid move based on starting square and ending square. 
        /// If the move is a promotion move, it will set the promotion type.
        /// </summary>
        /// <param name="from">The starting square.</param>
        /// <param name="to">The ending square.</param>
        /// <param name="promotionType">the promotion type.</param>
        /// <returns></returns>
        public Move GetMove(int from, int to, Type promotionType)
        {
            // loop through possible moves until a move with the same starting and ending square is found
            foreach (Move move in PossibleMoves)
            {
                if (move.From == from && move.To == to)
                {
                    if (move is PromotionMove)
                    {
                        (move as PromotionMove).PromotionType = promotionType;
                    }

                    return move;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a move which if it is made, it will probably end the game in a draw by repetition.
        /// </summary>
        /// <returns></returns>
        public Move RepetitiveMoveCandidate
        {
            get
            {
                // usually a game which ends with draw by repetition 
                // has the last moves in the history like "move1, move2, move3, move4, move1, move2, move3, move4"
                // so in order to find the repetition move 
                // we check if the last moves are like "move1, move2, move3, move4, move1, move2, move3" and return "move4"
                return
                    currentBoardIndex >= 7 &&
                    moveHistory[currentBoardIndex - 1].From == moveHistory[currentBoardIndex - 5].From &&
                    moveHistory[currentBoardIndex - 1].To == moveHistory[currentBoardIndex - 5].To &&
                    moveHistory[currentBoardIndex - 2].From == moveHistory[currentBoardIndex - 6].From &&
                    moveHistory[currentBoardIndex - 2].To == moveHistory[currentBoardIndex - 6].To &&
                    moveHistory[currentBoardIndex - 3].From == moveHistory[currentBoardIndex - 7].From &&
                    moveHistory[currentBoardIndex - 3].To == moveHistory[currentBoardIndex - 7].To
                    ?
                    GetMove(moveHistory[currentBoardIndex - 4].From, moveHistory[currentBoardIndex - 4].To, null)
                    :
                    null;
            }
        }

        /// <summary>
        /// Makes a move.
        /// If the move is illegal or it's null, throws an Argument(Null)Exception.
        /// </summary>
        /// <param name="move">The move</param>
        public void Make(Move move)
        {
            // check to see if the move it's not null and it's valid
            if (move == null) { throw new ArgumentNullException("move", "Resources.NullMoveMsg"); }
            bool findMove = false;
            foreach (Move m in PossibleMoves)
            {
                if (move.From == m.From && move.To == m.To)
                {
                    findMove = true;
                    break;
                }
            }
            if (!findMove) { throw new ArgumentException("Resources.IllegalMoveMsg", "move"); }

            // build the event args
            CancelMoveEventArgs moveEventArgs = new CancelMoveEventArgs(move, currentBoardIndex - 1);

            // raise the Moving event
            OnMoving(moveEventArgs);

            // if the move was cancelled
            if (moveEventArgs.Cancel) { return; }

            // remove the moves after current board index (if any)
            if (!IsLast)
            {
                moveHistory.RemoveRange(currentBoardIndex, moveHistory.Count - currentBoardIndex);
            }

            // if this is a promotion, the promotion is not set and the promotion delegate is not null, call the promotion delegate
            if (move is PromotionMove && (move as PromotionMove).PromotionType == null && Promote != null)
            {
                (move as PromotionMove).PromotionType = Promote();
            }

            // make the move
            move.Make(currentBoard);

            // add the current hash to the history
            AddHistoryHash();

            // generate the possible moves
            GenerateMoves();

            // set the status of the game
            SetStatus();

            // increment the current board index
            currentBoardIndex++;

            // add move to history
            moveHistory.Add(move);

            // raise the Moved event
            OnMoved(new MoveEventArgs(move, currentBoardIndex - 1));
        }

        /// <summary>
        /// Go to the next board configuration in history.
        /// </summary>
        public void Next()
        {
            // if the game is not the at the end
            if (!IsLast)
            {
                Move move = moveHistory[currentBoardIndex];

                // build the event args
                CancelMoveEventArgs moveEventArgs = new CancelMoveEventArgs(move, currentBoardIndex - 1);

                // raise the GoingForward event
                OnGoingForward(moveEventArgs);

                // if the operation was cancelled
                if (moveEventArgs.Cancel) { return; }

                // make the move
                move.Make(currentBoard);

                // add the current board hash to the history
                AddHistoryHash();

                // generate the possible moves
                GenerateMoves();

                // set the status of the game
                SetStatus();

                // increment the current board index
                currentBoardIndex++;

                // raise the GoneForward event
                OnGoneForward(new MoveEventArgs(move, currentBoardIndex - 1));
            }
        }

        /// <summary>
        /// Go to the previous board configuration in history.
        /// </summary>
        public void Previous()
        {
            // if the game is not at the begining
            if (!IsFirst)
            {
                Move move = moveHistory[currentBoardIndex - 1];

                // build the event args
                CancelMoveEventArgs moveEventArgs = new CancelMoveEventArgs(move, currentBoardIndex - 1);

                // raise the GoingBack event
                OnGoingBack(moveEventArgs);

                // if the operation was cancelled
                if (moveEventArgs.Cancel) { return; }

                // remove the current board hash from the history
                RemoveHistoryHash();

                // take back the move
                move.TakeBack(currentBoard);

                // generate the possible moves
                GenerateMoves();

                // set the status of the game
                SetStatus();

                // decrement the current board index
                currentBoardIndex--;

                // raise the GoneBack event
                OnGoneBack(new MoveEventArgs(move, currentBoardIndex - 1));
            }
        }

        /// <summary>
        /// Go to the last board configuration in history.
        /// </summary>
        public void GoToLast()
        {
            // build the event args
            CancelEventArgs emptyArgs = new CancelEventArgs();

            // raise the Modified event
            OnModifying(emptyArgs);

            // if the operation was cancelled
            if (emptyArgs.Cancel) { return; }

            // go to the last board configuration step by step
            while (!IsLast)
            {
                // make the move and increment the current board index
                moveHistory[currentBoardIndex++].Make(currentBoard);

                // add the current board hash to history
                AddHistoryHash();

                // generate the possible moves
                GenerateMoves();

                // set the game status
                SetStatus();
            }

            // raise the Modified event
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// Go to the first board configuration in history.
        /// </summary>
        public void GoToFirst()
        {
            // build the event args
            CancelEventArgs emptyArgs = new CancelEventArgs();

            // raise the Modified event
            OnModifying(emptyArgs);

            // if the operation was cancelled
            if (emptyArgs.Cancel) { return; }

            // go to the first board configuration step by step
            while (!IsFirst)
            {
                // remove the board hash from history
                RemoveHistoryHash();

                // take back the move and decrement the current board index
                moveHistory[--currentBoardIndex].TakeBack(currentBoard);

                // generate the possible moves
                GenerateMoves();

                // set the game status
                SetStatus();
            }

            // raise the Modified event
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// Generates the possible moves of the current board.
        /// </summary>
        private void GenerateMoves()
        {
            // clear the list
            possibleMoves.Clear();

            // loop the starting squares through all the squares 
            for (int fromIndex = 0; fromIndex < Board.SquareNo; fromIndex++)
            {
                // if it's a side to move piece on this square
                if (currentBoard.IsSideToMovePiece(fromIndex))
                {
                    // loop the ending squares through all the squares 
                    for (int toIndex = 0; toIndex < Board.SquareNo; toIndex++)
                    {
                        // try to generate the move
                        Move move = currentBoard[fromIndex].GenerateMove(currentBoard, fromIndex, toIndex);
                        if (move != null) { possibleMoves.Add(move); }
                    }
                }
            }
        }

        /// <summary>
        /// Set the game status.
        /// </summary>
        private void SetStatus()
        {
            // if there are no moves it's checkmate or stalemate
            if (possibleMoves.Count == 0)
            {
                status = IsCheck() ? GameStatus.Checkmate : GameStatus.Stalemate;
                return;
            }

            // if it's draw by insufficient material
            if (IsDrawInsufficientMaterial())
            {
                status = GameStatus.DrawInsufficientMaterial;
                return;
            }

            // if it's draw by 50-move rule
            if (currentBoard.Status.Ply >= 100)
            {
                status = GameStatus.Draw50Move;
                return;
            }

            // if it's draw by repetition
            if (historyHashes.ContainsValue(3))
            {
                status = GameStatus.DrawRepetition;
                return;
            }

            // if it's check or normal status
            status = IsCheck() ? GameStatus.Check : GameStatus.Normal;
        }

        /// <summary>
        /// Adds the current board hash to history.
        /// </summary>
        private void AddHistoryHash()
        {
            int hash = Utils.GetHash(currentBoard);

            // if the hash exists increment the frequency, otherwise add the hash with frequency 1
            historyHashes[hash] = historyHashes.ContainsKey(hash) ? historyHashes[hash] + 1 : 1;
        }

        /// <summary>
        /// Removes the current board hash from history.
        /// </summary>
        private void RemoveHistoryHash()
        {
            int hash = currentBoard.GetHashCode();
            int freq = historyHashes[hash];

            // if the frequency is more than 1 decrement it, otherwise remove the hash
            if (freq > 1)
            {
                historyHashes[hash] = freq - 1;
            }
            else
            {
                historyHashes.Remove(hash);
            }
        }

        /// <summary>
        /// Gets the captured pieces.
        /// Returns a dictionary with (piece type, frequency) pairs
        /// </summary>
        /// <returns></returns>
        public Dictionary<Type, int> GetCapturedPieces()
        {
            Piece capture;
            Dictionary<Type, int> captures = new Dictionary<Type, int>(Piece.TypesNo - 2);

            // loop through the move history and get the captured pieces from each move until the current board is reached
            for (int moveIndex = 0; moveIndex < currentBoardIndex; moveIndex++)
            {
                if ((capture = moveHistory[moveIndex].Capture) != null)
                {
                    Type captureType = capture.GetType();

                    // if the capture type exists increment the frequency, otherwise add the type with frequency 1
                    captures[captureType] = captures.ContainsKey(captureType) ? captures[captureType] + 1 : 1;
                }
            }

            return captures;
        }

        /// <summary>
        /// Verifies the current board for check.
        /// </summary>
        /// <returns></returns>
        private bool IsCheck()
        {
            return currentBoard.Status.WhiteTurn ? currentBoard.WhiteKingInCheck() : currentBoard.BlackKingInCheck();
        }

        /// <summary>
        /// Verifies the current board for draw by insufficient material.
        /// </summary>
        /// <returns></returns>
        private bool IsDrawInsufficientMaterial()
        {
            // "N" is true if there is one White Knight
            // "B" is true if there is one White Bishop
            // "BW" is true if the White Bishop is on a white square
            // "n" is true if there is one Black Knight
            // "b" is true if there is one Black Bishop
            // "bw" is true if the Black Bishop is on a white square
            bool N, B, BW, n, b, bw;
            N = B = BW = n = b = bw = false;

            // loop through the squares
            for (int sqIndex = 0; sqIndex < Board.SquareNo; sqIndex++)
            {
                if (currentBoard[sqIndex] != null)
                {
                    if (currentBoard[sqIndex] is WhiteKnight)
                    {
                        // if there is more than one White Knight there is no draw by insufficient material
                        if (N) { return false; }

                        N = true;

                        continue;
                    }

                    if (currentBoard[sqIndex] is WhiteBishop)
                    {
                        // if there is more than one White Bishop there is no draw by insufficient material
                        if (B) { return false; }

                        B = true;

                        // remember the Bishop square colour
                        BW = Board.IsWhiteSquare(sqIndex);

                        continue;
                    }

                    if (currentBoard[sqIndex] is BlackKnight)
                    {
                        // if there is more than one Black Knight there is no draw by insufficient material
                        if (n) { return false; }

                        n = true;

                        continue;
                    }

                    if (currentBoard[sqIndex] is BlackBishop)
                    {
                        // if there are more than one Black Bishop there is no draw by insufficient material
                        if (b) { return false; }

                        b = true;

                        // remember the Bishop square colour
                        bw = Board.IsWhiteSquare(sqIndex);

                        continue;
                    }

                    // there will be always the King
                    if (currentBoard[sqIndex] is IKing) { continue; }

                    // if there is a piece which is not Bishop, Knight or King there is no draw by insufficient material
                    return false;
                }
            }

            // if there are only the kings
            if (!B && !b && !N && !n)
            {
                return true;
            }

            // if there are only the kings and one bishop on one side
            if ((b && !B && !N && !n) || (B && !b && !n && !N))
            {
                return true;
            }

            // if there are only the kings and one knight on one side
            if ((n && !N && !B && !b) || (N && !n && !b && !B))
            {
                return true;
            }

            // if there are only the kings and one bishop on each side, both of them on squares with the same colour
            if (B && b && !N && !n && (bw == BW))
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Game status enumeration.
    /// </summary>
    public enum GameStatus { Normal, Check, Checkmate, Stalemate, Draw50Move, DrawInsufficientMaterial, DrawRepetition };

    /// <summary>
    /// Promotion delegate.
    /// </summary>
    public delegate Type PromotionHandler();
}

