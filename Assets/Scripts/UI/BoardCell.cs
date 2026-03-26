using UnityEngine;
using UnityEngine.UI;

public class BoardCell : MonoBehaviour
{
    [SerializeField] private Button clickButton;
    private int x;
    private int y;
    private System.Action<int, int> onClick;

    public void Setup(int cellX, int cellY, System.Action<int, int> clickCallback)
    {
        x = cellX;
        y = cellY;
        onClick = clickCallback;

        if (clickButton == null)
        {
            clickButton = GetComponent<Button>();
        }

        if (clickButton != null)
        {
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(NotifyClick);
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (clickButton != null)
        {
            clickButton.interactable = interactable;
        }
    }

    private void NotifyClick()
    {
        if (onClick != null)
        {
            onClick.Invoke(x, y);
        }
    }
}
