using Unity.Netcode;
using UnityEngine;

public class Limb : NetworkBehaviour
{
    [SerializeField] private Limbs limb;
    [SerializeField] private PlayerObject playerObject;

    public void TakeDamage(int damage)
    {
        playerObject.TakeDamageServerRpc(damage, limb);
    }
}
