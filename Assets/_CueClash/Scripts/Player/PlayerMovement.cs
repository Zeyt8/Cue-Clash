using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [HideInInspector]
    public CinemachinePOV Pov;

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _bottom;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _accelerationAir = 25f;
    [SerializeField] private float _maxSpeed = 5f;  // max speed along XZ plane
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _fallAcceleration = 2.5f;

    private bool _isGrounded = true;
    private bool _needsToJump;
    private Vector3 _movement;

    private Vector3 _goalVel = Vector3.zero;

    public void Move(InputHandler inputHandler)
    {
        // horizontal movement
        _movement = transform.TransformDirection(inputHandler.Movement);

        // jump
        if (_isGrounded && inputHandler.Jump && !_needsToJump)
        {
            _needsToJump = true;
            inputHandler.Jump = false;
        }

        // character rotation on Y axis (align with camera)
        Quaternion rotation = Quaternion.Euler(0, Pov.m_HorizontalAxis.Value, 0);
        _rigidbody.transform.localRotation = rotation;
    }

    public void FixedUpdate()
    {
        if (!IsOwner) return;
        // apply movement
        _isGrounded = Physics.Raycast(_bottom.position, Vector3.down, 0.1f);
        CharacterMove(_movement, !_isGrounded);

        // apply jump
        if (_needsToJump)
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _needsToJump = false;
        }

        // apply down force on the falling phase
        if (_rigidbody.velocity.y < 0)
        {
            _rigidbody.AddForce(Vector3.down * _fallAcceleration, ForceMode.Force);
        }
    }

    private void CharacterMove(Vector3 moveInput, bool inAir)
    {
        
        if (!inAir)
        {
            Vector3 goalVel = moveInput * _maxSpeed;
            _goalVel = Vector3.MoveTowards(_goalVel, goalVel, _acceleration * Time.fixedDeltaTime);
            Vector3 neededAccel = (_goalVel - _rigidbody.velocity) / Time.fixedDeltaTime;
            neededAccel = Vector3.ClampMagnitude(neededAccel, 150);
            _rigidbody.AddForce(Vector3.Scale(neededAccel, new Vector3(1, 0, 1)), ForceMode.Force);
        }
        else
        {
            Vector3 forward = _rigidbody.velocity;
            forward.y = 0;
            forward = forward.normalized;
            Vector3 sideways = Vector3.Cross(forward, Vector3.up);
            float forwardComponent = Vector3.Dot(forward, _movement);
            float sidewaysComponent = Vector3.Dot(sideways, _movement);
            _rigidbody.AddForce(forwardComponent * _acceleration * forward, ForceMode.Acceleration);
            _rigidbody.AddForce(sidewaysComponent * _accelerationAir * sideways, ForceMode.Acceleration);
            Vector3 velocity = _rigidbody.velocity;
            float yYelocity = _rigidbody.velocity.y;
            if (velocity.magnitude > _maxSpeed)
            {
                velocity = velocity.normalized * _maxSpeed;
            }
            velocity.y = yYelocity;
            _rigidbody.velocity = velocity;
        }
    }
}
