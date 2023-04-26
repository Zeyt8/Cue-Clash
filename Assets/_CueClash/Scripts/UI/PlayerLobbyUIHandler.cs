using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private LobbyPlayerUIItem playerUIItemPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

    private void Start()
    {
        SetPlayers();
        if (!LobbyManagerCustom.IsLobbyHost)
        {
            startButton.gameObject.SetActive(false);
            leaveButton.onClick.AddListener(() => MainMenuManager.Instance.LeaveLobby());
        }
        else
        {
            leaveButton.onClick.AddListener(() => MainMenuManager.Instance.DeleteLobby());
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
        lobbyCodeText.text = code;
    }

    public void SetPlayers()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (Player t in LobbyManagerCustom.JoinedLobby.Players)
        {
            LobbyPlayerUIItem item = Instantiate(playerUIItemPrefab, content);
            item.SetInformation(t.Data["PlayerName"].Value);
        }
    }
}
