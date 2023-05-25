using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball7and15Brown : Bullet
{
    private void Awake()
    {
        damage = damage * 2;
        bulletSpeed *= 1.1f;
    }
}
