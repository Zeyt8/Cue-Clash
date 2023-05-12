using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball2and10Blue : Bullet
{
    private void Awake()
    {
        ballNumber = 2;
        bulletSpeed *= 1.5f;
    }
}
