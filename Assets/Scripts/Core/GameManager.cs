using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameUI gameUI;

    private readonly TurnManager turnManager = new TurnManager();
    private readonly RuleChecker ruleChecker = new RuleChecker();
    private readonly GomokuAI aiController = new GomokuAI();

    private bool isGameOver;
    private bool isInputReady;
    private bool isAIThinking;
    private bool hasInitializedGame;
    private int playerPiece;
    private int aiPiece;
    private int firstPiece;

    private const string MainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBgmIfNeeded();
        }

        if (boardManager == null)
        {
            boardManager = FindObjectOfType<BoardManager>();
        }

        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }

        if (gameUI != null)
        {
            gameUI.Bind(UndoLastRound, ReturnToMenu, StartNewGame);
            gameUI.HideResult();
        }

        int loadMode = PlayerPrefs.GetInt("LoadMode", 0);
        bool loaded = false;

        if (loadMode == 1)
        {
            loaded = LoadFromSave();
        }

        if (!loaded)
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        SaveManager.ClearCurrentGame();
        isGameOver = false;
        isAIThinking = false;
        hasInitializedGame = true;

        bool playerGoesFirst = Random.Range(0, 2) == 0;
        firstPiece = 1;
        playerPiece = playerGoesFirst ? 1 : 2;
        aiPiece = playerPiece == 1 ? 2 : 1;

        turnManager.Initialize(playerGoesFirst);
        boardManager.InitializeBoard(HandlePlayerMove);

        if (gameUI != null)
        {
            gameUI.HideResult();
            gameUI.SetPlayerInfo(playerPiece);
            gameUI.SetTurnText(turnManager.IsPlayerTurn);
            gameUI.SetUndoInteractable(false);
        }

        SaveSnapshot();
        BeginInputGate();

        if (!turnManager.IsPlayerTurn)
        {
            StartCoroutine(HandleAIMove());
        }
    }

    public bool LoadFromSave()
    {
        GameData data = SaveManager.LoadCurrentGame();
        if (data == null || data.isGameOver)
        {
            SaveManager.ClearCurrentGame();
            return false;
        }

        playerPiece = data.playerPiece;
        aiPiece = data.aiPiece;
        firstPiece = data.firstPiece;
        isGameOver = data.isGameOver;

        turnManager.Initialize(data.isPlayerTurn);
        boardManager.LoadBoardFromData(data.flatBoard, new System.Collections.Generic.List<MoveData>(data.moveHistory), HandlePlayerMove);

        if (gameUI != null)
        {
            gameUI.HideResult();
            gameUI.SetPlayerInfo(playerPiece);
            gameUI.SetTurnText(turnManager.IsPlayerTurn);
            gameUI.SetUndoInteractable(boardManager.MoveHistory.Count > 0);
        }

        isAIThinking = false;
        hasInitializedGame = true;
        BeginInputGate();

        if (!isGameOver && !turnManager.IsPlayerTurn)
        {
            StartCoroutine(HandleAIMove());
        }

        return true;
    }

    public void HandlePlayerMove(int x, int y)
    {
        if (isGameOver || !isInputReady || isAIThinking || !turnManager.IsPlayerTurn)
        {
            return;
        }

        if (!boardManager.PlacePiece(x, y, playerPiece, true))
        {
            return;
        }

        SaveSnapshot();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlacePiece();
        }

        if (CheckGameAfterMove(x, y, playerPiece, true))
        {
            return;
        }

        turnManager.SwitchTurn();
        UpdateUIStates();
        StartCoroutine(HandleAIMove());
    }

    public IEnumerator HandleAIMove()
    {
        if (isGameOver)
        {
            yield break;
        }

        isAIThinking = true;
        UpdateBoardInputState();
        yield return new WaitForSeconds(0.2f);

        int[,] boardCopy = boardManager.GetBoardCopy();
        Vector2Int aiMove = aiController.GetBestMove(boardCopy, aiPiece, playerPiece);

        if (!boardManager.PlacePiece(aiMove.x, aiMove.y, aiPiece, true))
        {
            isAIThinking = false;
            UpdateBoardInputState();
            yield break;
        }

        SaveSnapshot();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlacePiece();
        }

        if (CheckGameAfterMove(aiMove.x, aiMove.y, aiPiece, false))
        {
            isAIThinking = false;
            UpdateBoardInputState();
            yield break;
        }

        turnManager.SwitchTurn();
        UpdateUIStates();
        isAIThinking = false;
        UpdateBoardInputState();
    }

    public bool CheckGameAfterMove(int x, int y, int piece, bool movedByPlayer)
    {
        int[,] boardCopy = boardManager.GetBoardCopy();
        if (ruleChecker.CheckWin(boardCopy, x, y, piece))
        {
            FinishGame(movedByPlayer ? GameResult.PlayerWin : GameResult.AIWin);
            return true;
        }

        if (ruleChecker.CheckDraw(boardCopy))
        {
            FinishGame(GameResult.Draw);
            return true;
        }

        return false;
    }

    public void FinishGame(GameResult result)
    {
        isGameOver = true;

        RecordData record = SaveManager.LoadRecordData();
        record.totalGames += 1;

        string resultText;
        switch (result)
        {
            case GameResult.PlayerWin:
                record.playerWins += 1;
                resultText = "你赢了！";
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayWin();
                }
                break;
            case GameResult.AIWin:
                record.aiWins += 1;
                resultText = "机器人获胜";
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLose();
                }
                break;
            default:
                record.draws += 1;
                resultText = "平局";
                break;
        }

        SaveManager.SaveRecordData(record);
        SaveSnapshot();

        if (gameUI != null)
        {
            gameUI.ShowResult(resultText);
            gameUI.SetUndoInteractable(false);
        }

        UpdateBoardInputState();
    }

    public void UndoLastRound()
    {
        if (isGameOver)
        {
            return;
        }

        int removeCount = 0;
        int historyCount = boardManager.MoveHistory.Count;

        if (historyCount <= 0)
        {
            return;
        }

        if (turnManager.IsPlayerTurn)
        {
            removeCount = Mathf.Min(2, historyCount);
        }
        else
        {
            removeCount = 1;
        }

        boardManager.RemoveLastMove(removeCount);

        bool nowPlayerTurn = true;
        int remaining = boardManager.MoveHistory.Count;
        if (remaining == 0)
        {
            nowPlayerTurn = playerPiece == firstPiece;
        }
        else
        {
            int lastPiece = boardManager.MoveHistory[remaining - 1].piece;
            nowPlayerTurn = lastPiece != playerPiece;
        }

        turnManager.SetPlayerTurn(nowPlayerTurn);
        SaveSnapshot();
        UpdateUIStates();
        UpdateBoardInputState();
    }

    public void ReturnToMenu()
    {
        SaveProgressIfPossible();
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveProgressIfPossible();
        }
    }

    private void OnApplicationQuit()
    {
        SaveProgressIfPossible();
    }

    private void SaveSnapshot()
    {
        GameData data = new GameData
        {
            flatBoard = boardManager.ToFlatBoard(),
            isPlayerTurn = turnManager.IsPlayerTurn,
            playerPiece = playerPiece,
            aiPiece = aiPiece,
            isGameOver = isGameOver,
            firstPiece = firstPiece,
            moveHistory = new System.Collections.Generic.List<MoveData>(boardManager.MoveHistory)
        };

        SaveManager.SaveCurrentGame(data);
    }

    private void SaveProgressIfPossible()
    {
        if (!hasInitializedGame || boardManager == null)
        {
            return;
        }

        SaveSnapshot();
    }

    private void UpdateUIStates()
    {
        if (gameUI == null)
        {
            return;
        }

        gameUI.SetTurnText(turnManager.IsPlayerTurn);
        gameUI.SetUndoInteractable(!isGameOver && boardManager.MoveHistory.Count > 0);
    }

    private void BeginInputGate()
    {
        isInputReady = true;
        UpdateBoardInputState();
    }

    private void UpdateBoardInputState()
    {
        bool canInteract = isInputReady && !isGameOver && !isAIThinking && turnManager.IsPlayerTurn;
        boardManager.SetCellsInteractable(canInteract);
    }
}

public enum GameResult
{
    PlayerWin,
    AIWin,
    Draw
}
