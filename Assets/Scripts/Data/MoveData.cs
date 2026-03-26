using System;

[Serializable]
public class MoveData
{
    public int x;
    public int y;
    public int piece;
    public int stepIndex;

    public MoveData(int x, int y, int piece, int stepIndex)
    {
        this.x = x;
        this.y = y;
        this.piece = piece;
        this.stepIndex = stepIndex;
    }
}
