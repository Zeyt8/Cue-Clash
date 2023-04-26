using UnityEngine;

public class Cue : MonoBehaviour
{
    public bool charging;
    public float cueForce = 0;

    [SerializeField] private Transform cueTop;
    [SerializeField] private float maxPower = 1000;

    private Ball ball;
    private Vector3 hitPoint;

    private void Update()
    {
        if (charging)
        {
            cueForce += 300 * Time.deltaTime;
            if (cueForce > maxPower)
            {
                cueForce = maxPower;
            }
            ShootRay();
        }
    }

    public void Shoot()
    {
        if (ball)
        {
            ball.AddForce(cueTop.transform.forward * cueForce, hitPoint);
        }
        cueForce = 0;
    }

    private void ShootRay()
    {
        this.ball = null;
        if (!Physics.Raycast(cueTop.transform.position, cueTop.transform.forward, out RaycastHit hit)) return;
        if (!hit.collider.gameObject.TryGetComponent(out Ball ball)) return;
        this.ball = ball;
        hitPoint = hit.point;
    }
}
