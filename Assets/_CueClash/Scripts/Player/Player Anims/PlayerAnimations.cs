using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

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

    [HideInInspector] public CinemachineVirtualCamera camera;

    [SerializeField] private PlayerHandController handController;

    private Coroutine hitWithCueRoutine;

    private readonly Vector3 gunOffset = new Vector3(0.3f, -0.5f, 0.1f);
    private readonly Vector3 billiardOffset = new Vector3(0.3f, 1.6f, 0.1f);

    private PlayerState playerState = PlayerState.Billiard;

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (PlayerState != PlayerState.Billiard)
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
    }

    public void AlignBilliardAim(Vector2 pos)
    {
        handController.desiredPosition.x = pos.x + billiardOffset.x;
        handController.desiredPosition.y = pos.y + billiardOffset.y;
        Debug.DrawRay(transform.TransformPoint(billiardOffset + new Vector3(0, 0, 2.1f)), Vector3.up, Color.red);
        Vector3 dir = transform.TransformPoint(billiardOffset + new Vector3(0, 0, 2.1f)) -
                      transform.TransformPoint(handController.desiredPosition);
        handController.desiredRotation = Quaternion.LookRotation(dir);
        handController.desiredPosition += handController.transform.forward * (dir.magnitude - 2.1f);
    }

    public void ChargeCue(float value)
    {
        if (hitWithCueRoutine != null)
        {
            StopCoroutine(hitWithCueRoutine);
            hitWithCueRoutine = null;
        }
        handController.desiredPosition.z = billiardOffset.z - 0.1f * Mathf.Sqrt(value) * 0.3f;
    }

    public void HitWithCue()
    {
        hitWithCueRoutine = StartCoroutine(HitWithCueRoutine());
    }

    private IEnumerator HitWithCueRoutine()
    {
        float target = 1;
        float elapsedTime = 0;
        float waitTime = 0.3f;
        float current = handController.desiredPosition.z;

        while (elapsedTime < waitTime)
        {
            handController.desiredPosition.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        handController.desiredPosition.z = target;
        yield return ReturnCueToUsualPosition();
    }

    private IEnumerator ReturnCueToUsualPosition()
    {
        float target = gunOffset.z;
        float elapsedTime = 0;
        float waitTime = 0.2f;
        float current = handController.desiredPosition.z;

        while (elapsedTime < waitTime)
        {
            handController.desiredPosition.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        handController.desiredPosition.z = target;
        yield return null;
    }
}
