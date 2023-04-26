using TMPro;
using UnityEngine;

public enum LoadingType
{
    Connecting,
    CreatingRoom,
    JoiningRoom,
    Error
}

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private bool canClose = false;

    private void ChangeType(LoadingType type, string customText = "")
    {
        switch (type)
        {
            case LoadingType.Connecting:
                text.text = "Connecting...";
                break;
            case LoadingType.CreatingRoom:
                text.text = "Creating room...";
                break;
            case LoadingType.JoiningRoom:
                text.text = "Joining room...";
                break;
            case LoadingType.Error:
                text.text = customText;
                break;
        }
    }

    public void ShowLoad(LoadingType type, string customText = "")
    {
        ChangeType(type, customText);
        gameObject.SetActive(true);
        canClose = type == LoadingType.Error;
    }

    public void Close()
    {
        if (canClose)
        {
            gameObject.SetActive(false);
        }
    }
}
