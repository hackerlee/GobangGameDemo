using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int[] flatBoard;
    public bool isPlayerTurn;
    public int playerPiece;
    public int aiPiece;
    public bool isGameOver;
    public int firstPiece;
    public bool playerStartsFirst;
    public List<MoveData> moveHistory;

    public GameData()
    {
        flatBoard = new int[15 * 15];
        moveHistory = new List<MoveData>();
    }
}
