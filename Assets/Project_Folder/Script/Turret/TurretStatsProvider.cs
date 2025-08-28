using UnityEngine;

public class TurretStatsProvider : MonoBehaviour, ITurretStats
{
    [SerializeField] private TurretStatsSO preset;

    // (����) ��Ÿ�� ������(���׷��̵�/����)
    [SerializeField] private float rangeBonus = 0f;
    [SerializeField] private float fireRateBonus = 0f;
    [SerializeField] private int damageBonus = 0;

    public float Range => Mathf.Max(0.1f, (preset ? preset.Range : 0f) + rangeBonus);
    public float TurnSpeed => Mathf.Max(0.1f, preset ? preset.TurnSpeed : 0f);
    public float CooldownSeconds => Mathf.Max(0.1f, (preset ? preset.CooldownSeconds : 0f) + fireRateBonus);
    public int Damage => Mathf.Max(1, (preset ? preset.Damage : 0) + damageBonus);

    // (����) ��Ÿ�ӿ��� ������ ��ü/���׷��̵� API
    public void SetPreset(TurretStatsSO next) => preset = next;
    public void AddUpgrade(float range�� = 0f, float fireRate�� = 0f, int damage�� = 0)
    { rangeBonus += range��; fireRateBonus += fireRate��; damageBonus += damage��; }
}
