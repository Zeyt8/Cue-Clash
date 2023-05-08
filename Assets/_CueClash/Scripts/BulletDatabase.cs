using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletDatabase", menuName = "CueClash/BulletDatabase", order = 1)]
public class BulletDatabase : ScriptableObject
{
    public List<Bullet> bullets = new List<Bullet>();
}
