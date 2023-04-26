using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public int ballNumber;
    public bool showAim;

    protected Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        AimAssist();
    }

    public void AddForce(Vector3 force, Vector3 position)
    {
        body.AddForceAtPosition(force, position);
    }

    void AimAssist()
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
