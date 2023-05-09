using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float life = 3;
    public int damage = 30;
    public int ballNumber = -1;
    public float bulletSpeed = 20f;
    public Player player;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject collisionObject = other.gameObject;
        if (collisionObject.TryGetComponent<Limb>(out Limb limb))
        {
            limb.TakeDamage(damage);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
