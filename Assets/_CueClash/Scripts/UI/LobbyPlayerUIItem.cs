using TMPro;
using UnityEngine;

public class LobbyPlayerUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    public void SetInformation(string playerName)
    {
        playerNameText.text = playerName;
    }
}
