using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float life = 3;

    private void Awake()
    {
        Destroy(gameObject, life);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collisionObject = collision.gameObject;
        int damageGiven = 30;
        if (collisionObject.TryGetComponent<Limb>(out Limb limb))
        {
            limb.TakeDamage(damageGiven);
        }
        Destroy(gameObject);
    }
}
