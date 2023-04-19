using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float life = 3;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        GameObject collisionObject = collision.gameObject;
        int damageGiven = 30;
        if (collisionObject.TryGetComponent<Limb>(out Limb limb))
        {
            limb.TakeDamage(damageGiven);
        }
        Destroy(gameObject);
    }
}
