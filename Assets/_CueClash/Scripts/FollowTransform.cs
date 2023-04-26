using UnityEngine;

[System.Flags]
public enum FollowTransformFlags
{
    Position = 1,
    Rotation = 2,
    Scale = 4,
}

public class FollowTransform : MonoBehaviour
{
    public Transform followTransform;
    [SerializeField] private Vector3 offset;
    [SerializeField] private FollowTransformFlags flags;

    void LateUpdate()
    {
        if (followTransform == null) return;
        if (flags.HasFlag(FollowTransformFlags.Position))
        {
            Vector3 position = followTransform.position + followTransform.forward * offset.z + followTransform.up * offset.y + followTransform.right * offset.x;
            transform.position = position;
        }
        if (flags.HasFlag(FollowTransformFlags.Rotation))
        {
            transform.rotation = followTransform.rotation;
        }
    }
}
