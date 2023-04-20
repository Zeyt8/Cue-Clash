using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public enum PlayerState
{
    Billiard,
    Melee,
    Ranged
}

public class PlayerObject : NetworkBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [Header("Children")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Transform _head;
    [SerializeField] private FollowTransform _headLookAt;
    [SerializeField] private PlayerHandController _handController;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera _cameraPrefab;

    private PlayerMovement _playerMovement;
    private PlayerState _playerState;
    private CinemachineVirtualCamera _camera;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _camera = Instantiate(_cameraPrefab, Vector3.zero, Quaternion.identity);
        _camera.Follow = _head;
        _camera.LookAt = _head;
        _playerMovement.Camera = _camera;
        _headLookAt._followTransform = _camera.transform;
    }

    private void OnEnable()
    {
        _inputHandler.OnShootWeapon.AddListener(Shoot);
        _inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
    }

    private void OnDisable()
    {
        _inputHandler.OnShootWeapon.RemoveListener(Shoot);
        _inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
    }

    private void Update()
    {
        if (!IsOwner) return;

        _animator.SetBool("Walking", _inputHandler.Movement != Vector3.zero);
        _playerMovement.Move(_inputHandler);

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (_playerState == PlayerState.Billiard)
            {
                _playerState = PlayerState.Ranged;
                _animator.SetInteger("Phase", 1);
            }
            else
            {
                _playerState = PlayerState.Billiard;
                _animator.SetInteger("Phase", 0);
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector3 pos = transform.InverseTransformPoint(
            _camera.transform.position +
            _camera.transform.up * -0.5f +
            _camera.transform.right * 0.3f
        );
        _handController.SetPosition(pos);
        Quaternion rot = Quaternion.Inverse(transform.rotation) * _camera.transform.rotation;
        _handController.SetRotation(rot);
    }

    private void Shoot()
    {
        if (!IsOwner || _playerState != PlayerState.Ranged) return;
        _weapon.Shoot();
    }

    private void SwitchWeapons()
    {

    }

    private void SwitchAmmo()
    {

    }
}
