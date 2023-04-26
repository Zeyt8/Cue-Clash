using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : Singleton<MainMenuManager>
{
    [SerializeField] private string lobbySceneName;
    [Header("UI Elements")]
    [SerializeField] private LoadingPanel loadingPanel;
    [SerializeField] private LobbyUIHandler lobbyListPanel;
    [SerializeField] private PlayerLobbyUIHandler lobbyPanel;
    [SerializeField] private TMP_InputField nameInput;

    private Lobby currentSelectedLobby;

    public async void ConnectToServer()
    {
        loadingPanel.ShowLoad(LoadingType.Connecting);
        try
        {
            await LobbyManagerCustom.ConnectToServer();
            loadingPanel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void JoinLobby()
    {
        loadingPanel.ShowLoad(LoadingType.JoiningRoom);
        try
        {
            LobbyManagerCustom.PlayerName = nameInput.text == "" ? "Player" : nameInput.text;
            await LobbyManagerCustom.JoinLobbyById(currentSelectedLobby.Id);
            loadingPanel.gameObject.SetActive(false);
            lobbyPanel.gameObject.SetActive(true);
            lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);
            LobbyManagerCustom.OnLobbyDisconnect += DisableLobbyMenu;
            
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            if (currentSelectedLobby == null)
                loadingPanel.ShowLoad(LoadingType.Error, "No lobby selected");
            else
                loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void JoinPrivateLobby()
    {
        loadingPanel.ShowLoad(LoadingType.JoiningRoom);
        try
        {
            LobbyManagerCustom.PlayerName = nameInput.text == "" ? "Player" : nameInput.text;
            await LobbyManagerCustom.JoinLobbyByCode(lobbyListPanel.PrivateLobbyCode);
            loadingPanel.gameObject.SetActive(false);
            lobbyPanel.gameObject.SetActive(true);
            lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);
            LobbyManagerCustom.OnLobbyDisconnect += DisableLobbyMenu;

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            if (lobbyListPanel.PrivateLobbyCode == "" || lobbyListPanel.PrivateLobbyCode == null)
                loadingPanel.ShowLoad(LoadingType.Error, "Please enter a lobby code");
            else
                loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public async void CreateLobby()
    {
        loadingPanel.ShowLoad(LoadingType.CreatingRoom);
        try
        {
            LobbyManagerCustom.PlayerName = nameInput.text == "" ? "Player" : nameInput.text;
            await LobbyManagerCustom.CreateLobby(lobbyListPanel.CreateLobbyName, lobbyListPanel.IsPrivateLobby);
            loadingPanel.gameObject.SetActive(false);
            lobbyPanel.gameObject.SetActive(true);
            lobbyPanel.SetCode(LobbyManagerCustom.JoinedLobby.LobbyCode);
            LobbyManagerCustom.OnLobbyDisconnect += DisableLobbyMenu;

            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            if (lobbyListPanel.CreateLobbyName == "" || lobbyListPanel.CreateLobbyName == null)
                loadingPanel.ShowLoad(LoadingType.Error, "Please enter a lobby name");
            else
                loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public void SetCurrentSelectedLobby(Lobby lobby)
    {
        currentSelectedLobby = lobby;
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
            loadingPanel.ShowLoad(LoadingType.Error, e.Message);
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
            loadingPanel.ShowLoad(LoadingType.Error, e.Message);
        }
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
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
        lobbyPanel.gameObject.SetActive(false);
    }
}
