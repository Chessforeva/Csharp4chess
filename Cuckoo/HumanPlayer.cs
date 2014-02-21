using System;
using System.Collections.Generic;

using Cuckoo;

namespace Cuckoo
{

/**
 * A player that reads input from the keyboard.
 */
public class HumanPlayer : Player {
    private string lastCmd = "";
    public string inp;

    public HumanPlayer() {
        inp = "";    // commands come heree
    }

    //@Override
    public string getCommand(Position pos, bool drawOffer, List<Position> history) {
        try {
            string moveStr = inp;
            if (moveStr == null)
                return "quit";
            if (moveStr.Length == 0) {
                return lastCmd;
            } else {
                lastCmd = moveStr;
            }
            return moveStr;
        } catch (IOException ex) {
            return "quit";
        }
    }
    
    //@Override
    public bool isHumanPlayer() {
        return true;
    }
    
    //@Override
    public void useBook(bool bookOn) {
    }

    //@Override
    public void timeLimit(int minTimeLimit, int maxTimeLimit, bool randomMode) {
    }

    //@Override
    public void clearTT() {
    }
}

}
