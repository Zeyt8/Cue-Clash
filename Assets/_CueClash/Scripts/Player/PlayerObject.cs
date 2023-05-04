using System.Collections.Generic;
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

public enum Limbs
{
    Head,
    LeftHand,
    LeftLeg,
    RightHand,
    RightLeg,
    Torso
}

public class PlayerObject : NetworkBehaviour
{
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
        { Limbs.LeftLeg, 100 },
        { Limbs.RightHand, 100 },
        { Limbs.RightLeg, 100 },
        { Limbs.Torso, 100 }
    };

    private PlayerMovement playerMovement;
    private PlayerState playerState = PlayerState.Billiard;

    private Cue cue;
    private Gun gun;
    private Sword sword;

    private Animator animator;
    private PlayerAnimations playerAnimations;
    private CinemachinePOV pov;

    private bool aimCue;

    private readonly int maxNrOfPoolShots = 3;

    private readonly float maxDurationOfBattle = 20;
    private float battleTimer = 0;

    private NetworkVariable<float> invincibleTime = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = animatorTransform.GetComponent<Animator>();
        playerAnimations = animatorTransform.GetComponent<PlayerAnimations>();
        cue = cueTransform.GetComponent<Cue>();
        gun = cueTransform.GetComponent<Gun>();
        sword = cueTransform.GetComponent<Sword>();
        playerAnimations.PlayerState = PlayerState.Billiard;
        Cursor.lockState = CursorLockMode.Locked;
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
        playerAnimations.camera = camera;
        cue.Activate();
    }

    private void OnEnable()
    {
        inputHandler.OnCueRelease.AddListener(HitWithCue);
        inputHandler.OnShootWeapon.AddListener(Shoot);
        inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
        inputHandler.AimCueStateChanged.AddListener(AimCueChangedState);
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

        animator.SetBool("Walking", inputHandler.Movement != Vector3.zero);
        playerMovement.Move(inputHandler);

        // Start charging cue
        if (playerState == PlayerState.Billiard)
        {
            cue.charging = inputHandler.Cue;
            if (inputHandler.Cue)
            {
                playerAnimations.ChargeCue(cue.cueForce);
            }
            else if (aimCue)
            {
                Vector2 pos = new Vector2(
                    inputHandler.MousePosition.x.Remap(0, Screen.width, -1, 1),
                    inputHandler.MousePosition.y.Remap(0, Screen.height, -1, 1)
                );
                pos.y = Mathf.Clamp(pos.y, 0, 1);
                playerAnimations.AlignBilliardAim(pos);
            }
        }    
        else if (playerState == PlayerState.Sword)
        {
            Vector2 pos = new Vector2(
                inputHandler.MousePosition.x.Remap(0, Screen.width, -0.75f, 0.75f),
                inputHandler.MousePosition.y.Remap(0, Screen.height, -0.75f, 0.75f)
            );
            playerAnimations.AlignSwordPosition(pos);
        }
        if (playerState != PlayerState.Billiard)
        {
            battleTimer += Time.deltaTime;
            if (battleTimer > maxDurationOfBattle)
            {
                playerState = PlayerState.Billiard;
                animator.SetInteger("Phase", 0);
                gun.Deactivate();
                sword.Deactivate();
                cue.Activate();
                playerAnimations.PlayerState = PlayerState.Billiard;
                playerAnimations.AlignBilliardAim(new Vector2(0, 0));
                Cursor.lockState = CursorLockMode.Locked;
                battleTimer = 0;
            }
        }
        invincibleTime.Value -= Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage, Limbs limb)
    {
        if (invincibleTime.Value > 0) return;
        TakeDamageClientRpc(damage, limb);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int damage, Limbs limb)
    {
        limbHealth[limb] -= damage;
        skinnedMeshRenderer.materials[(int)limb].color = Color.Lerp(Color.red, Color.white, limbHealth[limb] / 100f);
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
        print("here");
        if (playerState == PlayerState.Billiard)
        {
            playerState = PlayerState.Gun;
            animator.SetInteger("Phase", 1);
            gun.Activate();
            cue.Deactivate();
            playerAnimations.PlayerState = PlayerState.Gun;
        }
    }

    public void AddBullet(int bullet)
    {
        gun.bullets.Add(bullet);
    }

    private void HitWithCue()
    {
        if (!IsOwner || playerState != PlayerState.Billiard) return;
        cue.Shoot();
        playerAnimations.HitWithCue();
    }

    private void Shoot()
    {
        if (!IsOwner || playerState != PlayerState.Gun) return;
        gun.Shoot();

        /// TODO: For when Sword is implemented
        if (gun.nrOfBullets.Value < 1)
        {
            SwitchWeapons();
        }
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
            Cursor.lockState = CursorLockMode.Confined;
        }
        else if (playerState == PlayerState.Sword)
        {
            playerState = PlayerState.Gun;
            gun.Activate();
            sword.Deactivate();
            playerAnimations.PlayerState = PlayerState.Gun;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void SwitchAmmo()
    {

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

    private void StartParry()
    {
        if (!IsOwner || playerState != PlayerState.Sword) return;
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
