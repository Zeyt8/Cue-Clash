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
    [SerializeField] private TextMeshProUGUI _text;
    private bool _canClose = false;

    private void ChangeType(LoadingType type, string customText = "")
    {
        switch (type)
        {
            case LoadingType.Connecting:
                _text.text = "Connecting...";
                break;
            case LoadingType.CreatingRoom:
                _text.text = "Creating room...";
                break;
            case LoadingType.JoiningRoom:
                _text.text = "Joining room...";
                break;
            case LoadingType.Error:
                _text.text = customText;
                break;
        }
    }

    public void ShowLoad(LoadingType type, string customText = "")
    {
        ChangeType(type, customText);
        gameObject.SetActive(true);
        _canClose = type == LoadingType.Error;
    }

    public void Close()
    {
        if (_canClose)
        {
            gameObject.SetActive(false);
        }
    }
}
