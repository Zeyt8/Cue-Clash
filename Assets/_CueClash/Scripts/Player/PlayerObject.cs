using System.Collections.Generic;
using Cinemachine;
using JSAM;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public enum PlayerState
{
    Billiard,
    Sword,
    Gun
}

public enum Limbs
{
    Legs,
    Torso,
    LeftHand,
    RightHand,
    Head
}

public class PlayerObject : NetworkBehaviour
{
    public NetworkVariable<int> team = new NetworkVariable<int>();
    [SerializeField] private InputHandler inputHandler;
    [Header("Children")]
    [SerializeField] private Transform cueTransform;
    [SerializeField] private Transform head;
    [SerializeField] private Transform animatorTransform;
    [SerializeField] private FollowTransform headLookAt;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera cameraPrefab;

    private Dictionary<Limbs, int> limbHealth = new Dictionary<Limbs, int>()
    {
        { Limbs.Head, 100 },
        { Limbs.LeftHand, 100 },
        { Limbs.Legs, 100 },
        { Limbs.RightHand, 100 },
        { Limbs.Torso, 100 }
    };

    private PlayerMovement playerMovement;
    private PlayerState playerState = PlayerState.Billiard;

    private Cue cue;
    private Gun gun;
    private Sword sword;

    private Animator animator;
    private NetworkAnimator networkAnimator;
    private PlayerAnimations playerAnimations;
    private CinemachinePOV pov;

    private bool aimCue;

    private NetworkVariable<float> invincibleTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = animatorTransform.GetComponent<Animator>();
        networkAnimator = animatorTransform.GetComponent<NetworkAnimator>();
        playerAnimations = animatorTransform.GetComponent<PlayerAnimations>();
        cue = cueTransform.GetComponent<Cue>();
        gun = cueTransform.GetComponent<Gun>();
        sword = cueTransform.GetComponent<Sword>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CinemachineVirtualCamera camera = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        camera.Follow = head;
        camera.LookAt = head;
        pov = camera.GetCinemachineComponent<CinemachinePOV>();
        playerMovement.pov = pov;
        headLookAt.followTransform = camera.transform;
        playerAnimations.virtualCamera = camera;
        cue.Activate();
        SwitchToBilliard();
    }

    private void OnEnable()
    {
        inputHandler.OnCueRelease.AddListener(HitWithCue);
        inputHandler.OnShootWeapon.AddListener(Shoot);
        inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
        inputHandler.AimCueStateChanged.AddListener(AimCueChangedState);
        inputHandler.OnSwingBegin.AddListener(StartSwing);
        inputHandler.OnSwingEnd.AddListener(EndSwing);
        inputHandler.OnParryBegin.AddListener(StartParry);
        inputHandler.OnParryEnd.AddListener(EndParry);
    }

    private void OnDisable()
    {
        inputHandler.OnCueRelease.RemoveListener(HitWithCue);
        inputHandler.OnShootWeapon.RemoveListener(Shoot);
        inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
        inputHandler.AimCueStateChanged.RemoveListener(AimCueChangedState);
        inputHandler.OnParryBegin.RemoveListener(StartParry);
        inputHandler.OnParryEnd.RemoveListener(EndParry);
    }

    private void Update()
    {
        if (!IsOwner) return;

        animator.SetFloat("MoveX", inputHandler.Movement.x);
        animator.SetFloat("MoveZ", inputHandler.Movement.z);
        animator.SetBool("Moving", inputHandler.Movement != Vector3.zero);
        playerMovement.Move(inputHandler);

        // Start charging cue
        if (playerState == PlayerState.Billiard)
        {
            cue.charging = inputHandler.Cue;
            if (inputHandler.Cue && PoolManager.Instance.currentPoolPlayer == team.Value && !PoolManager.Instance.ballsMoving)
            {
                playerAnimations.ChargeCue(cue.cueForce);
            }
            else if (aimCue)
            {
                Vector2 pos = new Vector2(
                    inputHandler.MousePosition.x.Remap(0, Screen.width, -0.5f, 0.5f),
                    inputHandler.MousePosition.y.Remap(0, Screen.height, 0.2f, 1)
                );
                playerAnimations.AlignBilliardAim(pos);
            }
        }
        // Sword aiming
        else if (playerState == PlayerState.Sword)
        {
            Vector2 pos = new Vector2(
                inputHandler.MousePosition.x.Remap(0, Screen.width, -0.75f, 0.75f),
                inputHandler.MousePosition.y.Remap(0, Screen.height, -0.75f, 0.75f)
            );
            playerAnimations.AlignSwordPosition(pos);
        }
        invincibleTime.Value -= Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage, Limbs limb)
    {
        if (invincibleTime.Value > 0) return;
        networkAnimator.SetTrigger("Hit");
        TakeDamageClientRpc(damage, limb);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int damage, Limbs limb)
    {
        limbHealth[limb] -= damage;
        if (limbHealth[limb] <= 0) limbHealth[limb] = 1;
        skinnedMeshRenderer.materials[(int)limb].color = Color.Lerp(Color.red, Color.white, limbHealth[limb] / 100f);
        AudioManager.PlaySound(Sounds.Hit);
        playerMovement.speedDebuff = (limbHealth[Limbs.Legs] / 100.0f).Remap(0, 1, 0.5f, 1);
        gun.sway = ((limbHealth[Limbs.LeftHand] + limbHealth[Limbs.RightHand]) / 200.0f).Remap(0, 1, 0, 0.05f);
        PoolManager.Instance.damageTaken[team.Value] += damage;
        if (limbHealth[limb] <= 0)
        {
            //Die();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BecomeInvincibleServerRpc(float time)
    {
        invincibleTime.Value = time;
    }

    public void SwitchToFight()
    {
        if (playerState == PlayerState.Billiard)
        {
            playerState = PlayerState.Gun;
            animator.SetInteger("Phase", 1);
            gun.Activate();
            cue.Deactivate();
            playerAnimations.PlayerState = PlayerState.Gun;
            playerMovement.Swap(MovementState.Running);
        }
    }

    public void SwitchToBilliard()
    {
        playerState = PlayerState.Billiard;
        animator.SetInteger("Phase", 0);
        gun.Deactivate();
        sword.Deactivate();
        cue.Activate();
        playerAnimations.PlayerState = PlayerState.Billiard;
        playerAnimations.AlignBilliardAim(new Vector2(0, 0.2f));
        Cursor.lockState = CursorLockMode.Locked;
        playerMovement.Swap(MovementState.Walking);
    }

    [ClientRpc]
    public void AddBulletClientRpc(int bullet, ClientRpcParams clientRpcParams = default)
    {
        gun.bullets[bullet]++;
    }

    private void HitWithCue()
    {
        if (!IsOwner || playerState != PlayerState.Billiard ||
            PoolManager.Instance.currentPoolPlayer != team.Value || PoolManager.Instance.ballsMoving) return;
        cue.Shoot();
        playerAnimations.HitWithCue();
    }

    private void Shoot()
    {
        if (!IsOwner || playerState != PlayerState.Gun) return;
        networkAnimator.SetTrigger("Fire");
        gun.Shoot();
        AudioManager.PlaySound(Sounds.Shoot);
    }

    private void SwitchWeapons()
    {
        if (!IsOwner) return;
        if (playerState == PlayerState.Gun)
        {
            playerState = PlayerState.Sword;
            gun.Deactivate();
            sword.Activate();
            playerAnimations.PlayerState = PlayerState.Sword;
            playerAnimations.SetSword();
        }
        else if (playerState == PlayerState.Sword)
        {
            playerState = PlayerState.Gun;
            gun.Activate();
            sword.Deactivate();
            playerAnimations.PlayerState = PlayerState.Gun;
        }
    }

    private void SwitchAmmo(bool up)
    {
        if (!IsOwner || playerState != PlayerState.Gun) return;
        gun.SwitchBullet(up);
        AudioManager.PlaySound(Sounds.Reload);
    }

    private void AimCueChangedState(bool state)
    {
        if (!IsOwner || playerState != PlayerState.Billiard) return;
        if (state)
        {
            Cursor.lockState = CursorLockMode.Confined;
            pov.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            pov.enabled = true;
        }

        aimCue = state;
    }

    private void StartSwing()
    {
        if (!IsOwner || playerState != PlayerState.Sword || playerAnimations.parrying
            || playerAnimations.swinging || playerAnimations.swingTimer > 0.0f) return;
        sword.StartSwing();
        playerAnimations.parrying = false;
        playerAnimations.swinging = true;
        Cursor.lockState = CursorLockMode.Confined;
        AudioManager.PlaySound(Sounds.Slash);
        Debug.Log("Swinging");
    }

    private void EndSwing()
    {
        if (!IsOwner || playerState != PlayerState.Sword || playerAnimations.swingTimer > 0.0f) return;
        sword.EndSwing();
        playerAnimations.swinging = false;
        playerAnimations.swingTimer = playerAnimations.swingDuration;
        playerAnimations.swingDirection = inputHandler.Look.normalized;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Stopped swinging");
    }

    private void StartParry()
    {
        if (!IsOwner || playerState != PlayerState.Sword || playerAnimations.swinging || playerAnimations.swingTimer > 0.0f) return;
        sword.StartParry();
        playerAnimations.parrying = true;
    }

    private void EndParry()
    {
        if (!IsOwner || playerState != PlayerState.Sword) return;
        sword.EndParry();
        playerAnimations.parrying = false;
    }
}
