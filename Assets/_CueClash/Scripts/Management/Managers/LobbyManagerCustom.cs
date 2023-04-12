using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManagerCustom : Singleton<LobbyManagerCustom>
{
    public static Lobby JoinedLobby;
    public static event Action<List<Lobby>> OnLobbyListUpdated;

    private static Lobby _hostLobby;
    private float _heartbeatTimer;
    private float _lobbyUpdateTimer;
    private float _lobbiesUpdateTimer;

    private void OnDisable()
    {
        OnLobbyListUpdated = null;
    }

    private async void Update()
    {
        if (_hostLobby != null)
        {
            _heartbeatTimer += Time.deltaTime;
            if (_heartbeatTimer > 20)
            {
                _heartbeatTimer = 0;
                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }

        /*if (JoinedLobby != null)
        {
            _lobbyUpdateTimer += Time.deltaTime;
            if (_lobbyUpdateTimer > 5)
            {
                _lobbyUpdateTimer = 0;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                JoinedLobby = lobby;
            }
        }*/

        _lobbiesUpdateTimer += Time.deltaTime;
        if (UnityServices.State == ServicesInitializationState.Initialized && _lobbiesUpdateTimer > 10)
        {
            _lobbiesUpdateTimer = 0;
            List<Lobby> lobbies = await ListLobbies();
            OnLobbyListUpdated?.Invoke(lobbies);
        }
    }

    public static async Task ConnectToServer()
    {
        try
        {
            // For development, we want to use the development environment.
            var options = new InitializationOptions();
            options.SetEnvironmentName("development");
            
            await UnityServices.InitializeAsync(options);
            
            // To be able to use lobby with 2 local instances
#if UNITY_EDITOR
            AuthenticationService.Instance.ClearSessionToken();
#endif
            
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Connected to server");
            List<Lobby> lobbies = await ListLobbies();
            OnLobbyListUpdated?.Invoke(lobbies);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        int maxPlayers = 4;
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = GetPlayer()
        };
        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            _hostLobby = lobby;
            JoinedLobby = lobby;
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task JoinLobbyById(string lobbyId)
    {
        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
        {
            Player = GetPlayer()
        };
        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            JoinedLobby = lobby;
            Debug.Log("Joined Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task JoinLobbyByCode(string lobbyCode)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer()
        };
        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            JoinedLobby = lobby;
            Debug.Log("Joined Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            JoinedLobby = lobby;
            Debug.Log("Joined Lobby!");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private static Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "PlayerName",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Player" + UnityEngine.Random.Range(0, 100))
                }
            }
        };
    }

    public static async Task<List<Lobby>> ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 10,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            return queryResponse.Results;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task MigrateLobbyHost(Player newHost)
    {
        try
        {
            _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHost.Id
            });
            JoinedLobby = _hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async Task DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_hostLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }
}
