using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkSingleton<LevelManager>
{
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    [Header("UI")]
    public AmmoText AmmoText;

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
        NetworkObject no = Instantiate(_playerPrefab, _spawnPoints[clientId % (ulong)_spawnPoints.Length].position, Quaternion.identity);
        no.SpawnWithOwnership(clientId);
    }
}