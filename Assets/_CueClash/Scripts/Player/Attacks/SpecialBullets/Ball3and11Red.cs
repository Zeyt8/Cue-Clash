using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball3and11Red : Bullet
{
    private void Awake()
    {
        ballNumber = 3;
        damage *= 2;
    }
}
