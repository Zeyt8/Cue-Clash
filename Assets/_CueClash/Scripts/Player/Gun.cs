using System.Linq;
using Cinemachine;
using JSAM;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    public int[] bullets = new int[16];
    public float sway;
    [SerializeField] private BulletDatabase bulletDatabase;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float bulletSpeed = 20;
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] float maxDistance = 100f;

    private int currentSelectedBullet = 0;

    private bool HasAnyAmmo => bullets.Any(bullet => bullet > 0);

    public void Shoot()
    {
        Camera camera = Camera.main;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f + Random.Range(-sway, sway), 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance) && bullets[currentSelectedBullet] > 0)
        {
            SpawnBulletServerRpc(hit.point);
        }
        bullets[currentSelectedBullet] -= 1;
        if (bullets[currentSelectedBullet] == 0)
        {
            SwitchBullet(true);
        }

        if (HasAnyAmmo)
        {
            LevelManager.Instance.ammoText.UpdateAmmoText(bullets[currentSelectedBullet], currentSelectedBullet);
        }
        else
        {
            LevelManager.Instance.ammoText.gameObject.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc(Vector3 hit)
    {
        Bullet bullet = Instantiate(bulletDatabase.bullets[currentSelectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<NetworkObject>().Spawn(true);
        Vector3 bulletDirection = (hit - bulletSpawnPoint.position).normalized;
        bullet.GetComponent<Rigidbody>().AddForce(bulletDirection * bulletSpeed, ForceMode.VelocityChange);
        AudioManager.PlaySound(Sounds.Shoot);
    }

    public void Activate()
    {
        if (HasAnyAmmo)
        {
            SwitchBullet(true);
            LevelManager.Instance.ammoText.gameObject.SetActive(true);
        }
    }

    public void Deactivate()
    {
        LevelManager.Instance.ammoText.gameObject.SetActive(false);
    }

    public void GetPlayerCamera(CinemachineVirtualCamera camera)
    {
        playerCamera = camera;
    }

    public void SwitchBullet(bool up)
    {
        if (!HasAnyAmmo) return;
        if (up)
        {
            do
            {
                currentSelectedBullet = (currentSelectedBullet + 1) % 16;
            } while (bullets[currentSelectedBullet] == 0);
        }
        else
        {
            do
            {
                currentSelectedBullet -= 1;
                if (currentSelectedBullet < 0)
                {
                    currentSelectedBullet = 15;
                }
            } while (bullets[currentSelectedBullet] == 0);
        }
        LevelManager.Instance.ammoText.UpdateAmmoText(bullets[currentSelectedBullet], currentSelectedBullet);
    }
}
