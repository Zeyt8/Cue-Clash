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

    [HideInInspector] public CinemachineVirtualCamera camera;

    [SerializeField] private PlayerHandController handController;

    private Coroutine hitWithCueRoutine;

    private readonly Vector3 gunOffset = new Vector3(0.3f, -0.4f, 0.2f);
    private readonly Vector3 swordOffset = new Vector3(0.3f, -0.6f, 0.6f);
    private readonly Vector3 parryOffset = new Vector3(0.4f, -0.2f, 0.6f);
    private readonly Vector3 billiardOffset = new Vector3(0.2f, 1.45f, 0.2f);
    private readonly Vector3 billiardPivot = new Vector3(0.2f, 1.45f, 0.1f) + new Vector3(0, 0, 1.8f);

    private PlayerState playerState = PlayerState.Billiard;
    private Vector3? billiardShootStartPosition = null;

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (PlayerState == PlayerState.Gun)
        {
            Vector3 pos = transform.InverseTransformPoint(
                camera.transform.position +
                camera.transform.forward * gunOffset.z +
                camera.transform.up * gunOffset.y +
                camera.transform.right * gunOffset.x
            );
            handController.desiredPosition = pos;
            Quaternion rot = Quaternion.Inverse(transform.rotation) * camera.transform.rotation;
            handController.desiredRotation = rot;
        }
        else if (PlayerState == PlayerState.Sword)
        {
            if (parrying)
            {
                Vector3 pos = transform.InverseTransformPoint(
                    camera.transform.position +
                    camera.transform.forward * parryOffset.z +
                    camera.transform.up * parryOffset.y +
                    camera.transform.right * parryOffset.x
                );
                handController.desiredPosition = pos;
                Quaternion rot = Quaternion.Inverse(transform.rotation) * camera.transform.rotation;
                rot *= Quaternion.Euler(-90, 0, 0);
                rot *= Quaternion.Euler(0, -90, 0);
                handController.desiredRotation = rot;
            }
            else
            {
                Quaternion rot = Quaternion.Inverse(transform.rotation) * camera.transform.rotation;
                rot *= Quaternion.Euler(-90, 0, 0);
                handController.desiredRotation = rot;
            }
        }
    }

    #region Sword

    public void SetSword()
    {
        AlignSwordPosition((Vector2)swordOffset);
    }
    public void AlignSwordPosition(Vector3 position)
    {
        Vector3 pos = transform.InverseTransformPoint(
            camera.transform.position +
            camera.transform.forward * swordOffset.z +
            camera.transform.up * position.y +
            camera.transform.right * position.x
        );
        handController.desiredPosition = pos;
    }
#endregion

    #region Cue
    public void AlignBilliardAim(Vector2 pos)
    {
        handController.desiredPosition.x = pos.x + billiardOffset.x;
        handController.desiredPosition.y = pos.y + billiardOffset.y;
        Vector3 dir = billiardPivot - handController.desiredPosition;
        handController.desiredRotation = Quaternion.LookRotation(dir);
        handController.desiredPosition += transform.InverseTransformVector(handController.transform.forward) * (dir.magnitude - 1.8f);
    }

    public void ChargeCue(float value)
    {
        billiardShootStartPosition ??= handController.desiredPosition;
        if (hitWithCueRoutine != null)
        {
            StopCoroutine(hitWithCueRoutine);
            hitWithCueRoutine = null;
        }
        handController.desiredPosition = billiardShootStartPosition.Value - Mathf.Sqrt(value) * 0.02f * (billiardPivot - billiardShootStartPosition.Value).normalized;
    }

    public void HitWithCue()
    {
        hitWithCueRoutine = StartCoroutine(HitWithCueRoutine());
    }

    private IEnumerator HitWithCueRoutine()
    {
        Vector3 target = billiardShootStartPosition.Value + (billiardPivot - billiardShootStartPosition.Value).normalized * 0.1f;
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
