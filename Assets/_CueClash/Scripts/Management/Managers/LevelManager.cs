using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkSingleton<LevelManager>
{
    [SerializeField] private NetworkObject _playerPrefab;
    
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
        NetworkObject no = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
        no.SpawnWithOwnership(clientId);
    }
}