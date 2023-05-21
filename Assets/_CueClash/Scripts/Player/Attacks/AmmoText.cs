using UnityEngine;
using TMPro;

public class AmmoText : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUI;

    public void UpdateAmmoText(int ammo, int bullet)
    {
        textMeshProUI.SetText("Ammo: " + ammo + "\nBullet: " + bullet);
    }
}
