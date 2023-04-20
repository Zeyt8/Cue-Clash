using UnityEngine;

public class PlayerHandController : MonoBehaviour
{
    public void SetPosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.localRotation = rotation;
    }
}
