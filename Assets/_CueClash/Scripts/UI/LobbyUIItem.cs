using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIItem : MonoBehaviour
{
    [HideInInspector] public LobbyUIHandler lobbyUIHandler;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI sessionPlayersText;

    private Lobby lobby;

    public void SetInformation(Lobby lobby)
    {
        this.lobby = lobby;
        sessionNameText.text = lobby.Name;
        sessionPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void SelectSession()
    {
        lobbyUIHandler.DeselectAll();
        MainMenuManager.Instance.SetCurrentSelectedLobby(lobby);
        image.color = new Color(image.color.r, image.color.g, image.color.b, 255);
    }

    public void DeselectSession()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 150);
    }
}
