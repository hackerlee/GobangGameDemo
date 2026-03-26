using System.Collections.Generic;
using UnityEngine;

public class GomokuAI
{
    private const int BoardSize = 15;
    private readonly RuleChecker ruleChecker = new RuleChecker();

    public Vector2Int GetBestMove(int[,] board, int aiPiece, int playerPiece)
    {
        if (TryFindWinningMove(board, aiPiece, out Vector2Int winMove))
        {
            return winMove;
        }

        if (TryFindWinningMove(board, playerPiece, out Vector2Int blockMove))
        {
            return blockMove;
        }

        List<Vector2Int> candidates = GetCandidateCells(board);
        if (candidates.Count == 0)
        {
            return FindFirstLegalMove(board);
        }

        int bestScore = int.MinValue;
        Vector2Int bestMove = candidates[0];

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];
            int score = EvaluateMove(board, candidate.x, candidate.y, aiPiece, playerPiece);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = candidate;
            }
        }

        return bestMove;
    }

    public bool TryFindWinningMove(int[,] board, int piece, out Vector2Int move)
    {
        List<Vector2Int> candidates = GetCandidateCells(board);
        if (candidates.Count == 0)
        {
            candidates = GetAllEmptyCells(board);
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int cell = candidates[i];
            board[cell.x, cell.y] = piece;
            bool win = ruleChecker.CheckWin(board, cell.x, cell.y, piece);
            board[cell.x, cell.y] = 0;

            if (win)
            {
                move = cell;
                return true;
            }
        }

        move = default;
        return false;
    }

    public List<Vector2Int> GetCandidateCells(int[,] board)
    {
        HashSet<Vector2Int> set = new HashSet<Vector2Int>();
        bool hasPiece = false;

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (board[x, y] == 0)
                {
                    continue;
                }

                hasPiece = true;
                for (int oy = -2; oy <= 2; oy++)
                {
                    for (int ox = -2; ox <= 2; ox++)
                    {
                        if (Mathf.Abs(ox) + Mathf.Abs(oy) > 2)
                        {
                            continue;
                        }

                        int nx = x + ox;
                        int ny = y + oy;
                        if (nx < 0 || nx >= BoardSize || ny < 0 || ny >= BoardSize)
                        {
                            continue;
                        }

                        if (board[nx, ny] == 0)
                        {
                            set.Add(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }

        if (!hasPiece)
        {
            set.Add(new Vector2Int(BoardSize / 2, BoardSize / 2));
        }

        return new List<Vector2Int>(set);
    }

    public int EvaluateMove(int[,] board, int x, int y, int aiPiece, int playerPiece)
    {
        int score = 0;

        board[x, y] = aiPiece;
        score += EvaluatePatternScore(board, x, y, aiPiece) * 2;
        board[x, y] = 0;

        board[x, y] = playerPiece;
        score += EvaluatePatternScore(board, x, y, playerPiece);
        board[x, y] = 0;

        int centerBias = 14 - (Mathf.Abs(7 - x) + Mathf.Abs(7 - y));
        score += centerBias;

        return score;
    }

    private int EvaluatePatternScore(int[,] board, int x, int y, int piece)
    {
        int total = 0;
        total += LineScore(board, x, y, 1, 0, piece);
        total += LineScore(board, x, y, 0, 1, piece);
        total += LineScore(board, x, y, 1, 1, piece);
        total += LineScore(board, x, y, 1, -1, piece);
        return total;
    }

    private int LineScore(int[,] board, int x, int y, int dx, int dy, int piece)
    {
        int forward = CountContinuous(board, x, y, dx, dy, piece);
        int backward = CountContinuous(board, x, y, -dx, -dy, piece);
        int length = 1 + forward + backward;

        bool openEnd1 = IsOpenEnd(board, x, y, dx, dy, forward, piece);
        bool openEnd2 = IsOpenEnd(board, x, y, -dx, -dy, backward, piece);
        int openEnds = (openEnd1 ? 1 : 0) + (openEnd2 ? 1 : 0);

        if (length >= 5) return 100000;
        if (length == 4 && openEnds == 2) return 20000;
        if (length == 4 && openEnds == 1) return 8000;
        if (length == 3 && openEnds == 2) return 2500;
        if (length == 3 && openEnds == 1) return 600;
        if (length == 2 && openEnds == 2) return 200;

        return 20;
    }

    private int CountContinuous(int[,] board, int x, int y, int dx, int dy, int piece)
    {
        int count = 0;
        int cx = x + dx;
        int cy = y + dy;

        while (cx >= 0 && cx < BoardSize && cy >= 0 && cy < BoardSize && board[cx, cy] == piece)
        {
            count++;
            cx += dx;
            cy += dy;
        }

        return count;
    }

    private bool IsOpenEnd(int[,] board, int x, int y, int dx, int dy, int continuous, int piece)
    {
        int nx = x + dx * (continuous + 1);
        int ny = y + dy * (continuous + 1);
        if (nx < 0 || nx >= BoardSize || ny < 0 || ny >= BoardSize)
        {
            return false;
        }

        return board[nx, ny] == 0;
    }

    private Vector2Int FindFirstLegalMove(int[,] board)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (board[x, y] == 0)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(0, 0);
    }

    private List<Vector2Int> GetAllEmptyCells(int[,] board)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (board[x, y] == 0)
                {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }

        return result;
    }
}
