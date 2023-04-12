using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;

public class MainMenuManager : Singleton<MainMenuManager>
{
    [SerializeField] private string _lobbySceneName;
    [Header("UI Elements")]
    [SerializeField] private LoadingPanel _loadingPanel;
    [SerializeField] private TMP_InputField _createLobbyNameInput;
    [SerializeField] private TMP_InputField _joinPrivateLobbyCodeInput;
    [SerializeField] private Toggle _privateLobbyToggle;

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
            NetworkManager.Singleton.StartClient();
            _loadingPanel.gameObject.SetActive(false);
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
            await LobbyManagerCustom.JoinLobbyByCode(_joinPrivateLobbyCodeInput.text);
            NetworkManager.Singleton.StartClient();
            _loadingPanel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            _loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void QuickJoinLobby()
    {
        _loadingPanel.ShowLoad(LoadingType.JoiningRoom);
        try
        {
            await LobbyManagerCustom.QuickJoinLobby();
            NetworkManager.Singleton.StartClient();
            _loadingPanel.gameObject.SetActive(false);
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
            await LobbyManagerCustom.CreateLobby(_createLobbyNameInput.text, _privateLobbyToggle.isOn);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(_lobbySceneName, LoadSceneMode.Single);
            _loadingPanel.gameObject.SetActive(false);
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
}
