using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject collisionObject = other.gameObject;
        int damageGiven = 30;
        if (collisionObject.TryGetComponent(out Limb limb))
        {
            limb.TakeDamage(damageGiven);
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
