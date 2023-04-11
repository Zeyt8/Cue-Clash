using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public void Move(Vector3 move)
    {
        transform.position += move;
    }
}
