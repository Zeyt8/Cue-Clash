using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManagerCustom : Singleton<LobbyManagerCustom>
{
    public static Lobby JoinedLobby;
    public static event Action<List<Lobby>> OnLobbyListUpdated;
    public static event Action OnLobbyRefresh;
    public static bool IsLobbyHost => _hostLobby != null;

    private static Lobby _hostLobby;
    private float _heartbeatTimer;
    private float _lobbyUpdateTimer;
    private float _lobbiesUpdateTimer;

    private void OnDisable()
    {
        OnLobbyListUpdated = null;
        OnLobbyRefresh = null;
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

        if (JoinedLobby != null)
        {
            _lobbyUpdateTimer += Time.deltaTime;
            if (_lobbyUpdateTimer > 2)
            {
                _lobbyUpdateTimer = 0;

                RefreshLobby();
            }
        }

        _lobbiesUpdateTimer += Time.deltaTime;
        if (UnityServices.State == ServicesInitializationState.Initialized && _lobbiesUpdateTimer > 30)
        {
            _lobbiesUpdateTimer = 0;
            RefreshLobbyList();
        }
    }

    public static async Task ConnectToServer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            
            // To be able to use lobby with 2 local instances
#if UNITY_EDITOR
            AuthenticationService.Instance.ClearSessionToken();
#endif
            
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            RefreshLobbyList();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static void DisconnectFromServer()
    {
        AuthenticationService.Instance.SignOut();
    }

    public static async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        int maxPlayers = 2;
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = GetPlayer(),
            Data = new Dictionary<string, DataObject>
            {
                { "KEY_RELAY", new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }
        };
        try
        {
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            _hostLobby = lobby;
            JoinedLobby = lobby;
            string relay = await CreateRelay();
            lobby = await Lobbies.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "KEY_RELAY", new DataObject(DataObject.VisibilityOptions.Member, relay) }
                }
            });
            JoinedLobby = lobby;
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
            await JoinRelay(JoinedLobby.Data["KEY_RELAY"].Value);
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
            await JoinRelay(JoinedLobby.Data["KEY_RELAY"].Value);
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
            await JoinRelay(JoinedLobby.Data["KEY_RELAY"].Value);
        }
        catch (LobbyServiceException e)
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
            _hostLobby = null;
            JoinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public static async void RefreshLobbyList()
    {
        List<Lobby> lobbies = await ListLobbies();
        OnLobbyListUpdated?.Invoke(lobbies);
    }

    public static async void RefreshLobby()
    {
        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
        JoinedLobby = lobby;
        OnLobbyRefresh?.Invoke();
    }

    private static async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }

    private static async Task JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        }
        catch (RelayServiceException e)
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

    private static async Task<List<Lobby>> ListLobbies()
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
}
