using UnityEngine;

public class TurretStatsProvider : MonoBehaviour, ITurretStats
{
    [SerializeField] private TurretStatsSO preset;

    // (선택) 런타임 보정값(업그레이드/버프)
    [SerializeField] private float rangeBonus = 0f;
    [SerializeField] private float fireRateBonus = 0f;
    [SerializeField] private int damageBonus = 0;

    public float Range => Mathf.Max(0.1f, (preset ? preset.Range : 0f) + rangeBonus);
    public float TurnSpeed => Mathf.Max(0.1f, preset ? preset.TurnSpeed : 0f);
    public float CooldownSeconds => Mathf.Max(0.1f, (preset ? preset.CooldownSeconds : 0f) + fireRateBonus);
    public int Damage => Mathf.Max(1, (preset ? preset.Damage : 0) + damageBonus);

    // (선택) 런타임에서 프리셋 교체/업그레이드 API
    public void SetPreset(TurretStatsSO next) => preset = next;
    public void AddUpgrade(float rangeΔ = 0f, float fireRateΔ = 0f, int damageΔ = 0)
    { rangeBonus += rangeΔ; fireRateBonus += fireRateΔ; damageBonus += damageΔ; }
}
