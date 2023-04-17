using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyCodeText;
    [SerializeField] private LobbyPlayerUIItem _playerUIItemPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _leaveButton;

    private void Start()
    {
        SetPlayers();
        if (!LobbyManagerCustom.IsLobbyHost)
        {
            _startButton.gameObject.SetActive(false);
            _leaveButton.onClick.AddListener(() => MainMenuManager.Instance.LeaveLobby());
        }
        else
        {
            _leaveButton.onClick.AddListener(() => MainMenuManager.Instance.DeleteLobby());
        }
    }

    private void OnEnable()
    {
        LobbyManagerCustom.OnLobbyRefresh += SetPlayers;
    }

    private void OnDisable()
    {
        LobbyManagerCustom.OnLobbyRefresh -= SetPlayers;
    }

    public void SetCode(string code)
    {
        _lobbyCodeText.text = code;
    }

    public void SetPlayers()
    {
        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }

        foreach (Player t in LobbyManagerCustom.JoinedLobby.Players)
        {
            LobbyPlayerUIItem item = Instantiate(_playerUIItemPrefab, _content);
            item.SetInformation(t.Data["PlayerName"].Value);
        }
    }
}
