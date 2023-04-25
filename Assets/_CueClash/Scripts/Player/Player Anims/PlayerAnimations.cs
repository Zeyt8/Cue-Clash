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
                    _handController.DesiredPosition = _billiardOffset;
                    break;
            }
            _playerState = value;
        }
    }

    [HideInInspector] public CinemachineVirtualCamera Camera;

    [SerializeField] private PlayerHandController _handController;

    private Coroutine _hitWithCueRoutine;

    private readonly Vector3 _gunOffset = new Vector3(0.3f, -0.5f, 0.1f);
    private readonly Vector3 _billiardOffset = new Vector3(0.3f, 1.6f, 0.1f);

    private PlayerState _playerState = PlayerState.Billiard;

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (PlayerState != PlayerState.Billiard)
        {
            Vector3 pos = transform.InverseTransformPoint(
                Camera.transform.position +
                Camera.transform.forward * _gunOffset.z +
                Camera.transform.up * _gunOffset.y +
                Camera.transform.right * _gunOffset.x
            );
            _handController.DesiredPosition = pos;
            Quaternion rot = Quaternion.Inverse(transform.rotation) * Camera.transform.rotation;
            _handController.DesiredRotation = rot;
        }
    }

    public void AlignBilliardAim(Vector2 pos)
    {
        _handController.DesiredPosition.x = pos.x + _billiardOffset.x;
        _handController.DesiredPosition.y = pos.y + _billiardOffset.y;
        Debug.DrawRay(transform.TransformPoint(_billiardOffset + new Vector3(0, 0, 2)), Vector3.up, Color.red);
        _handController.DesiredRotation = Quaternion.LookRotation(
            transform.TransformPoint(_billiardOffset + new Vector3(0, 0, 2)) - transform.TransformPoint(_handController.DesiredPosition)
            );
    }

    public void ChargeCue(float value)
    {
        if (_hitWithCueRoutine != null)
        {
            StopCoroutine(_hitWithCueRoutine);
            _hitWithCueRoutine = null;
        }
        _handController.DesiredPosition.z = _billiardOffset.z - 0.1f * Mathf.Sqrt(value) * 0.3f;
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
        float current = _handController.DesiredPosition.z;

        while (elapsedTime < waitTime)
        {
            _handController.DesiredPosition.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _handController.DesiredPosition.z = target;
        yield return ReturnCueToUsualPosition();
    }

    private IEnumerator ReturnCueToUsualPosition()
    {
        float target = _gunOffset.z;
        float elapsedTime = 0;
        float waitTime = 0.2f;
        float current = _handController.DesiredPosition.z;

        while (elapsedTime < waitTime)
        {
            _handController.DesiredPosition.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _handController.DesiredPosition.z = target;
        yield return null;
    }
}
