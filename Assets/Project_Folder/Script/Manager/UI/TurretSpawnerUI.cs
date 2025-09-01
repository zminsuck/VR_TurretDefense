using UnityEngine;

public class TurretSpawnerUI : MonoBehaviour
{
    [Header("��޺� ������ (0:Normal, 1:Rare, 2:Elite, 3:Epic)")]
    [SerializeField] private GameObject[] rankPrefabs = new GameObject[0];

    [Header("���� ��ġ/�θ�")]
    [SerializeField] private Transform spawnPoint;     // ���� ��ġ/ȸ�� ����
    [SerializeField] private Transform parentOverride; // ���� spawnPoint.parent

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool warnIfParentInactive = true;

    // === UI ��ư OnClick�� ���� ===
    public void SpawnNormal() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnNormal"); SpawnByRank(0); }
    public void SpawnRare() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnRare"); SpawnByRank(1); }
    public void SpawnElite() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnElite"); SpawnByRank(2); }
    public void SpawnEpic() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnEpic"); SpawnByRank(3); }

    public void SpawnByRank(int rankIndex)
    {
        // �⺻ ����
        if (rankPrefabs == null || rankPrefabs.Length == 0)
        { Debug.LogWarning("[TurretSpawnerUI] rankPrefabs�� ����ֽ��ϴ�.", this); return; }

        if (!spawnPoint)
        { Debug.LogWarning("[TurretSpawnerUI] spawnPoint�� �����ϴ�.", this); return; }

        rankIndex = Mathf.Clamp(rankIndex, 0, rankPrefabs.Length - 1);
        var prefab = rankPrefabs[rankIndex];
        if (!prefab)
        { Debug.LogWarning($"[TurretSpawnerUI] rankPrefabs[{rankIndex}] �������� �������.", this); return; }

        var parent = parentOverride ? parentOverride : spawnPoint.parent;
        if (warnIfParentInactive && parent && !parent.gameObject.activeInHierarchy)
            Debug.LogWarning("[TurretSpawnerUI] parentOverride(�Ǵ� spawnPoint.parent)�� ��Ȱ�� �����Դϴ�. �����ŵ� �� ���� �� �־��.", parent);

        var pos = spawnPoint.position;
        var rot = spawnPoint.rotation;

        // ����
        var go = Instantiate(prefab, pos, rot, parent);
        if (debugLogs) Debug.Log($"[TurretSpawnerUI] Spawned '{go.name}' @ {pos}", go);

        // ��ũ ü�� ����(���� ���׷��̵��)
        var up = go.GetComponent<TurretRankUpgrader>();
        if (!up) up = go.AddComponent<TurretRankUpgrader>();
        up.Initialize(rankPrefabs, (TurretRankUpgrader.Rank)rankIndex, parent);

        // �ʼ� ���� ����(������ �ٷ� ���� ����)
        var turret = go.GetComponentInChildren<Turret>();
        if (!turret) Debug.LogWarning("[TurretSpawnerUI] Turret ������Ʈ�� ã�� ���߽��ϴ�. ������ ������ Ȯ���ϼ���.", go);

        var stats = go.GetComponentInChildren<ITurretStats>();
        if (stats == null) Debug.LogWarning("[TurretSpawnerUI] ITurretStats(TurretStatsProvider/Scriptable) ����.", go);

        var weapon = go.GetComponentInChildren<IWeapon>();
        if (weapon == null) Debug.LogWarning("[TurretSpawnerUI] IWeapon(Projectile/Hitscan) ����.", go);
    }

#if UNITY_EDITOR
    [ContextMenu("Spawn Normal (Editor Test)")]
    private void SpawnNormal_EditorTest() => SpawnByRank(0);
#endif

    private void OnValidate()
    {
        // �����Ϳ��� �ٷ� ���� �߰�
        if (rankPrefabs != null && rankPrefabs.Length > 0 && rankPrefabs[0] == null)
            Debug.LogWarning("[TurretSpawnerUI] rankPrefabs[0](Normal)�� ����ֽ��ϴ�.", this);
        if (!spawnPoint)
            Debug.LogWarning("[TurretSpawnerUI] spawnPoint�� ����ֽ��ϴ�.", this);
    }
}
