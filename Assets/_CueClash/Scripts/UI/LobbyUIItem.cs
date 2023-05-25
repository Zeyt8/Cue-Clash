using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIItem : MonoBehaviour
{
    [HideInInspector] public LobbyUIHandler lobbyUIHandler;
    private Image image;
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI sessionPlayersText;

    private Lobby lobby;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

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
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }

    public void DeselectSession()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.6f);
    }
}
