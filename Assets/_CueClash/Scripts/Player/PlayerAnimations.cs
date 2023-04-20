using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimations : NetworkBehaviour
{
    [HideInInspector] public CinemachineVirtualCamera Camera;

    [SerializeField] private PlayerHandController _handController;

    private Coroutine _hitWithCueRoutine;

    private readonly Vector3 _gunOffset = new Vector3(0.3f, -0.5f, 0);
    private Vector3 _cueOffset = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        _cueOffset = _gunOffset;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector3 pos = transform.InverseTransformPoint(
            Camera.transform.position +
            Camera.transform.forward * _cueOffset.z +
            Camera.transform.up * _cueOffset.y +
            Camera.transform.right * _cueOffset.x
        );
        _handController.SetPosition(pos);
        Quaternion rot = Quaternion.Inverse(transform.rotation) * Camera.transform.rotation;
        _handController.SetRotation(rot);
    }

    public void ChargeCue(float value)
    {
        if (_hitWithCueRoutine != null)
        {
            StopCoroutine(_hitWithCueRoutine);
            _hitWithCueRoutine = null;
        }
        _cueOffset.z = _gunOffset.z - 0.1f * Mathf.Sqrt(value) * 0.3f;
    }

    public void HitWithCue()
    {
        _hitWithCueRoutine = StartCoroutine(HitWithCueRoutine());
    }

    private IEnumerator HitWithCueRoutine()
    {
        float target = 1;
        float elapsedTime = 0;
        float waitTime = 0.3f;
        float current = _cueOffset.z;

        while (elapsedTime < waitTime)
        {
            _cueOffset.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _cueOffset.z = target;
        yield return ReturnCueToUsualPosition();
    }

    private IEnumerator ReturnCueToUsualPosition()
    {
        float target = _gunOffset.z;
        float elapsedTime = 0;
        float waitTime = 0.2f;
        float current = _cueOffset.z;

        while (elapsedTime < waitTime)
        {
            _cueOffset.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _cueOffset.z = target;
        yield return null;
    }
}
