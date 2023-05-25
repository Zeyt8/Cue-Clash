using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball6and14Green : Bullet
{
    private void Awake()
    {
        damage += damage / 2;
        bulletSpeed *= 1.25f;
    }
}
