using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public const int BoardSize = 15;

    [Header("Board UI")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject blackPiecePrefab;
    [SerializeField] private GameObject whitePiecePrefab;

    private int[,] board = new int[BoardSize, BoardSize];
    private readonly List<MoveData> moveHistory = new List<MoveData>();
    private readonly List<GameObject> spawnedPieces = new List<GameObject>();
    private readonly BoardCell[,] boardCells = new BoardCell[BoardSize, BoardSize];

    public IReadOnlyList<MoveData> MoveHistory => moveHistory;

    public void InitializeBoard(System.Action<int, int> onCellClicked)
    {
        ClearSpawnedPiecesOnly();

        board = new int[BoardSize, BoardSize];
        moveHistory.Clear();

        BuildCellsIfNeeded(onCellClicked);
        SetCellsInteractable(true);
    }

    public void LoadBoardFromData(int[] flatBoard, List<MoveData> history, System.Action<int, int> onCellClicked)
    {
        board = new int[BoardSize, BoardSize];
        moveHistory.Clear();

        if (history != null)
        {
            moveHistory.AddRange(history);
        }

        if (flatBoard != null && flatBoard.Length == BoardSize * BoardSize)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    board[x, y] = flatBoard[y * BoardSize + x];
                }
            }
        }
        else
        {
            RebuildBoardStateFromHistory();
        }

        BuildCellsIfNeeded(onCellClicked);
        RebuildBoardVisualFromState();
    }

    public bool CanPlace(int x, int y)
    {
        return IsInside(x, y) && board[x, y] == 0;
    }

    public bool PlacePiece(int x, int y, int piece, bool recordHistory = true)
    {
        if (!CanPlace(x, y))
        {
            return false;
        }

        board[x, y] = piece;

        if (recordHistory)
        {
            moveHistory.Add(new MoveData(x, y, piece, moveHistory.Count));
        }

        SpawnPieceVisual(x, y, piece);
        return true;
    }

    public void RemoveLastMove(int count)
    {
        int removeCount = Mathf.Clamp(count, 0, moveHistory.Count);
        if (removeCount == 0)
        {
            return;
        }

        moveHistory.RemoveRange(moveHistory.Count - removeCount, removeCount);
        RebuildBoardFromHistory();
    }

    public void RebuildBoardFromHistory()
    {
        RebuildBoardStateFromHistory();
        RebuildBoardVisualFromState();
    }

    public int[,] GetBoardCopy()
    {
        int[,] copy = new int[BoardSize, BoardSize];
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                copy[x, y] = board[x, y];
            }
        }

        return copy;
    }

    public bool IsBoardFull()
    {
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

    public int[] ToFlatBoard()
    {
        int[] flat = new int[BoardSize * BoardSize];

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                flat[y * BoardSize + x] = board[x, y];
            }
        }

        return flat;
    }

    public void SetCellsInteractable(bool interactable)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (boardCells[x, y] != null)
                {
                    boardCells[x, y].SetInteractable(interactable);
                }
            }
        }
    }

    private void BuildCellsIfNeeded(System.Action<int, int> onCellClicked)
    {
        if (boardRoot == null || cellPrefab == null)
        {
            return;
        }

        if (HasAllCells())
        {
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    boardCells[x, y].Setup(x, y, onCellClicked);
                }
            }

            return;
        }

        // 只在首次创建棋盘格时清理容器，避免后续对局中增删格子导致索引变化。
        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, boardRoot);
                BoardCell cell = cellObj.GetComponent<BoardCell>();
                if (cell == null)
                {
                    cell = cellObj.AddComponent<BoardCell>();
                }

                cell.Setup(x, y, onCellClicked);
                boardCells[x, y] = cell;
            }
        }
    }

    private bool HasAllCells()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (boardCells[x, y] == null)
                {
                    return false;
                }

                if (boardCells[x, y].gameObject == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void RebuildBoardStateFromHistory()
    {
        board = new int[BoardSize, BoardSize];
        for (int i = 0; i < moveHistory.Count; i++)
        {
            MoveData move = moveHistory[i];
            move.stepIndex = i;
            if (IsInside(move.x, move.y))
            {
                board[move.x, move.y] = move.piece;
            }
        }
    }

    private void RebuildBoardVisualFromState()
    {
        ClearSpawnedPiecesOnly();

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                int piece = board[x, y];
                if (piece != 0)
                {
                    SpawnPieceVisual(x, y, piece);
                }
            }
        }
    }

    private void SpawnPieceVisual(int x, int y, int piece)
    {
        if (boardRoot == null)
        {
            return;
        }

        GameObject prefab = piece == 1 ? blackPiecePrefab : whitePiecePrefab;
        if (prefab == null)
        {
            return;
        }

        Transform parent = boardCells[x, y] != null ? boardCells[x, y].transform : boardRoot;
        GameObject pieceObj = Instantiate(prefab, parent, false);
        pieceObj.name = $"Piece_{x}_{y}_{piece}";
        spawnedPieces.Add(pieceObj);
    }

    private void ClearSpawnedPiecesOnly()
    {
        for (int i = spawnedPieces.Count - 1; i >= 0; i--)
        {
            if (spawnedPieces[i] != null)
            {
                Destroy(spawnedPieces[i]);
            }
        }

        spawnedPieces.Clear();
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
    }
}
