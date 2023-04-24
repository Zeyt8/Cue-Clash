using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimations : NetworkBehaviour
{
    public PlayerState PlayerState
    {
        get => _playerState;
        set
        {
            switch (value)
            {
                case PlayerState.Billiard:
                    _cueOffset = _billiardOffset;
                    break;
                    break;
                case PlayerState.Gun:
                    _cueOffset = _gunOffset;
                    break;
            }
            _playerState = value;
        }
    }

    [HideInInspector] public CinemachineVirtualCamera Camera;

    [SerializeField] private PlayerHandController _handController;

    private Coroutine _hitWithCueRoutine;

    private readonly Vector3 _gunOffset = new Vector3(0.3f, -0.5f, 0);
    private readonly Vector3 _billiardOffset = new Vector3(0.3f, -0.95f, 0);
    private Vector3 _cueOffset = Vector3.zero;

    private PlayerState _playerState;

    // Start is called before the first frame update
    void Start()
    {
        _cueOffset = _billiardOffset;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (PlayerState != PlayerState.Billiard)
        {
            Vector3 pos = transform.InverseTransformPoint(
                Camera.transform.position +
                Camera.transform.forward * _cueOffset.z +
                Camera.transform.up * _cueOffset.y +
                Camera.transform.right * _cueOffset.x
            );
            _handController.DesiredPosition = pos;
            Quaternion rot = Quaternion.Inverse(transform.rotation) * Camera.transform.rotation;
            _handController.DesiredRotation = rot;
        }
        else
        {
            Vector3 pos = transform.InverseTransformPoint(
                Camera.transform.position + _cueOffset
            );
            _handController.DesiredPosition = pos;
        }
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
