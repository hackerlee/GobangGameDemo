using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoardCell : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Button clickButton;
    private int x;
    private int y;
    private System.Action<int, int> onClick;
    private bool isInteractable = true;

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
        }
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        if (clickButton != null)
        {
            clickButton.interactable = interactable;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (onClick != null)
        {
            onClick.Invoke(x, y);
        }
    }
}
