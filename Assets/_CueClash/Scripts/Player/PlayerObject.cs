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
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Cue _cue;
    [SerializeField] private Transform _head;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerAnimations _playerAnimations;
    [SerializeField] private FollowTransform _headLookAt;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera _cameraPrefab;

    private PlayerMovement _playerMovement;
    private PlayerState _playerState = PlayerState.Billiard;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CinemachineVirtualCamera camera = Instantiate(_cameraPrefab, Vector3.zero, Quaternion.identity);
        camera.Follow = _head;
        camera.LookAt = _head;
        _playerMovement.Camera = camera;
        _headLookAt._followTransform = camera.transform;
        _playerAnimations.Camera = camera;
    }

    private void OnEnable()
    {
        _inputHandler.OnCueRelease.AddListener(HitWithCue);
        _inputHandler.OnShootWeapon.AddListener(Shoot);
        _inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
    }

    private void OnDisable()
    {
        _inputHandler.OnCueRelease.RemoveListener(HitWithCue);
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
        
        // Start charging cue
        if (_playerState == PlayerState.Billiard)
        {
            _cue.Charging = _inputHandler.Cue;
            if (_inputHandler.Cue)
            {
                _playerAnimations.ChargeCue(_cue.CueForce);
            }
        }
    }

    private void HitWithCue()
    {
        if (!IsOwner || _playerState != PlayerState.Billiard) return;
        _cue.Shoot();
        _playerAnimations.HitWithCue();
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
