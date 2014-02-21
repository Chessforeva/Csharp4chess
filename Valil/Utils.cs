using System;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Collections.Generic;
//using Valil.Chess.Model.Properties;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Implements some utilities methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Array containing 789 random 64-bit integers.
        /// Needed for computing Zobrist hash.
        /// </summary>
        private static long[] zobristKeys = new long[] {
													// 12 * 64 numbers - each random number coresponds to a piece type and to a square
													/*R*/-1846786095,-330837598,1204920864,-777903953,-1574916205,548377369,-1349314304,-1827077998,419467990,9623074,-1831460280,-702396241,575188764,1219435542,-1357113601,471269267,385848300,-7082977,-1813241946,-333470202,530974350,-1509519771,109995384,-1905952583,1702410705,2025443718,-1177450951,-779732566,-2043041139,967478767,-1433538768,-1913704248,-282015657,818436091,-933758069,1476103159,-74713247,-1946721950,-144612737,1633844997,1652491520,2131034191,83906399,5201887,1331683266,1608499887,-540889247,-1028693739,-1352592059,1628784085,356898154,1171614351,-714436751,1787785717,-1888357098,1911887469,-183079563,376272241,1836413340,1970379906,1906082507,-1669149905,-2100613208,-886069177,
													/*Q*/799557496,-1471711161,1199064891,2017934329,1195112933,1006233042,-102378796,-439167878,-757826842,-730142986,2061956843,-420025512,-152348499,-346509956,1487764510,-1384374778,2082342463,503725830,104793652,1057371315,104117052,884161772,-1287852976,1022120044,-330273623,1349298678,1823078003,-1443466431,-160218829,1933652887,1093900141,865561892,-1754454793,1831139166,620191463,-144775346,1592217088,-414318556,1308632215,2398164,613930118,-1747679504,-729354158,-2031070659,-263045692,1379779634,1036268201,-1003312839,849951215,-1455820865,972013510,-272644521,-1077520596,-967365412,1462557846,752654012,-594100993,-1765998659,-1124090583,-4380317,-1121360911,694415746,1676771870,-243130637,
													/*P*/-2111900683,519304448,-202047450,-184539456,2539693,650161574,-1062361500,-1381604251,-1503369984,1684340966,1694557753,15088056,-432424708,968424536,-1191421914,-61331893,1478904707,642483003,1266891628,-2093257655,996952354,1816732395,1227025202,585839264,-349003737,849356640,-1608032001,660668318,1627364994,-6389024,-1635590096,-2099236781,-533703894,810756690,1395282464,710025443,1377887183,551800651,-472953922,-817119559,1270790562,-1095130423,-1180513946,-1563859260,-916011883,1724159246,-996864388,-1794212705,243048371,2090840890,-1615644048,-1288015702,980462098,1890193946,-1441654040,303753277,451427759,-398610629,1034894093,-1355084325,990763962,232503960,-608528271,-1164414641,
													/*N*/-1737404494,1901048422,1337091659,-1301918815,1716232459,1268845351,-1593104505,187139969,663191874,-2021571925,-2126337109,1118546746,-1414841844,-1422259141,973880064,205193401,989903106,12124740,-1191033824,38019266,1142997513,549587367,-1039554681,161974261,-1484262098,-2013974941,-181509336,778250365,1663597853,679288249,2099100049,498700672,-1181646831,-1853877859,-2146329161,295548701,-1648943669,-1222784220,499852490,-886781373,617235225,-901572295,1125726650,423213644,968510670,-1169371444,1288621118,-825475351,-868292343,1055459790,-385233193,164550614,-824715528,-673777525,-688354309,-125043824,-1946447852,-74443604,-1877693430,346819247,-1408585790,179290662,-1346230703,-1037676226,
													/*K*/642858642,1363055108,1049756713,-1845221036,69817368,693377054,1410866818,404652567,511842119,-2112403615,390554048,1197588573,1639996816,-1067610057,1569732516,-1875401676,933508151,-1540081673,876083026,938955467,-145568842,1389082256,-877227937,-1232052246,-1872762221,1609208630,-359451058,-1825157491,911117767,1317914421,-1916324498,-952799594,896439991,1855371176,-1766348557,-1213664320,-1460420380,-205462284,-1058736989,-453729501,-190635078,-1557939474,599453341,-1158767295,-291683862,-1656624586,1105868414,-365527485,914244465,2118349142,1131501227,1901505452,1454091390,-1414758806,-1401001433,2120886234,1780996689,668619175,-632182908,1369932861,-1484505753,-2076350555,1030202671,1738878747,
													/*B*/-1523639428,790330424,461125740,2084072648,946653422,1825107554,-923901263,-295521946,1655793234,-1318694327,1716668833,1380557308,1235352831,-1577255140,-50389833,-14895270,481778227,-1218825452,1513297099,857000795,348871471,-883216528,1529835751,795928566,1894249991,-403306716,-167304122,119817782,608581349,1178002806,921007643,-445244649,1981486857,454494515,386478980,154371278,864341722,-2066818343,-824518239,-623271611,-643742289,-1589268624,1169125458,-1351593265,1884475214,1389317828,-816921476,1321499814,-998463833,2091296569,-1498990225,-1489408235,963581263,1863667667,357553116,1339284571,-740533352,-597977036,1536701658,-1741366722,886718064,-633442295,1047529887,1879678924,
													/*r*/161467475,-1613999294,-866958621,1396892537,1122204076,-478565298,2041335414,-1404143977,1316394911,1989648198,-1751169516,-1622797229,1175737287,341034838,1405572853,-950602254,1458958906,-168674566,-231015921,989466435,-99663071,256057674,1126255353,558561564,1257839815,-115554350,482857609,-942503595,-762751518,-1990860264,1440880764,-501711765,410807095,2087401361,1798803851,932285293,-1853133369,-1955739715,1841806833,-943853270,-1108268374,-248862118,715807322,-1436919201,1515872143,1516212218,1603271192,-1879435169,-99065890,408935987,1608397701,-567048720,864415930,-2047821293,-256240646,-1173095847,335173951,-94814363,1497326870,1063589397,1695946190,370527859,365851641,-831260367,
													/*q*/1945710928,-114208738,827334195,1344156451,506667947,857975707,598449120,-1415847800,-1679783934,-527957333,-2013091043,44768616,-1424136109,493376391,1750304703,1401405227,-2017514653,-1087675451,727958794,1673857540,-989199277,168055758,72601091,1406010116,-838662948,50650331,81582934,-589605173,-615068886,1456155333,-886389280,717611142,-975141157,-528032999,-2032461384,-619071340,431527150,-1198199068,-1796283167,-286989887,-454966951,-507422243,-1051075179,1507693949,-577405608,-1786947407,2102964679,1488045853,-1312350780,-954350392,499435770,-993461733,-923133142,-98883018,455751287,708212577,913793383,2002872172,1634168061,1735196137,1828579660,-35042061,-380833016,1290995791,
													/*p*/-217559146,139433638,1335273213,-1767441062,-1493345644,-44395509,1519651641,-1811203589,188349367,972797911,-71837788,-1210604430,-677088650,-1536002441,1920366509,1987554595,2007835562,-1390171576,598362121,-1438119596,1208571047,156542910,1420279544,-1480656893,-1091042408,-133982117,60316499,-1738845420,1532171366,1393845970,342282776,1725044962,-770121007,417518013,-489570932,-776106772,-1114837992,-1930684363,-333957791,406151664,895611031,1643157486,-258478395,-1745959609,-289061079,-985191985,1193922474,701475387,-810927324,-1438964590,992252653,613608888,-1829914408,-306653062,-1193772303,-663031390,2062656252,-240976689,-1560490093,-53505275,-812448444,-1828371408,88355048,1144055856,
													/*n*/820523255,-399443969,821559281,-134221462,-955654,-244647338,1794791046,-94992804,1451646026,-2040772056,1548363799,1244141481,672639445,397006132,-1445645302,-718009791,873087397,172074494,1101397565,-1510064656,-29495119,1039184132,-256834452,-1325110230,74197724,1814748374,719115840,-589938504,-700401636,1085807678,-1206108469,473877321,1053509920,-884400069,1226849239,540792755,1003992021,-676080237,-1277848722,-711758272,-1821491072,1849721023,1082179365,-2134956590,-1088040448,634519610,-771736964,3833070,981265959,2095982485,-299395696,664113241,-1785702040,-1873188661,1500039975,1758144409,-886597274,664364793,-1721304599,1727654258,-102141217,-378347738,1927226916,-551148520,
													/*k*/639899663,605556640,403677186,262144767,-1610416265,50296705,-8945340,2004960479,-2126192876,1155470465,-552304159,344056275,-2115906563,-506200696,-738359287,-41416243,-2012623396,164486148,-841218828,-603655008,83140609,-190840531,-1610535658,19732167,756467597,382176557,-947049014,-1926378878,768246471,-897399028,-2100884344,-955479948,210269431,-2005600456,1962358994,-147270991,953332206,-760091074,-1309786548,-297906957,1045230444,1291021417,-210998797,1818882826,1777535585,-217423387,174187787,1642400618,-452236702,191521509,1784866168,1659205682,-445107511,2016594212,852042909,-920347373,614273842,-1659686348,322057242,842275329,874119607,436320239,28831729,-1209011829,
													/*b*/-269382897,-242544770,-1961918864,259944622,2121313813,1890456891,-1374340329,356194113,991379934,390192755,1105097702,-562829729,1944477555,-429952213,1601383394,1932255984,736292965,-487561929,-261802201,1698113299,925307659,655559609,319535497,196708668,-1182188458,-1992534392,1012303938,1451770446,-2008920467,1112436101,1315800508,1837481064,-2051249955,-1133978232,1759348843,-578262055,-2006197848,1809426605,-643256911,-1465011806,-1380867426,-1314742628,-1566663593,-1633921271,-1672017445,1460263893,165401952,-606773238,-715126079,1611317759,180486011,-1040221270,-8672574,2074788601,-1430062730,-1023838510,-109653468,1993483427,-769350694,614718191,-1545933012,-621859662,-282283323,749913452,

												    // corresponds to white turn
													-1295684460,

												    // for each castling capability
													-982739954,1821642494,-1810956674,251559471,

													// 16 numbers - match the squares that could be en passant targets
													-25284796,2117026932,793015511,1148508094,1960296086,-675375397,-1097409633,-1763991610,-610286023,-1614399106,-969310630,964581922,2119836358,1512228354,583402009,-972940984
												};

        /// <summary>
        /// Hashtable for converting a char to the piece type.
        /// </summary>
        private static readonly Dictionary<char, Type> pieceCharToTypeConversion;
        /// <summary>
        /// Hashtable for converting a piece type to its representation char.
        /// </summary>
        private static readonly Dictionary<Type, char> pieceTypeToCharConversion;
        /// <summary>
        /// Hashtable for converting a piece type to its lower representation char.
        /// </summary>
        private static readonly Dictionary<Type, char> pieceTypeToLowerCharConversion;
        /// <summary>
        /// Hashtable for converting a piece type to its upper representation char.
        /// </summary>
        private static readonly Dictionary<Type, char> pieceTypeToUpperCharConversion;
        /// <summary>
        /// Hashtable for finding the Zobrist key index of a piece type. 
        /// </summary>
        private static readonly Dictionary<Type, int> pieceZobristIndexTable;

        /// <summary>
        /// Static contructor.
        /// </summary>
        static Utils()
        {
            // the piece chars
            char[] pieceChars = { 'R', 'Q', 'P', 'N', 'K', 'B', 'r', 'q', 'p', 'n', 'k', 'b' };

            // the piece types
            Type[] pieceTypes = { typeof(WhiteRook), typeof(WhiteQueen), typeof(WhitePawn), typeof(WhiteKnight), typeof(WhiteKing), typeof(WhiteBishop), typeof(BlackRook), typeof(BlackQueen), typeof(BlackPawn), typeof(BlackKnight), typeof(BlackKing), typeof(BlackBishop) };

            // initialize the hastables
            pieceCharToTypeConversion = new Dictionary<char, Type>(Piece.TypesNo);
            pieceTypeToCharConversion = new Dictionary<Type, char>(Piece.TypesNo);
            pieceTypeToLowerCharConversion = new Dictionary<Type, char>(Piece.TypesNo);
            pieceTypeToUpperCharConversion = new Dictionary<Type, char>(Piece.TypesNo);
            pieceZobristIndexTable = new Dictionary<Type, int>(Piece.TypesNo);

            // populate the hashtables
            for (int pieceTypeIndex = 0; pieceTypeIndex < Piece.TypesNo; pieceTypeIndex++)
            {
                pieceCharToTypeConversion.Add(pieceChars[pieceTypeIndex], pieceTypes[pieceTypeIndex]);
                pieceTypeToCharConversion.Add(pieceTypes[pieceTypeIndex], pieceChars[pieceTypeIndex]);
                pieceTypeToLowerCharConversion.Add(pieceTypes[pieceTypeIndex], Char.ToLower(pieceChars[pieceTypeIndex]));
                pieceTypeToUpperCharConversion.Add(pieceTypes[pieceTypeIndex], Char.ToUpper(pieceChars[pieceTypeIndex]));
                pieceZobristIndexTable.Add(pieceTypes[pieceTypeIndex], Board.SquareNo * pieceTypeIndex);
            }
        }

        /// <summary>
        /// Gets a piece from its char representation.
        /// </summary>
        /// <param name="pieceChar">The char representation of the piece</param>
        /// <returns></returns>
        private static Piece GetPieceFromChar(char pieceChar)
        {
            return pieceCharToTypeConversion.ContainsKey(pieceChar) ? pieceCharToTypeConversion[pieceChar].GetConstructor(Type.EmptyTypes).Invoke(null) as Piece : null;
        }

        /// <summary>
        /// Gets a promotion type.
        /// </summary>
        /// <param name="whiteTurn">True to return a white piece, false to return a black piece</param>
        /// <param name="type">The piece char</param>
        /// <returns></returns>
        private static Type GetPromotionType(bool whiteTurn, char type)
        {
            return pieceCharToTypeConversion[whiteTurn ? Char.ToUpper(type) : Char.ToLower(type)];
        }

        /// <summary>
        /// Gets the board FEN (Forsyth-Edwards notation).
        /// </summary>
        /// <param name="board">The board</param>
        /// <returns></returns>
        public static string GetFEN(Board board)
        {
            // the board must not be null
            if (board == null) { throw new ArgumentNullException("board", "Resources.NullBoardMsg"); }

            StringBuilder sb = new StringBuilder(80);

            int emptySqNo = 0;

            for (int sqIndex = 0; sqIndex < Board.SquareNo; sqIndex++)
            {
                // if the end of row is reached
                if (sqIndex % Board.SideSquareNo == 0 && sqIndex > 0)
                {
                    // write the number of empty squares (if any) and reset it
                    if (emptySqNo != 0)
                    {
                        sb.Append(emptySqNo.ToString(CultureInfo.InvariantCulture));
                        emptySqNo = 0;
                    }

                    sb.Append('/');// write '/'
                }

                //if there is a piece on this square
                if (board[sqIndex] != null)
                {
                    // write the number of empty squares (if any) and reset it
                    if (emptySqNo != 0)
                    {
                        sb.Append(emptySqNo.ToString(CultureInfo.InvariantCulture));
                        emptySqNo = 0;
                    }

                    sb.Append(pieceTypeToCharConversion[board[sqIndex].GetType()]);// write piece char representation

                }
                // if the square is empty
                else
                {
                    emptySqNo++;// increment the number of empty squares
                }
            }
            // write the number of empty squares (if any)
            if (emptySqNo != 0)
            {
                sb.Append(emptySqNo.ToString(CultureInfo.InvariantCulture));
            }

            sb.Append(' ').Append(board.Status.WhiteTurn ? 'w' : 'b');// write the side to move char

            // write castling availability chars
            if (board.Status.WhiteCouldCastleLong || board.Status.WhiteCouldCastleShort || board.Status.BlackCouldCastleLong || board.Status.BlackCouldCastleShort)
            {
                sb.Append(' ');
                if (board.Status.WhiteCouldCastleShort) { sb.Append('K'); }
                if (board.Status.WhiteCouldCastleLong) { sb.Append('Q'); }
                if (board.Status.BlackCouldCastleShort) { sb.Append('k'); }
                if (board.Status.BlackCouldCastleLong) { sb.Append('q'); }
            }
            else
            {
                sb.Append(' ').Append('-');
            }

            // write en passant target notation
            sb.Append(' ').Append((board.Status.EnPassantTarget != null ? GetNotation(board.Status.EnPassantTarget.Value) : "-"));

            // write ply
            sb.Append(' ').Append(board.Status.Ply.ToString(CultureInfo.InvariantCulture));

            // write move number
            sb.Append(' ').Append(board.Status.Moves.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        /// <summary>
        /// Gets a board from its FEN (Forsyth-Edwards notation).
        /// Throws ArgumentException if it's not a valid position.
        /// </summary>
        /// <param name="fen">The FEN string</param>
        /// <returns></returns>
        public static Board GetFENBoard(string fen)
        {
            try
            {
                // the FEN string must not be null
                if (fen == null) { throw new ArgumentNullException("fen", "Resources.NullFENMsg"); }

                Piece[] squares = new Piece[Board.SquareNo];
                BoardStatus status = new BoardStatus();

                Piece piece;
                int index = 0, pos = 0;

                while (fen[index] != ' ')
                {
                    // get the piece from its char representation
                    piece = GetPieceFromChar(fen[index]);

                    // if there is a piece
                    if (piece != null)
                    {
                        squares[pos++] = piece;// put the piece on board 
                    }
                    // if there is a number
                    else if (fen[index] >= '1' && fen[index] <= '8')
                    {
                        pos += fen[index] - '0';// skip empty squares
                    }
                    // if there another char than '/' throw exception
                    else if (fen[index] != '/')
                    {
                        throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
                    }

                    index++;
                }

                index++;

                // set side to move
                if (fen[index] == 'w')
                {
                    status.WhiteTurn = true;
                }
                else if (fen[index] == 'b')
                {
                    status.BlackTurn = true;
                }
                else
                {
                    throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
                }

                index += 2;

                // set castling availability
                if (fen[index] == '-')
                {
                    index++;
                }
                else
                {
                    do
                    {
                        switch (fen[index])
                        {
                            case 'K':
                                status.WhiteCouldCastleShort = true;
                                break;
                            case 'Q':
                                status.WhiteCouldCastleLong = true;
                                break;
                            case 'k':
                                status.BlackCouldCastleShort = true;
                                break;
                            case 'q':
                                status.BlackCouldCastleLong = true;
                                break;
                            default:
                                throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
                        }
                    }
                    while (fen[++index] != ' ');
                }

                index++;

                // set en passant target
                if (fen[index] == '-')
                {
                    status.EnPassantTarget = null;
                    index++;
                }
                else
                {
                    // checks if the square notation is correct 
                    if (fen[index] < 'a' || fen[index] > 'h' || fen[index + 1] < '1' || fen[index + 1] > '8')
                    {
                        throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
                    }

                    status.EnPassantTarget = GetPosition(fen[index], fen[index + 1]);

                    index += 2;
                }

                index++;

                // set ply
                try
                {
                    status.Ply = Int32.Parse(fen.Substring(index, fen.IndexOf(' ', index) - index), CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
                }

                index = fen.IndexOf(' ', index) + 1;

                // set move number
                try
                {
                    status.Moves = Int32.Parse(fen.Substring(index), CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
                }

                Board board = new Board(squares, status);

                // verfies if the board is in valid state, throws ArgumentException if it's not
                VerifyState(board);

                return board;
            }
            // if the FEN string was too short
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException("Resources.IllegalFENFormatMsg", "fen");
            }
        }

        /// <summary>
        /// Verifies if the board is in a valid state, throws ArgumentException if it's not.
        /// </summary>
        /// <param name="board">The board</param>
        public static void VerifyState(Board board)
        {
            // the board must not be null
            if (board == null) { throw new ArgumentNullException("board", "Resources.NullBoardMsg"); }

            // hastable to remember the number of each piece type
            // a pair consists in a piece type and the number of pieces on this board
            Dictionary<Type, int> pieceNo = new Dictionary<Type, int>(Piece.TypesNo);

            // initialize the values with 0
            foreach (Type type in pieceZobristIndexTable.Keys) { pieceNo[type] = 0; }

            // loop through the squares
            for (int sqIndex = 0; sqIndex < Board.SquareNo; sqIndex++)
            {
                if (board[sqIndex] != null)
                {
                    // increment the number of this piece type
                    Type type = board[sqIndex].GetType();
                    pieceNo[type] = pieceNo[type] + 1;

                    // check if the White Pawn has rank 1
                    if (type == typeof(WhitePawn) && Utils.GetRankNotation(sqIndex) == '1')
                    {
                        throw new ArgumentException("Resources.WhitePawnRank1Msg", "board");
                    }

                    // check if the Black Pawn has rank 8
                    if (type == typeof(BlackPawn) && Utils.GetRankNotation(sqIndex) == '8')
                    {
                        throw new ArgumentException("Resources.BlackPawnRank8Msg", "board");
                    }
                }
            }

            // if there are too many White pieces
            // check if the number of Pawns 
            // plus the number of extra Queens, Rooks, Bishops, Knights
            // (which can result only by promotion) exceeds 8
            if ((pieceNo[typeof(WhitePawn)] + Math.Max(pieceNo[typeof(WhiteQueen)] - 1, 0) + Math.Max(pieceNo[typeof(WhiteRook)] - 2, 0) + Math.Max(pieceNo[typeof(WhiteBishop)] - 2, 0) + Math.Max(pieceNo[typeof(WhiteKnight)] - 2, 0)) > 8)
            {
                throw new ArgumentException("Resources.TooManyWhitePiecesMsg", "board");
            }

            // if there are too many Black pieces
            // check if the number of Pawns
            // plus the number of extra Queens, Rooks, Bishops, Knights 
            // (which can result only by promotion) exceeds 8
            if ((pieceNo[typeof(BlackPawn)] + Math.Max(pieceNo[typeof(BlackQueen)] - 1, 0) + Math.Max(pieceNo[typeof(BlackRook)] - 2, 0) + Math.Max(pieceNo[typeof(BlackBishop)] - 2, 0) + Math.Max(pieceNo[typeof(BlackKnight)] - 2, 0)) > 8)
            {
                throw new ArgumentException("Resources.TooManyBlackPiecesMsg", "board");
            }

            // check for White King
            if (pieceNo[typeof(WhiteKing)] > 1)
            {
                throw new ArgumentException("Resources.MoreThanOneWhiteKingMsg", "board");
            }
            if (pieceNo[typeof(WhiteKing)] < 1)
            {
                throw new ArgumentException("Resources.NoWhiteKingMsg", "board");
            }

            // check for Black King
            if (pieceNo[typeof(BlackKing)] > 1)
            {
                throw new ArgumentException("Resources.MoreThanOneBlackKingMsg", "board");
            }
            if (pieceNo[typeof(BlackKing)] < 1)
            {
                throw new ArgumentException("Resources.NoBlackKingMsg", "board");
            }

            // check if the King and the Rook are in place for castling availability
            if (board.Status.WhiteCouldCastleShort && !(board[Board.E1] is WhiteKing && board[Board.H1] is WhiteRook))
            {
                throw new ArgumentException("Resources.IllegalWhiteKingShortCastlingMsg", "board");
            }

            // check if the King and the Rook are in place for castling availability
            if (board.Status.WhiteCouldCastleLong && !(board[Board.E1] is WhiteKing && board[Board.A1] is WhiteRook))
            {
                throw new ArgumentException("Resources.IllegalWhiteKingLongCastlingMsg", "board");
            }

            // check if the King and the Rook are in place for castling availability
            if (board.Status.BlackCouldCastleShort && !(board[Board.E8] is BlackKing && board[Board.H8] is BlackRook))
            {
                throw new ArgumentException("Resources.IllegalBlackKingShortCastlingMsg", "board");
            }

            // check if the King and the Rook are in place for castling availability
            if (board.Status.BlackCouldCastleLong && !(board[Board.E8] is BlackKing && board[Board.A8] is BlackRook))
            {
                throw new ArgumentException("Resources.IllegalBlackKingLongCastlingMsg", "board");
            }

            // check the en passant target
            if (
                board.Status.EnPassantTarget != null &&
                !(
                (
                Board.Rank(board.Status.EnPassantTarget.Value) == 5 && board.Status.BlackTurn &&// the rank of en passant target is correct
                board[board.Status.EnPassantTarget.Value - Board.SideSquareNo] is WhitePawn &&// there is a pawn in front of the target 
                board[board.Status.EnPassantTarget.Value] == null && board[board.Status.EnPassantTarget.Value + Board.SideSquareNo] == null// the en passant target square and the one behind are empty

                ) ||
                (
                Board.Rank(board.Status.EnPassantTarget.Value) == 2 && board.Status.WhiteTurn &&// the rank of en passant target is correct
                board[board.Status.EnPassantTarget.Value + Board.SideSquareNo] is BlackPawn &&// there is a pawn in front of the target 
                board[board.Status.EnPassantTarget.Value] == null && board[board.Status.EnPassantTarget.Value - Board.SideSquareNo] == null// the en passant target square and the one behind are empty
                )
                )
                )
            {
                throw new ArgumentException("Resources.IllegalEPTargetMsg", "board");
            }

            // check if ply >= 0
            if (board.Status.Ply < 0)
            {
                throw new ArgumentException("Resources.NonNegativePlyMsg", "board");
            }

            // check if move number >= 1
            if (board.Status.Moves < 1)
            {
                throw new ArgumentException("Resources.PositiveMoveNoMsg", "board");
            }

            // check if the side which is not to move is in check
            if ((board.Status.WhiteTurn && board.BlackKingInCheck()) || (board.Status.BlackTurn && board.WhiteKingInCheck()))
            {
                throw new ArgumentException("Resources.NoSideNotToMoveCheckMsg", "board");
            }
        }

        /// <summary>
        /// Computes the board hash using Zobrist method.
        /// </summary>
        /// <param name="board">The board</param>
        /// <returns></returns>
        public static int GetHash(Board board)
        {
            // the board must not be null
            if (board == null) { throw new ArgumentNullException("board", "Resources.NullBoardMsg"); }

            long hash = 0;

            // loop through the squares
            for (int sqIndex = 0; sqIndex < Board.SquareNo; sqIndex++)
            {
                // if the square is not empty
                if (board[sqIndex] != null)
                {
                    // XOR the Zobrist key which coresponds to type of piece and for this square
                    // to find the key index, offset the key index of the type by the square number
                    hash ^= zobristKeys[pieceZobristIndexTable[board[sqIndex].GetType()] + sqIndex];
                }
            }

            // if White is to move, XOR the corresponding Zobrist key
            if (board.Status.WhiteTurn)
            {
                hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo];
            }

            // if the Kings could castle, the corresponding Zobrist keys
            if (board.Status.WhiteCouldCastleLong)
            {
                hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo + 1];
            }
            if (board.Status.WhiteCouldCastleShort)
            {
                hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo + 2];
            }
            if (board.Status.BlackCouldCastleLong)
            {
                hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo + 3];
            }
            if (board.Status.BlackCouldCastleShort)
            {
                hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo + 4];
            }

            // if there is an en passant target, XOR the corresponding Zobrist key
            if (board.Status.EnPassantTarget != null)
            {
                // the en passant targets have rank 2 or 5
                if (Board.Rank(board.Status.EnPassantTarget.Value) == 2)
                {
                    hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo + 5 + Board.File(board.Status.EnPassantTarget.Value)];
                }
                else if (Board.Rank(board.Status.EnPassantTarget.Value) == 5)
                {
                    hash ^= zobristKeys[Piece.TypesNo * Board.SquareNo + 13 + Board.File(board.Status.EnPassantTarget.Value)];
                }

            }

            // XOR the first 4 bytes with the last 4 bytes to return an 32-bit integer
            return (int)((hash & 0xFFFFFFFF) ^ (hash >> 32));
        }

        /// <summary>
        /// Gets the move CAN (coordinate algebraic notation) without checking the status of the game or if it's a valid move.
        /// </summary>
        /// <param name="move">The move</param>
        /// <returns></returns>
        public static string GetCAN(Move move)
        {
            // the move must not be null
            if (move == null) { throw new ArgumentNullException("move", "Resources.NullMoveMsg"); }

            StringBuilder sb = new StringBuilder(5);

            // add starting square notation
            sb.Append(GetNotation(move.From));

            // add ending square notation
            sb.Append(GetNotation(move.To));

            // add promotion notation (if any)
            if (move is PromotionMove)
            {
                sb.Append(pieceTypeToLowerCharConversion[(move as PromotionMove).PromotionType]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a move from its CAN (coordinate algebraic notation).
        /// Throws ArgumentException if it's not a valid move.
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="can">The CAN string</param>
        /// <returns></returns>
        public static Move GetCANMove(Game game, string can)
        {
            // the game must not be null
            if (game == null) { throw new ArgumentNullException("game", "Resources.NullGameMsg"); }

            // the CAN string must not be null
            if (can == null) { throw new ArgumentNullException("can", "Resources.NullCANMsg"); }

            // check if the string is well-formed
            if (can.Length < 4 || can[0] < 'a' || can[0] > 'h' || can[1] < '1' || can[1] > '8' || can[2] < 'a' || can[2] > 'h' || can[3] < '1' || can[3] > '8')
            {
                throw new ArgumentException("Resources.IllegalCANFormatMsg", "can");
            }

            // get the starting square
            int from = GetPosition(can[0], can[1]);

            // get the ending square
            int to = GetPosition(can[2], can[3]);

            // get the promotion
            Type promotionType = (can.Length > 4) ? GetPromotionType(game.CurrentBoard.Status.WhiteTurn, can[4]) : null;

            // look into the possible moves to find it
            Move move = game.GetMove(from, to, promotionType);

            if ((move == null) || (move is PromotionMove && (move as PromotionMove).PromotionType == null))
            {
                throw new ArgumentException("Resources.IllegalCANMoveMsg", "can");
            }

            return move;
        }

        /// <summary>
        /// Get the SAN (standard algebraic notation) index for a move - move number and the side to move
        /// without checking the status of the game or if it's a valid move.
        /// </summary>
        /// <param name="game">The game</param>
        /// <returns></returns>
        public static string GetSANIndex(Game game)
        {
            // the game must not be null
            if (game == null) { throw new ArgumentNullException("game", "Resources.NullGameMsg"); }

            StringBuilder sb = new StringBuilder(5);

            // add move number
            sb.Append(game.CurrentBoard.Status.Moves.ToString(CultureInfo.InvariantCulture));

            // add side to move notation
            if (game.CurrentBoard.Status.WhiteTurn)
            {
                sb.Append('.');
            }
            else
            {
                sb.Append("...");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the end of the SAN (standard algebraic notation) for a move - promotion and check/checkmate representation - after the move is made
        /// without checking the status of the game or if it's a valid move.
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="move">The move</param>
        /// <returns></returns>
        public static string GetSANEnd(Game game, Move move)
        {
            // the game must not be null
            if (game == null) { throw new ArgumentNullException("game", "Resources.NullGameMsg"); }

            // the move must not be null
            if (move == null) { throw new ArgumentNullException("move", "Resources.NullMoveMsg"); }

            StringBuilder sb = new StringBuilder(3);

            if (move is PromotionMove)
            {
                // add promotion notation
                sb.Append('=').Append(pieceTypeToUpperCharConversion[(move as PromotionMove).PromotionType]);
            }

            if (game.Status == GameStatus.Check)
            {
                // add check representation
                sb.Append('+');
            }
            else if (game.Status == GameStatus.Checkmate)
            {
                // add checkmate representation
                sb.Append('#');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the begin of the SAN (standard algebraic notation) for a move - without promotion or check/checkmate representation -  before the move is made
        /// without checking the status of the game or if it's a valid move.
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="move">The move</param>
        /// <returns></returns>
        public static string GetSANBegin(Game game, Move move)
        {
            // the game must not be null
            if (game == null) { throw new ArgumentNullException("game", "Resources.NullGameMsg"); }

            // the move must not be null
            if (move == null) { throw new ArgumentNullException("move", "Resources.NullMoveMsg"); }

            StringBuilder sb = new StringBuilder(6);

            //if it's a castling move
            if (move is CastlingMove)
            {
                if (move.To == Board.G1 || move.To == Board.G8)
                {
                    // short castling
                    sb.Append("O-O");
                }
                else if (move.To == Board.C1 || move.To == Board.C8)
                {
                    // long castling
                    sb.Append("O-O-O");
                }
            }
            else
            {
                // add piece type and disambiguation
                char p = pieceTypeToUpperCharConversion[game.CurrentBoard[move.From].GetType()];
                if (p == 'P')
                {
                    // if the pawn captures, add the starting square file
                    if (move.HasCapture)
                    {
                        sb.Append(GetFileNotation(move.From));
                    }
                }
                else
                {
                    // add piece char
                    sb.Append(p);

                    // add disambiguation

                    // disambigutationList will contain starting squares that contain the same
                    // type of piece and that can move to the same ending square as "move"
                    List<int> disambiguationList = new List<int>(5);
                    foreach (Move m in game.PossibleMoves)
                    {
                        if (m.To == move.To && m.From != move.From && game.CurrentBoard[m.From].ToString() == game.CurrentBoard[move.From].ToString())
                        {
                            disambiguationList.Add(m.From);
                        }
                    }

                    if (disambiguationList.Count > 0)
                    {
                        // see if the file is unique
                        bool isFileUnique = true;
                        foreach (int from in disambiguationList)
                        {
                            if (Board.File(move.From) == Board.File(from))
                            {
                                isFileUnique = false;
                                break;
                            }
                        }

                        if (isFileUnique)
                        {
                            // insert file
                            sb.Append(Utils.GetFileNotation(move.From));
                        }
                        else
                        {
                            // see if the rank is unique
                            bool isRankUnique = true;
                            foreach (int from in disambiguationList)
                            {
                                if (Board.Rank(move.From) == Board.Rank(from))
                                {
                                    isRankUnique = false;
                                    break;
                                }
                            }

                            if (isRankUnique)
                            {
                                // insert rank
                                sb.Append(Utils.GetRankNotation(move.From));
                            }
                            else
                            {
                                // both file and rank are not unique, insert both of them
                                sb.Append(Utils.GetNotation(move.From));
                            }
                        }
                    }
                }

                // if there is a capture, add capture notation
                if (move.HasCapture)
                {
                    sb.Append('x');
                }

                // add destination square
                sb.Append(GetNotation(move.To));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a move from its SAN (standard algebraic notation).
        /// Throws ArgumentException if it's not a valid move.
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="san">The SAN string</param>
        /// <returns></returns>
        public static Move GetSANMove(Game game, string san)
        {
            // the game must not be null
            if (game == null) { throw new ArgumentNullException("game", "Resources.NullGameMsg"); }

            // the SAN string must not be null
            if (san == null) { throw new ArgumentNullException("san", "Resources.NullSANMsg"); }

            Move move = null;

            // if it's a short castling move
            if (san == "O-O")
            {
                foreach (Move m in game.PossibleMoves)
                {
                    if (m is CastlingMove && (m.To == Board.G1 || m.To == Board.G8))
                    {
                        move = m;
                        break;
                    }
                }
            }
            // if it's a long castling move
            else if (san == "O-O-O")
            {
                foreach (Move m in game.PossibleMoves)
                {
                    if (m is CastlingMove && (m.To == Board.C1 || m.To == Board.C8))
                    {
                        move = m;
                        break;
                    }
                }
            }
            else
            {
                int index = san.Length - 1;

                // remove chess and checkmate representation (if any)
                if (index > -1 && (san[index] == '+' || san[index] == '#'))
                {
                    index--;
                }

                if (index < 1)
                {
                    throw new ArgumentException("Resources.IllegalSANFormatMsg", "san");
                }

                // get the promotion (if any)
                char prom = '\0';
                if (san[index - 1] == '=')
                {
                    prom = san[index];
                    index -= 2;
                }

                if (index < 1 || san[index - 1] < 'a' || san[index - 1] > 'h' || san[index] < '1' || san[index] > '8')
                {
                    throw new ArgumentException("Resources.IllegalSANFormatMsg", "san");
                }

                // get the ending square
                int to = Utils.GetPosition(san[index - 1], san[index]);
                index -= 2;


                // remove capture char (if any)
                if (index > -1 && san[index] == 'x')
                {
                    index--;
                }

                // get the rank of the starting square (if any)
                int? rank = null;
                if (index > -1 && san[index] >= '1' && san[index] <= '8')
                {
                    rank = GetRank(san[index]);
                    index--;
                }

                // get the file of the starting square (if any)
                int? file = null;
                if (index > -1 && san[index] >= 'a' && san[index] <= 'h')
                {
                    file = GetFile(san[index]);
                    index--;
                }

                // get piece type char (if any)
                char pieceChar = 'P';
                if (index > -1)
                {
                    pieceChar = san[index];
                    index--;
                }

                // look into possible moves
                foreach (Move m in game.PossibleMoves)
                {
                    if (
                        m.To == to &&// the ending squares match
                        (file == null || (file != null && Board.File(m.From) == file.Value)) &&// the starting squares files match (if any) 
                        (rank == null || (rank != null && Board.Rank(m.From) == rank.Value)) &&// the starting squares ranks match (if any)
                        pieceTypeToUpperCharConversion[game.CurrentBoard[m.From].GetType()] == pieceChar// the piece type chars match 
                        )
                    {
                        move = m;
                        break;
                    }
                }

                // if it's a promotion move, set the promotion
                if (move is PromotionMove)
                {
                    (move as PromotionMove).PromotionType = GetPromotionType(game.CurrentBoard.Status.WhiteTurn, prom);
                }
            }

            if (move != null)
            {
                return move;
            }
            else
            {
                throw new ArgumentException("Resources.IllegalSANMoveMsg", "san");
            }
        }

        /// <summary>
        /// Gets a square position from its rank and file chars.
        /// </summary>
        /// <param name="f">The file char</param>
        /// <param name="r">The rank char</param>
        /// <returns></returns>
        public static int GetPosition(char f, char r)
        {
            if (f < 'a' || f > 'h' || r < '1' || r > '8')
            {
                return -1;
            }

            return Board.Position('8' - r, f - 'a');
        }

        /// <summary>
        /// Gets the square notation.
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns></returns>
        public static string GetNotation(int position)
        {
            if (position < 0 || position > 63) { return null; }

            StringBuilder sb = new StringBuilder(2);

            sb.Append((char)((Board.File(position)) + 'a'));
            sb.Append((char)('8' - Board.Rank(position)));

            return sb.ToString();
        }

        /// <summary>
        /// Gets file as an int.
        /// </summary>
        /// <param name="c">The file char</param>
        /// <returns></returns>
        private static int GetFile(char c)
        {
            return c - 'a';
        }

        /// <summary>
        /// Gets the file char for a square.
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns></returns>
        private static char GetFileNotation(int position)
        {
            return (char)((Board.File(position)) + 'a');
        }

        /// <summary>
        /// Gets rank as an int.
        /// </summary>
        /// <param name="c">The rank file</param>
        /// <returns></returns>
        private static int GetRank(char c)
        {
            return '8' - c;
        }

        /// <summary>
        /// Gets the rank char for a square.
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns></returns>
        private static char GetRankNotation(int position)
        {
            return (char)('8' - Board.Rank(position));
        }
    }
}
