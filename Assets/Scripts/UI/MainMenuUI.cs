using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button historyButton;

    [SerializeField] private GameObject historyPanel;
    [SerializeField] private Text historyText;
    [SerializeField] private Text noSaveTipText;

    private const string GameSceneName = "GameScene";

    private void Start()
    {
        BindButtons();
        RefreshMenuState();
        CloseHistoryPanel();
    }

    private void OnEnable()
    {
        RefreshMenuState();
    }

    public void RefreshMenuState()
    {
        bool hasSave = SaveManager.HasUnfinishedGame();

        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
        }

        if (noSaveTipText != null)
        {
            noSaveTipText.gameObject.SetActive(false);
        }
    }

    private void BindButtons()
    {
        BindButton(continueButton, ContinueGame);
        BindButton(newGameButton, StartNewGame);
        BindButton(historyButton, ToggleHistoryPanel);
    }

    private void ContinueGame()
    {
        if (!SaveManager.HasUnfinishedGame())
        {
            if (noSaveTipText != null)
            {
                noSaveTipText.gameObject.SetActive(true);
                noSaveTipText.text = "没有可继续的未完成对局";
            }

            RefreshMenuState();
            return;
        }

        PlayerPrefs.SetInt("LoadMode", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(GameSceneName);
    }

    private void StartNewGame()
    {
        PlayerPrefs.SetInt("LoadMode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(GameSceneName);
    }

    private void ToggleHistoryPanel()
    {
        if (historyPanel == null)
        {
            return;
        }

        bool nextState = !historyPanel.activeSelf;
        historyPanel.SetActive(nextState);
        if (nextState)
        {
            RecordData record = SaveManager.LoadRecordData();
            if (historyText != null)
            {
                historyText.text =
                    $"总对局数: {record.totalGames}\n" +
                    $"玩家胜利: {record.playerWins}\n" +
                    $"机器人胜利: {record.aiWins}\n" +
                    $"平局: {record.draws}";
            }
        }
    }

    private void CloseHistoryPanel()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }
    }

    private void BindButton(Button button, System.Action callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback.Invoke());
    }
}
