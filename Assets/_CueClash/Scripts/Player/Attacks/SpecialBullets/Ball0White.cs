using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball0White : Bullet
{
    private void Awake()
    {
        damage /= 2;
        bulletSpeed *= 0.5f;
    }
}
