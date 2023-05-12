using UnityEngine;

public class Limb : MonoBehaviour
{
    [SerializeField] private Limbs limb;
    [SerializeField] private PlayerObject playerObject;

    public int GetTeam()
    {
        return playerObject.team;
    }

    public void TakeDamage(int damage)
    {
        playerObject.TakeDamageServerRpc(damage, limb);
    }
}
