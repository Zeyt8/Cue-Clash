using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;

public class MainMenuManager : Singleton<MainMenuManager>
{
    [SerializeField] private string _lobbySceneName;
    [Header("UI Elements")]
    [SerializeField] private LoadingPanel _loadingPanel;
    [SerializeField] private LobbyUIHandler _lobbyListPanel;
    [SerializeField] private PlayerLobbyUIHandler _lobbyPanel;

    private Lobby _currentSelectedLobby;

    public async void ConnectToServer()
    {
        _loadingPanel.ShowLoad(LoadingType.Connecting);
        try
        {
            await LobbyManagerCustom.ConnectToServer();
            _loadingPanel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void JoinLobby()
    {
        _loadingPanel.ShowLoad(LoadingType.JoiningRoom);
        try
        {
            await LobbyManagerCustom.JoinLobbyById(_currentSelectedLobby.Id);
            _loadingPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void JoinPrivateLobby()
    {
        _loadingPanel.ShowLoad(LoadingType.JoiningRoom);
        try
        {
            await LobbyManagerCustom.JoinLobbyByCode(_lobbyListPanel.PrivateLobbyCode);
            _loadingPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void CreateLobby()
    {
        _loadingPanel.ShowLoad(LoadingType.CreatingRoom);
        try
        {
            await LobbyManagerCustom.CreateLobby(_lobbyListPanel.CreateLobbyName, _lobbyListPanel.IsPrivateLobby);
            _loadingPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);

            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public void SetCurrentSelectedLobby(Lobby lobby)
    {
        _currentSelectedLobby = lobby;
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyManagerCustom.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            _lobbyPanel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyManagerCustom.DeleteLobby();
            NetworkManager.Singleton.Shutdown();
            _lobbyPanel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(_lobbySceneName, LoadSceneMode.Single);
    }

    public void RefreshLobbyList()
    {
        LobbyManagerCustom.RefreshLobbyList();
    }
}
