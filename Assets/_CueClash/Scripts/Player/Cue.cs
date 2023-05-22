using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Cue : MonoBehaviour
{
    public bool charging;
    public float cueForce = 0;

    [SerializeField] private Transform cueTop;
    [SerializeField] private float maxPower = 750;

    private Vector3 hitPoint;

    private bool isActive;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (charging)
        {
            cueForce += 250 * Time.deltaTime;
            if (cueForce > maxPower)
            {
                cueForce = maxPower;
            }
        }
        if (isActive) 
        {
            lineRenderer.SetPosition(0, cueTop.position);
            float distance = Mathf.Sqrt(cueForce) * 0.03f;
            if (Physics.Raycast(cueTop.transform.position, cueTop.transform.forward, out RaycastHit hit, distance + 0.3f))
            {
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                lineRenderer.SetPosition(1, cueTop.position);
            }
        }
    }

    public void Shoot()
    {
        StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        float time = 0.3f;
        while (time > 0)
        {
            if (Physics.Raycast(cueTop.transform.position, cueTop.transform.forward, out RaycastHit hit, 0.1f))
            {
                if (hit.collider.gameObject.TryGetComponent(out Ball ball))
                {
                    hitPoint = hit.point;
                    if (ball)
                    {
                        ball.AddForceServerRpc(cueTop.transform.forward * cueForce, hitPoint);
                        cueForce = 0;
                        yield break;
                    }
                }
            }
            time -= Time.deltaTime;
            yield return null;
        }
        cueForce = 0;
    }

    public void Activate()
    {
        isActive = true;
        lineRenderer.enabled = true;
    }

    public void Deactivate()
    {
        isActive = false;
        lineRenderer.enabled = false;
    }
}
