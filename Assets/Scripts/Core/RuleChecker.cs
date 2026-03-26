public class RuleChecker
{
    private const int BoardSize = 15;

    public bool CheckWin(int[,] board, int x, int y, int piece)
    {
        if (board == null || board.GetLength(0) != BoardSize || board.GetLength(1) != BoardSize)
        {
            return false;
        }

        return IsFiveOrMore(board, x, y, 1, 0, piece) ||
               IsFiveOrMore(board, x, y, 0, 1, piece) ||
               IsFiveOrMore(board, x, y, 1, 1, piece) ||
               IsFiveOrMore(board, x, y, 1, -1, piece);
    }

    public bool CheckDraw(int[,] board)
    {
        if (board == null)
        {
            return false;
        }

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (board[x, y] == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public int CountInDirection(int[,] board, int x, int y, int dx, int dy, int piece)
    {
        int count = 0;
        int cx = x + dx;
        int cy = y + dy;

        while (IsInside(cx, cy) && board[cx, cy] == piece)
        {
            count++;
            cx += dx;
            cy += dy;
        }

        return count;
    }

    private bool IsFiveOrMore(int[,] board, int x, int y, int dx, int dy, int piece)
    {
        int total = 1;
        total += CountInDirection(board, x, y, dx, dy, piece);
        total += CountInDirection(board, x, y, -dx, -dy, piece);
        return total >= 5;
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
    }
}
