using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private Text turnText;
    [SerializeField] private Text playerInfoText;
    [SerializeField] private Text resultText;

    [Header("Buttons")]
    [SerializeField] private Button undoButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button restartButton;

    [Header("Panels")]
    [SerializeField] private GameObject resultPanel;

    public void Bind(System.Action onUndo, System.Action onBack, System.Action onRestart)
    {
        BindButton(undoButton, onUndo);
        BindButton(backButton, onBack);
        BindButton(restartButton, onRestart);
    }

    public void SetPlayerInfo(int playerPiece)
    {
        string pieceText = playerPiece == 1 ? "黑子" : "白子";
        if (playerInfoText != null)
        {
            playerInfoText.text = "你执: " + pieceText;
        }
    }

    public void SetTurnText(bool isPlayerTurn)
    {
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "当前回合：玩家" : "当前回合：机器人";
        }
    }

    public void SetUndoInteractable(bool interactable)
    {
        if (undoButton != null)
        {
            undoButton.interactable = interactable;
        }
    }

    public void ShowResult(string message)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    public void HideResult()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void BindButton(Button button, System.Action callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        if (callback != null)
        {
            button.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayButtonClick();
                }

                callback.Invoke();
            });
        }
    }
}
