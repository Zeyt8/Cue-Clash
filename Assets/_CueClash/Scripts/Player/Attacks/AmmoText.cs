using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AmmoText : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUI;

    public void UpdateAmmoText(int ammo)
    {
        textMeshProUI.SetText("Ammo: " + ammo.ToString());
    }
}
