using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CueBall : MonoBehaviour
{
    Rigidbody body;
    public float AimRotationSpeedFactor;
    public GameObject whiteBall;
    public GameObject aimAssist;
    private float deltaX, deltaY, forceToHitBall;
    private Vector3 oldMousePosition;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        oldMousePosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        aimBall();
        powerBall();
        hitBall();
    }


    void aimBall()
    {
        //keep the aim assist on the ball
        aimAssist.transform.position = whiteBall.transform.position;

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        //right click
        if (Input.GetMouseButton(1))
        {
            print("äiming ball");
            //swapped some stuff because being consistent across axis is hard
            deltaX = -(mousePos.x - oldMousePosition.x);
            deltaY = mousePos.y - oldMousePosition.y;
            aimAssist.transform.eulerAngles += new Vector3(deltaY, deltaX, 0) * AimRotationSpeedFactor;
        }
        oldMousePosition = mousePos;
    }

    void powerBall()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            forceToHitBall += 100 * Time.deltaTime;
            print(forceToHitBall);
        }
    }

    void hitBall()
    {
        //left click
        if (Input.GetMouseButtonDown(0))
        {
            body.AddForce(aimAssist.transform.forward * forceToHitBall);

            forceToHitBall = 0;
        }
    }
}
