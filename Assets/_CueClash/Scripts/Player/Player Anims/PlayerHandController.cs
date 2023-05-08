using Unity.Netcode;
using UnityEngine;

public class PlayerHandController : NetworkBehaviour
{
    public Vector3 desiredPosition;
    public Quaternion desiredRotation;

    private void LateUpdate()
    {
        if (!IsOwner) return;
        transform.localPosition = desiredPosition;
        transform.localRotation = desiredRotation;
    }
}
