using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerHandController : MonoBehaviour
{
    public Vector3 DesiredPosition;
    public Quaternion DesiredRotation;
    public Transform ViewOrigin;
    [SerializeField] private Transform _cue;

    private float oldAngle = 0;

    private void LateUpdate()
    {
        Vector3? point = ClosestContactPoint();
        float angle = 0;
        if (point != null)
        {
            point += _cue.up * 0.01f;
            Vector3 dir = point.Value - ViewOrigin.position;
            angle = Vector3.SignedAngle(ViewOrigin.forward, dir, _cue.right);
        }
        transform.localPosition = DesiredPosition;
        transform.localRotation = DesiredRotation;
        if (point != null)
        {
            transform.RotateAround(point.Value, _cue.right, angle);
        }
    }

    private Vector3? ClosestContactPoint()
    {
        Vector3? closestPoint = null;
        // shoot sphere casts down along the length of _cue with a small interval between them
        for (int i = 13; i < 14; i++)
        {
            Vector3 pos = _cue.position + _cue.forward * i * 0.1f;
            Debug.DrawLine(pos, pos - _cue.up * 0.1f, Color.red);
            if (Physics.SphereCast(pos, 0.1f, -_cue.up, out RaycastHit hitInfo, 0.1f))
            {
                closestPoint = pos;
                break;
            }
        }

        return closestPoint;
    }
}
