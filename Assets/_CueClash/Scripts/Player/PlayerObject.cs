using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public enum PlayerState
{
    Billiard,
    Sword,
    Gun
}

public class PlayerObject : NetworkBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [Header("Children")]
    [SerializeField] private Transform _cueTransform;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _animatorTransform;
    [SerializeField] private FollowTransform _headLookAt;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera _cameraPrefab;

    private PlayerMovement _playerMovement;
    private PlayerState _playerState = PlayerState.Billiard;

    private Cue _cue;
    private Gun _gun;
    private Sword _sword;

    private Animator _animator;
    private PlayerAnimations _playerAnimations;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _animator = _animatorTransform.GetComponent<Animator>();
        _playerAnimations = _animatorTransform.GetComponent<PlayerAnimations>();
        _cue = _cueTransform.GetComponent<Cue>();
        _gun = _cueTransform.GetComponent<Gun>();
        _sword = _cueTransform.GetComponent<Sword>();
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
                _playerState = PlayerState.Gun;
                _animator.SetInteger("Phase", 1);
                _gun.Activate();
                _cue.Deactivate();
                _playerAnimations.PlayerState = PlayerState.Gun;
            }
            else
            {
                _playerState = PlayerState.Billiard;
                _animator.SetInteger("Phase", 0);
                _gun.Deactivate();
                _cue.Activate();
                _playerAnimations.PlayerState = PlayerState.Billiard;
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
        if (!IsOwner || _playerState != PlayerState.Gun) return;
        _gun.Shoot();
    }

    private void SwitchWeapons()
    {
        if (!IsOwner) return;
        if (_playerState == PlayerState.Gun)
        {
            _playerState = PlayerState.Sword;
            _gun.Deactivate();
            _sword.Activate();
            _playerAnimations.PlayerState = PlayerState.Sword;
        }
        else if (_playerState == PlayerState.Sword)
        {
            _playerState = PlayerState.Gun;
            _gun.Activate();
            _sword.Deactivate();
            _playerAnimations.PlayerState = PlayerState.Gun;
        }
    }

    private void SwitchAmmo()
    {

    }
}
