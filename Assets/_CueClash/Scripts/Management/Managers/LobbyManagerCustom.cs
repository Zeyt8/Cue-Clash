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
    public static string PlayerName;
    public static Lobby JoinedLobby;
    public static event Action<List<Lobby>> OnLobbyListUpdated;
    public static event Action OnLobbyRefresh;
    public static event Action OnLobbyDisconnect;
    public static bool IsLobbyHost { get; private set; }

    private static ILobbyEvents LobbyEvents;
    private float heartbeatTimer;
    private float lobbiesUpdateTimer;

    private void OnDisable()
    {
        OnLobbyListUpdated = null;
        OnLobbyRefresh = null;
        OnLobbyDisconnect = null;
    }

    private async void Update()
    {
        if (IsLobbyHost)
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer > 20)
            {
                heartbeatTimer = 0;
                await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
            }
        }

        lobbiesUpdateTimer += Time.deltaTime;
        if (UnityServices.State == ServicesInitializationState.Initialized && lobbiesUpdateTimer > 30)
        {
            lobbiesUpdateTimer = 0;
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
            JoinedLobby = lobby;
            IsLobbyHost = true;
            await SubscribeToLobbyChanges();
            string relay = await CreateRelay();
            await Lobbies.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "KEY_RELAY", new DataObject(DataObject.VisibilityOptions.Member, relay) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
        catch (RelayServiceException e)
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
            await SubscribeToLobbyChanges();
            await JoinRelay(JoinedLobby.Data["KEY_RELAY"].Value);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
        catch (RelayServiceException e)
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
            await SubscribeToLobbyChanges();
            await JoinRelay(JoinedLobby.Data["KEY_RELAY"].Value);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
        catch (RelayServiceException e)
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
            await SubscribeToLobbyChanges();
            await JoinRelay(JoinedLobby.Data["KEY_RELAY"].Value);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
        catch (RelayServiceException e)
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
            JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions
            {
                HostId = newHost.Id
            });
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
            await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);
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
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName)
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

    private static async Task SubscribeToLobbyChanges()
    {
        LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        callbacks.KickedFromLobby += OnKickedFromLobby;
        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
        try
        {
            LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(JoinedLobby.Id, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{JoinedLobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                default: throw;
            }
        }
    }

    private async static void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            OnLobbyDisconnect?.Invoke();
            OnLobbyDisconnect = null;
            RefreshLobbyList();
        }
        else
        {
            changes.ApplyToLobby(JoinedLobby);
            JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
            OnLobbyRefresh?.Invoke();
        }
    }

    private static void OnKickedFromLobby()
    {
        OnLobbyDisconnect?.Invoke();
        LobbyEvents = null;
        OnLobbyDisconnect = null;
        RefreshLobbyList();
    }

    private static void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed: /* Update the UI if necessary, as the subscription has been stopped. */ break;
            case LobbyEventConnectionState.Subscribing: /* Update the UI if necessary, while waiting to be subscribed. */ break;
            case LobbyEventConnectionState.Subscribed: /* Update the UI if necessary, to show subscription is working. */ break;
            case LobbyEventConnectionState.Unsynced: /* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */ break;
            case LobbyEventConnectionState.Error: OnLobbyDisconnect?.Invoke(); break;
        }
    }
}
