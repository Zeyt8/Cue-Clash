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
        GameObject limb = collision.gameObject;
        limb.GetComponent<Limb>().TakeDamage();
        Destroy(gameObject);
    }
}
