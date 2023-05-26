using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public int damage = 20;
    public float bulletSpeed = 20f;
    public Player player;

    private bool destroy = false;

    private NetworkObject networkObject;

    public void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        if (networkObject != null && networkObject.IsSpawned && destroy)
        {
            networkObject.Despawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        GameObject collisionObject = collision.gameObject;
        int damageGiven = 30;
        if (collisionObject.TryGetComponent(out Limb limb))
        {
            limb.TakeDamage(damageGiven);
        }

        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn();
        }
        else
        {
            destroy = true;
        }
    }
}
