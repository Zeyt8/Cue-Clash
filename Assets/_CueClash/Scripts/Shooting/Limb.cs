using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    private int health = 100;

    public void TakeDamage()
    {
        health -= 30;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
