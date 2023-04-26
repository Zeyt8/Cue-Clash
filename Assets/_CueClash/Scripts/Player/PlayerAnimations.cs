using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimations : NetworkBehaviour
{
    [HideInInspector] public CinemachineVirtualCamera camera;

    [SerializeField] private PlayerHandController handController;

    private Coroutine hitWithCueRoutine;

    private readonly Vector3 gunOffset = new Vector3(0.3f, -0.5f, 0);
    private Vector3 cueOffset = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        cueOffset = gunOffset;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector3 pos = transform.InverseTransformPoint(
            camera.transform.position +
            camera.transform.forward * cueOffset.z +
            camera.transform.up * cueOffset.y +
            camera.transform.right * cueOffset.x
        );
        handController.SetPosition(pos);
        Quaternion rot = Quaternion.Inverse(transform.rotation) * camera.transform.rotation;
        handController.SetRotation(rot);
    }

    public void ChargeCue(float value)
    {
        if (hitWithCueRoutine != null)
        {
            StopCoroutine(hitWithCueRoutine);
            hitWithCueRoutine = null;
        }
        cueOffset.z = gunOffset.z - 0.1f * Mathf.Sqrt(value) * 0.3f;
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
        float current = cueOffset.z;

        while (elapsedTime < waitTime)
        {
            cueOffset.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cueOffset.z = target;
        yield return ReturnCueToUsualPosition();
    }

    private IEnumerator ReturnCueToUsualPosition()
    {
        float target = gunOffset.z;
        float elapsedTime = 0;
        float waitTime = 0.2f;
        float current = cueOffset.z;

        while (elapsedTime < waitTime)
        {
            cueOffset.z = Mathf.Lerp(current, target, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cueOffset.z = target;
        yield return null;
    }
}
