using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject _lobbyItemListPrefab;
    [SerializeField] private Transform _lobbyListParent;

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

    private void AddSessionToList(Lobby lobby)
    {
        LobbyUIItem sessionItem = Instantiate(_lobbyItemListPrefab, _lobbyListParent).GetComponent<LobbyUIItem>();
        sessionItem.SetInformation(lobby);
    }
}
