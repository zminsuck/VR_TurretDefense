using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TurretRankUpgrader : MonoBehaviour
{
    public enum Rank { Normal, Rare, Elite, Epic }

    [Header("Rank Chain (0:Normal, 1:Rare, 2:Elite, 3:Epic)")]
    [SerializeField] private GameObject[] rankPrefabs = new GameObject[4];

    [Header("State")]
    [SerializeField] private Rank current = Rank.Normal;
    [SerializeField] private Transform parentOverride;

    [Header("FX (One-shot on Upgrade)")]
    [Tooltip("업그레이드 순간에 한 번 재생되는 VFX 프리팹 (ParticleSystem/임의 오브젝트)")]
    [SerializeField] private GameObject upgradeBurstVfx;
    [Tooltip("업그레이드 순간 짧게 번쩍이는 라이트(선택)")]
    [SerializeField] private Light flashLight;
    [SerializeField, Min(0f)] private float lightFlashTime = 0.06f;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private float sfxVolume = 1f;

    [Header("Persistent Aura (Optional)")]
    [Tooltip("랭크별로 계속 켜둘 장식 이펙트 (없으면 비움). 배열 길이/순서는 rankPrefabs와 동일.")]
    [SerializeField] private GameObject[] rankAuraPrefabs;

    [Header("Events")]
    public UnityEvent<int> onRankChanged; // 새 랭크 인덱스 콜백

    // 외부에서 체인/시작랭크/부모를 세팅할 때 호출(스폰 시 사용)
    public void Initialize(GameObject[] chain, Rank start, Transform parent)
    {
        if (chain != null && chain.Length > 0) rankPrefabs = chain;
        current = start;
        parentOverride = parent;
    }

    public void RankUp()
    {
        int next = (int)current + 1;
        if (rankPrefabs == null || next >= rankPrefabs.Length || !rankPrefabs[next])
        {
            // 더 이상 업그레이드 없음
            return;
        }

        // === 1) 효과 재생 (교체 전 현재 위치에서) ===
        PlayUpgradeFX();

        // === 2) 새 랭크 프리팹으로 교체 ===
        Transform parent = parentOverride ? parentOverride : transform.parent;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        GameObject newTurret = Instantiate(rankPrefabs[next], pos, rot, parent);

        // 다음 업그레이드를 위해 새 업그레이더 초기화
        var up = newTurret.GetComponent<TurretRankUpgrader>();
        if (!up) up = newTurret.AddComponent<TurretRankUpgrader>();
        up.Initialize(rankPrefabs, (Rank)next, parent);

        // 랭크별 지속 오라 적용
        up.AttachAuraForRank(next);

        onRankChanged?.Invoke(next);

        // 이전 포탑 제거
        Destroy(gameObject);
    }

    // 수동으로 특정 랭크로 강제 세팅하고 싶을 때
    public void SetRankImmediate(int rankIndex)
    {
        current = (Rank)Mathf.Clamp(rankIndex, 0, rankPrefabs.Length - 1);
        AttachAuraForRank(rankIndex);
    }

    // ---------- FX ----------
    void PlayUpgradeFX()
    {
        // 1) VFX
        if (upgradeBurstVfx)
        {
            var go = Instantiate(upgradeBurstVfx, transform.position, transform.rotation);
            var ps = go.GetComponent<ParticleSystem>();
            if (ps)
                Destroy(go, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(go, 3f);
        }

        // 2) 라이트 플래시
        if (flashLight) StartCoroutine(FlashLight());

        // 3) SFX
        if (upgradeClip)
        {
            if (audioSource) audioSource.PlayOneShot(upgradeClip, sfxVolume);
            else AudioSource.PlayClipAtPoint(upgradeClip, transform.position, sfxVolume);
        }
    }

    IEnumerator FlashLight()
    {
        flashLight.enabled = true;
        yield return new WaitForSeconds(lightFlashTime);
        if (flashLight) flashLight.enabled = false;
    }

    void AttachAuraForRank(int rankIndex)
    {
        // 기존 오라 제거
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c && c.CompareTag("TurretAura")) Destroy(c.gameObject);
        }

        if (rankAuraPrefabs == null || rankIndex < 0 || rankIndex >= rankAuraPrefabs.Length) return;
        var auraPrefab = rankAuraPrefabs[rankIndex];
        if (!auraPrefab) return;

        var aura = Instantiate(auraPrefab, transform);
        aura.tag = "TurretAura"; // 식별용
        aura.transform.localPosition = Vector3.zero;
        aura.transform.localRotation = Quaternion.identity;
        // 필요하면 localScale 조정
    }
}
