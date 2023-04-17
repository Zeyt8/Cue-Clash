using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIHandler : MonoBehaviour
{
    public string CreateLobbyName => _createLobbyNameInput.text;
    public string PrivateLobbyCode => _joinPrivateLobbyCodeInput.text;
    public bool IsPrivateLobby => _privateLobbyToggle.isOn;
    [SerializeField] private LobbyUIItem _lobbyItemListPrefab;
    [SerializeField] private Transform _lobbyListParent;
    [SerializeField] private TMP_InputField _createLobbyNameInput;
    [SerializeField] private TMP_InputField _joinPrivateLobbyCodeInput;
    [SerializeField] private Toggle _privateLobbyToggle;

    private List<LobbyUIItem> _lobbyUIItems = new List<LobbyUIItem>();

    private void OnEnable()
    {
        LobbyManagerCustom.OnLobbyListUpdated += AddSessionsToList;
    }

    private void OnDisable()
    {
        LobbyManagerCustom.OnLobbyListUpdated -= AddSessionsToList;
    }

    public void AddSessionsToList(List<Lobby> lobbies)
    {
        ClearList();
        _lobbyUIItems.Clear();
        foreach (Lobby lobby in lobbies)
        {
            AddSessionToList(lobby);
        }
    }
    
    public void ClearList()
    {
        foreach (Transform child in _lobbyListParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void DeselectAll()
    {
        foreach (LobbyUIItem lobbyUIItem in _lobbyUIItems)
        {
            lobbyUIItem.DeselectSession();
        }
    }

    private void AddSessionToList(Lobby lobby)
    {
        LobbyUIItem sessionItem = Instantiate(_lobbyItemListPrefab, _lobbyListParent).GetComponent<LobbyUIItem>();
        sessionItem.LobbyUIHandler = this;
        sessionItem.SetInformation(lobby);
        _lobbyUIItems.Add(sessionItem);
    }
}
