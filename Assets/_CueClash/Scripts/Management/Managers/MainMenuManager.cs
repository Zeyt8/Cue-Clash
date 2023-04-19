using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : Singleton<MainMenuManager>
{
    [SerializeField] private string _lobbySceneName;
    [Header("UI Elements")]
    [SerializeField] private LoadingPanel _loadingPanel;
    [SerializeField] private LobbyUIHandler _lobbyListPanel;
    [SerializeField] private PlayerLobbyUIHandler _lobbyPanel;
    [SerializeField] private TMP_InputField _nameInput;

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
            LobbyManagerCustom.PlayerName = _nameInput.text == "" ? "Player" : _nameInput.text;
            await LobbyManagerCustom.JoinLobbyById(_currentSelectedLobby.Id);
            _loadingPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);
            LobbyManagerCustom.OnLobbyDisconnect += DisableLobbyMenu;
            
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            if (_currentSelectedLobby == null)
                _loadingPanel.ShowLoad(LoadingType.Error, "No lobby selected");
            else
                _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void JoinPrivateLobby()
    {
        _loadingPanel.ShowLoad(LoadingType.JoiningRoom);
        try
        {
            LobbyManagerCustom.PlayerName = _nameInput.text == "" ? "Player" : _nameInput.text;
            await LobbyManagerCustom.JoinLobbyByCode(_lobbyListPanel.PrivateLobbyCode);
            _loadingPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);
            LobbyManagerCustom.OnLobbyDisconnect += DisableLobbyMenu;

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            if (_lobbyListPanel.PrivateLobbyCode == "" || _lobbyListPanel.PrivateLobbyCode == null)
                _loadingPanel.ShowLoad(LoadingType.Error, "Please enter a lobby code");
            else
                _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void CreateLobby()
    {
        _loadingPanel.ShowLoad(LoadingType.CreatingRoom);
        try
        {
            LobbyManagerCustom.PlayerName = _nameInput.text == "" ? "Player" : _nameInput.text;
            await LobbyManagerCustom.CreateLobby(_lobbyListPanel.CreateLobbyName, _lobbyListPanel.IsPrivateLobby);
            _loadingPanel.gameObject.SetActive(false);
            _lobbyPanel.gameObject.SetActive(true);
            _lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);
            LobbyManagerCustom.OnLobbyDisconnect += DisableLobbyMenu;

            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            if (_lobbyListPanel.CreateLobbyName == "" || _lobbyListPanel.CreateLobbyName == null)
                _loadingPanel.ShowLoad(LoadingType.Error, "Please enter a lobby name");
            else
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
            NetworkManager.Singleton.Shutdown();
            await LobbyManagerCustom.LeaveLobby();
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
            NetworkManager.Singleton.Shutdown();
            await LobbyManagerCustom.DeleteLobby();
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

    public void DisconnectFromServer()
    {
        LobbyManagerCustom.DisconnectFromServer();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void DisableLobbyMenu()
    {
        _lobbyPanel.gameObject.SetActive(false);
    }
}
