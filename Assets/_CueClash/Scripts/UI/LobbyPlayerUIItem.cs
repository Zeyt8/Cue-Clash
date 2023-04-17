using TMPro;
using UnityEngine;

public class LobbyPlayerUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;

    public void SetInformation(string playerName)
    {
        _playerNameText.text = playerName;
    }
}
