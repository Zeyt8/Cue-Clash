using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool _isGrounded;
    private Vector3 _movement;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _bottom;
    [SerializeField] private float _acceleration = 40f;
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _jumpForce = 3f;
    [SerializeField] private float _lookSensitivity = 0.1f;

    public void Move(InputHandler inputHandler)
    {
        // check if grounded before applying any forces
        _isGrounded = Physics.Raycast(_bottom.position, Vector3.down, 0.1f);
        if (_isGrounded)
        {
            // horizontal movement
            _movement = transform.TransformDirection(inputHandler.Movement);
            _rigidbody.AddForce(_movement * _acceleration, ForceMode.Acceleration);
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _maxSpeed);

            // vertical movement
            if (inputHandler.Jump)
            {
                _rigidbody.AddForce(new Vector3(0f, _jumpForce, 0f), ForceMode.Impulse);
            }
        }
        else
        {
            _movement = Vector3.zero;
        }

        // camera rotation
        transform.Rotate(new Vector3(0f, inputHandler.Look.x * _lookSensitivity, 0f));
    }
}
