using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public int damage = 30;
    public float bulletSpeed = 20f;
    public Player player;

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        GameObject collisionObject = collision.gameObject;
        int damageGiven = 30;
        if (collisionObject.TryGetComponent(out Limb limb))
        {
            limb.TakeDamage(damageGiven);
        }
        GetComponent<NetworkObject>().Despawn();
    }
}
