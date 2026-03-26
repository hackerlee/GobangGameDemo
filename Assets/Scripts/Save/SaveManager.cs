using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SaveManager
{
    private const string KeyExists = "CurrentGame_Exists";
    private const string KeyBoard = "CurrentGame_Board";
    private const string KeyIsPlayerTurn = "CurrentGame_IsPlayerTurn";
    private const string KeyPlayerPiece = "CurrentGame_PlayerPiece";
    private const string KeyAIPiece = "CurrentGame_AIPiece";
    private const string KeyIsGameOver = "CurrentGame_IsGameOver";
    private const string KeyFirstPiece = "CurrentGame_FirstPiece";
    private const string KeyPlayerStartsFirst = "CurrentGame_PlayerStartsFirst";
    private const string KeyMoveHistory = "CurrentGame_MoveHistory";

    private const string KeyRecordTotal = "Record_TotalGames";
    private const string KeyRecordPlayerWins = "Record_PlayerWins";
    private const string KeyRecordAIWins = "Record_AIWins";
    private const string KeyRecordDraws = "Record_Draws";

    public static void SaveCurrentGame(GameData data)
    {
        if (data == null || data.flatBoard == null || data.flatBoard.Length != 225)
        {
            return;
        }

        PlayerPrefs.SetInt(KeyExists, 1);
        PlayerPrefs.SetString(KeyBoard, SerializeBoard(data.flatBoard));
        PlayerPrefs.SetInt(KeyIsPlayerTurn, data.isPlayerTurn ? 1 : 0);
        PlayerPrefs.SetInt(KeyPlayerPiece, data.playerPiece);
        PlayerPrefs.SetInt(KeyAIPiece, data.aiPiece);
        PlayerPrefs.SetInt(KeyIsGameOver, data.isGameOver ? 1 : 0);
        PlayerPrefs.SetInt(KeyFirstPiece, data.firstPiece);
        PlayerPrefs.SetInt(KeyPlayerStartsFirst, data.playerStartsFirst ? 1 : 0);
        PlayerPrefs.SetString(KeyMoveHistory, SerializeHistory(data.moveHistory));
        PlayerPrefs.Save();
    }

    public static GameData LoadCurrentGame()
    {
        if (PlayerPrefs.GetInt(KeyExists, 0) != 1)
        {
            return null;
        }

        try
        {
            GameData data = new GameData
            {
                flatBoard = DeserializeBoard(PlayerPrefs.GetString(KeyBoard, string.Empty)),
                isPlayerTurn = PlayerPrefs.GetInt(KeyIsPlayerTurn, 1) == 1,
                playerPiece = PlayerPrefs.GetInt(KeyPlayerPiece, 1),
                aiPiece = PlayerPrefs.GetInt(KeyAIPiece, 2),
                isGameOver = PlayerPrefs.GetInt(KeyIsGameOver, 0) == 1,
                firstPiece = PlayerPrefs.GetInt(KeyFirstPiece, 1),
                playerStartsFirst = PlayerPrefs.GetInt(KeyPlayerStartsFirst, -1) == 1,
                moveHistory = DeserializeHistory(PlayerPrefs.GetString(KeyMoveHistory, string.Empty))
            };

            if (data.flatBoard == null || data.flatBoard.Length != 225)
            {
                return null;
            }

            if (data.moveHistory == null)
            {
                data.moveHistory = new List<MoveData>();
            }

            return data;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void ClearCurrentGame()
    {
        PlayerPrefs.DeleteKey(KeyExists);
        PlayerPrefs.DeleteKey(KeyBoard);
        PlayerPrefs.DeleteKey(KeyIsPlayerTurn);
        PlayerPrefs.DeleteKey(KeyPlayerPiece);
        PlayerPrefs.DeleteKey(KeyAIPiece);
        PlayerPrefs.DeleteKey(KeyIsGameOver);
        PlayerPrefs.DeleteKey(KeyFirstPiece);
        PlayerPrefs.DeleteKey(KeyPlayerStartsFirst);
        PlayerPrefs.DeleteKey(KeyMoveHistory);
        PlayerPrefs.Save();
    }

    public static bool HasUnfinishedGame()
    {
        return PlayerPrefs.GetInt(KeyExists, 0) == 1 && PlayerPrefs.GetInt(KeyIsGameOver, 0) == 0;
    }

    public static RecordData LoadRecordData()
    {
        return new RecordData
        {
            totalGames = PlayerPrefs.GetInt(KeyRecordTotal, 0),
            playerWins = PlayerPrefs.GetInt(KeyRecordPlayerWins, 0),
            aiWins = PlayerPrefs.GetInt(KeyRecordAIWins, 0),
            draws = PlayerPrefs.GetInt(KeyRecordDraws, 0)
        };
    }

    public static void SaveRecordData(RecordData data)
    {
        if (data == null)
        {
            return;
        }

        PlayerPrefs.SetInt(KeyRecordTotal, data.totalGames);
        PlayerPrefs.SetInt(KeyRecordPlayerWins, data.playerWins);
        PlayerPrefs.SetInt(KeyRecordAIWins, data.aiWins);
        PlayerPrefs.SetInt(KeyRecordDraws, data.draws);
        PlayerPrefs.Save();
    }

    private static string SerializeBoard(int[] board)
    {
        return string.Join(",", board.Select(v => v.ToString()).ToArray());
    }

    private static int[] DeserializeBoard(string serialized)
    {
        if (string.IsNullOrEmpty(serialized))
        {
            return new int[225];
        }

        string[] parts = serialized.Split(',');
        if (parts.Length != 225)
        {
            return null;
        }

        int[] board = new int[225];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out board[i]))
            {
                return null;
            }
        }

        return board;
    }

    private static string SerializeHistory(List<MoveData> history)
    {
        if (history == null || history.Count == 0)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        for (int i = 0; i < history.Count; i++)
        {
            MoveData move = history[i];
            parts.Add($"{move.x}_{move.y}_{move.piece}_{move.stepIndex}");
        }

        return string.Join("|", parts);
    }

    private static List<MoveData> DeserializeHistory(string serialized)
    {
        List<MoveData> result = new List<MoveData>();
        if (string.IsNullOrEmpty(serialized))
        {
            return result;
        }

        string[] chunks = serialized.Split('|');
        for (int i = 0; i < chunks.Length; i++)
        {
            string[] parts = chunks[i].Split('_');
            if (parts.Length < 3)
            {
                continue;
            }

            if (!int.TryParse(parts[0], out int x) ||
                !int.TryParse(parts[1], out int y) ||
                !int.TryParse(parts[2], out int piece))
            {
                continue;
            }

            int step = i;
            if (parts.Length >= 4)
            {
                int.TryParse(parts[3], out step);
            }

            result.Add(new MoveData(x, y, piece, step));
        }

        return result;
    }
}
