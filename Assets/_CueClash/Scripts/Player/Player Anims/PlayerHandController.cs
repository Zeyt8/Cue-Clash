using UnityEngine;

public class PlayerHandController : MonoBehaviour
{
    public Vector3 DesiredPosition;
    public Quaternion DesiredRotation;

    private void LateUpdate()
    {
        transform.localPosition = DesiredPosition;
        transform.localRotation = DesiredRotation;
    }
}
