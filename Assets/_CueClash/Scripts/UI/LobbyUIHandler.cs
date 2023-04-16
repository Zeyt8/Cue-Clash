using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUIHandler : MonoBehaviour
{
    [SerializeField] private LobbyUIItem _lobbyItemListPrefab;
    [SerializeField] private Transform _lobbyListParent;

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
