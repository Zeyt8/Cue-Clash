using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball5and13Orange : Bullet
{
    private void Awake()
    {
        damage = damage * 2 + damage / 2;
        bulletSpeed *= 0.9f;
    }
}
