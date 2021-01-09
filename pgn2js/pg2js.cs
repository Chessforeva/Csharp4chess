using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace pgn2js
{
    class pgn2js
    {

        static void Main(string[] args)
        {

         if (args.Length < 1)
            {
                Console.Write("Creates a 5xtimes shorter .js file for web-pages \n\n");
                Console.Write("  usage: pgn2js <pgn file of 1 event tournament>\n\n");

                Console.Write("Very slow! Applicable for c0_chess.js,c0_pgn.js based 3D-chess web pages\n");
                Console.Write("on Chessforeva or self made derivatives (freely)\n\n"); 
                Console.Write("Blog at: http://Chessforeva.blogspot.com\n");
                Console.Write("GitHub: https://gitlab.com/chessforeva/pgn2web\n");

                Console.Write("\nClick Enter.\n");
                Console.ReadKey(true);
                return;
            }


        String CurPath = Environment.CurrentDirectory.ToString();
        if (CurPath.Length > 0) CurPath += "\\";

        c0_chess C0 = new c0_chess();

        StreamReader sr = null;
        StreamWriter sw = null;
        string s = "";               // current line
        string S = "";              // contains 1 game
        bool ok = true;

        C0.c0_PGN_short = true;
        //C0.c0_SAMPLES();

        string fname = args[0];
        string fn = C0.c0_ReplaceAll(fname, ".pgn", "");
        string fnjs = fn + ".js";

    try {
        sr = new StreamReader(CurPath + fname);
        try
            {
            sw = new StreamWriter(CurPath + fn + ".js");
            }
    catch
            {
            Console.Write("Can't write file.\n");
            ok = false;
            }
        }
    catch
        {
        Console.Write("Can't read file.\n");
        ok = false;
        }

    int bl = 0;  // 1-header block, 2-game data
    int gm = 0;     // count of games
    string EvDate = "";
    string EvName = "";
    string EvSite = "\"\"";

    if (ok)
    {
        Console.Write("processing: " + fname + "\n");

        sw.Write("var pgn_" + fn + " = { event:");
        while ((s = sr.ReadLine()) != null)
        {

            if (s.IndexOf("[") >= 0)
            {
                int e;
                string w;
                if (EvName.Length == 0)
                    {
                    e = s.ToUpper().IndexOf("[EVENT ");
                    if (e >= 0)
                        {
                         w = s.Substring(s.IndexOf('"'));
                         w = w.Substring(0, w.LastIndexOf('"')+1);
                         EvName = w;
                         Console.Write(EvName + "\n");
                         sw.Write(EvName + ", games:[\n");
                        }

                    }
                if (EvSite.Length <3)
                {
                    e = s.ToUpper().IndexOf("[SITE ");
                    if (e >= 0)
                    {
                        w = s.Substring(s.IndexOf('"'));
                        w = w.Substring(0, w.LastIndexOf('"') + 1);
                        EvSite = w;
                    }

                }
                if (EvDate.Length == 0)
                    {
                    e = s.ToUpper().IndexOf("[DATE ");
                    if (e < 0) e = s.ToUpper().IndexOf("[EVENTDATE ");
                    if (e >= 0)
                    {
                        w = s.Substring(s.IndexOf('"'));
                        w = w.Substring(0, w.LastIndexOf('"') + 1);
                        EvDate = w;
                    }
                    }


                    if (bl == 2)
                    {
                        addGame(C0, sw, S, EvDate);
                        S = ""; bl = 0;
                        gm++;
                        Console.Write(gm + " ");
                    }
                if (bl == 0) bl = 1;
            }
            else
            {
                if (bl == 1 && s.IndexOf(".") >= 0) bl = 2;
            }
            S += s + " ";
        }
        if (bl == 2) { addGame(C0, sw, S, EvDate); gm++; }
        sw.Write(EvDate + "], site:" + EvSite + ", count:" + gm + " };\n");
    }
    sw.Write("\n");
    sw.Close();
    sr.Close();

        Console.Write("\n" + gm + " games");
        //Console.ReadKey(true);
        return;
        }

        static void addGame(c0_chess C0, StreamWriter sw, string S, string edt)
        {
            C0.c0_side = 1;
            C0.c0_start_FEN = "";
            C0.c0_set_start_position("");

            string mlist = C0.c0_get_moves_from_PGN(S);

            string m = C0.c0_PG_sh;

            
              // to verify backwards
            /*
            C0.c0_set_start_position("");
            C0.c0_short2list();
            if (C0.c0_moveslist != mlist) C0.Log("Not same as encoded.");
            */

                string Out = "";

                sw.Write('"');
                for (int i = 0; i < C0.c0_PGN_header.Length; i++)
                {
                    string g = C0.c0_PGN_header[i].Trim();
                    if (g.Length > 0)
                    {

                        string w = g;
                        int e = w.IndexOf('"');
                        if(e>=0) w = (w+" ").Substring(w.IndexOf('"')+1);
                        e = w.LastIndexOf('"');
                        if(e>0) w = w.Substring(0, e);
                        g = g.ToUpper();

                        if (g.StartsWith("DATE ")) Out += ("[D_" + w + "]");
                        if (g.StartsWith("SETUP ")) Out += ("[S_" + w + "]");
                        if (g.StartsWith("FEN ")) Out += ("[F_" + w + "]");
                        if (g.StartsWith("ROUND ")) Out += ("[R_" + w + "]");
                        if (g.StartsWith("WHITE ")) Out += ("[W_" + w + "]");
                        if (g.StartsWith("BLACK ")) Out += ("[B_" + w + "]");
                        if (g.StartsWith("WHITEELO ")) Out += ("[w_" + w + "]");
                        if (g.StartsWith("BLACKELO ")) Out += ("[b_" + w + "]");
                        if (g.StartsWith("ECO ")) Out += ("[C_" + w + "]");
                        if (g.StartsWith("RESULT "))
                        {
                            if (w.IndexOf("2") >= 0 || w.IndexOf("5") >= 0) w = "1/2";
                            else w = (w + "   ").Substring(0, 3);
                            Out += ("[Z_" + w + "]");
                        }
                    }
                }
                Out += C0.c0_PG_sh + (C0.c0_errflag ? "[ERROR]" : "");
                Out = Out.Replace('"',' ');
                Out = Out.Replace('\'', ' ');
                sw.Write( Out + '"' + ",\n\r");

        }

    }
}
