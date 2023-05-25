using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball3and11Red : Bullet
{
    private void Awake()
    {
        damage *= 3;
        bulletSpeed *= 0.6f;
    }
}
