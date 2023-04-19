using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weapon : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20;
    public int nrOfBullets = 10;
    public AmmoText ammoTextBox;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && nrOfBullets > 0)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
            nrOfBullets--;
        }
        ammoTextBox.UpdateAmmoText(nrOfBullets);
    }
}
