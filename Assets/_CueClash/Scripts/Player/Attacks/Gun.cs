using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    public List<int> bullets = new List<int>();
    [SerializeField] private BulletDatabase bulletDatabase;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float bulletSpeed = 20;
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] float maxDistance = 100f;
    public NetworkVariable<int> nrOfBullets = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int currentSelectedBullet = 0;

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
            Camera camera = Camera.main;
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                Bullet bullet = Instantiate(bulletDatabase.bullets[currentSelectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                bullet.GetComponent<NetworkObject>().Spawn();
                Vector3 bulletDirection = (hit.point - bulletSpawnPoint.position).normalized;
                bullet.GetComponent<Rigidbody>().AddForce(bulletDirection * bulletSpeed, ForceMode.VelocityChange);
                nrOfBullets.Value--;
            }
        }
    }

    public void Activate()
    {

    }

    public void Deactivate()
    {

    }

    public void GetPlayerCamera(CinemachineVirtualCamera camera)
    {
        this.playerCamera = camera;
    }
}
