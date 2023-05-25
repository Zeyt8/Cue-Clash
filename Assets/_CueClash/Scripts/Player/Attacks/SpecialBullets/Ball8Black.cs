using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball8Black : Bullet
{
    private void Awake()
    {
        damage += damage/2;
        bulletSpeed *= 2f;
    }
}
