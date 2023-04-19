using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public NetworkObject bulletPrefab;
    public float bulletSpeed = 20;
    public NetworkVariable<int> nrOfBullets = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public AmmoText ammoTextBox;

    public void Shoot()
    {
        SpawnBulletServerRpc();
        //ammoTextBox.UpdateAmmoText(nrOfBullets);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc()
    {
        if (nrOfBullets.Value > 0)
        {
            NetworkObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.Spawn();
            bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
            nrOfBullets.Value--;
        }
    }
}
