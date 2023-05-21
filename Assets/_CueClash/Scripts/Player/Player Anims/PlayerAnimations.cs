using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerAnimations : NetworkBehaviour
{
    public PlayerState PlayerState
    {
        get => playerState;
        set
        {
            switch (value)
            {
                case PlayerState.Billiard:
                    handController.desiredPosition = billiardOffset;
                    break;
            }
            playerState = value;
        }
    }

    public bool parrying;
    public bool swinging;
    public float swingTimer = 0.0f;
    public float swingDuration = 0.25f;
    public Vector2 swingDirection = Vector2.zero;
    private Vector3 posOffset = Vector3.zero;
    private Quaternion rotOffset = Quaternion.identity;

    [HideInInspector] public CinemachineVirtualCamera virtualCamera;

    [SerializeField] private PlayerHandController handController;

    private Coroutine hitWithCueRoutine;

    private readonly Vector3 gunOffset = new Vector3(0.3f, -0.4f, 0.2f);
    private readonly Vector3 swordOffset = new Vector3(0.1f, -0.2f, 0.6f);
    private readonly Vector3 parryOffset = new Vector3(0.4f, -0.05f, 0.6f);
    private readonly Vector3 billiardOffset = new Vector3(0.15f, 1.3f, 0.2f);
    private readonly Vector3 billiardPivot = new Vector3(0.15f, 1.32f, 0.1f) + new Vector3(0, 0, 1.8f);

    private PlayerState playerState = PlayerState.Billiard;
    private Vector3? billiardShootStartPosition = null;

    private void LateUpdate()
    {
        if (!IsOwner) return;

        if (PlayerState == PlayerState.Gun)
        {
            Vector3 pos = virtualCamera.transform.position +
                          virtualCamera.transform.forward * gunOffset.z +
                          virtualCamera.transform.up * gunOffset.y +
                          virtualCamera.transform.right * gunOffset.x;
            pos = transform.parent.InverseTransformPoint(pos);
            handController.desiredPosition = pos;
            Quaternion rot = Quaternion.Inverse(transform.rotation) * virtualCamera.transform.rotation;
            handController.desiredRotation = rot;
        }
    }

    #region Sword

    public void SetSword()
    {
        AlignSwordPosition(swordOffset);
    }

    public void AlignSwordPosition(Vector3 mousePos)
    {
        if (swinging)
        {
            posOffset = new Vector3(Mathf.Clamp(mousePos.x * 1.0f, -0.5f, 0.5f), 0, 0);
            handController.desiredPosition = transform.parent.InverseTransformPoint(
                virtualCamera.transform.position +
                virtualCamera.transform.forward * swordOffset.z +
                virtualCamera.transform.up * swordOffset.y +
                virtualCamera.transform.right * swordOffset.x
            ) + posOffset;

            rotOffset = Quaternion.Euler(-120, 0, 0) * Quaternion.Euler(0, Mathf.Clamp((-mousePos.x) * 540, -120, 120), 0);
            handController.desiredRotation = Quaternion.Inverse(transform.rotation) * virtualCamera.transform.rotation * rotOffset;
        }
        else if (swingTimer > 0.0f)
        {
            handController.desiredPosition = transform.parent.InverseTransformPoint(
                virtualCamera.transform.position +
                virtualCamera.transform.forward * swordOffset.z +
                virtualCamera.transform.up * swordOffset.y +
                virtualCamera.transform.right * swordOffset.x
            ) + posOffset + Vector3.ClampMagnitude(transform.parent.InverseTransformVector(virtualCamera.transform.up * swingDirection.y
                + virtualCamera.transform.right * swingDirection.x) * ((swingDuration - swingTimer) * 4.0f), 1.0f);

            handController.desiredRotation = Quaternion.Inverse(transform.rotation) * virtualCamera.transform.rotation * rotOffset
                * Quaternion.Euler(Mathf.Clamp((swingDuration - swingTimer) * 500.0f, -125.0f, 125.0f),
                                    Mathf.Clamp(-swingDirection.x * (swingDuration - swingTimer) * 500.0f, -125.0f, 125.0f), 0);

            swingTimer -= Time.deltaTime;
        }
        else if (parrying)
        {
            handController.desiredPosition = transform.parent.InverseTransformPoint(
                virtualCamera.transform.position +
                virtualCamera.transform.forward * parryOffset.z +
                virtualCamera.transform.up * parryOffset.y +
                virtualCamera.transform.right * parryOffset.x
            );

            handController.desiredRotation = Quaternion.Inverse(transform.rotation) * virtualCamera.transform.rotation
                * Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, -90, 0);
        }
        else
        {
            handController.desiredPosition = transform.parent.InverseTransformPoint(
                virtualCamera.transform.position +
                virtualCamera.transform.forward * swordOffset.z +
                virtualCamera.transform.up * swordOffset.y +
                virtualCamera.transform.right * swordOffset.x
            );

            handController.desiredRotation = Quaternion.Inverse(transform.rotation) * virtualCamera.transform.rotation
                * Quaternion.Euler(-120, 0, 0);
        }
    }
    #endregion

    #region Cue
    public void AlignBilliardAim(Vector2 pos)
    {
        handController.desiredPosition.x = pos.x + billiardOffset.x;
        handController.desiredPosition.y = pos.y + billiardOffset.y;
        Vector3 dir = billiardPivot - handController.desiredPosition;
        handController.desiredRotation = Quaternion.LookRotation(dir);
        handController.desiredPosition += transform.parent.InverseTransformVector(handController.transform.forward) * (dir.magnitude - 1.8f);
    }

    public void ChargeCue(float value)
    {
        billiardShootStartPosition ??= handController.desiredPosition;
        if (hitWithCueRoutine != null)
        {
            StopCoroutine(hitWithCueRoutine);
            hitWithCueRoutine = null;
        }
        handController.desiredPosition = billiardShootStartPosition.Value - Mathf.Sqrt(value) * 0.03f * (billiardPivot - billiardShootStartPosition.Value).normalized;
    }

    public void HitWithCue()
    {
        hitWithCueRoutine = StartCoroutine(HitWithCueRoutine());
    }

    private IEnumerator HitWithCueRoutine()
    {
        Vector3 target = billiardShootStartPosition.Value + (billiardPivot - billiardShootStartPosition.Value).normalized * 0.2f;
        float elapsedTime = 0;
        float waitTime = 0.3f;
        Vector3 current = handController.desiredPosition;

        while (elapsedTime < waitTime)
        {
            handController.desiredPosition = Vector3.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        handController.desiredPosition = target;
        yield return ReturnCueToUsualPosition();
    }

    private IEnumerator ReturnCueToUsualPosition()
    {
        Vector3 target = billiardShootStartPosition.Value;
        float elapsedTime = 0;
        float waitTime = 0.2f;
        Vector3 current = handController.desiredPosition;

        while (elapsedTime < waitTime)
        {
            handController.desiredPosition = Vector3.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        handController.desiredPosition = target;
        billiardShootStartPosition = null;
        yield return null;
    }
    #endregion
}
