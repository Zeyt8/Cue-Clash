using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [HideInInspector]
    public CinemachinePOV pov;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform bottom;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float accelerationAir = 25f;
    [SerializeField] private float maxSpeed = 5f;  // max speed along XZ plane
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallAcceleration = 2.5f;

    private bool isGrounded = true;
    private bool needsToJump;
    private Vector3 movement;

    private Vector3 goalVel = Vector3.zero;

    public void Move(InputHandler inputHandler)
    {
        // horizontal movement
        movement = transform.TransformDirection(inputHandler.Movement);

        // jump
        if (isGrounded && inputHandler.Jump && !needsToJump)
        {
            needsToJump = true;
            inputHandler.Jump = false;
        }

        // character rotation on Y axis (align with camera)
        Quaternion rotation = Quaternion.Euler(0, pov.m_HorizontalAxis.Value, 0);
        rb.transform.localRotation = rotation;
    }

    public void FixedUpdate()
    {
        if (!IsOwner) return;
        // apply movement
        isGrounded = Physics.Raycast(bottom.position, Vector3.down, 0.1f);
        CharacterMove(movement, !isGrounded);

        // apply jump
        if (needsToJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            needsToJump = false;
        }

        // apply down force on the falling phase
        if (rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.down * fallAcceleration, ForceMode.Force);
        }
    }

    private void CharacterMove(Vector3 moveInput, bool inAir)
    {
        if (!inAir)
        {
            Vector3 goalVel = moveInput * maxSpeed;
            this.goalVel = Vector3.MoveTowards(this.goalVel, goalVel, acceleration * Time.fixedDeltaTime);
            Vector3 neededAccel = (this.goalVel - rb.velocity) / Time.fixedDeltaTime;
            neededAccel = Vector3.ClampMagnitude(neededAccel, 150);
            rb.AddForce(Vector3.Scale(neededAccel, new Vector3(1, 0, 1)), ForceMode.Force);
        }
        else
        {
            Vector3 forward = rb.velocity;
            forward.y = 0;
            forward = forward.normalized;
            Vector3 sideways = Vector3.Cross(forward, Vector3.up);
            float forwardComponent = Vector3.Dot(forward, movement);
            float sidewaysComponent = Vector3.Dot(sideways, movement);
            rb.AddForce(forwardComponent * acceleration * forward, ForceMode.Acceleration);
            rb.AddForce(sidewaysComponent * accelerationAir * sideways, ForceMode.Acceleration);
            Vector3 velocity = rb.velocity;
            float yYelocity = rb.velocity.y;
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
            velocity.y = yYelocity;
            rb.velocity = velocity;
        }
    }
}
