using System.Collections;
using UnityEngine;

public class Cue : MonoBehaviour
{
    public bool Charging;
    public float CueForce = 0;

    [SerializeField] private Transform _cueTop;
    [SerializeField] private float _maxPower = 1000;

    private Ball _ball;
    private Vector3 _hitPoint;

    private void Update()
    {
        if (Charging)
        {
            CueForce += 300 * Time.deltaTime;
            if (CueForce > _maxPower)
            {
                CueForce = _maxPower;
            }
            ShootRay();
        }
    }

    public void Shoot()
    {
        StartCoroutine(ShootCoroutine());
    }

    private void ShootRay()
    {
        _ball = null;
        if (!Physics.Raycast(_cueTop.transform.position, _cueTop.transform.forward, out RaycastHit hit, 0.1f)) return;
        if (!hit.collider.gameObject.TryGetComponent(out Ball ball)) return;
        _ball = ball;
        _hitPoint = hit.point;
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        if (_ball)
        {
            _ball.AddForce(_cueTop.transform.forward * CueForce, _hitPoint);
        }
        CueForce = 0;
    }

    public void Activate()
    {

    }

    public void Deactivate()
    {

    }
}
