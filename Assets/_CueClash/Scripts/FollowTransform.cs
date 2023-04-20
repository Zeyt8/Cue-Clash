using UnityEngine;

[System.Flags]
public enum FollowTransformFlags
{
    FollowPosition = 1,
    FollowRotation = 2,
    FollowScale = 4,
}

public class FollowTransform : MonoBehaviour
{
    public Transform _followTransform;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private FollowTransformFlags _flags;

    // Update is called once per frame
    void Update()
    {
        if (_followTransform == null) return;
        if (_flags.HasFlag(FollowTransformFlags.FollowPosition))
        {
            Vector3 position = _followTransform.position + _followTransform.forward * _offset.z + _followTransform.up * _offset.y + _followTransform.right * _offset.x;
            transform.position = position;
        }
        if (_flags.HasFlag(FollowTransformFlags.FollowRotation))
        {
            transform.rotation = _followTransform.rotation;
        }
    }
}
