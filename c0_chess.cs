//=============================================================================
// C# chess logic library as a class file
// No chess engine here. Samples below.
//
// Just define variable in upper class
//   c0_chess C0 = new c0_chess();
//=============================================================================
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

public class c0_chess
{
public string c0_position;
public int c0_side;
public int c0_sidemoves;

public bool c0_wKingmoved, c0_bKingmoved, c0_wLRockmoved, c0_wRRockmoved;
public bool c0_bLRockmoved, c0_bRRockmoved, c0_w00, c0_b00;

public int c0_lastmovepawn;

public string c0_become;
public string c0_become_from_engine;			// just promos from engine

public string c0_moveslist;

public string c0_moves2do;

public string c0_foundmove;

public string c0_start_FEN;
public bool c0_fischer;
public string c0_fischer_cst;

public bool c0_PG_viewer;		// Set true for on-error-prints or false for silent processing...

public string c0_PG_1;

public string[] c0_PGN_header = new String[20];

public bool c0_errflag;

public string PGN_text;					// PGN support, the game data will be here....
public string c0_NAGs;

public string[] c0_opn = new String[12];


public c0_chess()
{
c0_Initvariables();
// Show samples of chess logic in Log window...
//c0_SAMPLES();
}


public void c0_Initvariables()
{
c0_position="";
c0_side=1;
c0_sidemoves=1;
c0_wKingmoved = false;
c0_bKingmoved = false;
c0_wLRockmoved = false;
c0_wRRockmoved = false;
c0_bLRockmoved = false;
c0_bRRockmoved = false;
c0_w00 = false;
c0_b00 = false;
c0_lastmovepawn = 0;
c0_become="";
c0_become_from_engine="";
c0_moveslist = "";
c0_moves2do="";
c0_foundmove="";
c0_start_FEN="";
c0_fischer=false;
c0_fischer_cst="";
c0_PG_viewer=true;
c0_PG_1="";
c0_errflag=false;
PGN_text="";
c0_NAGs="";
c0_Openings_define();
}

public string Mid(string s, int f, int l) { return (f<0 || l<=0 || f>=s.Length ? "" : s.Substring(f,l)); }
public string Mid2(string s, int f) { return (f<0 || f>=s.Length ? "" : s.Substring(f)); }
public int Asc(string s) { return (int)(s[0]); }
public string Chr(int i) { return (""+(char)i); }
public int charCodeAt(string str, int at) { return Asc(Mid(str,at,1)); }
public int Int(string s)
{ int c=charCodeAt(s,0)-48; return ((c>=0 && c<=9) ? Convert.ToInt32(s) : 0); }

public bool window_confirm(string messtxt) { return true; }	// No dialogs at the moment...
	
public string c0_ReplaceAll(string Source, string stringToFind, string stringToReplace)
{ return Source.Replace(stringToFind,stringToReplace); }

public string LowCase(string Str) { return Str.ToLower(); }	// Lowercase
public string Caps(string Str) { return Str.ToUpper(); }	// Uppercase
public int Len(string s) { return s.Length; }
public int InStr(string s, string s2) { return s.IndexOf(s2); }
public string Str(int i) { return i.ToString(); }
public void Log(string s) { System.Diagnostics.Debug.WriteLine(s); }

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Call this public to get samples working...
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
public void c0_SAMPLES()
{
 string PGN0, mlist0, PGN3, mlist3;

	Log("============================================");
	Log("===== Routine of SAMPLES for chess logic ===");
	Log("============================================");
 

	c0_side=1;					        // This side is white.   For black set -1
	c0_set_start_position("");			// Set the initial position... 
	
	// 1. Test for basic chess publics (ok)...
	Log( "Setting up the starting position" );
	c0_set_start_position("");
	Log( c0_position );
	Log( "FEN public : " + c0_get_FEN() );

	// Make a first move e4...
	c0_move_to("e2","e4");
	Log( "Position after e4:");
	Log( c0_position );
    
	// Show the last move made...
	Log( "Last move:" + c0_D_last_move_was() );

	// switch sides...
	c0_sidemoves = -c0_sidemoves;
	Log( "All movements till now:" + c0_moveslist );
    
	// To see possible movements...
	Log( "Now possible moves:");
	Log( c0_get_next_moves() );
    
	// And take it back...
	c0_take_back();
	Log( "Position after takeback" );
	Log( c0_position );
	c0_sidemoves = -c0_sidemoves;
  
	// Other publics:
	// Is e2-e4 a legal move in current position...
	Log( "Can move a2-a4? ");
	Log( (c0_D_can_be_moved("a2","a4") ? "can" : "can't") );
    
	// Is there stalemate to the white king right now? ("b"/"w"- the parameter)
	Log( "White king in stalemate? ");
	Log( (c0_D_is_pate_to_king("w") ? "is" : "isn't" ) );
    
	// Is there check to the white king right now? 
	Log( "Check to white king?");
    Log( (c0_D_is_check_to_king("w") ? "is" : "isn't") );
    
	// Is there checkmate to the white king right now? 
	Log( "Is checkmate to white king?");
	Log( (c0_D_is_mate_to_king("w") ? "is" : "isn't") );
	
	// What a piece on the square g7?
	Log( "Piece on g7:" + c0_D_what_at("g7") );
    
	// Is the square g6 empty? (no piece on it)
	Log( "Is empty square g6? ");
    Log( (c0_D_is_empty("g6") ? "is" : "isn't") );
    
	// FEN position setup test
	c0_start_FEN="7k/Q7/2P2K2/8/8/8/8/8 w - - 0 70";
	c0_set_start_position("");
	// other way
	//c0_set_FEN ("7k/Q7/2P2K2/8/8/8/8/8 w - - 0 70");
	Log( "Position after setup FEN=7k/Q7/2P2K2/8/8/8/8/8 w - - 0 70");
	Log( c0_position );

	c0_start_FEN="";
	c0_set_start_position("");


	// 2.PGN publics test (ok):
	Log("PGN -> moves");
	PGN0="1.d4 d5 2.c4 e6 {comment goes here} 3.Nf3 Nf6 4.g3 Be7 (4.h4 or variant) 5.Bg2 0-0 6.0-0 dxc4 7.Qc2 a6 8.Qxc4 b5 9.Qc2 Bb7 10.Bd2 Be4 11.Qc1 Bb7 12.Qc2 Ra7 13.Rc1 Be4 14.Qb3 Bd5 15.Qe3 Nbd7 16.Ba5 Bd6 17.Nc3 Bb7 18.Ng5 Bxg2 19.Kxg2 Qa8+ 20.Qf3 Qxf3+ 21.Kxf3 e5 22.e3 Be7 23.Ne2 Re8 24.Kg2 Nd5 25.Nf3 Bd6 26.dxe5 Nxe5 27.Nxe5 Rxe5 28.Nd4 Ra8 29.Nc6 Re6 30.Rc2 Nb6 31.b3 Kf8 32.Rd1 Ke8 33.Nd4 Rf6 34.e4 Rg6 35.e5 Be7 36.Rxc7 Nd5 37.Rb7 Bd8 38.Nf5 Nf4+ 39.Kf3 Bxa5 40.gxf4 Bb4 41.Rdd7 Rc8 42.Rxf7 Rc3+ 43.Ke4 1-0";

	mlist0=c0_get_moves_from_PGN(PGN0);
	Log(mlist0);

    
	Log("moves -> PGN (reverse action)");
	Log( c0_put_to_PGN(mlist0) );
 
 	c0_start_FEN="";
	c0_set_start_position("");
   
	// 3.Fischerrandom support test (ok):
	Log("Fischer-random  PGN -> moves");
	PGN3="[White Aronian, Levon][Black Rosa, Mike][Result 0:1][SetUp 1][FEN bbrkqnrn/pppppppp/8/8/8/8/PPPPPPPP/BBRKQNRN w GCgc - 0 0]";
	PGN3 = PGN3 + "1. c4 e5 2. Nhg3 Nhg6 3. b3 f6 4. e3 b6 5. Qe2 Ne6 6. Qh5 Rh8 7. Nf5 Ne7 8. Qxe8+ Kxe8 9. N1g3 h5 10. Nxe7 Kxe7 11. d4 d6 12. h4 Kf7 13. d5 Nf8 14. f4 c6 15. fxe5 dxe5 16. e4 Bd6 17. Bd3 Ng6 18. O-O Nxh4 19. Be2 Ng6 20. Nf5 Bc5+ 21. Kh2 Nf4 22. Rc2 cxd5 23. exd5 h4 24. Bg4 Rce8 25. Bb2 g6 26. Nd4 exd4 27. Rxf4 Bd6  0-1";
	mlist3= c0_get_moves_from_PGN(PGN3);
	Log(mlist3);

	Log("moves -> PGN (reverse action)");  
	Log( c0_put_to_PGN(mlist3) );

	// clear it all
	c0_start_FEN="";
	c0_set_start_position("");
	Log("Starting position...");

	// There is a short data openings database public
	Log("Openings, variants on request: e2e4...");
	Log( c0_Opening("e2e4") );
		
	Log("On request: 1.d4 d5 2.c4...");
	Log( c0_Opening("d2d4d7d5c2c4" ));
		
	Log("Chess for C# code :)");
}


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// set up starting position...
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
public void c0_set_start_position( string c0_mlist )
{
int c0_z, c0_q;
string c0_from_at, c0_to_at;
string c0_pos2;

if( Len(c0_start_FEN)>0 )
	{
	c0_set_FEN( c0_start_FEN );
	if(c0_fischer) c0_fischer_adjustmoved();
	}
else
{
c0_position="";

if(Len(c0_mlist)==0)
	{
	c0_add_piece("wpa2"); c0_add_piece("wpb2"); c0_add_piece("wpc2"); c0_add_piece("wpd2");
	c0_add_piece("wpe2"); c0_add_piece("wpf2"); c0_add_piece("wpg2"); c0_add_piece("wph2");
	c0_add_piece("wRa1"); c0_add_piece("wNb1"); c0_add_piece("wBc1"); c0_add_piece("wQd1");
	c0_add_piece("wKe1"); c0_add_piece("wBf1"); c0_add_piece("wNg1"); c0_add_piece("wRh1");
	c0_add_piece("bpa7"); c0_add_piece("bpb7"); c0_add_piece("bpc7"); c0_add_piece("bpd7");
	c0_add_piece("bpe7"); c0_add_piece("bpf7"); c0_add_piece("bpg7"); c0_add_piece("bph7");
	c0_add_piece("bRa8"); c0_add_piece("bNb8"); c0_add_piece("bBc8"); c0_add_piece("bQd8");
	c0_add_piece("bKe8"); c0_add_piece("bBf8"); c0_add_piece("bNg8"); c0_add_piece("bRh8");
	}
else
	{
	c0_position = "wpa2;wpb2;wpc2;wpd2;wpe2;wpf2;wpg2;wph2;" +
		"wRa1;wNb1;wBc1;wQd1;wKe1;wBf1;wNg1;wRh1;" +
		"bpa7;bpb7;bpc7;bpd7;bpe7;bpf7;bpg7;bph7;" +
		"bRa8;bNb8;bBc8;bQd8;bKe8;bBf8;bNg8;bRh8;";
	}

c0_wKingmoved = false;
c0_bKingmoved = false;
c0_wLRockmoved = false;
c0_wRRockmoved = false;
c0_bLRockmoved = false;
c0_bRRockmoved = false;
c0_w00 = false;
c0_b00 = false;

c0_lastmovepawn = 0;
c0_sidemoves=1;
}

c0_become="";
c0_become_from_engine="";			// just engine

c0_moveslist = "";

if(Len(c0_mlist)>0)
	{
	for(c0_z=0;c0_z<Len(c0_mlist);c0_z+=4)
		{
		c0_from_at=Mid(c0_mlist,c0_z,2);
		c0_to_at=Mid(c0_mlist,c0_z+2,2);
		if(c0_z+4<Len(c0_mlist) && Mid(c0_mlist,c0_z+4,1)=="[")
			{
			c0_become_from_engine=Mid(c0_mlist,c0_z+5,1);
			c0_z+=3;
			}
		else c0_become_from_engine="";

		if(c0_fischer) c0_fischer_cstl_move(c0_from_at + c0_to_at,false);		
		else 
		c0_moveto(c0_convH888(c0_from_at), c0_convH888(c0_to_at), false);
		c0_sidemoves=-c0_sidemoves;
		}
	if( Len(c0_start_FEN)>0 )
		{
		c0_set_board_situation( c0_position, c0_wKingmoved, c0_wLRockmoved, c0_wRRockmoved, c0_w00,
		 c0_bKingmoved, c0_bLRockmoved, c0_bRRockmoved, c0_b00, c0_lastmovepawn, c0_moveslist, c0_sidemoves );
		}
	else
		{
		c0_pos2=c0_position;
		c0_position="";
		for(c0_q=0;c0_q<Len(c0_pos2);c0_q+=5) c0_add_piece(Mid(c0_pos2,c0_q,4));
		}
	}

c0_moveslist = c0_mlist;
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Set board situation...
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
public void c0_set_board_situation( string c0_figlist, bool c0_wK, bool c0_wLR, bool c0_wRR,
		bool c0_w_00, bool c0_bK, bool c0_bLR, bool c0_bRR, bool c0_b_00, int c0_elpas, string c0_ml, int c0_s )
{
int i;

c0_position="";
i=0;
while(i<Len(c0_figlist))
	{
	c0_add_piece( Mid(c0_figlist,i,4) );
	i+=4; if( i<Len(c0_figlist) && Mid(c0_figlist,i,1)==";" ) i++;
	}

c0_wKingmoved = c0_wK;
c0_bKingmoved = c0_bK;
c0_wLRockmoved = c0_wLR;
c0_wRRockmoved = c0_wRR;
c0_bLRockmoved = c0_bLR;
c0_bRRockmoved = c0_bRR;
c0_w00 = c0_w_00;
c0_b00 = c0_b_00;

c0_lastmovepawn = c0_elpas;

c0_become="";
c0_become_from_engine="";			// just engine

c0_moveslist = c0_ml;
c0_sidemoves=c0_s;
}



//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// add a piece at position...
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
public void c0_add_piece( string c0_pstring )
{
string c0_1_at;

c0_1_at=Mid(c0_pstring,2,2);

// There were other visual activities before...
if(InStr(c0_position,c0_1_at)<0) c0_position = c0_position + c0_pstring + ";";
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// remove a piece from position...
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
public void c0_clear_at( string c0_1_at )
{
int c0_a;
c0_a=InStr(c0_position,c0_1_at);

if(c0_a>=0) c0_position=Mid(c0_position,0,c0_a-2) + Mid2(c0_position,c0_a+3);
}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
// move visualy a piece...
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------
public void c0_just_move_piece( string c0_2_from, string c0_2_to )
{
int c0_a;

c0_clear_at( c0_2_to );
c0_a=InStr(c0_position,c0_2_from);
if(c0_a>=0)
	{
	c0_position=c0_ReplaceAll( c0_position, c0_2_from, c0_2_to );
	c0_moves2do = c0_moves2do + c0_2_from + c0_2_to;
	}
}

// and add a promotion (or castling) indicator...
public void c0_and_promote_or_castle()
{
 if(Len(c0_become)>0) c0_moves2do = c0_moves2do + "[" + c0_become + "]";
}

//############################################################
// CHESS related part for chess play
//############################################################
//-------------------------------------------------
public string c0_convE2(int c0_vertikali, int c0_horizontali)
{
return Chr(96+c0_horizontali) + Str(c0_vertikali);
}

//-------------------------------------------------
public string c0_convE777(string c0_verthoriz)
{
return Chr(96+Int(Mid(c0_verthoriz,1,1))) + Mid(c0_verthoriz,0,1);
}

//-------------------------------------------------
public string c0_conv52(int c0_vertikali, int c0_horizontali)
{
return Str(c0_vertikali) + Str(c0_horizontali);
}

//-------------------------------------------------
public string c0_convH888(string c0_at8)
{
int c0_8horiz, c0_8vert; 
c0_8horiz=Asc( Mid(c0_at8,0,1) ) - 96;
c0_8vert=Int( Mid(c0_at8,1,1));
return Str(c0_8vert) + Str(c0_8horiz);
}

//-------------------------------------------------
public void c0_move_to(string c0_Zstr1, string c0_Zstr2)
{
c0_moveto( c0_convH888(c0_Zstr1), c0_convH888(c0_Zstr2), true );
}

//-------------------------------------------------
public void c0_moveto(string c0_from_at, string c0_to_at, bool c0_draw)
{
int c0_vert, c0_horiz, c0_vert2, c0_horiz2, c0_p, c0_p2, c0_p3;
string c0_color, c0_figure, save_c0_position;

c0_vert = Int(Mid(c0_from_at,0,1));
c0_horiz= Int(Mid(c0_from_at,1,1));
c0_vert2 = Int(Mid(c0_to_at,0,1));
c0_horiz2= Int(Mid(c0_to_at,1,1));

c0_p=InStr(c0_position, c0_convE2(c0_vert,c0_horiz) );
c0_color=Mid(c0_position,c0_p-2,1);
c0_figure=Mid(c0_position,c0_p-1,1);

save_c0_position=c0_position;
	
c0_lastmovepawn = 0; 
if(c0_draw) c0_become="";

 if(c0_draw)
	{
	save_c0_position=c0_position;
	c0_just_move_piece( c0_convE2(c0_vert, c0_horiz), c0_convE2(c0_vert2, c0_horiz2) );
	c0_position=save_c0_position;
	}

 c0_p2=InStr(c0_position, c0_convE2(c0_vert2,c0_horiz2) );
 if(c0_p2>=0)
  {
   c0_position = Mid(c0_position,0,c0_p2-2) + Mid2(c0_position,c0_p2+3);
   
   if(!c0_wLRockmoved && c0_convE2(c0_vert2,c0_horiz2)=="a1") c0_wLRockmoved=true;
   if(!c0_wRRockmoved && c0_convE2(c0_vert2,c0_horiz2)=="h1") c0_wRRockmoved=true;
   if(!c0_bLRockmoved && c0_convE2(c0_vert2,c0_horiz2)=="a8") c0_bLRockmoved=true;
   if(!c0_bRRockmoved && c0_convE2(c0_vert2,c0_horiz2)=="h8") c0_bRRockmoved=true;
 
  }
 else
  {		
   if(c0_figure=="R")
    {
     if(c0_color=="w")
	{
	 if(c0_convE2(c0_vert,c0_horiz)=="a1") c0_wLRockmoved=true;
	 if(c0_convE2(c0_vert,c0_horiz)=="h1") c0_wRRockmoved=true;
	}
     else
	{
	 if(c0_convE2(c0_vert,c0_horiz)=="a8") c0_bLRockmoved=true;
	 if(c0_convE2(c0_vert,c0_horiz)=="h8") c0_bRRockmoved=true;
	}
    }
   
	
   if(c0_figure=="K")
    {
    if(!c0_wKingmoved && c0_color=="w")
	{
	if(c0_convE2(c0_vert,c0_horiz)=="e1" && c0_convE2(c0_vert2,c0_horiz2)=="g1")	// 0-0
		{
		if(c0_draw)
		{
		save_c0_position=c0_position;
		c0_just_move_piece("h1","f1");
		c0_position=save_c0_position;
		}
		c0_position = c0_ReplaceAll(c0_position, "h1", "f1" );		// Rf1
		c0_w00 = true;
		c0_become="0";
		}
	if(c0_convE2(c0_vert,c0_horiz)=="e1" && c0_convE2(c0_vert2,c0_horiz2)=="c1")	// 0-0-0
		{
		if(c0_draw)
		{
		save_c0_position=c0_position;
		c0_just_move_piece("a1","d1");
		c0_position=save_c0_position;
		}
		c0_position = c0_ReplaceAll(c0_position, "a1", "d1" );		// Rd1
		c0_w00 = true;
		c0_become="0";
		}
	c0_wKingmoved=true;
	}
    if(!c0_bKingmoved && c0_color=="b")
	{
	if(c0_convE2(c0_vert,c0_horiz)=="e8" && c0_convE2(c0_vert2,c0_horiz2)=="g8")	// 0-0
		{
		if(c0_draw)
		{
		save_c0_position=c0_position;
		c0_just_move_piece("h8","f8");
		c0_position=save_c0_position;
		}
		c0_position = c0_ReplaceAll(c0_position, "h8", "f8" );		// Rf8
		c0_b00 = true;
		c0_become="0";
		}
	if(c0_convE2(c0_vert,c0_horiz)=="e8" && c0_convE2(c0_vert2,c0_horiz2)=="c8")	// 0-0-0
		{
		if(c0_draw)
		{
		save_c0_position=c0_position;
		c0_just_move_piece("a8","d8");
		c0_position=save_c0_position;
		}
		c0_position = c0_ReplaceAll(c0_position, "a8", "d8" );		// Rd8
		c0_b00 = true;
		c0_become="0";
		}
	c0_bKingmoved=true;
	}
    }	
  }

 if(c0_figure=="p")		// pawn
	{
	 if(c0_vert2==8 || c0_vert2==1)
		{
		if(Len(c0_become_from_engine)>0)
		 {
		  c0_figure= c0_become_from_engine;
		 }
		else
		 {
		 if(c0_draw)
			{
			 if(window_confirm("Promote a QUEEN?"))
				{
				c0_figure = "Q";
				}
			 else if(window_confirm("Then a ROOK?"))
				{
				c0_figure = "R";
				}
			 else if(window_confirm("Maybe a BISHOP?"))
				{
				c0_figure = "B";
				}
			 else if(window_confirm("Really a KNIGHT????"))
				{
				c0_figure = "N";
				}
			 else
				{
				//Log("I know, You need a new QUEEN.");
				c0_figure = "Q";
				}
			 }	
			else c0_figure="Q";
		  }
		if(c0_draw)
			{
			c0_become=c0_figure;
																		// just put in queue... (no,will be detected above in 3D)...
			//save_c0_position=c0_position;
			//c0_moves2do = c0_moves2do + c0_convE2(c0_vert2,c0_horiz2) + "=" + c0_become;
			//c0_position=save_c0_position;
			}
		c0_position = c0_ReplaceAll(c0_position, "p" + c0_convE2(c0_vert,c0_horiz),
			 c0_figure + c0_convE2(c0_vert,c0_horiz) );
		}
	 if(c0_p2<0 && c0_horiz!=c0_horiz2)
		{
		if(c0_draw)
			{
			save_c0_position=c0_position;
			c0_clear_at( c0_convE2(c0_vert,c0_horiz2) );
			c0_position=save_c0_position;
			}
		c0_p3=InStr(c0_position, c0_convE2(c0_vert,c0_horiz2) );
		c0_position = Mid(c0_position,0,c0_p3-2) + Mid2(c0_position,c0_p3+3);
		}
	 if((c0_vert==2 && c0_vert2==4) || (c0_vert==7 && c0_vert2==5)) c0_lastmovepawn = c0_horiz;
	}

 c0_position = c0_ReplaceAll(c0_position, c0_convE2(c0_vert,c0_horiz), c0_convE2(c0_vert2,c0_horiz2) );

 if(c0_draw)
  {
  c0_moveslist = c0_moveslist + c0_convE2(c0_vert,c0_horiz) + c0_convE2(c0_vert2,c0_horiz2);
  if(Len(c0_become)>0) c0_moveslist = c0_moveslist + "[" + c0_become + "]";

  c0_and_promote_or_castle();
  }

}

//-------------------------------------------------
public string c0_D_last_move_was()
{
string c0_ret;
c0_ret="";
if( Len(c0_moveslist)>0 )
 {
 if (Mid(c0_moveslist, Len(c0_moveslist)-1, 1 )=="]" ) c0_ret= Mid(c0_moveslist, Len(c0_moveslist)-7, 7 );
 else c0_ret= Mid(c0_moveslist, Len(c0_moveslist)-4, 4 );
 }
return c0_ret;
}

//-------------------------------------------------
public void c0_take_back()
{
string c0_movespre;
c0_movespre="";
if( Len(c0_moveslist)>0 )
 {
 if( Mid(c0_moveslist, Len(c0_moveslist)-1, 1 )=="]" ) c0_movespre= Mid(c0_moveslist, 0, Len(c0_moveslist)-7 );
 else c0_movespre= Mid(c0_moveslist, 0, Len(c0_moveslist)-4 );
 }

c0_set_start_position( c0_movespre );
}


//-------------------------------------------------
public bool c0_D_is_empty(string c0_Zstr)
{
string c0_Zs2;
c0_Zs2=c0_convH888(c0_Zstr);
return c0_is_empty( Int(Mid(c0_Zs2,0,1)), Int(Mid(c0_Zs2,1,1)));
}

//-------------------------------------------------
public bool c0_is_empty(int c0_Zvert, int c0_Zhoriz)
{
 bool c0_good;
 int c0_pz2;
 c0_good = true;
 if(c0_Zvert<1 || c0_Zvert>8 || c0_Zhoriz<1 || c0_Zhoriz>8) c0_good=false;
 else
  {
   c0_pz2=InStr( c0_position, c0_convE2(c0_Zvert,c0_Zhoriz) );
   if(c0_pz2>=0) c0_good=false;
  }
 return c0_good;
}


//-------------------------------------------------
public string c0_D_what_at(string c0_Zstr1)
{
 string c0_ret;
 int c0_pz2;
 c0_ret="";
 c0_pz2=InStr(c0_position, c0_Zstr1 );
 if(c0_pz2>=0) c0_ret=Mid(c0_position,c0_pz2-2,2);
 return c0_ret;
}


//-------------------------------------------------
public bool c0_D_is_enemy(string c0_Zstr,string c0_mycolor)
{
string c0_Zs2;
c0_Zs2=c0_convH888(c0_Zstr);
return c0_is_enemy( Int(Mid(c0_Zs2,0,1)),Int(Mid(c0_Zs2,1,1)), c0_mycolor);
}


//-------------------------------------------------
public bool c0_is_enemy(int c0_Zvert, int c0_Zhoriz, string c0_mycolor)
{
 bool c0_is_there;
 int c0_pz2;
 c0_is_there =false;
 if(c0_Zvert>=1 && c0_Zvert<=8 && c0_Zhoriz>=1 && c0_Zhoriz<=8)
  {
   c0_pz2=InStr(c0_position, c0_convE2(c0_Zvert,c0_Zhoriz) );

   if(c0_pz2>=0 && Mid(c0_position,c0_pz2-2,1)!=c0_mycolor) c0_is_there=true;
  }
 return c0_is_there;
}


//-------------------------------------------------
public bool c0_D_is_emptyline(string c0_Zstr1, string c0_Zstr2 )
{
string c0_Zs1, c0_Zs2;
c0_Zs1=c0_convH888(c0_Zstr1);
c0_Zs2=c0_convH888(c0_Zstr2);
return c0_is_emptyline( Int(Mid(c0_Zs1,0,1)), Int(Mid(c0_Zs1,1,1)) , Int(Mid(c0_Zs2,0,1)), Int(Mid(c0_Zs2,1,1)));
}

//-------------------------------------------------
public bool c0_is_emptyline(int c0_Zvert, int c0_Zhoriz, int c0_Zvert2, int c0_Zhoriz2)
{
 bool c0_good;
 int c0_DZvert, c0_DZhoriz, c0_PZvert, c0_PZhoriz;
 c0_good = true;
 c0_DZvert=c0_Zvert2-c0_Zvert; if(c0_DZvert<0) c0_DZvert=-1; else if(c0_DZvert>0) c0_DZvert=1;
 c0_DZhoriz=c0_Zhoriz2-c0_Zhoriz; if(c0_DZhoriz<0) c0_DZhoriz=-1; else if(c0_DZhoriz>0) c0_DZhoriz=1;
 c0_PZvert=c0_Zvert+c0_DZvert;
 c0_PZhoriz=c0_Zhoriz+c0_DZhoriz;
 while( c0_PZvert!=c0_Zvert2 || c0_PZhoriz!=c0_Zhoriz2 )
	{
	if( !c0_is_empty( c0_PZvert, c0_PZhoriz ) )
		{
		 c0_good=false;
		 break;
		}		
	c0_PZvert+=c0_DZvert;
	c0_PZhoriz+=c0_DZhoriz;
	}
 return c0_good;
}


//-------------------------------------------------
public bool c0_D_is_check_to_king(string c0_ZKcolor)
{
return c0_is_check_to_king(c0_ZKcolor);
}

//-------------------------------------------------
public bool c0_is_check_to_king(string c0_ZKcolor)
{
 bool c0_is_check;
 int c0_Zp, c0_ZKhoriz, c0_ZKvert, c0_i, c0_Zhoriz, c0_Zvert;
 string c0_Zcolor, c0_ZK_at, c0_Z_at;

 c0_is_check=false;
 c0_Zp=InStr(c0_position,c0_ZKcolor + "K");
 c0_ZKhoriz=Asc(Mid(c0_position,c0_Zp+2,1)) - 96;
 c0_ZKvert=Int(Mid(c0_position,c0_Zp+3,1));
 c0_ZK_at = Str(c0_ZKvert) + Str(c0_ZKhoriz);

 for(c0_i=0;Len(c0_position)>c0_i; c0_i+=5)
	{
	c0_Zcolor=Mid(c0_position,c0_i,1);
	
	if(c0_Zcolor!=c0_ZKcolor)
		{
		 c0_Zhoriz=Asc(Mid(c0_position,c0_i+2,1)) - 96;
		 c0_Zvert=Int(Mid(c0_position,c0_i+3,1));
		 c0_Z_at = Str(c0_Zvert) + Str(c0_Zhoriz);

		 if(c0_can_be_moved( c0_Z_at, c0_ZK_at, true))
			{
			 c0_is_check=true;
			 break;
			}
		}
	}
 return c0_is_check;
}


//-------------------------------------------------
public bool c0_is_attacked_king_before_move(string c0_Qfrom_at, string c0_Qto_at, string c0_Qcolor)
{
  bool c0_is_attack;
  string c0_save_position, c0_save_become;
  bool c0_save_wKingmoved, c0_save_bKingmoved, c0_save_wLRockmoved, c0_save_wRRockmoved;
  bool c0_save_bLRockmoved, c0_save_bRRockmoved, c0_save_w00, c0_save_b00;
  int c0_save_sidemoves, c0_save_lastmovepawn;

  c0_is_attack=false;

  c0_save_position=c0_position;
  c0_save_sidemoves=c0_sidemoves;
  c0_save_wKingmoved=c0_wKingmoved;
  c0_save_bKingmoved=c0_bKingmoved;
  c0_save_wLRockmoved=c0_wLRockmoved;
  c0_save_wRRockmoved=c0_wRRockmoved;
  c0_save_bLRockmoved=c0_bLRockmoved;
  c0_save_bRRockmoved=c0_bRRockmoved;
  c0_save_w00=c0_w00;
  c0_save_b00=c0_b00;
  c0_save_become=c0_become;

  c0_save_lastmovepawn=c0_lastmovepawn;

  c0_moveto(c0_Qfrom_at, c0_Qto_at, false);
  c0_sidemoves=-c0_sidemoves;

  if( c0_is_check_to_king(c0_Qcolor) )
	{
	c0_is_attack=true;
	}

  c0_position=c0_save_position;
  c0_sidemoves=c0_save_sidemoves;
  c0_wKingmoved=c0_save_wKingmoved;
  c0_bKingmoved=c0_save_bKingmoved;
  c0_wLRockmoved=c0_save_wLRockmoved;
  c0_wRRockmoved=c0_save_wRRockmoved;
  c0_bLRockmoved=c0_save_bLRockmoved;
  c0_bRRockmoved=c0_save_bRRockmoved;
  c0_lastmovepawn=c0_save_lastmovepawn;
  c0_w00=c0_save_w00;
  c0_b00=c0_save_b00;
  c0_become=c0_save_become;

  return c0_is_attack;
}


//-------------------------------------------------
public bool c0_D_is_mate_to_king(string c0_ZKcolor)
{
return c0_is_mate_to_king(c0_ZKcolor, false);
}

//-------------------------------------------------
public bool c0_is_mate_to_king(string c0_ZKcolor, bool c0_just_mate)
{
 bool c0_is_mate;
 int c0_i, c0_Zhoriz, c0_Zvert, c0_vi, c0_vj;
 string c0_Z_at, c0_Z_to_at, c0_Zcolor;

 c0_is_mate=false;

 if( c0_just_mate || c0_is_check_to_king(c0_ZKcolor) )
  {
   c0_i=0;
   for(c0_is_mate=true;c0_is_mate && (Len(c0_position)>c0_i); c0_i+=5)
	{
	c0_Zcolor=Mid(c0_position,c0_i,1);
	if(c0_Zcolor==c0_ZKcolor)
		{
		 c0_Zhoriz=Asc(Mid(c0_position,c0_i+2,1)) - 96;
		 c0_Zvert=Int(Mid(c0_position,c0_i+3,1));
		 c0_Z_at = Str(c0_Zvert) + Str(c0_Zhoriz);

		 for(c0_vi=1;c0_is_mate && c0_vi<=8;c0_vi++)
		  for(c0_vj=1;c0_is_mate && c0_vj<=8;c0_vj++)
			{
			c0_Z_to_at=Str(c0_vi) + Str(c0_vj);
			if(c0_can_be_moved( c0_Z_at, c0_Z_to_at, false))
				{
				 c0_is_mate=false;
				 break;
				}
			}
		}
	}

  } 
 return c0_is_mate;
}

//-------------------------------------------------
public bool c0_D_is_pate_to_king(string c0_ZWcolor)
{
return c0_is_pate_to_king(c0_ZWcolor) && !c0_is_mate_to_king(c0_ZWcolor, false);
}

//-------------------------------------------------
public bool c0_is_pate_to_king(string c0_ZWcolor)
{
 bool c0_is_pate;
 int c0_j, c0_Whoriz, c0_Wvert, c0_wi, c0_wj;
 string c0_Wcolor, c0_W_at, c0_W_to_at;
 c0_is_pate=true;

 for(c0_j=0;c0_is_pate && Len(c0_position)>c0_j; c0_j+=5)
	{
	c0_Wcolor=Mid(c0_position,c0_j,1);
	if(c0_Wcolor==c0_ZWcolor)
		{
		 c0_Whoriz=Asc(Mid(c0_position,c0_j+2,1)) - 96;
		 c0_Wvert=Int(Mid(c0_position,c0_j+3,1));
		 c0_W_at = Str(c0_Wvert) + Str(c0_Whoriz);
		 for(c0_wi=1;c0_is_pate && c0_wi<=8;c0_wi++)
		  for(c0_wj=1;c0_is_pate && c0_wj<=8;c0_wj++)
			{
			c0_W_to_at=Str(c0_wi) + Str(c0_wj);
			if(c0_can_be_moved( c0_W_at, c0_W_to_at, false))
				{
				 c0_is_pate=false;
				 break;
				}
			}
		}
	}

 return c0_is_pate;
}


//-------------------------------------------------
public bool c0_D_can_be_moved(string c0_Zstr1, string c0_Zstr2)
{
return c0_can_be_moved( c0_convH888(c0_Zstr1), c0_convH888(c0_Zstr2), false);
}


//-------------------------------------------------
public bool c0_can_be_moved(string c0_from_at, string c0_to_at, bool c0_just_move_or_eat)
{
 bool c0_can;
 int c0_vert, c0_horiz, c0_vert2, c0_horiz2, c0_p, c0_Dvert, c0_Dhoriz, c0_virziens;
 string c0_color, c0_figure;

 c0_can = false;
 c0_vert = Int(Mid(c0_from_at,0,1));		
 c0_horiz= Int(Mid(c0_from_at,1,1));
 c0_vert2 = Int(Mid(c0_to_at,0,1));
 c0_horiz2= Int(Mid(c0_to_at,1,1));

 c0_p=InStr(c0_position, c0_convE2(c0_vert,c0_horiz) );
 if(c0_p>=0)
 {
 c0_color=Mid(c0_position,c0_p-2,1);
 c0_figure=Mid(c0_position,c0_p-1,1);

 if(c0_is_empty(c0_vert2,c0_horiz2) || c0_is_enemy(c0_vert2,c0_horiz2,c0_color))
 {
 c0_Dvert=c0_vert2-c0_vert; if(c0_Dvert<0) c0_Dvert=-c0_Dvert;
 c0_Dhoriz=c0_horiz2-c0_horiz; if(c0_Dhoriz<0) c0_Dhoriz=-c0_Dhoriz;

 if(c0_figure=="p")
	{
	if( c0_color=="w" ) c0_virziens=1; else c0_virziens=-1;
	if(c0_horiz2==c0_horiz)
	 {
	  if( (c0_vert2==c0_vert+c0_virziens && c0_is_empty(c0_vert2,c0_horiz2)) ||
	   (c0_color=="w" && c0_vert2==4 && c0_vert==2 && c0_is_empty(3,c0_horiz2) && c0_is_empty(4,c0_horiz2)) ||
	   (c0_color=="b" && c0_vert2==5 && c0_vert==7 && c0_is_empty(5,c0_horiz2) && c0_is_empty(6,c0_horiz2)) )
		c0_can = true;
	 }
	else
	 {
	  if( (c0_horiz2==c0_horiz+1 || c0_horiz2==c0_horiz-1) && c0_vert2==c0_vert+c0_virziens)
	    if(c0_is_enemy(c0_vert2,c0_horiz2,c0_color) ||
		 (c0_lastmovepawn==c0_horiz2 && 
			((c0_color=="w" && c0_vert2==6) || (c0_color=="b" && c0_vert2==3)) ) ) c0_can=true;
	 }
	}
 if(c0_figure=="N")
	{
	if( c0_Dvert+c0_Dhoriz==3 && c0_Dvert!=0 && c0_Dhoriz!=0 ) c0_can=true;
	}
 if(c0_figure=="B")
	{
	if( (c0_Dvert>0 && c0_Dvert==c0_Dhoriz) && c0_is_emptyline(c0_vert,c0_horiz,c0_vert2,c0_horiz2)) c0_can=true;			
	}
 if(c0_figure=="R")
	{
	if( ((c0_Dvert==0||c0_Dhoriz==0) && c0_Dvert!=c0_Dhoriz) && c0_is_emptyline(c0_vert,c0_horiz,c0_vert2,c0_horiz2)) c0_can=true;	
	}
 if(c0_figure=="Q")
	{
	if( (c0_Dvert==0||c0_Dhoriz==0||c0_Dvert==c0_Dhoriz) && c0_is_emptyline(c0_vert,c0_horiz,c0_vert2,c0_horiz2)) c0_can=true;	
	}
 if(c0_figure=="K")
	{
	if((c0_Dvert==0 && c0_Dhoriz==1)||(c0_Dhoriz==0 && c0_Dvert==1)||(c0_Dhoriz==1 && c0_Dvert==1)) c0_can=true;
	else 
	 if (!c0_just_move_or_eat && !c0_is_check_to_king(c0_color) && (!c0_fischer))
		{
		if(c0_color=="w")
		 {
		  if(!c0_wKingmoved && c0_vert==1 && c0_horiz==5 && c0_vert2==1)
			{
			if( (c0_horiz2==7 && !c0_wRRockmoved &&
				c0_is_empty(1,6) && c0_is_empty(1,7) &&
				!c0_is_attacked_king_before_move("15", "16", c0_color) &&
				!c0_is_attacked_king_before_move("15", "17", c0_color)) ||
			    (c0_horiz2==3 && !c0_wLRockmoved &&
				c0_is_empty(1,2) && c0_is_empty(1,3) && c0_is_empty(1,4) &&
				!c0_is_attacked_king_before_move("15", "14", c0_color) &&
				!c0_is_attacked_king_before_move("15", "13", c0_color)) ) c0_can=true;
			}
		 }
		else
		 {
		  if(!c0_bKingmoved && c0_vert==8 && c0_horiz==5 && c0_vert2==8)
			{
			if( (c0_horiz2==7 && !c0_bRRockmoved &&
				c0_is_empty(8,6) && c0_is_empty(8,7) &&
				!c0_is_attacked_king_before_move("85", "86", c0_color) &&
				!c0_is_attacked_king_before_move("85", "87", c0_color)) ||
			    (c0_horiz2==3 && !c0_bLRockmoved &&
				c0_is_empty(8,2) && c0_is_empty(8,3) && c0_is_empty(8,4) &&
				!c0_is_attacked_king_before_move("85", "84", c0_color) &&
				!c0_is_attacked_king_before_move("85", "83", c0_color)) ) c0_can=true;
			}
		 }
		}
	}
 if(!c0_just_move_or_eat && c0_can)
 {
  c0_can = !c0_is_attacked_king_before_move(c0_from_at, c0_to_at, c0_color);
 }
 }
 }
 return c0_can;
}

//---------------------------------------
//  public to get next possible moves
//---------------------------------------
public string c0_get_next_moves()
{
 string c0_Dposs;
 int c0_Da, c0_Dhoriz, c0_Dvert;
 string c0_Dcolor, c0_Dfigure, c0_Dfrom_move;
 c0_Dposs="";
 for(c0_Da=0;Len(c0_position)>c0_Da; c0_Da+=5)
	{
	c0_Dcolor=Mid(c0_position,c0_Da,1);
	if((c0_sidemoves>0 && c0_Dcolor=="w")||(c0_sidemoves<0 && c0_Dcolor=="b"))
		{
		c0_Dfigure=Mid(c0_position,c0_Da+1,1);
		c0_Dhoriz=Asc(Mid(c0_position,c0_Da+2,1)) - 96;
		c0_Dvert=Int(Mid(c0_position,c0_Da+3,1));
		c0_Dfrom_move=Str(c0_Dvert) + Str(c0_Dhoriz);

		if(c0_Dfigure=="p")
			{
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,c0_sidemoves,0,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,(2*c0_sidemoves),0,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,c0_sidemoves,1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,c0_sidemoves,-1,1);
			}
		if(c0_Dfigure=="N")
			{
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,2,1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,2,-1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,2,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,-2,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,2,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,-2,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-2,1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-2,-1,1);
			}
		if(c0_Dfigure=="B" || c0_Dfigure=="Q")
			{
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,1,8);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,-1,8);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,1,8);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,-1,8);
			}
		if(c0_Dfigure=="R" || c0_Dfigure=="Q")
			{
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,0,8);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,0,8);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,0,1,8);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,0,-1,8);
			}
		if(c0_Dfigure=="K")
			{
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,0,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,1,-1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,0,1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,0,-1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,1,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,0,1);
			 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,-1,-1,1);	
			 if((c0_Dcolor=="w" && c0_Dfrom_move=="15") || (c0_Dcolor=="b" && c0_Dfrom_move=="85"))
				{
				 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,0,-2,1);	
				 c0_Dposs = c0_Dposs + c0_DCN(c0_Dfrom_move,0,2,1);	
				}
			}
		}
	}
 return c0_Dposs;
}


//---------------------------------------
//
public string c0_DCN(string c0_D7from_at, int c0_Dvert_TX, int c0_Dhoriz_TX, int c0_Dcntx)
{
 string c0_D7poss, c0_D7to_at;

 string saveD1position, saveD1become;
 bool saveD1wKingmoved, saveD1bKingmoved, saveD1wLRockmoved, saveD1wRRockmoved;
 bool saveD1bLRockmoved, saveD1bRRockmoved, saveD1w00, saveD1b00;
 int saveD1sidemoves, saveD1lastmovepawn;

 int c0_D7vert, c0_D7horiz, c0_Dj;

 saveD1sidemoves=c0_sidemoves;
 saveD1wKingmoved=c0_wKingmoved;
 saveD1bKingmoved=c0_bKingmoved;
 saveD1wLRockmoved=c0_wLRockmoved;
 saveD1wRRockmoved=c0_wRRockmoved;
 saveD1bLRockmoved=c0_bLRockmoved;
 saveD1bRRockmoved=c0_bRRockmoved;
 saveD1w00=c0_w00;
 saveD1b00=c0_b00;
 saveD1lastmovepawn=c0_lastmovepawn;
 saveD1position=c0_position;		
 saveD1become=c0_become;

 c0_D7poss="";


 c0_D7vert=Int(Mid(c0_D7from_at,0,1));
 c0_D7horiz=Int(Mid(c0_D7from_at,1,1));

 for(c0_Dj=0; c0_Dj<c0_Dcntx; c0_Dj++)
  {
   c0_D7vert+=c0_Dvert_TX;
   c0_D7horiz+=c0_Dhoriz_TX;
   if(c0_D7vert>=1 && c0_D7vert<=8 && c0_D7horiz>=1 && c0_D7horiz<=8)
    {
	c0_D7to_at=Str(c0_D7vert) + Str(c0_D7horiz);

	if( c0_can_be_moved( c0_D7from_at, c0_D7to_at, false ) )
		{
		c0_foundmove = c0_convE777( c0_D7from_at ) + c0_convE777( c0_D7to_at );
		c0_D7poss = c0_D7poss + c0_foundmove + ",";
		}

	c0_wKingmoved=saveD1wKingmoved;
	c0_bKingmoved=saveD1bKingmoved;
	c0_wLRockmoved=saveD1wLRockmoved;
	c0_wRRockmoved=saveD1wRRockmoved;
	c0_bLRockmoved=saveD1bLRockmoved;
	c0_bRRockmoved=saveD1bRRockmoved;
	c0_w00=saveD1w00;
	c0_b00=saveD1b00;

	c0_lastmovepawn=saveD1lastmovepawn;
	c0_position=saveD1position;		
	c0_sidemoves=saveD1sidemoves;		
	c0_become=saveD1become;
   }
  }
 return c0_D7poss;
}

//----------------------------------------------------------------------------
//	PGN support part...
//----------------------------------------------------------------------------

//------------- Analyse PGN ...

public void c0_PG_gettable()
{
string str2, buf2, buf3, htms, Event_Name, Event_Site, Event_Date, Roundv;
string White, Black, Result, ECO, WhiteElo, BlackElo, Game_Date, Source_Date, AddInfo;

int at2, at3, at2_1, at2_2, at9;

Event_Name="";
Event_Site="";
Event_Date="";
Roundv="";
White="";
Black="";
Result="";
ECO="";
WhiteElo="";
BlackElo="";
Game_Date="";
Source_Date="";

AddInfo="";

htms="";

for (int h2 = 0; h2 < c0_PGN_header.Length; h2++) c0_PGN_header[h2] = "";

PGN_text= c0_ReplaceAll( PGN_text,"  ", " " );

str2=PGN_text;

while(true)
 {
 at2=InStr(str2,"[");
 if(at2<0) break;

 at2_1=InStr(str2,"(");
 at2_2=InStr(str2,"{");
 if((at2_1>=0 && at2_1<at2) || (at2_2>=0 && at2_2<at2)) break;

 buf2= Mid2(str2,at2+1);
 buf2= Mid(buf2,0, InStr(buf2,"]"));
 str2= Mid2(str2,at2+Len(buf2)+2);

 for (int h2 = 0; h2 < c0_PGN_header.Length; h2++)
 { if (c0_PGN_header[h2].Length == 0) { c0_PGN_header[h2] = buf2; break; }; }

 buf2= c0_ReplUrl(buf2);
 
 buf2= c0_ReplaceAll( buf2,"'","" );
 buf2= c0_ReplaceAll( buf2,Chr(34),"" );
 buf2= c0_ReplaceAll( buf2,"–","-" );
  
 buf3=Caps(buf2);

 at9 = InStr(buf3,"SETUP ");
 if(at9>=0 && at9<3) { c0_fischer=(Mid(buf2,at9+6,1)=="1"); }

 at3 = InStr(buf3,"FEN ");
 if(at3>=0 && at3<3)
 	{ if( Len(c0_start_FEN)==0 ) { c0_start_FEN=Mid2(buf2,at3+4); c0_set_start_position(""); } }
 else
 	{
 	at3 = InStr(buf3,"EVENT ");
 	if(at3>=0) Event_Name=Mid2(buf2,at3+6);
 	if(at3<0) { at3 = InStr(buf3,"SITE "); if(at3>=0) Event_Site=Mid2(buf2,at3+5); }
 	if(at3<0) { at3 = InStr(buf3,"DATE ");	if(at3>=0 && at3<3) Game_Date=Mid2(buf2,at3+5); }
 	if(at3<0) { at3 = InStr(buf3,"ROUND "); if(at3>=0) Roundv=Mid2(buf2,at3+6); }
 	if(at3<0) { at3 = InStr(buf3,"WHITE "); if(at3>=0) White=Mid2(buf2,at3+6); }
 	if(at3<0) { at3 = InStr(buf3,"BLACK "); if(at3>=0) Black=Mid2(buf2,at3+6); }
 	if(at3<0) { at3 = InStr(buf3,"ECO "); if(at3>=0) ECO=Mid2(buf2,at3+4); }
 	if(at3<0) { at3 = InStr(buf3,"WHITEELO "); if(at3>=0) WhiteElo=Mid2(buf2,at3+9); }
 	if(at3<0) { at3 = InStr(buf3,"BLACKELO "); if(at3>=0) BlackElo=Mid2(buf2,at3+9); }
 	if(at3<0) { at3 = InStr(buf3,"EVENTDATE "); if(at3>=0) Event_Date=Mid2(buf2,at3+10); }
 	if(at3<0) { at3 = InStr(buf3,"SOURCEDATE "); if(at3>=0) Source_Date=Mid2(buf2,at3+11); }
 	if(at3<0) { at3 = InStr(buf3,"RESULT "); if(at3>=0) Result=Mid2(buf2,at3+7); }
 	if(at3<0)
		{
		if(Len(AddInfo)>0) AddInfo = AddInfo + "<BR>";
		AddInfo = AddInfo + buf2;
	 	}
	}

 }

 str2= c0_ReplUrl(str2);
 
 c0_errflag=c0_PG_parseString(str2);
 if(c0_fischer && Len(c0_fischer_cst)>0) c0_fischer_adjustmoved();
 
 at3 = InStr(str2,"*");
 if(at3>=0) Result="not finished";
 at3 = InStr(str2,"1/2");
 if(at3>=0)  Result="1/2-1/2";
 at3 = InStr(str2,"1-0");
 if(at3>=0)  Result="1:0";
 at3 = InStr(str2,"1:0");
 if(at3>=0) Result="1:0";
 at3 = InStr(str2,"0-1");
 if(at3>=0) Result="0:1";
 at3 = InStr(str2,"0:1");
 if(at3>=0) Result="0:1";


 htms=Event_Name + Event_Site + Event_Date + Roundv + White + 
	Black + Result + ECO + WhiteElo + BlackElo + Game_Date + Source_Date + AddInfo;
 htms=htms + " just can use it for something";

}

//------------------------------ PGN parser on chess moves
public string c0_ReplUrl(string str)				// Replaces urls to links...
{
string str2, urls;
int at, at2;

str2=str;
while(true)
{
 urls="";
 at=InStr(str2,"http://");
 if(at>=0) urls="HTTP://" + Mid2(str2,at+7);
 else
   {
   at=InStr(str2,"https://");
   if(at>=0) urls="HTTPS://" + Mid2(str2,at+8);
   }
  if(Len(urls)>0)
   {
   at2=InStr(urls," ");
   if(at2>=0) urls=Mid(urls,0,at2);

   str2=Mid(str2,0,at) + "<a href='" + urls + "' target='blank' >link»</a>" + Mid2(str2,at +Len(urls));
   }
  else break;
}

str2= c0_ReplaceAll( str2, "HTTP://", "http://" );
str2= c0_ReplaceAll( str2, "HTTPS://", "https://" );

return(str2);
}


//------------------------------ PGN parser on chess moves

public string c0_get_moves_from_PGN(string c0_PGN_str)	// Parses PGN moves from string variable own string for chess moves...
{
PGN_text= c0_PGN_str;

c0_PG_gettable();

if(c0_errflag) Log("There was an error in PGN parsing!");

return c0_PG_1;
}


//------------------------------ PGN parser on chess moves

public bool c0_PG_parseString(string str)			// Parses own string for chess moves...
{
bool f_error;
int gaj, k, i, j, st_gaj, st_atq, atwas, atcnt, Nag_at, Nag_at2, cc;
string move, color7, resultl, commentv, c_v, c, c1, c2, pj1, reminder, st_s, st_c, Nag, Nag_txt;

string move2, from_move, to_move;
int from_horiz4, from_vert4, to_horiz4, to_vert4;

string c0_1save_position, c0_1save_become, c0_1save_become_from_engine, c0_1save_moveslist;
bool c0_1save_wKingmoved, c0_1save_bKingmoved, c0_1save_wLRockmoved, c0_1save_wRRockmoved;
bool c0_1save_bLRockmoved, c0_1save_bRRockmoved, c0_1save_w00, c0_1save_b00;
int c0_1save_sidemoves, c0_1save_lastmovepawn;

f_error=false;
gaj=1;
move="";
color7="w";
resultl="[1:0][1-0][1 : 0][1 - 0][0:1][0-1][0 : 1][0 - 1][1/2][1 / 2] [0.5:0.5][1/2:1/2][1/2-1/2][1/2 - 1/2][1/2 : 1/2][*]";

commentv="";

c0_PG_1="";

if(Len(c0_NAGs)==0) c0_NAGs_define();

c0_1save_position=c0_position;
c0_1save_sidemoves=c0_sidemoves;
c0_1save_wKingmoved=c0_wKingmoved;
c0_1save_bKingmoved=c0_bKingmoved;
c0_1save_wLRockmoved=c0_wLRockmoved;
c0_1save_wRRockmoved=c0_wRRockmoved;
c0_1save_bLRockmoved=c0_bLRockmoved;
c0_1save_bRRockmoved=c0_bRRockmoved;
c0_1save_w00=c0_w00;
c0_1save_b00=c0_b00;
c0_1save_become=c0_become;
c0_1save_become_from_engine=c0_become_from_engine;
c0_1save_lastmovepawn= c0_lastmovepawn;
c0_1save_moveslist= c0_moveslist;

if( Len(c0_start_FEN)>0 ) {  str= ( "{[FEN " + c0_start_FEN + "]} " ) + str; if(c0_sidemoves<0) color7="b";  }
else
{
c0_position = "wpa2,wpb2,wpc2,wpd2,wpe2,wpf2,wpg2,wph2," + 
"wRa1,wNb1,wBc1,wQd1,wKe1,wBf1,wNg1,wRh1," + 
"bpa7,bpb7,bpc7,bpd7,bpe7,bpf7,bpg7,bph7," + 
"bRa8,bNb8,bBc8,bQd8,bKe8,bBf8,bNg8,bRh8,";

c0_moveslist = "";

c0_wKingmoved = false;
c0_bKingmoved = false;
c0_wLRockmoved = false;
c0_wRRockmoved = false;
c0_bLRockmoved = false;
c0_bRRockmoved = false;
c0_w00 = false;
c0_b00 = false;

c0_lastmovepawn = 0;
c0_sidemoves=1;
}

c0_become="";
c0_become_from_engine="";
c_v="0123456789";
k=0;
reminder="";

st_gaj=1;
st_atq=InStr(str,".")-1;
if(st_atq>=0)
 {
  for(st_s=""; st_atq>=0; st_atq--)
	{
	 st_c=Mid(str,st_atq,1);
	 if( InStr(c_v,st_c) < 0 ) break;
	 st_s=st_c + st_s;		
	}
 if(Len(st_s)>0) st_gaj=Int(st_s);
 }

for(i=Len(str);i>0;i--) if( Mid2(str,i-1)!=" " ) break;
str=Mid(str,0,i);

atwas=-1;
atcnt=0;
Nag="";
Nag_txt="";
Nag_at2=0;
		
for(i=0;i<Len(str);i++)
 {
 if( atwas<i ) { atwas=i; atcnt=0; }
 else if( atwas<=i ) atcnt++;
 if( atcnt>50 ) { if(c0_PG_viewer) Log("Sorry, can't parse this PGN! Errors inside."); f_error=true; break;  }

 c=Mid(str,i,1);
 while(c==" " && (i+1)<Len(str) && Mid(str,i+1,1)==" ") { i++; c=Mid(str,i,1); }
 if( c==" " && (i+1)<Len(str) && InStr("{([$", Mid(str,i+1,1) )>=0) { i++; c=Mid(str,i,1); }

 commentv="";

 if(c=="$")
	{
	 Nag= Mid(str,i,3);
	 for(k=0; k<Len(Nag); k++)
		{
		c=Mid(Nag,k,1);
		if( InStr(c_v,c) < 0 ) { Nag=Mid(Nag,0,k); break; }
		}
	 if(Len(Nag)>0)
		{
		Nag_txt="";
		Nag_at2 = InStr(c0_NAGs,"[" + Nag + "]");
		if(Nag_at2>=0)
			{
			 Nag_txt = Mid2(c0_NAGs,Nag_at2+Len(Nag)+3);
			 Nag_txt = Mid(Nag_txt,0, InStr(Nag_txt,"[")-1);
			}
		else Nag_txt = "Nag:" + Nag;
		str=Mid(str,0,i) +  "{" + "[" + Nag_txt + "]" + "}" + Mid2(str,i+Len(Nag)+1);
		}
	  c=Mid(str,i,1);
	  }

 if(c=="{" || c=="(")
  {
   cc=1;
   c1=")";
   if(c=="{") c1="}";
   commentv=c;
   for(i++;i<Len(str) && cc>0;i++)
	{
	c2=Mid(str,i,1);
	commentv = commentv + c2;
	if(c2==c) cc++;
	if(c2==c1) cc--;
	if(i+1==Len(str) && cc>0) commentv = commentv + c1;
	}
  if(Len(commentv)>0)
	{
	while(true)
	  {	
	   Nag_at=InStr(commentv,"$");
	   if( Nag_at<0) break;
	   Nag= Mid(commentv,Nag_at+1,3);
	   for(k=0; k<Len(Nag); k++)
		{
		c=Mid(Nag,k,1);
		if( InStr(c_v,c) < 0 ) { Nag=Mid(Nag,0,k); break; }
		}
	  if(Len(Nag)>0)
		{
		Nag_txt="";
		Nag_at2 = InStr(c0_NAGs,"[" + Nag + "]");
		if(Nag_at2>=0)
			{
			 Nag_txt = Mid2(c0_NAGs,Nag_at2+Len(Nag)+3);
			 Nag_txt = Mid(Nag_txt,0, InStr(Nag_txt,"[")-1);
			}
		else Nag_txt = "Nag:" + Nag;

		commentv=Mid(commentv,0,Nag_at) + "[" + Nag_txt + "]" + Mid2(commentv,Nag_at + Len(Nag)+1);
		}
	 else break;
	  }

	}
  if( color7=="b" )
	{
	for( j=i; j<i+15; j++)
		{
		pj1=Mid(str,j,1);
		if( InStr("{([$",pj1)>=0 ) break;
		if( Mid(str,j,3)=="..." ) { i=j+3; break; }
		}
	}
  i--;
  }

 else if( c=="."  || (c==" " && color7=="b"  ) )
     {
     for(move="";i<Len(str) && (Mid(str,i,1)==" " || Mid(str,i,1)==".");i++);

     for(c=Mid(str,i,1);i<Len(str);i++)
	{
	c=Mid(str,i,1);
	if( c==" "  ) break;
	move = move + c;
	}
     if(Len(move)>0 && InStr(move,"Z0")<0)
	{
	if(InStr(resultl,move)>=0 )
		{
		break;
		}
	else
	 {
	 
	move2=c0_from_Crafty_standard(move,color7);
	
	if(Len(move2)==0) { if(c0_PG_viewer) Log("Can't parse this PGN! move:" + Str(gaj) + "." + color7 + " " + move);
				 f_error=true; break;  }

	from_horiz4=Asc(Mid(move2,0,1)) - 96;
	from_vert4=Int(Mid(move2,1,1));
	to_horiz4=Asc(Mid(move2,2,1)) - 96;
	to_vert4=Int(Mid(move2,3,1));

	from_move = Str(from_vert4) + Str(from_horiz4);
	to_move = Str(to_vert4) + Str(to_horiz4);

	if(Len(move2)>4 && Mid(move2,4,1)=="[") c0_become_from_engine=Mid(move2,5,1);
	else c0_become_from_engine="Q";

	if(c0_fischer) c0_fischer_cstl_move(move2,false);
	else c0_moveto(from_move, to_move, false);
	c0_sidemoves=-c0_sidemoves;

	c0_PG_1 = c0_PG_1 + move2;

	c0_become_from_engine="";
	c0_become="";
	
	if( color7=="w" )
		{
		color7="b"; i--;
		}
	else
		{
		color7="w"; 
		gaj++;
		}

	if( color7=="w" && Len(str)-i<10)
		        {
		        reminder = Mid2(str,i+1);
		        while(Len(reminder)>0 && Mid(reminder,0,1)==" ") reminder=Mid2(reminder,1);
		        if(Len(reminder)>0 && InStr(resultl,reminder)>=0 )
					{
					break;
					}
		         }

	 }
	}
     }
 else
    {
     if(Len(str)-i<10)
        {
        reminder = Mid2(str,i);
        while(Len(reminder)>0 && Mid(reminder,0,1)==" ") reminder=Mid2(reminder,1);;
        if(Len(reminder)>0 && InStr(resultl,reminder)>=0 )
					{
					break;
					}
        }
    }
 }

c0_position=c0_1save_position;
c0_sidemoves=c0_1save_sidemoves;
c0_wKingmoved=c0_1save_wKingmoved;
c0_bKingmoved=c0_1save_bKingmoved;
c0_wLRockmoved=c0_1save_wLRockmoved;
c0_wRRockmoved=c0_1save_wRRockmoved;
c0_bLRockmoved=c0_1save_bLRockmoved;
c0_bRRockmoved=c0_1save_bRRockmoved;
c0_w00=c0_1save_w00;
c0_b00=c0_1save_b00;
c0_become=c0_1save_become;
c0_become_from_engine=c0_1save_become_from_engine;
c0_lastmovepawn=c0_1save_lastmovepawn;
c0_moveslist=c0_1save_moveslist;

return f_error;
}

//------------------------------ just get tag string

public string c0_get_tag( string str, string tag)
{
 string ret, ctg1, ctg2;
 int at1;
 ret="";
 ctg1="[" + tag + "]";
 ctg2="[/" + tag + "]";
 at1=InStr(str,ctg1);
 if(at1>=0)
	{
	str=Mid2(str,at1+Len(ctg1));
	at1=InStr(str,ctg2);
	if(at1>=0) ret=Mid(str,0, at1);
	}
 return ret;
}



//-------------------------------------------------
// Crafty notation (quite a standard)
//-------------------------------------------------
public string c0_from_Crafty_standard(string c0_move, string c0_color47)
{

string c0_becomes7, c0_figure7, c0_ret7, c0_Z8color, c0_Z8figure, c0_Z8from_at72, c0_Z8to_at72;
int c0_sh7, c0_cp7, c0_i8, c0_Z81horiz, c0_Z82horiz, c0_Z8horiz, c0_Z8vert, c0_Z82vert;


string c0_to_at7, c0_to_at72, c0_Z4color, c0_Z4figure, c0_Z4from_at72, c0_Z4from_at7;
int c0_vert72, c0_horiz72, c0_vert71, c0_horiz71, c0_i4, c0_Z4horiz, c0_Z4vert;


c0_move=c0_ReplaceAll( c0_move, "ep", "" );
c0_move=c0_ReplaceAll( c0_move, "8Q", "8=Q" );
c0_move=c0_ReplaceAll( c0_move, "8R", "8=R" );
c0_move=c0_ReplaceAll( c0_move, "8B", "8=B" );
c0_move=c0_ReplaceAll( c0_move, "8N", "8=N" );
c0_move=c0_ReplaceAll( c0_move, "1Q", "1=Q" );
c0_move=c0_ReplaceAll( c0_move, "1R", "1=R" );
c0_move=c0_ReplaceAll( c0_move, "1B", "1=B" );
c0_move=c0_ReplaceAll( c0_move, "1N", "1=N" );

c0_becomes7="";
c0_sh7=InStr(c0_move,"=");

c0_ret7=c0_fischer_cst_fCr(c0_move);
if(Len(c0_ret7)>0) return c0_ret7;

else if(Len(c0_move)>4 && (Mid(c0_move,0,5)=="O-O-O" || Mid(c0_move,0,5)=="0-0-0"))
	{
	if(c0_color47=="w")
		{ 
		  if(InStr(c0_position,"wKc1")<0 && c0_can_be_moved( "15","13",false) ) c0_ret7="e1c1[0]";
	 	}
	else
		{
		  if(InStr(c0_position,"bKc8")<0 && c0_can_be_moved( "85","83",false) ) c0_ret7="e8c8[0]";
	 	}
	}
else if(Len(c0_move)>2 && (Mid(c0_move,0,3)=="O-O" || Mid(c0_move,0,3)=="0-0"))
	{
	if(c0_color47=="w")
		{ 
		  if(InStr(c0_position,"wKg1")<0 && c0_can_be_moved( "15","17",false) ) c0_ret7="e1g1[0]";
		}
	else
		{
		  if(InStr(c0_position,"bKg8")<0 && c0_can_be_moved( "85","87",false) ) c0_ret7="e8g8[0]";
		}
	}
else if( InStr("{ab}{ba}{bc}{cb}{cd}{dc}{de}{ed}{ef}{fe}{fg}{gf}{gh}{hg}", Mid(c0_move,0,2))>=0 )
  {
       c0_Z81horiz=Asc(Mid(c0_move,0,1)) - 96;
       c0_Z82horiz=Asc(Mid(c0_move,1,1)) - 96;

       for(c0_i8=0; Len(c0_position)>c0_i8; c0_i8+=5)
	{
	c0_Z8color=Mid(c0_position,c0_i8,1);
	c0_Z8figure=Mid(c0_position,c0_i8+1,1);
	c0_Z8horiz=Asc(Mid(c0_position,c0_i8+2,1)) - 96;
	c0_Z8vert=Int(Mid(c0_position,c0_i8+3,1));

	if(c0_color47=="w") c0_Z82vert=c0_Z8vert+1;
	else c0_Z82vert=c0_Z8vert-1;

	c0_Z8from_at72 = Str(c0_Z8vert) + Str(c0_Z8horiz);
	c0_Z8to_at72 = Str(c0_Z82vert) + Str(c0_Z82horiz);

	if(c0_Z8color==c0_color47 && c0_Z8figure=="p" && c0_Z81horiz==c0_Z8horiz )
		{
		if( c0_can_be_moved( c0_Z8from_at72, c0_Z8to_at72,false) )
			{
			c0_ret7=c0_convE777(c0_Z8from_at72) + c0_convE777(c0_Z8to_at72);
			break;
			}
		}
	}
       
	if(c0_sh7>=0)
	{
	c0_becomes7="[" + Mid(c0_move,c0_sh7+1,1) + "]";
	}
       c0_ret7 = c0_ret7 + c0_becomes7;
   }
else
 {
 c0_cp7=Len(c0_move);

 c0_figure7=Mid(c0_move,0,1);
 if(c0_figure7=="N" || c0_figure7=="B" || c0_figure7=="R" || c0_figure7=="Q" || c0_figure7=="K") c0_move = Mid2(c0_move,1);
 else c0_figure7="p";

 if(c0_sh7>=0)
	{
	c0_becomes7="[" + Mid(c0_move,c0_sh7+1,1) + "]";
	c0_move = Mid(c0_move, 0, c0_sh7);
	}
 c0_move=c0_ReplaceAll( c0_move, "+", "" );
 c0_move=c0_ReplaceAll( c0_move, "-", "" );
 c0_move=c0_ReplaceAll( c0_move, "x", "" );
 c0_move=c0_ReplaceAll( c0_move, "X", "" );
 c0_move=c0_ReplaceAll( c0_move, "#", "" );
 c0_move=c0_ReplaceAll( c0_move, "!", "" );
 c0_move=c0_ReplaceAll( c0_move, "?", "" );

 c0_cp7=Len(c0_move);
 c0_cp7--;	
 c0_to_at7 = Mid(c0_move,c0_cp7-1,2);
 c0_vert72=Int(Mid(c0_move,c0_cp7--,1));
 c0_horiz72=Asc(Mid(c0_move,c0_cp7--,1)) - 96;
 c0_to_at72 = Str(c0_vert72) + Str(c0_horiz72);

 if(c0_cp7>=0)
  {
  c0_vert71=Int(Mid(c0_move,c0_cp7,1));
  if(c0_vert71<1 || c0_vert71>8) c0_vert71=0; else c0_cp7--;
  }
  else c0_vert71=0;

 if(c0_cp7>=0)
  {
  c0_horiz71=Asc(Mid(c0_move,c0_cp7--,1)) - 96;
  if(c0_horiz71<1 || c0_horiz71>8) c0_horiz71=0;
  }
  else c0_horiz71=0;

 for(c0_i4=0;Len(c0_position)>c0_i4; c0_i4+=5)
	{
	c0_Z4color=Mid(c0_position,c0_i4,1);
	c0_Z4figure=Mid(c0_position,c0_i4+1,1);
	c0_Z4horiz=Asc(Mid(c0_position,c0_i4+2,1)) - 96;
	c0_Z4vert=Int(Mid(c0_position,c0_i4+3,1));
	c0_Z4from_at72 = Str(c0_Z4vert) + Str(c0_Z4horiz);
	c0_Z4from_at7 = Mid(c0_position,c0_i4+2,2);

	
	if(c0_Z4color==c0_color47 && c0_figure7==c0_Z4figure)
		{
		 if((c0_vert71==0 || c0_vert71==c0_Z4vert) &&
			(c0_horiz71==0 || c0_horiz71==c0_Z4horiz) )
				{
				if( c0_can_be_moved( c0_Z4from_at72,c0_to_at72,false))
					{
					c0_ret7=c0_Z4from_at7 + c0_to_at7 + c0_becomes7;
					break;
					}
				}
		}
	}
 }
return c0_ret7;
}


//============================ ADDITIONAL UPPER LEVEL publicS...

//----------------------------------
// Gets  FEN for position
//----------------------------------
public string c0_get_FEN()
{
int c0_mcount7;
int c0_vert7, c0_horz7, c0_em7, c0_at7, c0_i7;
string c0_fs1, c0_pos7, c0_color7, c0_ch7, c0_col79, c0_enpass7, c0_lmove7;
  
c0_vert7=8;
c0_horz7=1;
c0_fs1="";
c0_em7=0;
c0_at7=0;

while( c0_vert7>=1 )
 {
 for( c0_horz7=1; c0_horz7<=8; c0_horz7++ )
	{
	c0_pos7 = Chr(96+c0_horz7) + Str(c0_vert7);
	c0_at7=InStr(c0_position, c0_pos7 );
	if( c0_at7>=0 )
		{
		if( c0_em7>0 ) { c0_fs1 = c0_fs1 + Str(c0_em7); c0_em7=0; }
		c0_ch7=Mid(c0_position, c0_at7-1, 1 );
		c0_color7=Mid(c0_position, c0_at7-2, 1 );
		if( c0_color7=="w" ) c0_fs1 = c0_fs1 + Caps(c0_ch7); else c0_fs1 = c0_fs1 + LowCase(c0_ch7);
		}
	else c0_em7++;
	}
 if( c0_em7>0 ) { c0_fs1 = c0_fs1 + Str(c0_em7); c0_em7=0; }
 c0_vert7--;
 if(c0_vert7<1) break;
 c0_fs1 = c0_fs1 + "/";
 }

c0_col79="b";
if( c0_sidemoves>0 ) c0_col79="w";

c0_fs1 = c0_fs1 + " " + c0_col79 + " ";

if(  (c0_w00 || c0_wKingmoved || (c0_wLRockmoved && c0_wRRockmoved))  && 
     (c0_b00 || c0_bKingmoved || (c0_bLRockmoved && c0_bRRockmoved)) ) c0_fs1 = c0_fs1 + "- ";
else
 {
  if( !(c0_w00 || c0_wKingmoved) && !c0_wLRockmoved ) c0_fs1 = c0_fs1 + "Q";
  if( !(c0_w00 || c0_wKingmoved) && !c0_wRRockmoved ) c0_fs1 = c0_fs1 + "K";
  if( !(c0_b00 || c0_bKingmoved) && !c0_bLRockmoved ) c0_fs1 = c0_fs1 + "q";
  if( !(c0_b00 || c0_bKingmoved) && !c0_bRRockmoved ) c0_fs1 = c0_fs1 + "k";
  c0_fs1 = c0_fs1 + " ";
 }

 c0_enpass7="-";

 if(c0_lastmovepawn>0)
	{
	c0_lmove7=Mid(c0_moveslist, Len(c0_moveslist)-4, 4 );
	c0_vert7 = Asc(Mid(c0_lmove7,1,1));

	if( Mid(c0_lmove7,0,1)==Mid(c0_lmove7,2,1) &&
		(Asc(Mid(c0_lmove7,0,1))-96==c0_lastmovepawn) &&
		 (( Mid(c0_lmove7,1,1)=="7" && Mid(c0_lmove7,3,1)=="5" ) ||
		  ( Mid(c0_lmove7,1,1)=="2" && Mid(c0_lmove7,3,1)=="4" )) )
	{
	 c0_at7=InStr(c0_position, Mid(c0_lmove7,2,2) );
	 if( c0_at7>=0 && Mid(c0_position,c0_at7-1,1 )=="p" )
		{
		c0_enpass7=Mid(c0_lmove7,0,1);
		if( Mid(c0_lmove7,1,1)=="7" ) c0_enpass7=c0_enpass7 + "6"; else c0_enpass7=c0_enpass7 + "3";
		}
	}
	}
c0_fs1 = c0_fs1 + c0_enpass7 + " ";

c0_fs1 = c0_fs1 + "0 ";		// position repeating moves....

c0_mcount7=2;
c0_i7=0;
while( c0_i7<Len(c0_moveslist) )
	{
	c0_i7+=4;
	if((Len(c0_moveslist)>c0_i7) && (Mid(c0_moveslist,c0_i7,1)=="[")) c0_i7+=3;
	c0_mcount7+=1;
	}
c0_fs1 = c0_fs1 + Str(Int( Str((int)(c0_mcount7/2)) )) + " ";

return c0_fs1;
}

//----------------------------------
// Sets position using FEN
//----------------------------------
public void c0_set_FEN( string c0_fen_str )
{
int c0_vert7, c0_horz7, c0_i7, c0_pusto, c0_j7, c0_side7move, c0_at7, c0_enpass7;
string c0_fs1, c0_fs2, c0_ch7, c0_pos7, c0_color7, c0_q7;

bool c0_wK7, c0_wRL7, c0_wRR7, c0_wcastl7;
bool c0_bK7, c0_bRL7, c0_bRR7, c0_bcastl7;

c0_vert7=8;
c0_horz7=1;

c0_fs1="";
c0_fs2="";

for(c0_i7=0; c0_i7<Len(c0_fen_str); c0_i7++)
{
c0_ch7=Mid(c0_fen_str,c0_i7,1);
if( c0_ch7==" " ) break;
c0_pusto=Int(c0_ch7);
if(c0_pusto>=1 && c0_pusto<=8)  { for( c0_j7=1; c0_j7<=c0_pusto; c0_j7++) c0_fs1 = c0_fs1 + "."; }
else c0_fs1 = c0_fs1 + c0_ch7;
}
c0_fs1 = c0_fs1 + (" " + Mid2(c0_fen_str,c0_i7));

for(c0_i7=0; c0_i7<Len(c0_fs1); c0_i7++)
{
c0_ch7=Mid(c0_fs1,c0_i7,1);
if( c0_ch7==" " ) break;

c0_pos7 = Chr(96+c0_horz7) + Str(c0_vert7);
c0_color7=" ";
if(c0_ch7=="p" || c0_ch7=="n" || c0_ch7=="b" || c0_ch7=="r" || c0_ch7=="q" || c0_ch7=="k" ) c0_color7="b";
if(c0_ch7=="P" || c0_ch7=="N" || c0_ch7=="B" || c0_ch7=="R" || c0_ch7=="Q" || c0_ch7=="K" ) c0_color7="w";
if(c0_color7!=" ")
	 {
	 if( c0_ch7=="P" ||  c0_ch7=="p" ) c0_ch7="p";
	 else c0_ch7=Caps(c0_ch7);

	 c0_fs2 = c0_fs2 + (c0_color7 + c0_ch7 + c0_pos7 + ";");
	 }
if(c0_ch7=="/") { if(c0_horz7>1) {c0_vert7--; c0_horz7=1;} }
else { c0_horz7++; if(c0_horz7>8) { c0_horz7=1; c0_vert7--; } }

if(c0_vert7<1) break;
}

while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)==" " ) break; c0_i7++; }
while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)!=" " ) break; c0_i7++; }

// which moves
c0_side7move=1;
if(c0_i7<Len(c0_fs1) && Mid(c0_fs1,c0_i7,1)=="b") c0_side7move=-1;

while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)==" " ) break; c0_i7++; }
while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)!=" " ) break; c0_i7++; }

// castlings

c0_wK7=false; c0_wRL7=false; c0_wRR7=false; c0_wcastl7=false;
c0_bK7=false; c0_bRL7=false; c0_bRR7=false; c0_bcastl7=false;

c0_q7="-";
if(c0_i7<Len(c0_fs1))
{
 c0_q7=Mid2(c0_fs1,c0_i7);
 c0_at7=InStr(c0_q7," ");
 if( c0_at7>=0 ) c0_q7=Mid(c0_q7,0,c0_at7);
}
if( InStr(c0_q7,"K")<0 ) c0_wRR7=true;
if( InStr(c0_q7,"Q")<0 ) c0_wRL7=true;

if( InStr(c0_q7,"k")<0 ) c0_bRR7=true;
if( InStr(c0_q7,"q")<0 ) c0_bRL7=true;

if( InStr(c0_q7,"-")>=0 ) { c0_wK7=true;  c0_bK7=true; }

c0_fisch_castl_save(c0_q7,c0_fs2);

while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)==" " ) break; c0_i7++; }
while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)!=" " ) break; c0_i7++; }

// en passant

c0_q7="-";
if(c0_i7<Len(c0_fs1)) c0_q7=Mid(c0_fs1,c0_i7,1);

c0_enpass7=0;
if( InStr(c0_q7,"-")<0 ) c0_enpass7=Asc(Mid(c0_q7,0,1))-96;

while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)==" " ) break; c0_i7++; }
while( c0_i7<Len(c0_fs1) ) { if( Mid(c0_fs1,c0_i7,1)!=" " ) break; c0_i7++; }

// remaining information is omitted

c0_set_board_situation( c0_fs2, c0_wK7, c0_wRL7, c0_wRR7, c0_wcastl7, c0_bK7, c0_bRL7,
	c0_bRR7, c0_bcastl7, c0_enpass7, c0_moveslist, c0_side7move );

}

//----------------------------------
// PGN parser on chess moves
//----------------------------------
public string c0_put_to_PGN(string c0_moves_str)		// To write moveslist to PGN string...
{

string c0_1save_position, c0_1save_become, c0_1save_become_from_engine, c0_1save_moveslist;
bool c0_1save_wKingmoved, c0_1save_bKingmoved, c0_1save_wLRockmoved, c0_1save_wRRockmoved;
bool c0_1save_bLRockmoved, c0_1save_bRRockmoved, c0_1save_w00, c0_1save_b00;
int c0_1save_sidemoves, c0_1save_lastmovepawn;

string c0_PGN_ret, Result, CR, c0_q9, c07_col, c0_move8="", c0_move9, c0_h8;
int c0_i7, c0_at_q8, c0_at_q9, c07_gaj;

if( Len(c0_moves_str)==0 ) c0_moves_str=c0_moveslist;

c0_errflag=false;


c0_1save_position=c0_position;
c0_1save_sidemoves=c0_sidemoves;
c0_1save_wKingmoved=c0_wKingmoved;
c0_1save_bKingmoved=c0_bKingmoved;
c0_1save_wLRockmoved=c0_wLRockmoved;
c0_1save_wRRockmoved=c0_wRRockmoved;
c0_1save_bLRockmoved=c0_bLRockmoved;
c0_1save_bRRockmoved=c0_bRRockmoved;
c0_1save_w00=c0_w00;
c0_1save_b00=c0_b00;
c0_1save_become=c0_become;
c0_1save_become_from_engine=c0_become_from_engine;
c0_1save_lastmovepawn= c0_lastmovepawn;
c0_1save_moveslist= c0_moveslist;

if( Len(c0_start_FEN)>0 ) { c0_set_FEN( c0_start_FEN ); c0_fischer_adjustmoved(); }
else
{
c0_position = "wpa2,wpb2,wpc2,wpd2,wpe2,wpf2,wpg2,wph2," + 
"wRa1,wNb1,wBc1,wQd1,wKe1,wBf1,wNg1,wRh1," + 
"bpa7,bpb7,bpc7,bpd7,bpe7,bpf7,bpg7,bph7," + 
"bRa8,bNb8,bBc8,bQd8,bKe8,bBf8,bNg8,bRh8,";

c0_moveslist = "";

c0_wKingmoved = false;
c0_bKingmoved = false;
c0_wLRockmoved = false;
c0_wRRockmoved = false;
c0_bLRockmoved = false;
c0_bRRockmoved = false;
c0_w00 = false;
c0_b00 = false;

c0_lastmovepawn = 0;
c0_sidemoves=1;
}

c0_become="";
c0_become_from_engine="";

c0_PGN_ret="";

Result="";

CR=(Chr(13) + Chr(10));

for(c0_i7=0; c0_i7<c0_PGN_header.Length; c0_i7++)
{
    c0_h8 = c0_PGN_header[c0_i7];
    if ((c0_h8 != null) && (c0_h8.Length > 0))
	{
    c0_q9 = Caps(c0_h8);
	c0_at_q8=InStr(c0_q9, "FEN " );
	if(c0_at_q8<0 && c0_fischer) c0_at_q8=InStr(c0_q9,"SETUP " );
	if( c0_at_q8<0 || c0_at_q8>3 )
	{
        c0_PGN_ret = c0_PGN_ret + ("[" + c0_h8 + "]" + CR);
	c0_at_q9=InStr(c0_q9,"RESULT " );
	if( c0_at_q9>=0 )
		{
        Result = Mid2(c0_h8, c0_at_q9 + 7);
		Result=c0_ReplaceAll( Result, "'", "" );
 		}
	}
	}
}
if( Len(c0_start_FEN)>0 )
	{
	if(c0_fischer) c0_PGN_ret = c0_PGN_ret + "[SetUp " + Chr(34) + "1" + Chr(34) + "]" + CR;
	c0_PGN_ret = c0_PGN_ret +  "[FEN " + Chr(34) + c0_start_FEN + Chr(34) + "]" + CR;
	}
if( Len(c0_PGN_ret)>0 ) c0_PGN_ret = c0_PGN_ret +  CR;

c07_gaj=0;
c07_col="b";
c0_i7=0;

while( c0_i7< Len(c0_moves_str) )
 {
if(c07_col=="w") c07_col="b";
else { c07_col="w"; c07_gaj++; }


 c0_move8=Mid(c0_moves_str, c0_i7, 4 );
 c0_i7+=4;
 if( c0_i7< Len(c0_moves_str) && Mid(c0_moves_str, c0_i7, 1 )=="[" )
	{
	c0_move8 = c0_move8 + Mid(c0_moves_str, c0_i7, 3 );
	c0_i7+=3;
	}

 c0_move9=c0_to_Crafty_standard( c0_move8, c07_col );
 if( Len(c0_move9)>0 )
	{
	if( c07_col=="w" ) c0_PGN_ret = c0_PGN_ret + Str(c07_gaj) + ". ";
	c0_PGN_ret = c0_PGN_ret + c0_move9 + " ";
	}
else { c0_errflag=true; break; }
}

if(!c0_errflag) c0_PGN_ret = c0_PGN_ret + " " + Result;

c0_position=c0_1save_position;
c0_sidemoves=c0_1save_sidemoves;
c0_wKingmoved=c0_1save_wKingmoved;
c0_bKingmoved=c0_1save_bKingmoved;
c0_wLRockmoved=c0_1save_wLRockmoved;
c0_wRRockmoved=c0_1save_wRRockmoved;
c0_bLRockmoved=c0_1save_bLRockmoved;
c0_bRRockmoved=c0_1save_bRRockmoved;
c0_w00=c0_1save_w00;
c0_b00=c0_1save_b00;
c0_become=c0_1save_become;
c0_become_from_engine=c0_1save_become_from_engine;
c0_lastmovepawn=c0_1save_lastmovepawn;
c0_moveslist=c0_1save_moveslist;

if(c0_errflag) Log("Can't parse " + Str(c07_gaj) + c07_col + ":" + c0_move8);

if( Len(c0_start_FEN)>0 )
	{
	c0_set_board_situation( c0_position, c0_wKingmoved, c0_wLRockmoved, c0_wRRockmoved, c0_w00,
		 c0_bKingmoved, c0_bLRockmoved, c0_bRRockmoved, c0_b00, c0_lastmovepawn, c0_moveslist, c0_sidemoves );
	}

return c0_PGN_ret;
}

//-------------------------------------------------
// Crafty notation (quite a standard)
//-------------------------------------------------
public string c0_to_Crafty_standard(string c0_move,string c0_color47)
{
 string c0_ret9, c0_pos9, c0_9figure, c0_9color, c0_Z4from_at72, c0_Z5to_at72;
 string c0_Q4color, c0_Q4figure, c0_Q4from_at72, c0_Q4from_at7, c0_color49; 
 int c0_at9, c0_Z4horiz, c0_Z4vert, c0_Z5horiz, c0_Z5vert, c0_figc9, c0_i4;
 int c0_Q4horiz, c0_Q4vert;

 c0_ret9=c0_fischer_cst_tCr(c0_move);
 if(Len(c0_ret9)>0)
	{
	c0_fischer_cstl_move(c0_move,false);
	c0_sidemoves=-c0_sidemoves;
	return c0_ret9;
	}

 c0_pos9=c0_position;
 c0_at9=InStr(c0_position, Mid(c0_move,0,2) );

 c0_become_from_engine="";
 if( Len(c0_move)>4 ) c0_become_from_engine=Mid(c0_move,5,1);

 if(c0_at9>=0 )
  {
  c0_9figure=Mid(c0_position, c0_at9-1,1 );
  c0_9color=Mid(c0_position, c0_at9-2,1 );
  if( c0_9color==c0_color47 )
   {
    c0_Z4horiz=Asc(Mid(c0_move,0,1)) - 96;
    c0_Z4vert=Int(Mid(c0_move,1,1));
    c0_Z4from_at72 = Str(c0_Z4vert) + Str(c0_Z4horiz);
    c0_Z5horiz=Asc(Mid(c0_move,2,1)) - 96;
    c0_Z5vert=Int(Mid(c0_move,3,1));
    c0_Z5to_at72 = Str(c0_Z5vert) + Str(c0_Z5horiz);

    if( Len(c0_become_from_engine)==0 && c0_9figure=="p" && (c0_Z5vert==8 || c0_Z5vert==1) ) c0_become_from_engine="Q";

    if( c0_can_be_moved( c0_Z4from_at72,c0_Z5to_at72,false ) )
      {
        if( c0_9figure!="p" )
	{
	c0_figc9=0;
	for(c0_i4=0;Len(c0_position)>c0_i4; c0_i4+=5)
	{
	c0_Q4color=Mid(c0_position,c0_i4,1);
	c0_Q4figure=Mid(c0_position,c0_i4+1,1);
	if(c0_Q4color==c0_color47 && c0_9figure==c0_Q4figure) c0_figc9++;
	}

	for(c0_i4=0;Len(c0_position)>c0_i4; c0_i4+=5)
	{
	c0_Q4color=Mid(c0_position,c0_i4,1);
	c0_Q4figure=Mid(c0_position,c0_i4+1,1);
	c0_Q4horiz=Asc(Mid(c0_position,c0_i4+2,1)) - 96;
	c0_Q4vert=Int(Mid(c0_position,c0_i4+3,1));
	c0_Q4from_at72 = Str(c0_Q4vert) + Str(c0_Q4horiz);
	c0_Q4from_at7 = Mid(c0_position,c0_i4+2,2);

	if(c0_Q4color==c0_color47 && c0_9figure==c0_Q4figure && c0_Q4from_at7 !=Mid(c0_move,0,2) )
		{
		if( c0_can_be_moved( c0_Q4from_at72, c0_Z5to_at72,false))
			{
			if( c0_figc9 < 3 && c0_Z4horiz!=c0_Q4horiz )
				{
				c0_ret9 = c0_ret9 + Mid(c0_move,0,1);
				}
			else
				{
				c0_ret9 = c0_ret9 + Mid(c0_move,0,2) + "-" ;
				}
			break;
			}
		}
	}
	}
	c0_moveto( c0_Z4from_at72,c0_Z5to_at72,false );
	c0_sidemoves=-c0_sidemoves;
	
	if( c0_9figure=="K" && c0_9color=="w" && Mid(c0_move,0,4) == "e1g1" ) c0_ret9="O-O";
	else if( c0_9figure=="K" && c0_9color=="b" && Mid(c0_move,0,4) == "e8g8" ) c0_ret9="O-O";
	else if( c0_9figure=="K" && c0_9color=="w" && Mid(c0_move,0,4) == "e1c1" ) c0_ret9="O-O-O";
	else if( c0_9figure=="K" && c0_9color=="b" && Mid(c0_move,0,4) == "e8c8" ) c0_ret9="O-O-O";
		else
		{
		if(c0_9figure!="p") c0_ret9 = c0_9figure + c0_ret9;


		if( Len(c0_pos9) > Len(c0_position) )
			{
			 if( (Len(c0_ret9)>0) && Mid(c0_ret9, Len(c0_ret9)-1,1)=="-" ) c0_ret9=Mid(c0_ret9,0,Len(c0_ret9)-1);
			 c0_ret9 = c0_ret9 + "x";
			}

		if( Len(c0_ret9)>0 && Mid(c0_ret9,0,1)=="x" ) c0_ret9= Mid(c0_move,0,1) + c0_ret9;

		c0_ret9 = c0_ret9 + Mid(c0_move,2,2); 
		if( Len(c0_become_from_engine)>0 ) c0_ret9 = c0_ret9 + "=" + c0_become_from_engine;

		c0_color49="w";
		if(c0_color47=="w") c0_color49="b";

		if( c0_is_mate_to_king( c0_color49, true ) ) c0_ret9 = c0_ret9 + "#";
		else if( c0_is_check_to_king( c0_color49 ) ) c0_ret9 = c0_ret9 + "+";
		}
       }
   }
  }
return c0_ret9;
}

//-------------------------------------------------
// Fischerrandom support publics...
//-------------------------------------------------

//------- Get castling settings into variable...
public void c0_fisch_castl_save(string c0_fen_c, string c0_fen_pos)
{
int atW, atB, c0_q8;
string c0_cl, c0_ch, c0_vt, c0_hz, c0_rook, c0_pc;

c0_fischer_cst="";
atW=InStr(c0_fen_pos,"wK");
atB=InStr(c0_fen_pos,"bK");

if(atW>=0 && atB>=0)
	{
	c0_fischer_cst=c0_fischer_cst + ("{wK}" + Mid(c0_fen_pos,atW,5) + "{bK}" + Mid(c0_fen_pos,atB,5));

	for(c0_q8=1; c0_q8<=16; c0_q8++)
		{
		c0_ch=Chr(64+c0_q8-8);
		c0_cl="w";
		c0_vt="1";
		c0_hz=Chr(96+c0_q8-8);

		if(c0_q8<9)
			{
			c0_ch=Chr(96+c0_q8);
			c0_cl="b";
			c0_vt="8";
			c0_hz=Chr(96+c0_q8);
			}
		c0_rook=c0_cl + "R" + c0_hz + c0_vt + ";";
		if(InStr(c0_fen_c,c0_ch)>=0 && InStr(c0_fen_pos,c0_rook)>=0)
		 {
		 if(c0_q8<9)
		  {
		  if(Asc(Mid(c0_fen_pos,atB+2,1))>Asc(c0_hz)) c0_fischer_cst = c0_fischer_cst + "{bLR}";
		  else c0_fischer_cst = c0_fischer_cst + "{bRR}";
		  }
		  else
		  {
		  if(Asc(Mid(c0_fen_pos,atW+2,1))>Asc(c0_hz)) c0_fischer_cst = c0_fischer_cst + "{wLR}";
		  else c0_fischer_cst = c0_fischer_cst + "{wRR}";
		  }
		 c0_fischer_cst = c0_fischer_cst + c0_rook;
		 }
		}
	for(c0_q8=0; c0_q8<Len(c0_fen_pos); c0_q8+=5)
		{
		c0_pc=Mid(c0_fen_pos,c0_q8+1,1);
		if(c0_pc=="R")
		{
		c0_cl=Mid(c0_fen_pos,c0_q8,1);
		c0_hz=Mid(c0_fen_pos,c0_q8+2,1);
		c0_rook=Mid(c0_fen_pos,c0_q8,5);

		if(c0_cl=="w")
		{
		if(InStr(c0_fischer_cst,"{wLR}")<0 && InStr(c0_fen_c,"Q")>=0 &&
			Asc(Mid(c0_fen_pos,atW+2,1))>Asc(c0_hz)) c0_fischer_cst = c0_fischer_cst + "{wLR}" + c0_rook;
		else if(InStr(c0_fischer_cst,"{wRR}")<0 && InStr(c0_fen_c,"K")>=0 &&
			Asc(Mid(c0_fen_pos,atW+2,1))<Asc(c0_hz)) c0_fischer_cst = c0_fischer_cst + "{wRR}" + c0_rook;
		}
		else
		{
		if(InStr(c0_fischer_cst,"{bLR}")<0 && InStr(c0_fen_c,"q")>=0 &&
			Asc(Mid(c0_fen_pos,atB+2,1))>Asc(c0_hz)) c0_fischer_cst = c0_fischer_cst + "{bLR}" + c0_rook;
		else if(InStr(c0_fischer_cst,"{bRR}")<0 && InStr(c0_fen_c,"k")>=0 &&
			Asc(Mid(c0_fen_pos,atB+2,1))<Asc(c0_hz)) c0_fischer_cst = c0_fischer_cst + "{bRR}" + c0_rook;

		}
		}
		}
	}
}

//------- Adjust main variables after position is set...
public void c0_fischer_adjustmoved()
{
if(InStr(c0_fischer_cst,"{bLR}")>=0 && InStr(c0_fischer_cst,"{bK}")>=0)
	{ c0_bKingmoved = false; c0_bLRockmoved = false; c0_b00 = false; }
if(InStr(c0_fischer_cst,"{bRR}")>=0 && InStr(c0_fischer_cst,"{bK}")>=0)
	{ c0_bKingmoved = false; c0_bRRockmoved = false; c0_b00 = false; }
if(InStr(c0_fischer_cst,"{wLR}")>=0 && InStr(c0_fischer_cst,"{wK}")>=0)
	{ c0_wKingmoved = false; c0_wLRockmoved = false; c0_w00 = false; }
if(InStr(c0_fischer_cst,"{wRR}")>=0 && InStr(c0_fischer_cst,"{wK}")>=0)
	{ c0_wKingmoved = false; c0_wRRockmoved = false; c0_w00 = false; }
}

//------- Does fischer movings for castling...
public void c0_fischer_cstl_move(string c0_movz, bool c0_draw)
{
string c0_king, c0_rook, c0_king2, c0_rook2, c0_from_at, c0_to_at;
c0_king="";
c0_rook="";
c0_king2="";
c0_rook2="";

if(Mid(c0_movz,0,4)=="00**")
	{
	if(c0_sidemoves>0)
		{
		c0_king=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{wK}")+4,4 );
		c0_rook=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{wRR}")+5,4 );
		c0_king2="wKg1"; c0_rook2="wRf1";
		c0_wKingmoved=true;c0_wLRockmoved=true;c0_wRRockmoved=true;c0_w00=true;
		}
	else
		{
		c0_king=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{bK}")+4,4 );
		c0_rook=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{bRR}")+5,4 );	
		c0_king2="bKg8"; c0_rook2="bRf8";
		c0_bKingmoved=true;c0_bLRockmoved=true;c0_bRRockmoved=true;c0_b00=true;
		}
	}
else if(Mid(c0_movz,0,4)=="000*")
	{
	if(c0_sidemoves>0)
		{
		c0_king=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{wK}")+4,4 );
		c0_rook=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{wLR}")+5,4 );
		c0_king2="wKc1"; c0_rook2="wRd1";
		c0_wKingmoved=true;c0_wLRockmoved=true;c0_wRRockmoved=true;c0_w00=true;
		}
	else
		{
		c0_king=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{bK}")+4,4 );
		c0_rook=Mid(c0_fischer_cst, InStr(c0_fischer_cst,"{bLR}")+5,4 );
		c0_king2="bKc8"; c0_rook2="bRd8";
		c0_bKingmoved=true;c0_bLRockmoved=true;c0_bRRockmoved=true;c0_b00=true;
		}
	}
else
	{
	c0_from_at=Mid(c0_movz,0,2);
	c0_to_at=Mid(c0_movz,2,2);
	c0_moveto(c0_convH888(c0_from_at), c0_convH888(c0_to_at), c0_draw);
	}

if(Len(c0_king)>0 && Len(c0_rook)>0)
	{
	if(c0_draw)
		{
		c0_clear_at(Mid(c0_king,2,2));
		c0_clear_at(Mid(c0_rook,2,2));
		c0_add_piece(Mid(c0_king2,0,2) + Mid(c0_rook2,2,2));
		c0_moveto(c0_convH888(Mid(c0_rook2,2,2)), c0_convH888(Mid(c0_king2,2,2)), c0_draw);
		c0_add_piece(c0_rook2);
		}
	else
		{
		if(!(c0_king==c0_king2)) c0_position=c0_ReplaceAll(c0_position,c0_king,c0_king2);
		if(!(c0_rook==c0_rook2)) c0_position=c0_ReplaceAll(c0_position,c0_rook,c0_rook2);
		}
	}
}

//------- Saves fischer movings for castling from Crafty standard...
public string c0_fischer_cst_fCr(string c0_move)
{
string c0_ret8;
c0_ret8="";

if(c0_fischer)
{
if((Len(c0_move)>4) && (Mid(c0_move,0,5)=="O-O-O" || Mid(c0_move,0,5)=="0-0-0")) c0_ret8="000*";
else if((Len(c0_move)>2) && (Mid(c0_move,0,3)=="O-O" || Mid(c0_move,0,3)=="0-0")) c0_ret8="00**";
}
return c0_ret8;
}

//------- Saves to Crafty standard...
public string c0_fischer_cst_tCr(string c0_move)
{
string c0_ret8;
c0_ret8="";
if(c0_fischer)
{
if(Mid(c0_move,0,4)=="000*") c0_ret8="0-0-0";
else if(Mid(c0_move,0,4)=="00**") c0_ret8="0-0";
}
return c0_ret8;
}

public void c0_NAGs_define()		// Just data of NAG codes...
{
string Ng1, Ng2, Ng3, Ng4;

Ng1 = "[0] null annotation [1] good move ('!') [2] poor move ('?') [3] very good move ('!!') [4] very poor move ('??') " + 
"[5] speculative move ('!?') [6] questionable move ('?!') [7] forced move (all others lose quickly) [8] singular move (no reasonable alternatives) " + 
"[9] worst move [10]  drawish position [11] equal chances, quiet position (=) [12] equal chances, active position (ECO ->/<-) " + 
"[13] unclear position (emerging &) [14] White has a slight advantage (+=) [15] Black has a slight advantage (=+) " + 
"[16] White has a moderate advantage (+/-) [17] Black has a moderate advantage (-/+) [18]  White has a decisive advantage (+-) " + 
"[19] Black has a decisive advantage (-+) [20] White has a crushing advantage (Black should resign) (+--) " + 
"[21] Black has a crushing advantage (White should resign) (--+) [22] White is in zugzwang (zz) [23] Black is in zugzwang (zz) " + 
"[24] White has a slight space advantage [25]  Black has a slight space advantage [26]  White has a moderate space advantage (O) " + 
"[27] Black has a moderate space advantage (O) [28] White has a decisive space advantage [29] Black has a decisive space advantage " + 
"[30] White has a slight time (development) advantage [31] Black has a slight time (development) advantage " + 
"[32] White has a moderate time (development) advantage (@) [33] Black has a moderate time (development) advantage (@) " + 
"[34] White has a decisive time (development) advantage [35] Black has a decisive time (development) advantage " + 
"[36] White has the initiative (^) [37]  Black has the initiative (^) [38] White has a lasting initiative " + 
"[39] Black has a lasting initiative [40] White has the attack (->) ";

Ng2 = "[41] Black has the attack (->) [42] White has insufficient compensation for material deficit [43] Black has insufficient compensation for material deficit " + 
"[44] White has sufficient compensation for material deficit (=/&) [45] Black has sufficient compensation for material deficit (=/&) " + 
"[46] White has more than adequate compensation for material deficit [47] Black has more than adequate compensation for material deficit " + 
"[48] White has a slight center control advantage [49] Black has a slight center control advantage [50] White has a moderate center control advantage (#) " + 
"[51] Black has a moderate center control advantage (#) [52] White has a decisive center control advantage " + 
"[53] Black has a decisive center control advantage [54] White has a slight kingside control advantage [55] Black has a slight kingside control advantage " + 
"[56] White has a moderate kingside control advantage (>>) [57] Black has a moderate kingside control advantage (>>) " + 
"[58] White has a decisive kingside control advantage [59] Black has a decisive kingside control advantage [60] White has a slight queenside control advantage " + 
"[61] Black has a slight queenside control advantage [62] White has a moderate queenside control advantage (<<) " + 
"[63] Black has a moderate queenside control advantage (<<)  [64] White has a decisive queenside control advantage " + 
"[65] Black has a decisive queenside control advantage [66] White has a vulnerable first rank [67] Black has a vulnerable first rank " + 
"[68] White has a well protected first rank [69] Black has a well protected first rank [70] White has a poorly protected king " + 
"[71] Black has a poorly protected king [72] White has a well protected king [73] Black has a well protected king [74] White has a poorly placed king "  + 
"[75] Black has a poorly placed king [76] White has a well placed king [77] Black has a well placed king [78] White has a very weak pawn structure " + 
"[79] Black has a very weak pawn structure [80] White has a moderately weak pawn structure (DR:x a5) " + 
"[81] Black has a moderately weak pawn structure (DR:x a5) [82] White has a moderately strong pawn structure " + 
"[83] Black has a moderately strong pawn structure [84] White has a very strong pawn structure [85] Black has a very strong pawn structure ";

Ng3 = "[86] White has poor knight placement [87] Black has poor knight placement [88] White has good knight placement " + 
"[89] Black has good knight placement [90] White has poor bishop placement [91] Black has poor bishop placement " + 
"[92] White has good bishop placement (diagonal) [93] Black has good bishop placement [94] White has poor rook placement " + 
"[95] Black has poor rook placement [96] White has good rook placement (rank <=> file ||) [97] Black has good rook placement " + 
"[98] White has poor queen placement [99] Black has poor queen placement [100] White has good queen placement " + 
"[101] Black has good queen placement [102] White has poor piece coordination [103] Black has poor piece coordination " + 
"[104] White has good piece coordination [105] Black has good piece coordination [106] White has played the opening very poorly " + 
"[107] Black has played the opening very poorly [108] White has played the opening poorly [109] Black has played the opening poorly " + 
"[110] White has played the opening well [111] Black has played the opening well [112] White has played the opening very well " + 
"[113] Black has played the opening very well [114] White has played the middlegame very poorly [115] Black has played the middlegame very poorly " + 
"[116] White has played the middlegame poorly [117] Black has played the middlegame poorly [118] White has played the middlegame well " + 
"[119] Black has played the middlegame well [120] White has played the middlegame very well [121] Black has played the middlegame very well " + 
"[122] White has played the ending very poorly [123] Black has played the ending very poorly [124] White has played the ending poorly " + 
"[125] Black has played the ending poorly [126] White has played the ending well [127] Black has played the ending well " + 
"[128] White has played the ending very well [129] Black has played the ending very well [130] White has slight counterplay "  + 
"[131] Black has slight counterplay [132] White has moderate counterplay (->/<-) [133] Black has moderate counterplay " + 
"[134] White has decisive counterplay [135] Black has decisive counterplay [136] White has moderate time control pressure " + 
"[137] Black has moderate time control pressure [138] White has severe time control pressure [139] Black has severe time control pressure ";

Ng4 = "[140] With the idea [141] Aimed against [142] Better move [143] Worse move [144] Equivalent move [145] Editors Remark ('RR') " + 
"[146] Novelty ('N') [147] Weak point [148] Endgame [149] Line [150] Diagonal [151] White has a pair of Bishops [152] Black has a pair of Bishops " + 
"[153] Bishops of opposite color [154] Bishops of same color [190] Etc. [191] Doubled pawns [192] Isolated pawn [193] Connected pawns " + 
"[194] Hanging pawns [195] Backwards pawn [201] Diagram ('D', '#') [xyz]";


c0_NAGs = Ng1 + Ng2 + Ng3 + Ng4;
}

// public returns most played openings
// Sample at http://chessforeva.appspot.com/C0_demo4.htm

public void c0_Openings_define()		// Just 30Kb of coded openings data...
{
string t1, t2, t3, t4;

t1="A00.b1c31c7c54-1d7d55e2e49.A01.b2b31d7d52c1b29-2e7e55c1b29b8c67e2e39g8f69-3d7d62-3g8f61c1b29.A00.b2b41e7e59c1b29.A10.c2c41A40.b7b61b1c39c8b79-3A30.c7c51A34.b1c34A35.b8c65g1f32-1A36.g2g37g7g69f1g29f8g79A37.g1f39-6A34.g7g61g2g39f8g79f1g29b8c69-5g8f62g2g39-3A30.g1f33b8c63b1c35-1d2d44c5d49f3d49-4g7g61-1g8f64b1c39-3g2g31b8c64f1g29g7g69-3g7g65f1g29f8g79b1c39b8c69-7A11.c7c61b1c31d7d59-2d2d42d7d59-2e2e43d7d59e4d59c6d59d2d49g8f69b1c39-7A12.g1f33d7d59b2b34-1e2e35g8f69-5A10.d7d61-1A20.e7e52A21.b1c37A25.b8c63A27.g1f33-1A25.g2g36g7g69f1g29f8g79-5A21.d7d61g2g39-2f8b41-1A22.g8f64g1f35b8c69e2e33-1g2g36d7d59c4d59f6d59-6g2g34A23.d7d56c4d59f6";
t2="d59f1g29d5b69g1f39b8c69-7A22.f8b44f1g29-5A20.g2g32b8c64f1g29g7g69b1c39f8g79-5g8f65f1g29c7c63-1d7d56c4d59f6d59-7A13.e7e61b1c33d7d59d2d49c7c64-1g8f65-4d2d41d7d59b1c39-3g1f33d7d57b2b32-1d2d43g8f69-2g2g33g8f69-3g8f62g2g39-3g2g32d7d59f1g29g8f69g1f39f8e79-7A10.f7f51b1c34g8f69g2g39-3g1f32g8f69-2g2g33g8f69f1g29-4g7g61b1c34f8g79d2d42-1g2g37e7e59f1g29-5d2d41f8g79-2e2e41-1g1f31f8g79-2g2g32f8g79f1g29d7d64-1e7e55b1c39-6A15.g8f63A16.b1c36c7c51g1f35-1g2g34-2d7d51c4d59f6d59A17.g2g39g7g69f1g29-6A16.e7e51g1f36b8c69g2g39-3g2g33-2A17.e7e62d2d42-1A18.e2e43d7d59e4e59-3A17.g1f33d7d59d2d49-4A16.g7g63d2d41f8g79-2e2e43d7d69d2d49f8g79-4g2g35f8g79f1g29";
t3="e8g89e2e44-1g1f35d7d69-8A15.d2d41e7e69-2g1f31c7c51-1e7e63g2g39-2g7g64b1c34-1g2g35f8g79f1g29e8g89-6g2g31c7c62-1e7e62f1g29d7d59-3g7g65f1g29f8g79b1c39e8g89.A40.d2d43b7b61-1b8c61g1f39-2A43.c7c51d4d58d7d61-1A44.e7e54e2e49d7d69b1c39-4A43.e7e61-1g8f63A56.c2c49-3A43.e2e31-2A41.c7c61c2c49-2A84.d7d52D00.b1c31g8f69D01.c1g59-3D00.c1f41g8f69-2c1g51c7c64-1h7h65g5h49c7c69-4D01.c2c47D06.b8c61b1c33-1D07.c4d53d8d59e2e39e7e59b1c39f8b49c1d29b4c39-8D06.g1f33c8g49-3D10.c7c64b1c32d5c41-1e7e61-1g8f68c4d51c6d59c1f49-3e2e34a7a63d1c29-2e7e65g1f39b8d79d1c24f8d69-2f1d35d5c49d3c49b7b59c4d39-8g7g61g1f39f8g79-4g1f34a7a61-1d5c43a2a49c8f59-3e7e64c1g55-1e2e34b8";
t4="d79-6c4d51c6d59b1c37b8c63g1f39g8f69c1f49-4g8f66c1f43b8c69e2e39-3g1f36b8c69c1f49c8f59e2e39e7e69f1d39-9g1f32";
c0_opn[1]=t1 + t2 + t3 + t4;

t1="g8f69b1c39b8c69c1f49-7e2e31g8f69b1c39-3D11.g1f35e7e61-1g8f69D15.b1c36a7a61c4c59-2d5c43D16.a2a49D17.c8f59D18.e2e35D19.e7e69f1c49f8b49e1g19b8d74-1e8g85d1e29-7D17.f3e54b8d75e5c49-2e7e64-5D15.e7e64c1g55d5c43-1h7h66g5f69d8f69-4e2e34b8d79d1c25f8d69-2f1d34d5c49d3c49b7b59-8D13.c4d51D14.c6d59b1c39b8c69c1f49c8f59e2e39e7e69-8D11.d1c21-1e2e32a7a62-1D12.c8f54b1c39e7e69f3h49-4D11.e7e63-5D06.c8f51-1D20.d5c41b1c31-1e2e32g8f69f1c49e7e69g1f39c7c59e1g19a7a69-8e2e41e7e54g1f39e5d49-3g8f65e4e59f6d59f1c49d5b69-6D21.g1f35a7a61D22.e2e39-2D21.e7e61-1D23.g8f67D24.b1c31-1D25.e2e38D26.e7e69f1c49c7c59e1g19D27.a7a69-9D08.e7e51d4e59d5d49g1f39b8c69-5D30.e7e6";
t2="3D31.b1c36D32.c7c51c4d59e6d59g1f39b8c69D33.g2g39g8f69f1g29D34.f8e79e1g19e8g89c1g59-12D31.c7c62c4d51e6d59-2e2e34g8f69g1f39b8d79d1c25f8d69-2f1d34-5e2e41-1g1f33d5c44-1g8f65-3f8e71c4d52e6d59c1f49-3g1f37g8f69c1f42-1c1g57e8g84e2e39-2h7h65g5h49e8g89-7D35.g8f64D50.c1g54D51.b8d73e2e39-2D50.f8e76D53.e2e37D54.e8g89D55.g1f39-3D50.g1f32-3D35.c4d53e6d59D36.c1g59c7c63e2e39-2f8e76e2e39c7c64-1e8g85f1d39-7D37.g1f32f8e79-4D30.c4d51e6d59b1c39-3g1f32c7c51c4d59e6d59g2g39-4c7c62d1c25-1e2e34-2g8f66b1c35c7c63-1f8e76c1g59-3c1g51-1g2g33f8e79f1g29e8g89e1g19-8D06.g8f61-2D00.e2e31g8f69-2D02.g1f32b8c61c1f49-2c7c51-1c7c61c2c49e7e64-1g8f65-3c8f51-1e7e61c2c49-2";
t3="g8f66c1f41c7c54-1e7e65e2e39-3D03.c1g51e7e65-1f6e44-2D02.c2c45c7c64b1c35d5c45a2a49c8f59-3e7e64-2c4d51c6d59b1c39b8c69-4e2e32-2D25.d5c41e2e39e7e69f1c49c7c59-5D02.e7e63b1c37c7c63-1f8e76c1g59-3g2g32-3D04.e2e31D05.e7e69f1d39-3D02.g2g31-4A41.d7d61A42.c2c42e7e57g1f39e5e49-3g7g62b1c39f8g79-4A41.e2e43g7g62-1g8f67b1c39g7g69-4g1f33c8g43c2c49-2g7g64c2c49f8g79b1c39-4g8f62c2c49-3g2g31-2A40.e7e61c2c46b7b61a2a33-1b1c33-1e2e43c8b79-3A43.c7c51d4d59-2A40.d7d51b1c36-1g1f33-2A84.f7f52b1c32g8f69-2g1f32g8f69-2g2g35g8f69f1g29f8e79-5A40.f8b41c1d29-2g8f62b1c35f8b49d1c24-1e2e35-3g1f34b7b69-4e2e41d7d59b1c39-3g1f32c7c51-1d7d51-1f7f52g2g39g8f69f1g29-4g8f63c2";
t4="c49-3g2g31-2A80.f7f51b1c31d7d54-1g8f65c1g59d7d59-4c1g51g7g69-2A84.c2c41g8f69b1c36A85.g7g69-2A86.g2g33";
c0_opn[2]=t1 + t2 + t3 + t4;

t1="-3A80.e2e41A82.f5e49b1c39A83.g8f69c1g59-5A80.g1f31g8f69g2g39g7g69f1g29f8g79e1g19e8g89-8A81.g2g33g7g61-1g8f68f1g29e7e62-1g7g67c2c42f8g79-2g1f35f8g79e1g19e8g89c2c49d7d69b1c39-7g1h31-6A40.g7g61c2c45f8g79b1c35c7c52d4d59-2d7d67e2e49b8c69-4e2e42d7d69b1c39-3g1f31d7d69-4e2e43f8g79b1c35d7d69-2c2c42-1g1f32d7d69-4g1f31f8g79c2c49-4A45.g8f65b1c31d7d59c1g59b8d79-4c1f41-1c1g51c7c51g5f69g7f69-3d7d51g5f69e7f69e2e39-4e7e62e2e49h7h69g5f69d8f69b1c39-6f6e43g5f48c7c56f2f39d8a59c2c39e4f69b1d29c5d49d2b39-8d7d53-2g5h41-2g7g61g5f69e7f69-4c2c31-1A50.c2c46b8c61g1f39e7e69-3A56.c7c51d4d58A57.b7b55c4b58a7a69b1c31-1b5a64A58.c8a64A59.b1c39d7d65-1g7g64-3A57.g7";
t2="g65b1c39c8a69e2e49a6f19e1f19d7d69-8b5b62d7d63b1c39-2d8b62b1c39-2e7e63b1c39-3e2e31-1f2f31-3g1f31g7g69-3A56.d7d61b1c39g7g69e2e49f8g79-5e7e51b1c39d7d69e2e49f8e79-5A60.e7e62b1c39A61.e6d59c4d59d7d69A65.e2e47A66.g7g69f2f46A67.f8g79f1b59f6d79-4A70.g1f33-3A61.g1f32A62.g7g69-7A56.g7g61b1c39f8g79e2e49d7d69-6e2e31g7g69-2g1f31c5d49f3d49e7e59-5A50.c7c61b1c39d7d59g1f39-4A53.d7d61b1c37b8d74e2e46e7e59g1f39-3g1f33-2e7e52A54.g1f39-2A53.g7g62e2e49f8g79-4g1f32b8d75-1g7g64-3A51.e7e51d4e59f6e41-1A52.f6g48c1f44b8c69g1f39f8b49-4g1f35f8c59e2e39b8c69-7E00.e7e64E20.b1c34c7c51d4d59e6d59c4d59d7d69e2e49g7g69-7d7d51c1g54f8e79e2e39-3c4d53e6d59c1g59f8e79e2e39-5";
t3="g1f32-2f8b48E24.a2a31b4c39b2c39-3E30.c1g51h7h69E31.g5h49-3E32.d1c23E38.c7c53d4c59b4c52-1b8a62a2a39-2E39.e8g85a2a39b4c59g1f39b7b69-7E34.d7d51E35.c4d59d8d59-3E33.e8g85a2a39b4c39c2c39b7b68c1g59c8a62-1c8b77f2f39-4f6e41c3c29-7E40.e2e34E43.b7b62f1d33c8b79-2E44.g1e26-2E41.c7c53f1d36b8c69g1f39-3E42.g1e23c5d49e3d49-4E46.e8g84E47.f1d37c7c52g1f39-2E48.d7d57g1f39c7c59e1g19-5E46.g1e22d7d59a2a39b4e79c4d59-7E20.f2f31d7d59a2a39b4c39b2c39-5g1f31b7b64-1c7c55g2g39-3g2g31-3E10.g1f34E12.b7b65a2a32c8a63d1c29a6b79b1c39c7c59e2e49c5d49f3d49-8c8b76b1c39d7d59c4d59f6d59-6b1c31c8b75a2a39d7d59-3f8b44c1g59-3E14.e2e31c8b79f1d39-3E15.g2g35c8a66b1d21-1b2b36f8b49";
t4="c1d29b4e79f1g29c7c69d2c39d7d59-8d1a41-2c8b73E16.f1g29E17.f8e79b1c32-1e1g17e8g89E18.b1c39E19.f6e49";
c0_opn[3]=t1 + t2 + t3 + t4;

t1="c3e49b7e49f3e19-12E10.c7c51d4d59e6d59c4d59d7d69b1c39g7g69-7d7d52b1c38b8d71-1c7c61-1d5c41-1f8b41-1f8e74c1f43e8g89e2e39-3c1g56e8g85e2e39-2h7h64-4g2g31-2E11.f8b41b1d23b7b69a2a39-3c1d26a7a52-1d8e77g2g39b8c69-6E00.g2g31c7c52d4d55e6d59c4d59-3g1f34-2E01.d7d55E02.f1g26E03.d5c44-1E06.f8e75E07.g1f39e8g89E08.e1g19-5E01.g1f33-2E00.f8b42c1d29-4D70.g7g63b1c38d7d52D82.c1f41D83.f8g79e2e39c7c59-4D80.c1g51f6e49-2D85.c4d55f6d59c1d21f8g79e2e49-3e2e49d5c39D86.b2c39f8g79c1e31c7c59d1d29-3f1b51-1f1c44c7c56g1e29b8c65c1e39e8g89e1g19-4e8g84e1g19-4e8g83g1e29-3g1f33c7c59a1b19e8g89f1e29c5d49c3d49-13D90.g1f33f8g79D92.c1f41D93.e8g89-2D91.c1g52f6e49c4d59e4g59f3";
t2="g59e7e69-6D90.c4d51f6d59e2e49d5c39b2c39-5D96.d1b33D97.d5c49b3c49e8g89e2e49a7a69-6D94.e2e31e8g89-5E61.f8g77c1g51-1E70.e2e48d7d69f1d31e8g89g1e29-3E73.f1e22e8g89c1g54b8a63-1E74.c7c53E75.d4d59-2E73.h7h62g5e39-3g1f35e7e59e1g19b8c69d4d59c6e79-8E80.f2f32E81.e8g89c1e37E83.b8c62g1e29-2E81.b8d71-1c7c51-1E85.e7e54E87.d4d56-1E86.g1e23-3E81.c1g51-1g1e21-3E76.f2f41e8g89g1f39b8a62-1c7c57d4d59e7e69f1e29e6d59c4d59-9E70.g1e21e8g89e2g39-3E90.g1f33e8g89E91.f1e29b8a61-1b8d71-1E92.e7e58c1e31-1d4d51a7a59-2d4e51d6e59d1d89f8d89c1g59-5E94.e1g16b8a61-1E97.b8c68d4d59c6e79b2b46a7a54-1f6h55f1e19-3E98.f3e13f6d79-8E90.h2h31-3E71.h2h31e8g89c1g59-4E70.e8g81g1f39";
t3="d7d69f1e29e7e59-6E61.g1f31E62.d7d62-1E61.e8g87c1g55-1g2g34-3g2g31e8g89f1g29d7d69-6D70.f2f31-1g1f31E60.f8g79b1c31-1g2g38e8g89f1g29d7d69e1g19b8d79-8D70.g2g31E60.f8g79f1g29d7d52c4d59f6d59e2e49-4e8g87b1c37d7d69g1f39b8c64-1b8d75e1g19-5g1f32d7d69-8A45.e2e31g7g69-2A46.g1f32A47.b7b61-1A46.c7c51c2c31-1d4d55b7b53c1g59-2d7d61-1e7e62-1g7g62b1c39-3e2e32g7g69-3d7d51c1f41-1c2c47c7c63b1c39-2e7e66b1c39f8e79-4e2e31-2d7d61c2c49-2e7e63c1f41c7c59-2c1g51c7c54e2e39-2d7d51-1f8e71-1h7h62g5h49-3c2c44b7b64a2a32-1b1c32-1g2g35c8a66b2b39-2c8b73f1g29-4c7c51d4d59-2d7d51b1c39-2f8b42c1d29d8e79g2g39-5e2e31b7b66f1d39c8b79e1g19-4c7c53f1d39-3g2g31b7b51-1b7b63f1g29";
t4="c8b79e1g19-4c7c52f1g29-2d7d52f1g29-4A48.g7g64D02.b1c31d7d59c1f49f8g79e2e39e8g89f1e29-7A48.c1f41";
c0_opn[4]=t1 + t2 + t3 + t4;

t1="f8g79e2e39d7d63-1e8g86-4c1g51f8g79b1d29d7d54e2e39e8g89-3e8g85-4c2c31f8g79-2c2c44c7c51-1f8g79b1c37d7d52c4d59f6d59e2e49d5c39b2c39c7c59-7d7d61e2e49e8g89f1e29e7e59-5e8g85c1g51-1e2e48d7d69f1e29e7e59e1g19b8c69d4d59c6e79-10g2g32e8g89f1g29d7d69e1g19-7e2e31f8g79-2A49.g2g32f8g79f1g29e8g89c2c41-1e1g18d7d52-1d7d67c2c49b8d79-10A45.g2g31g7g69f1g29f8g79.A40.e2e45B00.b7b61d2d49c8b79f1d39-4b8c61d2d44d7d56-1e7e53-2g1f35d7d66d2d49g8f69b1c39c8g49-5e7e53-3B20.c7c54B23.b1c31a7a61-1b8c66f1b51c6d47b5c49e7e69-3g7g62-2f2f42d7d61g1f39-2e7e62g1f39d7d59-3g7g66g1f39f8g79f1b55c6d49e1g19-3f1c44e7e69-6g1e21-1g1f31d7d64-1g7g65-2B24.g2g34B25.g7g69f1g29f8g79B26.d2";
t2="d39d7d67c1e35a8b84-1e7e65d1d29-3f2f44e7e69g1f39g8e79e1g19e8g89c1e39c6d49-9e7e62c1e39d7d69-4B25.g1e21-6B23.d7d61f2f45b8c65g1f39g7g69-3g7g64g1f39f8g79-4g2g34b8c69f1g29g7g69d2d39-6e7e61f2f42d7d59-2g1e21-1g1f33a7a65-1b8c64-2g2g33b8c63f1g29-2d7d56-3g7g61-2B20.b2b31b8c69c1b29-3B22.c2c31d7d53e4d59d8d59d2d49b8c61g1f39c8g49f1e29-4c5d41c3d49-2e7e61g1f39g8f69-3g7g61-1g8f65g1f39c8g46f1e29e7e69e1g14b8c69-2h2h35g4h59e1g19-6e7e63-7d7d61d2d49g8f69f1d39-4e7e61d2d49d7d59e4d56e6d59g1f39b8c69-4e4e53-4g7g61d2d49c5d49c3d49d7d59-5g8f63e4e59f6d59d2d47c5d49c3d42d7d69-2d1d41e7e69-2g1f36b8c65c3d43d7d69f1c49-3f1c46d5b69c4b39d7d59e5d69d8d69-7e7e64c3d49b7b64";
t3="-1d7d65-6g1f32b8c65f1c49-2e7e64-6B20.c2c41b8c69b1c39g7g69-4d2d31b8c69g2g39g7g69f1g29f8g79f2f49-7d2d41c5d49c2c39d4c37b1c39b8c69g1f39-4g8f62e4e59f6d59-6B21.f2f41b8c64g1f39-2d7d55-2B20.g1e21b8c69-2B27.g1f37B28.a7a61c2c33-1c2c43-1d2d43c5d49f3d49g8f69-5B30.b8c63b1c31d7d61d2d49c5d49f3d49g8f69-5e7e51f1c49f8e79d2d39-4e7e62d2d49c5d49f3d49-4g7g62d2d49c5d49f3d49f8g79c1e39g8f69f1c49-8g8f61-2c2c31d7d55e4d59d8d59d2d49-4g8f64e4e59f6d59d2d49c5d49-6d2d31g7g69g2g39f8g79f1g29-5B32.d2d46c5d49f3d49d7d61-1d8b61d4b39g8f69b1c39e7e69-5d8c71b1c39e7e69c1e34a7a69-2f1e25a7a69e1g19g8f69-7e7e51d4b59a7a61b5d69f8d69d1d69d8f69-5d7d68b1c34a7a69b5a39b7b59c3d59-5c2";
t4="c45f8e79b1c39a7a69b5a39-8e7e61b1c39d8c79-3B34.g7g61B35.b1c34f8g79c1e39g8f69f1c47d8a52-1e8g87c4b39";
c0_opn[5]=t1 + t2 + t3 + t4;

t1="d7d69-4f1e22-5B34.c1e31-1B36.c2c44B37.f8g76B38.c1e39B39.g8f69b1c39e8g86f1e29d7d69e1g19c8d79-5f6g43d1g49c6d49g4d19-8B36.g8f63b1c39d7d69f1e29c6d49d1d49f8g79-9B32.g8f65B33.b1c39d7d63c1g54c8d71-1e7e68d1d29a7a66e1c19c8d74f2f49-2h7h65g5e39-4f8e73e1c19e8g89-6f1c42d8b64-1e7e65c1e39-3f1e22e7e59d4b39f8e79-4f2f31-2e7e55d4b59d7d69a2a41-1c1g58a7a69b5a39b7b59c3d55d8a52g5d29a5d89d2g59d8a59g5d29a5d89-7f8e77g5f69e7f69c2c39e8g86a3c29f6g59a2a49-4f6g53a3c29-7g5f64g7f69c3d59f6f56c2c33f8g79e4f59c8f59a3c29-5f1d36c8e69e1g19e6d59e4d59-6f8g73c2c34f6f59e4f59c8f59-4f1d35c6e79d5e79d8e79-10g5f61g7f69b5a39b7b59c3d59-7c3d51f6d59e4d59c6b89c2c49-8e7e61d4b59-2g7g61";
t2="-6B30.f1b51d7d61e1g19c8d79f1e19g8f69-5e7e62b5c63b7c69d2d39-3e1g16g8e79c2c34a7a69-2f1e15a7a69-5B31.g7g65b5c63b7c63e1g19f8g79-3d7c66d2d39f8g79h2h39g8f69b1c39-7e1g16f8g79c2c34g8f69f1e19e8g89d2d49-5f1e15e7e53-1g8f66-5B30.g8f61-3B50.d7d64b1c31g8f69-2c2c31g8f69f1d32b8c69-2f1e25b8c62-1b8d72-1g7g64e1g19f8g79-4h2h31-3d2d31-1B53.d2d48c5d49d1d41a7a63-1b8c66f1b59c8d79b5c69d7c69b1c39g8f69c1g59e7e69e1c19f8e79-12B54.f3d49B55.g8f69B56.b1c39B90.a7a65a2a41-1c1e32e7e54d4b39c8e67d1d22-1f2f37b8d75g2g49-2f8e74d1d29-4f8e72f2f39-4e7e63f2f36b7b59-2g2g43-2f6g41e3g59h7h69g5h49g7g59h4g39f8g79-8B94.c1g52B95.e7e69B96.f2f49b8d71d1f39d8c79e1c19-4B97.d8b63d1d27";
t3="b6b29a1b19b2a39f4f59b8c69f5e69f7e69d4c69b7c69-10d4b32-2B98.f8e74d1f39B99.d8c79e1c19b8d79g2g49b7b59g5f69d7f69g4g59f6d79f4f59-15B90.f1c41e7e69c4b38b7b55e1g19f8e79-3b8d72-1f8e72-2e1g11-3B92.f1e22e7e56d4b39f8e79c1e32c8e69-2e1g17e8g89c1e33c8e69-2g1h16-6e7e63e1g19f8e79f2f49-5B90.f2f31e7e55d4b39c8e69c1e39-4e7e64c1e39b7b59-4B93.f2f41d8c72-1e7e54d4f39b8d79a2a49-4e7e62-2B91.g2g31e7e59d4e29-4B56.b8c61B60.c1g55c8d71-1B62.e7e68B63.d1d29B66.a7a66B67.e1c19B68.c8d75B69.f2f49-2B67.h7h64g5e39-4B64.f8e73B65.e1c19e8g89-6B57.f1c42d8b64d4b39e7e69-3e7e65c1e39-3B58.f1e21e7e59-2B56.f2f31-2B80.e7e61c1e32-1B83.f1e24-1B81.g2g43-2B70.g7g62B72.c1e37f8g79f1e21";
t4="-1B75.f2f39b8c63d1d29e8g89e1c14-1f1c45c8d79e1c19-6B76.e8g86d1d28B77.b8c69e1c14c6d43e3d49c8e69";
c0_opn[6]=t1 + t2 + t3 + t4;

t1="-3d6d56e4d59f6d59d4c69b7c69-6f1c45B78.c8d79B79.e1c19a8c89c4b39c6e59-8B76.f1c41b8c69-6B70.f1c41f8g79-2f1e21f8g79c1e33-1e1g16e8g89-4B71.f2f41-3B55.f2f31e7e59-5B53.g8f61b1c39c5d49f3d49a7a65-1b8c61-1g7g62-6B51.f1b51b8c61e1g19c8d79f1e19-4b8d72d2d46g8f69b1c39-3e1g13-2B52.c8d76b5d79b8d71e1g19g8f69-3d8d78c2c44b8c69b1c39-3e1g15b8c67c2c39g8f69-3g8f62-6B50.f1c41g8f69d2d39-4B40.e7e62b1c31a7a65d2d47c5d49f3d49d8c79-4g2g32-2b8c64d2d49c5d49f3d49-5b2b31-1c2c31d7d55e4d57d8d55d2d49g8f69-3e6d54d2d49-3e4e52-2g8f64e4e59f6d59d2d49c5d49c3d49d7d69-8c2c41b8c69b1c39-3d2d31b8c67g2g39d7d54b1d29-2g7g65f1g29f8g79e1g19g8e79-7d7d52b1d29-3d2d47c5d49B41.f3d49a7a64";
t2="B43.b1c33b7b53f1d39d8b69-3d8c76f1d33b8c65-1g8f64-2f1e23g8f69e1g19-3g2g32-3B41.c2c41g8f69b1c39-3B42.f1d34b8c61d4c69-2d8b61-1d8c71e1g19g8f69-3f8c52d4b39c5a73-1c5e76e1g19-4g7g61-1g8f63e1g19d7d64c2c49-2d8c75d1e29d7d69-6B41.f1e21-2B44.b8c62B45.b1c38B46.a7a62f1e29-2B45.d7d61-1B47.d8c76B48.c1e33B49.a7a69f1d39g8f69e1g19-5B47.f1e24a7a69e1g19g8f69c1e35f8b49-2g1h14-5g2g31a7a69f1g29g8f69e1g19-7B44.d4b51d7d69c1f43e6e59f4e39-3c2c46g8f69b1c39a7a69b5a39f8e79f1e29-10B41.d8b61d4b39-2g8f62b1c39B45.b8c63d4b57d7d66c1f49e6e59f4g59a7a69b5a39b7b59c3d55-1g5f64g7f69c3d59-10f8b43a2a39b4c39b5c39d7d59-6d4c62b7c69-3B41.d7d65c1e32a7a69-2f1e24a7a63-1f8e76e1g19";
t3="e8g89-4g2g42h7h69-3f8b41-2f1d31b8c69-7B27.g7g61c2c31f8g79-2d2d48c5d44f3d49f8g79-3f8g75b1c39c5d49f3d49b8c69-7B29.g8f61b1c35-1e4e54f6d59-4B20.g2g31b8c69f1g29-4B10.c7c61b1c31d7d59B11.g1f39c8g47h2h39g4f39d1f39e7e69-5B12.d5e42c3e49-5B10.c2c41d7d59c4d53c6d59e4d59g8f69-4e4d56c6d59c4d59g8f69b1c39f6d59-8d2d31d7d59b1d29e7e59g1f39f8d69-6d2d48B12.d7d59B15.b1c32d5e49c3e49B17.b8d72e4g53g8f69f1d39e7e69g1f39f8d69d1e29h7h69g5e49f6e49e2e49-11f1c42g8f69e4g59e7e69d1e29-5g1f33g8f69e4f69d7f69-5B18.c8f55e4g39B19.f5g69f1c41-1g1f32b8d79h2h49h7h69h4h59g6h79-6h2h46h7h69g1f39b8d79h4h59g6h79f1d39h7d39d1d39e7e69c1f49-14B15.g8f61e4f69B16.g7f69-5B15.g7g61-2B12.";
t4="b1d21d5e49d2e49b8d73e4g53g8f69f1d39e7e69-4f1c42g8f69-2g1f33g8f69-3c8f55e4g39f5g69g1f32-1h2h47";
c0_opn[7]=t1 + t2 + t3 + t4;

t1="h7h69g1f39b8d79h4h59g6h79f1d39h7d39d1d39e7e69-13g8f61e4f69g7f69-6B13.e4d52c6d59B14.c2c47g8f69b1c39b8c62c1g53-1g1f36c8g49c4d59f6d59d1b39g4f39g2f39-8e7e66g1f39f8b46c4d56f6d59c1d29-3f1d33-2f8e73c4d59f6d59f1d39-6g7g61-4B13.f1d32b8c69c2c39g8f69c1f49c8g49d1b39-9B12.e4e52c6c51d4c59-2c8f58b1c34e7e69g2g49f5g69g1e29c6c59-6c2c31e7e69c1e39-3g1f33e7e69f1e29b8d74e1g19-2c6c55-4h2h41-3f2f31-2B10.g7g61b1c39-3g1f31d7d59b1c39-4B01.d7d51b1c31-1e4d59d8d55b1c39d5a57d2d47c7c63f1c43-1g1f36g8f69-3g8f66f1c42-1g1f37c7c66f1c49c8f59-3c8g43-4f1c41-1g1f31g8f69-3d5d61d2d49g8f69g1f39a7a69-5d5d81d2d49-4g8f64b1c31f6d59-2c2c41c7c65-1e7e64-2d2d45c8g43-1f6d56c2c45d5";
t2="b69g1f39-3g1f34g7g69-4f1b51c8d79b5e29-3g1f31f6d59d2d49-6B07.d7d61b1c31-1d2d49g7g61b1c38f8g79c1e35-1f2f44-3g1f31-2g8f68b1c39b8d71g1f39e7e59f1c49f8e79-5c7c61f2f46d8a59f1d39e7e59-4g1f33-2e7e51d4e53d6e59d1d89e8d89-4g1f36b8d79f1c49f8e79e1g19e8g89f1e19c7c69a2a49-10g7g66c1e31c7c66d1d29b7b59-3f8g73d1d29-3c1g51f8g79d1d29-3f1e21f8g79-2f2f31-1B09.f2f42f8g79g1f39c7c53f1b59c8d79e4e59f6g49-5e8g86f1d39-5B08.g1f32f8g79f1e29e8g89e1g19c7c65-1c8g44-6B07.g2g31f8g79f1g29e8g89g1e29e7e59-8f1d31e7e59-2f2f31-4C20.e7e52C23.b1c31b8c62-1C25.g8f67C27.f1c43-1C26.f2f43C29.d7d59f4e59f6e49-4C25.g2g33-3C21.d2d41e5d49C22.d1d49b8c69d4e39g8f69-6C23.f1c41b8c61-1g8f6";
t3="8C24.d2d39b8c64g1f39-2c7c63g1f39-2f8c52-4C30.f2f41C31.d7d51e4d59-2C33.e5f46f1c42-1C34.g1f37C37.g7g59-3C30.f8c51g1f39d7d69-4C25.g1f38C44.b8c68C46.b1c31g8f69C47.d2d44e5d49f3d49f8b49d4c69b7c69f1d39d7d59e4d59c6d59e1g19e8g89c1g59c7c69-14C48.f1b53c6d44-1C49.f8b45e1g19e8g89d2d39d7d69-6C46.g2g32f8c59f1g29d7d69-6C44.c2c31g8f69d2d49-3d2d41e5d49c2c31-1f1c41f8c52-1g8f67e1g14-1e4e55d7d59c4b59-5C45.f3d47f8c55c1e34d8f69c2c39g8e79f1c49-5d4b31-1d4c64d8f69d1d29d7c69b1c39-6g8f64b1c32f8b49d4c69b7c69f1d39d7d59e4d59-7d4c67b7c69e4e59d8e79d1e29f6d59c2c49c8a65b2b39-2d5b64-12C60.f1b56C68.a7a67C70.b5a48b7b51a4b39-2C71.d7d61C74.c2c35C75.c8d79-2C72.e1g14-2C77";
t4=".g8f69d1e21b7b59a4b39-3d2d31b7b54a4b39-2d7d65c2c39-3d2d41e5d49e1g19f8e79f1e19-5C78.e1g18b7b51";
c0_opn[8]=t1 + t2 + t3 + t4;

t1="a4b39c8b73f1e19f8c59-3f8c53a2a49-2f8e73f1e19d7d64-1e8g85-5C80.f6e41d2d49b7b59a4b39d7d59d4e59C81.c8e69b1d24e4c59c2c39-3C82.c2c35f8c59-9C78.f8c51-1C84.f8e77C85.a4c61d7c69d2d39-3C86.d1e21b7b59a4b39-3C87.f1e19C88.b7b59a4b39d7d66C90.c2c39e8g89C91.d2d41c8g49-2C92.h2h39c6a54C96.b3c29c7c59d2d49d8c79b1d29c5d49c3d49-8C94.c6b81C95.d2d49b8d79b1d29c8b79b3c29f8e89d2f19-8C92.c8b72d2d49f8e89b1d23e7f89-2f3g56e8f89g5f39f8e89f3g59-8f6d71d2d49-2f8e81-5C88.e8g83a2a41c8b79d2d39d7d69-4C89.c2c35d7d54e4d59f6d59f3e59c6e59e1e59c7c69d2d49e7d69e5e19-10C90.d7d65h2h39c6a59b3c29c7c59d2d49-7C88.d2d41-1h2h31c8b79d2d39-11C68.b5c61d7c69b1c31-1d2d41e5d49d1d49d8d49f3d4";
t2="9-5e1g17c8g42h2h39h7h59d2d39-4C69.d8d62-1f7f65d2d49c8g43d4e59d8d19f1d19-4e5d46f3d49c6c59d4b39d8d19f1d19-12C61.c6d41f3d49e5d49-3C62.d7d61d2d49-2C63.f7f51b1c36f5e49c3e49-3d2d33f5e49d3e49-4C64.f8c51c2c33-1e1g16-2C60.g7g61-1g8e71-1C65.g8f61d2d31d7d69-2e1g18C67.f6e48d2d49e4d69b5c69d7c69d4e59d6f59d1d89e8d89b1c39d8e89h2h39-12C65.f8c51c2c39-5C50.f1c41f8c54C51.b2b41c5b49c2c39-3C53.c2c35g8f69d2d35a7a63-1d7d66-2d2d44e5d49C54.c3d49c5b49b1c34-1c1d25-7C50.d2d31g8f69-2e1g11g8f69-3f8e71-1C55.g8f64d2d35f8c53c2c39-2f8e75e1g19e8g89f1e19d7d69-5h7h61-2C56.d2d41e5d49e1g14-1e4e55-3C57.f3g52d7d59e4d59C58.c6a59c4b59c7c69C59.d5c69b7c69-11C41.d7d61d2d48b8d72";
t3="f1c49-2e5d45f3d49g8f69b1c39f8e79-5g8f62b1c39b8d79-4f1c41-2C40.f7f51-1C42.g8f61b1c31b8c66d2d44e5d49f3d49-3f1b55-2f8b43-2C43.d2d41f6e49f1d39d7d59f3e59b8d79e5d79c8d79e1g19-9C42.f3e56d7d69e5f39f6e49b1c31e4c39d2c39f8e79-4d1e22d8e79d2d39e4f69c1g59e7e29f1e29f8e79b1c39c7c69-10d2d46d6d59f1d39b8c64e1g19f8e79c2c49c6b49d3e29e8g89b1c39-8f8d63e1g19e8g89c2c49c7c69-5f8e72e1g19b8c69c2c49-14C00.e7e61b1c31d7d59-2d1e21c7c59-2d2d31c7c52g1f39b8c69g2g39-4d7d57b1d27c7c53g1f39b8c69g2g39-4g8f66g1f39b7b63-1b8c62-1c7c53g2g39b8c69f1g29-7d1e22-3d2d48c7c51-1d7d59C01.b1c34b8c61-1C10.d5e41c3e49b8d76g1f39g8f69e4f69d7f69-5c8d73g1f39d7c69f1d39b8d79-7C15.f8b45e4d51e6";
t4="d59f1d39b8c69a2a39-5C16.e4e58b7b61-1C17.c7c57C18.a2a38b4a51b2b49c5d49c3b59a5c79-5C19.b4c38";
c0_opn[9]=t1 + t2 + t3 + t4;

t1="b2c39d8a51c1d29-2d8c72d1g44f7f59-2g1f35-2g8e76d1g46d8c74g4g79h8g89g7h79c5d49g1e29-6e8g85f1d39-3g1f33-5C17.c1d21g8e79-3C16.d8d71-1g8e71a2a39b4c39b2c39c7c59d1g49-7C15.g1e21d5e49a2a39b4e79-5C10.g8f63C11.c1g56C13.d5e43c3e49b8d73g1f39-2f8e76g5f69e7f65g1f39-2g7f64g1f39-6C12.f8b42e4e59h7h69g5d29b4c39b2c39f6e49d1g49g7g69f1d39e4d29e1d29c7c59-13C13.f8e73e4e59C14.f6d79g5e76d8e79f2f49a7a65g1f39c7c59-3e8g84g1f39-5h2h43-5C11.e4e53f6d79c3e21c7c59c2c39-3f2f48c7c59g1f39b8c69c1e39a7a64d1d29b7b59-3c5d45f3d49f8c59d1d29e8g89e1c19-15C03.b1d23a7a61g1f39c7c59-3C04.b8c61g1f39g8f69e4e59f6d79-5C07.c7c52e4d56d8d55g1f39c5d49f1c49d5d69e1g19g8f69d2b39b8c69b3d49";
t2="c6d49f3d49a7a69-13C08.e6d54f1b53-1C09.g1f36b8c69f1b59f8d69-6C07.g1f33b8c63e4d59e6d59-3c5d44e4d59d8d59f1c49d5d69-5g8f62-3C03.d5e41d2e49b8d76g1f39g8f69e4f69d7f69-5c8d73g1f39d7c69f1d39b8d79-7f8e71f1d35c7c59d4c59g8f69d1e29-5g1f34g8f69-3C05.g8f64C06.e4e59f6d79c2c32c7c59f1d39b8c69g1e29c5d49c3d49f7f69e5f69d7f69-10f1d35c7c59c2c39b8c69g1e29c5d47c3d49f7f69e5f69d7f69d2f36f8d69e1g19-3e1g13f8d69d2f39-8d8b62d2f39c5d49c3d49f7f69-10f2f41c7c59c2c39b8c69d2f39d8b69-8C05.f1d31c7c59-4C01.e4d51e6d59c2c41g8f69-2f1d34b8c63c2c39f8d69-3f8d66g1f39-3g1f34f8d64c2c49-2g8f65f1d39-5C02.e4e51c7c59c2c39b8c67g1f39c8d74a2a33-1f1e26g8e79-3d8b64a2a35c5c49-2f1d31-1f1e22";
t3="-2g8e71-3d8b62g1f39b8c64a2a39-2c8d75-4g1f31-5C00.g1f31d7d59b1c35g8f69e4e59f6d79d2d49c7c59d4c59-7e4e54c7c59b2b49-6A40.g7g61b1c31f8g79-2B06.d2d49c7c61-1d7d61b1c39f8g79-3f8g78b1c36c7c63c1e31-1f1c42d7d69-2f2f42d7d59e4e59-3g1f33d7d55-1d7d64-3d7d66c1e34a7a66d1d29-2c7c63d1d29-3f2f43g8f69g1f39e8g89-4g1f31-3c2c31d7d69-2c2c41d7d69b1c39-3f2f41-1g1f31d7d69b1c35-1f1c44-6B02.g8f61b1c31d7d58e4d54f6d59f1c49-3e4e55-2e7e51-2e4e58f6d59b1c31-1c2c41d5b69c4c53b6d59-2d2d46d7d69e5d69-5B03.d2d48d7d69c2c43d5b69e5d67c7d64b1c39g7g69-3e7d65b1c39f8e79-4f2f42d6e59f4e59-5B04.g1f36c8g45B05.f1e29c7c63-1e7e66e1g19f8e79c2c49d5b69-7B04.d6e52f3e59-2g7g62f1c49d5b69c4b39";
t4="f8g79.A02.f2f41A03.d7d56g1f39g7g64-1g8f65-3A02.e7e51-1g8f61g1f39.A04.g1f31b7b61-1b8c61";
c0_opn[10]=t1 + t2 + t3 + t4;

t1="d2d49d7d59-3c7c51b2b31-1c2c46b8c64b1c35e7e53-1g7g66-2d2d43c5d49f3d49-3g2g31-2g7g62d2d49-2g8f63b1c37e7e69g2g39-3g2g32-3e2e41-1g2g32b8c67f1g29g7g69e1g19f8g79-5g7g62f1g29f8g79-5A06.d7d52b2b31c8g44-1g8f65c1b29-3A09.c2c42c7c64b2b32-1d2d42g8f69-2e2e34g8f69b1c39-4d5c41-1d5d41-1e7e63d2d43-1g2g36g8f69f1g29-5A06.d2d43c7c61c2c49e7e64-1g8f65-3e7e61c2c49-2g8f66c2c49c7c64b1c39-2d5c41e2e39-2e7e64b1c37f8e79-2g2g32-5e2e31-1A07.g2g33b8c61-1A08.c7c51f1g29b8c69-3A07.c7c62f1g29c8g47e1g19b8d79-3g8f62-3c8g41f1g29b8d79-3g7g61f1g29f8g79-3g8f63f1g29c7c65e1g19-2e7e64e1g19f8e79-7A04.d7d61d2d49c8g45-1g8f64-3e7e61c2c45-1g2g34-2f7f51c2c41g8f69-2d2d43g8f69-2g2g35";
t2="g8f69f1g29g7g69-5g7g61c2c42f8g79b1c34-1d2d45-3d2d43f8g79c2c49-3e2e41f8g79d2d49d7d69-4g2g32f8g79f1g29-4A05.g8f64b2b31g7g69c1b29f8g79g2g39-5c2c45b7b61g2g39c8b79f1g29e7e69e1g19-6c7c51b1c37b8c64g2g39-2d7d52c4d59f6d59-3e7e62g2g39-3g2g32b7b69f1g29c8b79-5c7c61b1c34-1d2d45d7d59-3d7d61d2d49-2e7e62b1c34d7d55d2d49f8e79-3f8b44d1c29e8g89-4d2d41-1g2g34b7b63f1g29c8b79e1g19f8e79-5d7d56d2d42-1f1g27f8e79e1g19e8g89-7g7g63b1c36d7d52c4d59f6d59-3f8g77d2d41e8g89-2e2e47d7d69d2d49e8g89f1e29e7e59e1g19b8c69d4d59c6e79-10g2g31e8g89f1g29-5b2b31f8g79c1b29-3d2d41f8g79g2g39-3g2g32f8g79f1g29e8g89d2d42-1e1g17d7d69b1c34-1d2d45-9d2d41d7d52c2c49-2e7e63c2c49-2g7g64c2c49f8";
t3="g79b1c39-5g2g32b7b51f1g29c8b79-3b7b61f1g29c8b79e1g19e7e69-5c7c51f1g29-2d7d52f1g29c7c67e1g19c8g49-3e7e62-3g7g65b2b32f8g79c1b29e8g89f1g29d7d69d2d49-7f1g27f8g79c2c41-1e1g18e8g89c2c43d7d69-2d2d33d7d54-1d7d65-2d2d43d7d69.A00.g2g31c7c51f1g29b8c69-3d7d53f1g26c7c64-1g8f65-2g1f33-2e7e51f1g29-2g7g61f1g29f8g79c2c49-4g8f62f1g29d7d55-1g7g64";
c0_opn[11]=t1 + t2 + t3;
}

public string c0_Opening(string c0_fmoves)
{
string c0_retdata, c0_mvs, c0_s, c0_c, c0_ECO, c0_kf, c0_NMoves, c0_OName, c0_op, c0_next;
int c0_i, c0_j, c0_pt, c0_nm;

c0_retdata="";

c0_mvs="";
c0_s="";
c0_c="";

c0_ECO="";
c0_kf="";

c0_i=0;
c0_j=0;

c0_pt=0;
c0_nm=0;


c0_NMoves="";
c0_OName="";
c0_op="";

for(c0_i=1; c0_i<=11; c0_i++)
	{
	c0_s=c0_opn[ c0_i ];
	c0_j=0;
	while( c0_j<Len(c0_s) )
		{
		c0_c=Mid(c0_s,c0_j, 1 );		// Looking for special symbols or type of information...
		if(c0_c=="-")				// Other variant...
			{
			c0_j++;
			for(c0_nm=0; c0_j+c0_nm<Len(c0_s) &&
				InStr("0123456789",Mid(c0_s,c0_j+c0_nm,1))>=0; c0_nm++);

							// Next value is length for moves to shorten...
			c0_mvs=Mid(c0_mvs,0, Len(c0_mvs)- (4*Int( Mid(c0_s,c0_j,c0_nm) )) );
			c0_j+=c0_nm;
			}
		else if(c0_c==".")			// Will be other opening or variant...
			{
			c0_j++;
			c0_mvs="";
			}
		else if(InStr("abcdefgh",c0_c)>=0)	// If it is a chess move...
			{
			c0_mvs=c0_mvs + Mid(c0_s,c0_j,4);
			c0_j+=4;
			}
		else if(InStr("0123456789",c0_c)>=0)	// If it is a coefficient (for best move searches)...
			{
			c0_kf=c0_c;
			if((Len(c0_mvs)>Len(c0_fmoves)) && (Mid(c0_mvs,0,Len(c0_fmoves))==c0_fmoves))
				{
				c0_next= Mid(c0_mvs,Len(c0_fmoves),4);

				if(InStr(c0_NMoves,c0_next)<0) c0_NMoves = c0_NMoves + c0_next + " (" + c0_kf + ") ";
				}
			c0_j++;
			}
		else					// Opening information... ECO code and name (Main name for x00)
			{
			c0_ECO=Mid(c0_s,c0_j,3);
			c0_j+=3;
			for(c0_pt=0; Mid(c0_s,c0_j+c0_pt,1)!="."; c0_pt++);

			if((Len(c0_mvs)<=Len(c0_fmoves)) && (Mid(c0_fmoves,0,Len(c0_mvs))==c0_mvs))
				{
				if(Len(c0_mvs)>Len(c0_op) && Len(c0_op)<Len(c0_fmoves))
					{
					c0_op=c0_mvs;
					c0_OName="ECO " + c0_ECO;
					}
				}

			c0_j+=(c0_pt+1);
			}
		}
	}
					// Sorting by coeff. descending
for(c0_i=1;c0_i<10;c0_i++)
	{
	c0_j=6;
	while(c0_j<Len(c0_NMoves)-9)
		{
		c0_j+=9;
		if( Int(Mid(c0_NMoves,c0_j,1))==c0_i && Int(Mid(c0_NMoves,c0_j,1))>=Int(Mid(c0_NMoves,6,1)) )
			{
			c0_NMoves=Mid(c0_NMoves,c0_j-6,9) + Mid(c0_NMoves,0,c0_j-6) + Mid2(c0_NMoves,c0_j-6+9);
			}
		}
	}

if( Len(c0_NMoves)>0 ) c0_retdata=c0_NMoves + c0_OName;

return c0_retdata;
}

}	// end of class
