using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkSingleton<LevelManager>
{
    [SerializeField] private NetworkObject playerPrefab;
    public Transform[] spawnPoints;
    public List<PlayerObject> players = new List<PlayerObject>();

    [Header("UI")]
    public AmmoText ammoText;

    private void Start()
    {
        Cursor.visible = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            NetworkManager.SceneManager.OnLoadComplete += LevelManager_OnLoadComplete;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsHost)
        {
            NetworkManager.SceneManager.OnLoadComplete -= LevelManager_OnLoadComplete;
        }
    }

    private void LevelManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        NetworkObject no = Instantiate(playerPrefab, spawnPoints[clientId % (ulong)spawnPoints.Length].position, Quaternion.identity);
        no.SpawnWithOwnership(clientId);
    }
}