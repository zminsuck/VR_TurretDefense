using UnityEngine;

[CreateAssetMenu(menuName = "Turret/Weapon Config", fileName = "WeaponConfig")]
public class WeaponConfigSO : ScriptableObject
{

    [Header("Hitscan Only")]
    [Min(0f)] public float laserShowTime = 0.05f;
    public bool drawLaser = true;
}
