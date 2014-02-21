using System;
using System.Globalization;
using System.Collections.Generic;

//using Valil.Chess.Engine.Properties;

namespace Valil.Chess.Engine
{
    public sealed partial class ChessEngine
    {

        // Just deep=3 for now...
        private const int Settings_Default_ChessEngineMaxDepth = 10;
        private const int Settings_Default_OpeningBookMaxMoveNo = 5;

        #region Piece and color constants
        internal const int LIGHT = 0;
        internal const int DARK = 1;

        internal const int PAWN = 0;
        internal const int KNIGHT = 1;
        internal const int BISHOP = 2;
        internal const int ROOK = 3;
        internal const int QUEEN = 4;
        internal const int KING = 5;

        internal const int EMPTY = 6;
        #endregion

        #region Size constants
        private const int SQUARE_NO = 64;
        private const int PIECE_TYPE_NO = 12;
        private const int MAX_PLY = 32;
        private const int MAX_MOV = MAX_PLY * 40;
        #endregion

        # region Frequently used squares constants
        private const int A1 = 56;
        private const int B1 = 57;
        private const int C1 = 58;
        private const int D1 = 59;
        private const int E1 = 60;
        private const int F1 = 61;
        private const int G1 = 62;
        private const int H1 = 63;

        private const int A8 = 0;
        private const int B8 = 1;
        private const int C8 = 2;
        private const int D8 = 3;
        private const int E8 = 4;
        private const int F8 = 5;
        private const int G8 = 6;
        private const int H8 = 7;
        #endregion

        // the mailbox arrays used to validate moves
        private static readonly int[] mailbox = 
{
	-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	-1,  0,  1,  2,  3,  4,  5,  6,  7, -1,
	-1,  8,  9, 10, 11, 12, 13, 14, 15, -1,
	-1, 16, 17, 18, 19, 20, 21, 22, 23, -1,
	-1, 24, 25, 26, 27, 28, 29, 30, 31, -1,
	-1, 32, 33, 34, 35, 36, 37, 38, 39, -1,
	-1, 40, 41, 42, 43, 44, 45, 46, 47, -1,
	-1, 48, 49, 50, 51, 52, 53, 54, 55, -1,
	-1, 56, 57, 58, 59, 60, 61, 62, 63, -1,
	-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
	-1, -1, -1, -1, -1, -1, -1, -1, -1, -1
};
        private static readonly int[] mailbox64 = 
{
	21, 22, 23, 24, 25, 26, 27, 28,
	31, 32, 33, 34, 35, 36, 37, 38,
	41, 42, 43, 44, 45, 46, 47, 48,
	51, 52, 53, 54, 55, 56, 57, 58,
	61, 62, 63, 64, 65, 66, 67, 68,
	71, 72, 73, 74, 75, 76, 77, 78,
	81, 82, 83, 84, 85, 86, 87, 88,
	91, 92, 93, 94, 95, 96, 97, 98
};

        // slide, offsets, and offset are basically the vectors that pieces can move in
        // if slide for the piece is false, it can only move one square in any one direction
        // offsets is the number of directions it can move in, and offset is an array of the actual directions
        private static readonly bool[] slide = { false, false, true, true, true, false };
        private static readonly int[] offsets = { 0, 8, 4, 4, 8, 8 };
        private static readonly int[,] offset = 
{
	{   0,   0,   0,   0,   0,   0,   0,   0},
	{ -21, -19, -12,  -8,   8,  12,  19,  21},
	{ -11,  -9,   9,  11,   0,   0,   0,   0},
	{ -10,  -1,   1,  10,   0,   0,   0,   0},
	{ -11, -10,  -9,  -1,   1,   9,  10,  11},
	{ -11, -10,  -9,  -1,   1,   9,  10,  11}
};

        // used to determine the castling permissions after a move
        // by logical-AND the castle bits with the castle_mask bits for both of the move's squares
        private static readonly int[] castleMask = 
{
	 7, 15, 15, 15,  3, 15, 15, 11,
	15, 15, 15, 15, 15, 15, 15, 15,
	15, 15, 15, 15, 15, 15, 15, 15,
	15, 15, 15, 15, 15, 15, 15, 15,
	15, 15, 15, 15, 15, 15, 15, 15,
	15, 15, 15, 15, 15, 15, 15, 15,
	15, 15, 15, 15, 15, 15, 15, 15,
	13, 15, 15, 15, 12, 15, 15, 14
};

        /// <summary>
        /// Array containing 789 random 64-bit integers.
        /// Needed for computing Zobrist hash.
        /// </summary>
        private static long[] zobristKeys = new long[] {
													// 12 * 64 numbers - each random number coresponds to a piece type and to a square
                                                    /*P*/-2111900683,519304448,-202047450,-184539456,2539693,650161574,-1062361500,-1381604251,-1503369984,1684340966,1694557753,15088056,-432424708,968424536,-1191421914,-61331893,1478904707,642483003,1266891628,-2093257655,996952354,1816732395,1227025202,585839264,-349003737,849356640,-1608032001,660668318,1627364994,-6389024,-1635590096,-2099236781,-533703894,810756690,1395282464,710025443,1377887183,551800651,-472953922,-817119559,1270790562,-1095130423,-1180513946,-1563859260,-916011883,1724159246,-996864388,-1794212705,243048371,2090840890,-1615644048,-1288015702,980462098,1890193946,-1441654040,303753277,451427759,-398610629,1034894093,-1355084325,990763962,232503960,-608528271,-1164414641,
                                                    /*N*/-1737404494,1901048422,1337091659,-1301918815,1716232459,1268845351,-1593104505,187139969,663191874,-2021571925,-2126337109,1118546746,-1414841844,-1422259141,973880064,205193401,989903106,12124740,-1191033824,38019266,1142997513,549587367,-1039554681,161974261,-1484262098,-2013974941,-181509336,778250365,1663597853,679288249,2099100049,498700672,-1181646831,-1853877859,-2146329161,295548701,-1648943669,-1222784220,499852490,-886781373,617235225,-901572295,1125726650,423213644,968510670,-1169371444,1288621118,-825475351,-868292343,1055459790,-385233193,164550614,-824715528,-673777525,-688354309,-125043824,-1946447852,-74443604,-1877693430,346819247,-1408585790,179290662,-1346230703,-1037676226,
                                                    /*B*/-1523639428,790330424,461125740,2084072648,946653422,1825107554,-923901263,-295521946,1655793234,-1318694327,1716668833,1380557308,1235352831,-1577255140,-50389833,-14895270,481778227,-1218825452,1513297099,857000795,348871471,-883216528,1529835751,795928566,1894249991,-403306716,-167304122,119817782,608581349,1178002806,921007643,-445244649,1981486857,454494515,386478980,154371278,864341722,-2066818343,-824518239,-623271611,-643742289,-1589268624,1169125458,-1351593265,1884475214,1389317828,-816921476,1321499814,-998463833,2091296569,-1498990225,-1489408235,963581263,1863667667,357553116,1339284571,-740533352,-597977036,1536701658,-1741366722,886718064,-633442295,1047529887,1879678924,
													/*R*/-1846786095,-330837598,1204920864,-777903953,-1574916205,548377369,-1349314304,-1827077998,419467990,9623074,-1831460280,-702396241,575188764,1219435542,-1357113601,471269267,385848300,-7082977,-1813241946,-333470202,530974350,-1509519771,109995384,-1905952583,1702410705,2025443718,-1177450951,-779732566,-2043041139,967478767,-1433538768,-1913704248,-282015657,818436091,-933758069,1476103159,-74713247,-1946721950,-144612737,1633844997,1652491520,2131034191,83906399,5201887,1331683266,1608499887,-540889247,-1028693739,-1352592059,1628784085,356898154,1171614351,-714436751,1787785717,-1888357098,1911887469,-183079563,376272241,1836413340,1970379906,1906082507,-1669149905,-2100613208,-886069177,
													/*Q*/799557496,-1471711161,1199064891,2017934329,1195112933,1006233042,-102378796,-439167878,-757826842,-730142986,2061956843,-420025512,-152348499,-346509956,1487764510,-1384374778,2082342463,503725830,104793652,1057371315,104117052,884161772,-1287852976,1022120044,-330273623,1349298678,1823078003,-1443466431,-160218829,1933652887,1093900141,865561892,-1754454793,1831139166,620191463,-144775346,1592217088,-414318556,1308632215,2398164,613930118,-1747679504,-729354158,-2031070659,-263045692,1379779634,1036268201,-1003312839,849951215,-1455820865,972013510,-272644521,-1077520596,-967365412,1462557846,752654012,-594100993,-1765998659,-1124090583,-4380317,-1121360911,694415746,1676771870,-243130637,
													/*K*/642858642,1363055108,1049756713,-1845221036,69817368,693377054,1410866818,404652567,511842119,-2112403615,390554048,1197588573,1639996816,-1067610057,1569732516,-1875401676,933508151,-1540081673,876083026,938955467,-145568842,1389082256,-877227937,-1232052246,-1872762221,1609208630,-359451058,-1825157491,911117767,1317914421,-1916324498,-952799594,896439991,1855371176,-1766348557,-1213664320,-1460420380,-205462284,-1058736989,-453729501,-190635078,-1557939474,599453341,-1158767295,-291683862,-1656624586,1105868414,-365527485,914244465,2118349142,1131501227,1901505452,1454091390,-1414758806,-1401001433,2120886234,1780996689,668619175,-632182908,1369932861,-1484505753,-2076350555,1030202671,1738878747,
													/*p*/-217559146,139433638,1335273213,-1767441062,-1493345644,-44395509,1519651641,-1811203589,188349367,972797911,-71837788,-1210604430,-677088650,-1536002441,1920366509,1987554595,2007835562,-1390171576,598362121,-1438119596,1208571047,156542910,1420279544,-1480656893,-1091042408,-133982117,60316499,-1738845420,1532171366,1393845970,342282776,1725044962,-770121007,417518013,-489570932,-776106772,-1114837992,-1930684363,-333957791,406151664,895611031,1643157486,-258478395,-1745959609,-289061079,-985191985,1193922474,701475387,-810927324,-1438964590,992252653,613608888,-1829914408,-306653062,-1193772303,-663031390,2062656252,-240976689,-1560490093,-53505275,-812448444,-1828371408,88355048,1144055856,
													/*n*/820523255,-399443969,821559281,-134221462,-955654,-244647338,1794791046,-94992804,1451646026,-2040772056,1548363799,1244141481,672639445,397006132,-1445645302,-718009791,873087397,172074494,1101397565,-1510064656,-29495119,1039184132,-256834452,-1325110230,74197724,1814748374,719115840,-589938504,-700401636,1085807678,-1206108469,473877321,1053509920,-884400069,1226849239,540792755,1003992021,-676080237,-1277848722,-711758272,-1821491072,1849721023,1082179365,-2134956590,-1088040448,634519610,-771736964,3833070,981265959,2095982485,-299395696,664113241,-1785702040,-1873188661,1500039975,1758144409,-886597274,664364793,-1721304599,1727654258,-102141217,-378347738,1927226916,-551148520,
													/*b*/-269382897,-242544770,-1961918864,259944622,2121313813,1890456891,-1374340329,356194113,991379934,390192755,1105097702,-562829729,1944477555,-429952213,1601383394,1932255984,736292965,-487561929,-261802201,1698113299,925307659,655559609,319535497,196708668,-1182188458,-1992534392,1012303938,1451770446,-2008920467,1112436101,1315800508,1837481064,-2051249955,-1133978232,1759348843,-578262055,-2006197848,1809426605,-643256911,-1465011806,-1380867426,-1314742628,-1566663593,-1633921271,-1672017445,1460263893,165401952,-606773238,-715126079,1611317759,180486011,-1040221270,-8672574,2074788601,-1430062730,-1023838510,-109653468,1993483427,-769350694,614718191,-1545933012,-621859662,-282283323,749913452,
													/*r*/161467475,-1613999294,-866958621,1396892537,1122204076,-478565298,2041335414,-1404143977,1316394911,1989648198,-1751169516,-1622797229,1175737287,341034838,1405572853,-950602254,1458958906,-168674566,-231015921,989466435,-99663071,256057674,1126255353,558561564,1257839815,-115554350,482857609,-942503595,-762751518,-1990860264,1440880764,-501711765,410807095,2087401361,1798803851,932285293,-1853133369,-1955739715,1841806833,-943853270,-1108268374,-248862118,715807322,-1436919201,1515872143,1516212218,1603271192,-1879435169,-99065890,408935987,1608397701,-567048720,864415930,-2047821293,-256240646,-1173095847,335173951,-94814363,1497326870,1063589397,1695946190,370527859,365851641,-831260367,
													/*q*/1945710928,-114208738,827334195,1344156451,506667947,857975707,598449120,-1415847800,-1679783934,-527957333,-2013091043,44768616,-1424136109,493376391,1750304703,1401405227,-2017514653,-1087675451,727958794,1673857540,-989199277,168055758,72601091,1406010116,-838662948,50650331,81582934,-589605173,-615068886,1456155333,-886389280,717611142,-975141157,-528032999,-2032461384,-619071340,431527150,-1198199068,-1796283167,-286989887,-454966951,-507422243,-1051075179,1507693949,-577405608,-1786947407,2102964679,1488045853,-1312350780,-954350392,499435770,-993461733,-923133142,-98883018,455751287,708212577,913793383,2002872172,1634168061,1735196137,1828579660,-35042061,-380833016,1290995791,
                                                    /*k*/639899663,605556640,403677186,262144767,-1610416265,50296705,-8945340,2004960479,-2126192876,1155470465,-552304159,344056275,-2115906563,-506200696,-738359287,-41416243,-2012623396,164486148,-841218828,-603655008,83140609,-190840531,-1610535658,19732167,756467597,382176557,-947049014,-1926378878,768246471,-897399028,-2100884344,-955479948,210269431,-2005600456,1962358994,-147270991,953332206,-760091074,-1309786548,-297906957,1045230444,1291021417,-210998797,1818882826,1777535585,-217423387,174187787,1642400618,-452236702,191521509,1784866168,1659205682,-445107511,2016594212,852042909,-920347373,614273842,-1659686348,322057242,842275329,874119607,436320239,28831729,-1209011829,
													
												    // corresponds to white turn
													-1295684460,

												    // for each castling capability
													-982739954,1821642494,-1810956674,251559471,

													// 16 numbers - match the squares that could be en passant targets
													-25284796,2117026932,793015511,1148508094,1960296086,-675375397,-1097409633,-1763991610,-610286023,-1614399106,-969310630,964581922,2119836358,1512228354,583402009,-972940984
												};

        private int[] color = new int[SQUARE_NO];// LIGHT, DARK, or EMPTY
        private int[] piece = new int[SQUARE_NO];// PAWN, KNIGHT, BISHOP, ROOK, QUEEN, KING, or EMPTY
        private int side;// the side to move
        private int xside;// the side not to move
        private int castle;// a bitfield with the castling permissions: 1 is white kingside, 2 is white queenside, 4 is black kingside and 8 is black queenside
        private int ep;// the en passant target square
        private int fifty;// the number of moves since a capture or pawn move, used to handle the fifty-move-draw rule
        private int ply;// the number of half-moves (ply) since the root of the search tree
        private int moves;// the number of moves

        private int[,] heuristicHistory = new int[SQUARE_NO, SQUARE_NO];// the history heuristic array (used for move ordering)
        private HistoryMove[] history = new HistoryMove[MAX_PLY];// history of moves

        private Move repMove;// a possible repetition move

		private 	OBookMem obookmem = new OBookMem();
										
												
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="withOpeningBook">True to load the opening book, false otherwise</param>
        public ChessEngine(bool withOpeningBook)
        {
           if (withOpeningBook) { InitializeOpeningBook(); }
        }

        /// <summary>
        /// Returns true if this "side" is in check, false otherwise.
        /// </summary>
        private bool InCheck(int side)
        {
            for (int i = 0; i < SQUARE_NO; i++)
                if (piece[i] == KING && color[i] == side)
                    return Attacks(i, side ^ 1);

            return true;
        }

        /// <summary>
        /// Returns true if a "square" is attacked by this "side", false otherwise.
        /// </summary>
        private bool Attacks(int square, int side)
        {
            for (int i = 0; i < SQUARE_NO; ++i)
            {
                if (color[i] == side)
                {
                    if (piece[i] == PAWN)
                    {

                        // a pawn attacks the front and side squares
                        if (side == LIGHT)
                        {
                            if ((i & 7) != 0 && i - 9 == square) return true;

                            if ((i & 7) != 7 && i - 7 == square) return true;
                        }
                        else
                        {
                            if ((i & 7) != 0 && i + 7 == square) return true;

                            if ((i & 7) != 7 && i + 9 == square) return true;
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

                                // if the next square is the square we're looking for, return true  
                                if (k == square) return true;

                                // if the next square is not empty, stop the iteration
                                if (color[k] != EMPTY) break;

                                // if the piece cannot slide, stop after the first iteration
                                if (!slide[piece[i]]) break;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to make a pseudo-legal move.
        /// Returns true if the move has been actually made, false otherwise.
        /// </summary>
        private bool Make(Move move)
        {
            // if this is a castling move, move the rook
            // the king will be moved with the usual move code later
            if ((move.bits & 2) != 0)
            {
                if (InCheck(side)) return false;

                int from, to;

                switch (move.to)
                {
                    case G1:
                        if (color[F1] != EMPTY || color[G1] != EMPTY || Attacks(F1, xside) || Attacks(G1, xside))
                            return false;

                        from = H1;
                        to = F1;

                        break;
                    case C1:
                        if (color[B1] != EMPTY || color[C1] != EMPTY || color[D1] != EMPTY || Attacks(C1, xside) || Attacks(D1, xside))
                            return false;

                        from = A1;
                        to = D1;

                        break;
                    case G8:
                        if (color[F8] != EMPTY || color[G8] != EMPTY || Attacks(F8, xside) || Attacks(G8, xside))
                            return false;

                        from = H8;
                        to = F8;

                        break;
                    case C8:
                        if (color[B8] != EMPTY || color[C8] != EMPTY || color[D8] != EMPTY || Attacks(C8, xside) || Attacks(D8, xside))
                            return false;

                        from = A8;
                        to = D8;

                        break;
                    default:

                        from = -1;
                        to = -1;

                        break;
                }

                // move the rook
                color[to] = color[from];
                piece[to] = piece[from];
                color[from] = EMPTY;
                piece[from] = EMPTY;
            }

            // back up information so the move can be taken back
            history[ply].move = move;
            history[ply].capture = piece[move.to];
            history[ply].castle = castle;
            history[ply].ep = ep;
            history[ply].fifty = fifty;
            ply++;

            // update the castle, en passant, and fifty-move-draw variables
            castle &= castleMask[move.from] & castleMask[move.to];
            ep = (move.bits & 8) != 0 ? (side == LIGHT ? move.to + 8 : move.to - 8) : -1;
            fifty = (move.bits & 17) != 0 ? 0 : fifty + 1;

            // move the piece
            color[move.to] = side;
            piece[move.to] = (move.bits & 32) != 0 ? move.promote : piece[move.from];
            color[move.from] = EMPTY;
            piece[move.from] = EMPTY;

            // erase the pawn if this is an en passant move
            if ((move.bits & 4) != 0)
            {
                color[side == LIGHT ? move.to + 8 : move.to - 8] = EMPTY;
                piece[side == LIGHT ? move.to + 8 : move.to - 8] = EMPTY;
            }

            // switch sides
            side ^= 1;
            xside ^= 1;

            // test for legality 
            // if we can capture the opponent's king, it's an illegal position and the move must be taken back
            if (InCheck(xside))
            {
                TakeBack();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Takes back the last move.
        /// </summary>
        private void TakeBack()
        {
            // switch sides
            side ^= 1;
            xside ^= 1;

            // restore the board status
            ply--;
            Move move = history[ply].move;
            castle = history[ply].castle;
            ep = history[ply].ep;
            fifty = history[ply].fifty;

            // move back the piece and put the capture back, if any
            color[move.from] = side;
            piece[move.from] = (move.bits & 32) != 0 ? PAWN : piece[move.to];
            color[move.to] = history[ply].capture == EMPTY ? EMPTY : xside;
            piece[move.to] = history[ply].capture == EMPTY ? EMPTY : history[ply].capture;

            // move back the rook if this was a castle move
            if ((move.bits & 2) != 0)
            {
                int from, to;

                switch (move.to)
                {
                    case G1:
                        from = F1;
                        to = H1;

                        break;
                    case C1:
                        from = D1;
                        to = A1;

                        break;
                    case G8:
                        from = F8;
                        to = H8;

                        break;
                    case C8:
                        from = D8;
                        to = A8;

                        break;
                    default:
                        from = -1;
                        to = -1;

                        break;
                }

                // move back the rook
                color[to] = side;
                piece[to] = ROOK;
                color[from] = EMPTY;
                piece[from] = EMPTY;
            }

            // put back pawn if this was an en passant move
            if ((move.bits & 4) != 0)
            {
                color[side == LIGHT ? move.to + 8 : move.to - 8] = xside;
                piece[side == LIGHT ? move.to + 8 : move.to - 8] = PAWN;
            }
        }

        /// <summary>
        /// Gets the next move.
        /// </summary>
        /// <param name="fen">The board configuration.</param>
        /// <param name="repetitionMoveCandidate">
        /// The move that, if made, it will result a draw by repetition. 
        /// The engine will try to avoid the move if there is a better one.
        /// </param>
        /// <param name="depth">Search depth level.</param>
        /// <returns></returns>
        public string GetNextMove(string fen, string repetitionMoveCandidate, int depth)
        {

            // the engine is still thinking, return
            if (thinking) { return null; }

            // initialize the engine
            try
            {
                Initialize(fen);
            }
            catch
            {
                return null;
            }

           int hash;

            // look in the opening book (OBookMem.cs) in memory buffer...
			
			short obMove=0;
			if (moves <= Settings_Default_OpeningBookMaxMoveNo) obMove=obookmem.OBookGet(hash = GetHashCode());
            if(obMove>0)
				{
		        Move move = new Move();
                move.from = (byte)(obMove>> 8);
                move.to = (byte)(obMove & 255);

                return move.ToString();		
				}
				
			// look in the opening book
			
           if (book != null && moves <= Settings_Default_OpeningBookMaxMoveNo && book.ContainsKey(hash = GetHashCode()))
           {
                // get the list of available moves for this board
                List<short> list = book[hash];

                //return a random move from the list
                short shortMove = list[random.Next(list.Count)];

                Move move = new Move();
                move.from = (byte)(shortMove >> 8);
                move.to = (byte)(shortMove & 255);

                return move.ToString();
            }
            else
            {
                // set repetition move
                // a repetition move cannot be a promotion move, a castling move, a capture move or a pawn move
                // so the move bits and capture are 0
                repMove = Move.ParseRegularCAN(repetitionMoveCandidate);

                // the depth must be between 1 and ChessEngineMaxDepth
                if (depth < 1)
                    depth = 1;
                else if (depth > Settings_Default_ChessEngineMaxDepth)
                    depth = Settings_Default_ChessEngineMaxDepth;
//                else if (depth > Settings.Default.ChessEngineMaxDepth)
//                    depth = Settings.Default.ChessEngineMaxDepth;

            
            
            // search
                try
                {
                    thinking = true;
                    Think(depth);
                }
                catch
                {
                    // the search has ended abruptly or something went wrong
                }
                finally
                {
                    thinking = false;
                }

                // pv[0, 0] is the best move
                return pv[0, 0].ToString();
           }
        }

        /// <summary>
        /// Gives a hint for the best next move.
        /// </summary>
        /// 
// Not used... Sorry...
//      public string GetHintMove(string fen, string repetitionMoveCandidate)
//      {
//
//      return GetNextMove(fen, repetitionMoveCandidate, Settings.Default.ChessEngineHintDepth);
//      }

        /// <summary>
        /// Parses a FEN string.
        /// </summary>
        /// <param name="fen">The FEN string</param>
        /// <returns></returns>
        private void ParseFEN(string fen)
        {
            int i, j;

            // empty the board
            for (i = 0; i < SQUARE_NO; i++)
            {
                color[i] = EMPTY;
                piece[i] = EMPTY;
            }

            i = 0; j = 0;

            while (fen[i] != ' ')
            {
                // if it's a number, skip
                if (fen[i] >= '1' && fen[i] <= '8')
                {
                    j += fen[i] - '0';
                }
                else
                {
                    // set the piece according to the char
                    switch (fen[i])
                    {
                        case 'P':
                        case 'p':
                            piece[j] = PAWN;
                            break;
                        case 'R':
                        case 'r':
                            piece[j] = ROOK;
                            break;
                        case 'Q':
                        case 'q':
                            piece[j] = QUEEN;
                            break;
                        case 'K':
                        case 'k':
                            piece[j] = KING;
                            break;
                        case 'N':
                        case 'n':
                            piece[j] = KNIGHT;
                            break;
                        case 'B':
                        case 'b':
                            piece[j] = BISHOP;
                            break;
                    }

                    // set the piece color
                    if (piece[j] != EMPTY)
                    {
                        color[j++] = Char.IsUpper(fen[i]) ? LIGHT : DARK;
                    }
                }

                i++;
            }

            // set the side to move
            if (fen[++i] == 'w')
            {
                side = LIGHT;
                xside = DARK;
            }
            else if (fen[i] == 'b')
            {
                xside = LIGHT;
                side = DARK;
            }

            i += 2;

            // set castling availability
            castle = 0;
            if (fen[i] == '-')
            {
                i++;
            }
            else
            {
                do
                {
                    switch (fen[i])
                    {
                        case 'K':
                            castle += 1;
                            break;
                        case 'Q':
                            castle += 2;
                            break;
                        case 'k':
                            castle += 4;
                            break;
                        case 'q':
                            castle += 8;
                            break;
                    }
                }
                while (fen[++i] != ' ');
            }

            // set en passant target
            if (fen[++i] == '-')
            {
                ep = -1;
                i++;
            }
            else
            {
                ep = (('8' - fen[i + 1]) << 3) + fen[i] - 'a';
                i += 2;
            }

            fifty = Int32.Parse(fen.Substring(++i, fen.IndexOf(' ', i) - i), CultureInfo.InvariantCulture);

            i = fen.IndexOf(' ', i) + 1;

            moves = Int32.Parse(fen.Substring(i), CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Initialize the engine with this board configuration.
        /// </summary>
        private void Initialize(string fen)
        {
            // parse FEN string
            ParseFEN(fen);

            // reset the ply
            ply = 0;

            // reset the heuristic history
            for (int i = 0; i < heuristicHistory.Length; i++)
                heuristicHistory[i % SQUARE_NO, i / SQUARE_NO] = 0;

            // reset the pv
            int x, y;
            for (int i = 0; i < pv.Length; i++)
            {
                x = i % MAX_PLY;
                y = i / MAX_PLY;

                pv[x, y] = Move.Empty;
            }
            for (int i = 0; i < pvLength.Length; i++)
            {
                pvLength[i] = 0;
            }
        }

        /// <summary>
        /// Computes the board hash using Zobrist keys.
        /// </summary>
        public override int GetHashCode()
        {
            long hash = 0;

            // loop through the squares
            for (int i = 0; i < SQUARE_NO; i++)
            {
                if (piece[i] != EMPTY)
                {
                    // XOR the Zobrist key which coresponds to type of piece and for this square
                    // to find the key index, offset the key index of the type by the square number
                    hash ^= zobristKeys[(color[i] == LIGHT ? piece[i] * SQUARE_NO : (piece[i] + (PIECE_TYPE_NO >> 1)) * SQUARE_NO) + i];
                }
            }

            // if White is to move, XOR the corresponding Zobrist key
            if (side == LIGHT)
            {
                hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO];
            }

            // if the Kings could castle, the corresponding Zobrist keys
            if ((castle & 1) == 1)
            {
                hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO + 1];
            }
            if ((castle & 2) == 2)
            {
                hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO + 2];
            }
            if ((castle & 4) == 4)
            {
                hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO + 3];
            }
            if ((castle & 8) == 8)
            {
                hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO + 4];
            }

            // if there is an en passant target, XOR the corresponding Zobrist key
            if (ep > -1)
            {
                // the en passant targets have rank 2 or 5
                if ((ep >> 3) == 2)
                {
                    hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO + 5 + (ep & 7)];
                }
                else if ((ep >> 3) == 5)
                {
                    hash ^= zobristKeys[PIECE_TYPE_NO * SQUARE_NO + 13 + (ep & 7)];
                }

            }

            // XOR the first 4 bytes with the last 4 bytes to return an 32-bit integer
            return (int)((hash & 0xFFFFFFFF) ^ (hash >> 32));
        }
    }
}
