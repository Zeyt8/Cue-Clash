using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPocket : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
        PoolManager.Instance.AddSinkedBall(other.GetComponent<Ball>());
    }
}
