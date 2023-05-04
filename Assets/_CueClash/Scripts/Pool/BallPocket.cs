using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPocket : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Ball ball))
        {
            PoolManager.Instance.AddSinkedBall(ball);
        }
    }
}
