using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIItem : MonoBehaviour
{
    [HideInInspector] public LobbyUIHandler LobbyUIHandler;
    [SerializeField] private Image _image;
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
        LobbyUIHandler.DeselectAll();
        MainMenuManager.Instance.SetCurrentSelectedLobby(_lobby);
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 255);
    }

    public void DeselectSession()
    {
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 150);
    }
}
