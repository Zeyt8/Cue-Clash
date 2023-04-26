using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [HideInInspector] public CinemachineVirtualCamera Camera;

    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private Transform bottom;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float accelerationAir = 25f;
    [SerializeField] private float maxSpeed = 5f;  // max speed along XZ plane
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallAcceleration = 2.5f;

    private bool isGrounded = true;
    private bool needsToJump;
    private Vector3 movement;
    private CinemachinePOV pov;

    private Vector3 goalVel = Vector3.zero;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        // set up aiming
        pov = Camera.GetCinemachineComponent<CinemachinePOV>();
    }

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
        rigidbody.transform.localRotation = rotation;
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
            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            needsToJump = false;
        }

        // apply down force on the falling phase
        if (rigidbody.velocity.y < 0)
        {
            rigidbody.AddForce(Vector3.down * fallAcceleration, ForceMode.Force);
        }
    }

    private void CharacterMove(Vector3 moveInput, bool inAir)
    {
        
        if (!inAir)
        {
            Vector3 goalVel = moveInput * maxSpeed;
            this.goalVel = Vector3.MoveTowards(this.goalVel, goalVel, acceleration * Time.fixedDeltaTime);
            Vector3 neededAccel = (this.goalVel - rigidbody.velocity) / Time.fixedDeltaTime;
            neededAccel = Vector3.ClampMagnitude(neededAccel, 150);
            rigidbody.AddForce(Vector3.Scale(neededAccel, new Vector3(1, 0, 1)), ForceMode.Force);
        }
        else
        {
            Vector3 forward = rigidbody.velocity;
            forward.y = 0;
            forward = forward.normalized;
            Vector3 sideways = Vector3.Cross(forward, Vector3.up);
            float forwardComponent = Vector3.Dot(forward, movement);
            float sidewaysComponent = Vector3.Dot(sideways, movement);
            rigidbody.AddForce(forwardComponent * acceleration * forward, ForceMode.Acceleration);
            rigidbody.AddForce(sidewaysComponent * accelerationAir * sideways, ForceMode.Acceleration);
            Vector3 velocity = rigidbody.velocity;
            float yYelocity = rigidbody.velocity.y;
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
            velocity.y = yYelocity;
            rigidbody.velocity = velocity;
        }
    }
}
