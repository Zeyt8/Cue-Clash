using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIHandler : MonoBehaviour
{
    public string CreateLobbyName => createLobbyNameInput.text;
    public string PrivateLobbyCode => joinPrivateLobbyCodeInput.text;
    public bool IsPrivateLobby => privateLobbyToggle.isOn;
    [SerializeField] private LobbyUIItem lobbyItemListPrefab;
    [SerializeField] private Transform lobbyListParent;
    [SerializeField] private TMP_InputField createLobbyNameInput;
    [SerializeField] private TMP_InputField joinPrivateLobbyCodeInput;
    [SerializeField] private Toggle privateLobbyToggle;

    private List<LobbyUIItem> lobbyUIItems = new List<LobbyUIItem>();

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
        lobbyUIItems.Clear();
        foreach (Lobby lobby in lobbies)
        {
            AddSessionToList(lobby);
        }
    }
    
    public void ClearList()
    {
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void DeselectAll()
    {
        foreach (LobbyUIItem lobbyUIItem in lobbyUIItems)
        {
            lobbyUIItem.DeselectSession();
        }
    }

    private void AddSessionToList(Lobby lobby)
    {
        LobbyUIItem sessionItem = Instantiate(lobbyItemListPrefab, lobbyListParent).GetComponent<LobbyUIItem>();
        sessionItem.lobbyUIHandler = this;
        sessionItem.SetInformation(lobby);
        lobbyUIItems.Add(sessionItem);
    }
}
