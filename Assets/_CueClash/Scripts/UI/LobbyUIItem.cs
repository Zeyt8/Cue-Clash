using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _sessionNameText;
    [SerializeField] private TextMeshProUGUI _sessionPlayersText;

    private Lobby _lobby;

    public void SetInformation(Lobby lobby)
    {
        _lobby = lobby;
        _sessionNameText.text = lobby.Name;
        _sessionPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void SelectSession()
    {
        MainMenuManager.Instance.SetCurrentSelectedLobby(_lobby);
    }
}
