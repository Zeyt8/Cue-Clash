using UnityEngine;

public class PlayerHandController : MonoBehaviour
{
    public Vector3 desiredPosition;
    public Quaternion desiredRotation;

    private void LateUpdate()
    {
        transform.localPosition = desiredPosition;
        transform.localRotation = desiredRotation;
    }
}
