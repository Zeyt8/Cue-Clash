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
    public Transform _followTransform;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private FollowTransformFlags _flags;

    void FixedUpdate()
    {
        if (_followTransform == null) return;
        if (_flags.HasFlag(FollowTransformFlags.Position))
        {
            Vector3 position = _followTransform.position + _followTransform.forward * _offset.z + _followTransform.up * _offset.y + _followTransform.right * _offset.x;
            transform.position = position;
        }
        if (_flags.HasFlag(FollowTransformFlags.Rotation))
        {
            transform.rotation = _followTransform.rotation;
        }
    }
}
