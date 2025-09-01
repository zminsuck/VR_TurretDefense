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
    [Tooltip("���׷��̵� ������ �� �� ����Ǵ� VFX ������ (ParticleSystem/���� ������Ʈ)")]
    [SerializeField] private GameObject upgradeBurstVfx;
    [Tooltip("���׷��̵� ���� ª�� ��½�̴� ����Ʈ(����)")]
    [SerializeField] private Light flashLight;
    [SerializeField, Min(0f)] private float lightFlashTime = 0.06f;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private float sfxVolume = 1f;

    [Header("Persistent Aura (Optional)")]
    [Tooltip("��ũ���� ��� �ѵ� ��� ����Ʈ (������ ���). �迭 ����/������ rankPrefabs�� ����.")]
    [SerializeField] private GameObject[] rankAuraPrefabs;

    [Header("Events")]
    public UnityEvent<int> onRankChanged; // �� ��ũ �ε��� �ݹ�

    // �ܺο��� ü��/���۷�ũ/�θ� ������ �� ȣ��(���� �� ���)
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
            // �� �̻� ���׷��̵� ����
            return;
        }

        // === 1) ȿ�� ��� (��ü �� ���� ��ġ����) ===
        PlayUpgradeFX();

        // === 2) �� ��ũ ���������� ��ü ===
        Transform parent = parentOverride ? parentOverride : transform.parent;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        GameObject newTurret = Instantiate(rankPrefabs[next], pos, rot, parent);

        // ���� ���׷��̵带 ���� �� ���׷��̴� �ʱ�ȭ
        var up = newTurret.GetComponent<TurretRankUpgrader>();
        if (!up) up = newTurret.AddComponent<TurretRankUpgrader>();
        up.Initialize(rankPrefabs, (Rank)next, parent);

        // ��ũ�� ���� ���� ����
        up.AttachAuraForRank(next);

        onRankChanged?.Invoke(next);

        // ���� ��ž ����
        Destroy(gameObject);
    }

    // �������� Ư�� ��ũ�� ���� �����ϰ� ���� ��
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

        // 2) ����Ʈ �÷���
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
        // ���� ���� ����
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c && c.CompareTag("TurretAura")) Destroy(c.gameObject);
        }

        if (rankAuraPrefabs == null || rankIndex < 0 || rankIndex >= rankAuraPrefabs.Length) return;
        var auraPrefab = rankAuraPrefabs[rankIndex];
        if (!auraPrefab) return;

        var aura = Instantiate(auraPrefab, transform);
        aura.tag = "TurretAura"; // �ĺ���
        aura.transform.localPosition = Vector3.zero;
        aura.transform.localRotation = Quaternion.identity;
        // �ʿ��ϸ� localScale ����
    }
}
