using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public NetworkObject bulletPrefab;
    public float bulletSpeed = 20;
    public NetworkVariable<int> nrOfBullets = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void Shoot()
    {
        SpawnBulletServerRpc();
        LevelManager.Instance.ammoText.UpdateAmmoText(nrOfBullets.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc()
    {
        if (nrOfBullets.Value > 0)
        {
            NetworkObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.Spawn();
            bullet.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward * bulletSpeed, ForceMode.VelocityChange);
            nrOfBullets.Value--;
        }
    }
}
