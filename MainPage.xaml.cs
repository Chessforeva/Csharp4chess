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

// chess engines - resources
// Valil: http://www.valil.com/
// GarboChess: http://forwardcoding.com/projects/chess/chess.html
// Lousy: http://ocmp.phys.rug.nl/Misc/Chess/EngineInfo.html
// OliThink: http://home.arcor.de/dreamlike/ (java sources)
// CuckooChess: http://web.comhem.se/petero2home/javachess/ (java sources)

using Valil.Chess.Engine;       // Valil chess is a source folder
using GarboChess;               // GarboChess is a .cs file
using LousyChess;               // Lousy chess is a source folder
using OliThink;                 // Olithink is a .cs file
using Cuckoo;                   // Cuckoo is a source folder

namespace cs_chess
{
    public partial class MainPage : UserControl
    {
        const String pc_C = "PNBRQKpnbrqk";
        const String pc_htms = "♙♘♗♖♕♔♟♞♝♜♛♚";
        const String StartFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private static int[] pc_sizp = { 7, 8, 9, 8, 9, 9 };
        private static int[] pc_mrg1 = { 7, 5, 4, 6, 5, 4 };
        private static int[] pc_mrg2 = { 0, 3, 4, 2, 5, 5 };
        private TextBlock[] tb= new TextBlock[64];
        private Rectangle[] rt= new Rectangle[64];
        private c0_chess C0 = new c0_chess();       // Library for independant chess logic
        private string moveslist;
        private string pgntext;
        private bool gameover;

        // Valil chess engine
        private Valil.Chess.Engine.ChessEngine ValilE;

        // GarboChess
        private GarboChess.Searcher GarboS;
        private GarboChess.Position GarboP;

        // LousyChess
        private LousyChess.Engine LousyE;

        // OliThink
        private OliThink.OliThink OliE;

        // Cuckoo
        private Cuckoo.Cuckoo Cuck;

        private DispatcherTimer timer;          // Timer for automatic move
        private Random rand = new Random();     // Random seed

        public MainPage()
        {
         InitializeComponent();
         System.Windows.Threading.DispatcherTimer waitForRefreshBoardTimer = new System.Windows.Threading.DispatcherTimer();
         InitDspOnce();
         StartNewGame();
         StartTimerOnce();
        }

        //======================================================================
        // This sets starting positions on all chess engines and prepares them
        //======================================================================
        private void StartNewGame()
        {
            C0.c0_set_start_position("");
            moveslist = "";
            pgntext = "";
            gameover = false;

            //Valil
            ValilE = new Valil.Chess.Engine.ChessEngine(true);

            //Garbo
            GarboP = new GarboChess.Position();
            GarboP.ResetBoard();
            GarboS = new GarboChess.Searcher(GarboP);
            GarboS.AllottedTime = 2000;

            //Lousy
            LousyE = new LousyChess.Engine();
            LousyE.SetupInitialBoard(true);
            LousyE.SetAllowedSearchTime(5, 8);

            //OliThink
            OliE = new OliThink.OliThink();
            OliE.MainInit();

            //Cuckoo
            Cuck = new Cuckoo.Cuckoo();

            DispBoard();
        }

        //======================================================================
        // This sends requests or position changes to chess engines
        //======================================================================
       private void DoNextMove()
       {
        bool q = true;      // let's call engine if no other things to do
        String Answ = "", From, To, Promo;
        String last, lprm;
        int l = moveslist.Length;
        ListBox LB = (C0.c0_sidemoves > 0 ? LBW : LBB);
        int eng = LB.SelectedIndex;

        if (l< C0.c0_moveslist.Length)    // there was a new move
        {
            last = C0.c0_D_last_move_was();
            if (last.Length > 4)
              {
                lprm = last.Substring(5, 1).ToLower();
                last = last.Substring(0, 4) + ((("qrbn").IndexOf(lprm) >= 0) ? lprm : "");
               }
            // Garbochess position changes
            ushort lastmv = GarboChess.MoveHelper.GetMoveFromUCIString(GarboP, last);
            GarboP.MakeMove(lastmv);

            // Lousy position changes
            LousyE.MakeMove(last);

            moveslist = C0.c0_moveslist;
            pgntext = C0.c0_put_to_PGN(moveslist);
            PgnText.Text = pgntext;
            q = false;
        }
            
        if (q && (!gameover))              // Start engine
            {
                string opening = C0.c0_Opening(C0.c0_moveslist);    // this contains opening data
                string curFEN = C0.c0_get_FEN();

                if (eng == 0)           // Valil chess
                {
                    // ignoring repetitions in this sample by giving only position FEN
                    Answ = ValilE.GetNextMove(curFEN, null, 5);
                }
                if (eng == 1)           // GarboChess
                {
                    ushort bestmv = GarboS.Search();
                    if (bestmv > 0)
                    {
                        Answ = GarboChess.MoveHelper.GetUCIString(bestmv);
                    }
                }
                if (eng == 2)           // LousyChess
                {
                    // Let's use small opening book of chess library
                    if (opening.Length > 0) Answ = opening.Substring(0, 4);
                    else
                    {
                        LousyE.Think();
                        LousyChess.EngineResults engineResults = LousyE.GetCurrentEngineResults();
                        LousyChess.Move currentBestMove = engineResults.GetBestMove();
                        Answ = currentBestMove.ToLANString();
                    }
                }
               if (eng == 3)           // OliThink chess
                {
                    // ignoring repetitions in this sample by giving only position FEN
                    OliE.do_inp("setboard " + curFEN);
                    OliE.do_inp("go");
                    Answ = OliE.bestmove;
                }

               if (eng == 4)           // Cuckoo chess
               {
                   // ignoring repetitions in this sample by giving only position FEN
                   Answ = Cuckoo.Cuckoo.simplyCalculateMove(curFEN);
               }
 
             }

        if (Answ.Length > 0)
        {
            From = Answ.Substring(0, 2);
            To = Answ.Substring(2, 2);
            Promo = (Answ.Length > 4 ? Answ.Substring(4, 1) : "");
            C0.c0_become_from_engine = ((Promo.Length > 0) ? Promo.ToUpper() : "Q" );
            C0.c0_become = C0.c0_become_from_engine;

            C0.c0_move_to(From,To);
            C0.c0_sidemoves = -C0.c0_sidemoves;
            DispBoard();            // moving and asserting game status
            gameover = (C0.c0_D_is_mate_to_king("w") || C0.c0_D_is_mate_to_king("b")
                || C0.c0_D_is_pate_to_king((C0.c0_sidemoves>0?"w":"b")) );

        }

       }

        private void StartTimerOnce()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        // ===================================================================
        // Very minimal code to show Chess board in Silverlight UserControl
        // Uses HTML unicode symbols for chess pieces
        // ===================================================================

        private void InitDspOnce()
        {
            CreaRowCols();
            DrawSquares();
            AddListBox(0); // black
            AddListBox(1); // white
        }

        // Displays current board
        private void DispBoard()
        {
           int x, y, i;
           string at, pc;
           for (int n = 0; n < 64; n++)
           {
               y = (int)(n / 8);
               x = (n % 8);
               at = "" + (char)(97 + x) + (char)(49 + y);
               i = C0.c0_position.IndexOf(at);
               pc = "*";
               if (i >= 0)
               {
                   pc = "" + C0.c0_position[i-1];
                   pc = ((C0.c0_position[i-2] == 'w') ? pc.ToUpper() : pc.ToLower());
               }
               AddPiece(pc, at);
           }
        }

        // Puts a piece on square
        private void AddPiece(String pc, String at)
        {
            int p = pc_C.IndexOf(pc);
            int x = (int)(at[0]) - 97;
            int y = (int)(at[1]) - 49;
            int n = (y * 8) + x;
            int p2 = (Math.Max(p, 0) % 6);
            if(tb[n]==null)
            {
                tb[n] = new TextBlock();
                tb[n].Name = "Piece_" + at;
                tb[n].SetValue(Grid.RowProperty, 7 - y);
                tb[n].SetValue(Grid.ColumnProperty, x);
                Gridza.Children.Add(tb[n]);
            }
            tb[n].FontSize = (int)((32 * pc_sizp[p2]) / 9);
            tb[n].Margin = new Thickness(pc_mrg1[p2], -pc_mrg2[p2], 0, 0);
            tb[n].Text = "" + (p<0? "" : ""+pc_htms[p]);
        }

        // Add Listbox for white and black side engine
        private void AddListBox(int white)
        {
            int itemcnt, selidx;
            List<string> itemz = new List<string>();
            ListBox LB = (white > 0 ? LBW : LBB);
            itemz.Add("Valil C#");
            itemz.Add("Garbochess C#");
            itemz.Add("Lousy C#");
            itemz.Add("OliThink C#");
            itemz.Add("Cuckoo C#");
            LB.ItemsSource = itemz;
            itemcnt = itemz.Count();
            selidx = rand.Next(itemcnt);
            LB.SelectedIndex = selidx;
        }


        // Create Rows and columns for Gridza grid-board, predefined in main xaml
        private void CreaRowCols()
        {
            for (int i = 0; i < 8; i++) { AddCol(); AddRow(); }
        }

        // Add single col
        private void AddCol()
        {
            ColumnDefinition Cd = new ColumnDefinition();
            Cd.Width = new GridLength(40);  // 8x40 = 320 = width of grid
            Gridza.ColumnDefinitions.Add(Cd);
        }

        // Add single row
        private void AddRow()
        {
            RowDefinition Rd = new RowDefinition();
            Rd.Height = new GridLength(40);  // 8x40 = 320 = height of grid
            Gridza.RowDefinitions.Add(Rd);
        }

        // Draws 8x8 board of squares
        private void DrawSquares()
        {
            for(int n=0;n<64;n++) AddSquare(n);
        }

        // Puts a rectangle as a white or black square
        private void AddSquare(int n)
        {
            Color wSq = Color.FromArgb(0xFF, 0xEE, 0xEE, 0xEE);
            Color bSq = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC);
            int y = (int)(n / 8);
            int x = (n % 8);
            rt[n] = new Rectangle();
            rt[n].SetValue(Grid.RowProperty, 7 - y);
            rt[n].SetValue(Grid.ColumnProperty, x);
            rt[n].Width = 40;
            rt[n].Height = 40;
            rt[n].Name = "sq_" + n.ToString();
            rt[n].Fill = new SolidColorBrush((x + y) % 2 == 0 ? bSq : wSq);
            Gridza.Children.Add(rt[n]);
        }

        // Events and timers
        private void OnTimerTick(object sender, EventArgs e)
        {
           DoNextMove();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) { }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

    }
}
