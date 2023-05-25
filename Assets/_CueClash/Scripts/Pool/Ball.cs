using JSAM;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public int ballNumber;
    public bool sinked = false;
    private bool showAim;

    protected Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        AimAssist();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddForceServerRpc(Vector3 force, Vector3 position)
    {
        AddForceClientRpc(force, position);
    }

    [ClientRpc]
    private void AddForceClientRpc(Vector3 force, Vector3 position)
    {
        body.AddForceAtPosition(force, position);
        PoolManager.Instance.HitBall(this);
        AudioManager.PlaySound(Sounds.Billiard, transform);
    }

    protected void AimAssist()
    {
        /*aimAssist.transform.position = whiteBall.transform.position;

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        if (Input.GetMouseButton(1))
        {
            print("äiming ball");
            //swapped some stuff because being consistent across axis is hard
            deltaX = -(mousePos.x - oldMousePosition.x);
            deltaY = mousePos.y - oldMousePosition.y;
            aimAssist.transform.eulerAngles += new Vector3(deltaY, deltaX, 0) * AimRotationSpeedFactor;
        }
        oldMousePosition = mousePos;*/
    }
}
