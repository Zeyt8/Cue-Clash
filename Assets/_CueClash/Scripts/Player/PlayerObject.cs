using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Billiard,
    Sword,
    Gun
}

public class PlayerObject : NetworkBehaviour
{
    [SerializeField] private InputHandler inputHandler;
    [Header("Children")]
    [SerializeField] private Transform _cueTransform;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _animatorTransform;
    [SerializeField] private FollowTransform _headLookAt;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera cameraPrefab;

    private PlayerMovement playerMovement;
    private PlayerState playerState = PlayerState.Billiard;

    private Cue _cue;
    private Gun _gun;
    private Sword _sword;

    private Animator _animator;
    private PlayerAnimations _playerAnimations;
    private CinemachinePOV _pov;

    private bool _aimCue;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _animator = _animatorTransform.GetComponent<Animator>();
        _playerAnimations = _animatorTransform.GetComponent<PlayerAnimations>();
        _cue = _cueTransform.GetComponent<Cue>();
        _gun = _cueTransform.GetComponent<Gun>();
        _sword = _cueTransform.GetComponent<Sword>();
        _playerAnimations.PlayerState = PlayerState.Billiard;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CinemachineVirtualCamera camera = Instantiate(_cameraPrefab, Vector3.zero, Quaternion.identity);
        camera.Follow = _head;
        camera.LookAt = _head;
        _pov = camera.GetCinemachineComponent<CinemachinePOV>();
        _playerMovement.Pov = _pov;
        _headLookAt._followTransform = camera.transform;
        _playerAnimations.Camera = camera;
    }

    private void OnEnable()
    {
        _inputHandler.OnCueRelease.AddListener(HitWithCue);
        _inputHandler.OnShootWeapon.AddListener(Shoot);
        _inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
        _inputHandler.AimCueStateChanged.AddListener(AimCueChangedState);
    }

    private void OnDisable()
    {
        _inputHandler.OnCueRelease.RemoveListener(HitWithCue);
        _inputHandler.OnShootWeapon.RemoveListener(Shoot);
        _inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
        _inputHandler.AimCueStateChanged.RemoveListener(AimCueChangedState);
    }

    private void Update()
    {
        if (!IsOwner) return;

        animator.SetBool("Walking", inputHandler.Movement != Vector3.zero);
        playerMovement.Move(inputHandler);

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (playerState == PlayerState.Billiard)
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
        if (playerState == PlayerState.Billiard)
        {
            cue.charging = inputHandler.Cue;
            if (inputHandler.Cue)
            {
                playerAnimations.ChargeCue(cue.cueForce);
            }
            else if (_aimCue)
            {
                Vector2 pos = new Vector2(
                    _inputHandler.MousePosition.x.Remap(0, Screen.width, -1, 1),
                    _inputHandler.MousePosition.y.Remap(0, Screen.height, -1, 1)
                );
                _playerAnimations.AlignBilliardAim(pos);
            }
        }
    }

    private void HitWithCue()
    {
        if (!IsOwner || playerState != PlayerState.Billiard) return;
        cue.Shoot();
        playerAnimations.HitWithCue();
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

    private void AimCueChangedState(bool state)
    {
        if (state == true)
        {
            Cursor.lockState = CursorLockMode.Confined;
            _pov.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            _pov.enabled = true;
        }
        _aimCue = state;
    }
}
