using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
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
