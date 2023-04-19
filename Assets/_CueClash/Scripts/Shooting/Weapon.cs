using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public Transform bulletSpawnPoint;
    public NetworkObject bulletPrefab;
    public float bulletSpeed = 20;
    public int nrOfBullets = 10;
    public AmmoText ammoTextBox;

    public void Shoot()
    {
        if (nrOfBullets > 0)
        {
            NetworkObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.Spawn();
            bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
            nrOfBullets--;
        }
        //ammoTextBox.UpdateAmmoText(nrOfBullets);
    }
}
