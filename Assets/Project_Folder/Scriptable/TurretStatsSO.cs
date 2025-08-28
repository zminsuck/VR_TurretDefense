using UnityEngine;

[CreateAssetMenu(menuName = "Turret/Stats Preset", fileName = "TurretStats")]
public class TurretStatsSO : ScriptableObject, ITurretStats
{
    [Header("Base Stats")]
    [Min(0.1f)] public float range = 12f;
    [Min(0.1f)] public float turnSpeed = 8f;
    [Min(0.1f)] public float cooldownSeconds = 2f;   // 초당 발사 수
    [Min(1)] public int damage = 10;

    // ITurretStats 구현 (읽기 전용만 노출)
    public float Range => range;
    public float TurnSpeed => turnSpeed;
    public float CooldownSeconds => cooldownSeconds;
    public int Damage => damage;
}
