using UnityEngine;

public class TurretSpawnerUI : MonoBehaviour
{
    [Header("등급별 프리팹 (0:Normal, 1:Rare, 2:Elite, 3:Epic)")]
    [SerializeField] private GameObject[] rankPrefabs = new GameObject[0];

    [Header("스폰 위치/부모")]
    [SerializeField] private Transform spawnPoint;     // 생성 위치/회전 기준
    [SerializeField] private Transform parentOverride; // 비우면 spawnPoint.parent

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool warnIfParentInactive = true;

    // === UI 버튼 OnClick에 연결 ===
    public void SpawnNormal() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnNormal"); SpawnByRank(0); }
    public void SpawnRare() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnRare"); SpawnByRank(1); }
    public void SpawnElite() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnElite"); SpawnByRank(2); }
    public void SpawnEpic() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnEpic"); SpawnByRank(3); }

    public void SpawnByRank(int rankIndex)
    {
        // 기본 검증
        if (rankPrefabs == null || rankPrefabs.Length == 0)
        { Debug.LogWarning("[TurretSpawnerUI] rankPrefabs가 비어있습니다.", this); return; }

        if (!spawnPoint)
        { Debug.LogWarning("[TurretSpawnerUI] spawnPoint가 없습니다.", this); return; }

        rankIndex = Mathf.Clamp(rankIndex, 0, rankPrefabs.Length - 1);
        var prefab = rankPrefabs[rankIndex];
        if (!prefab)
        { Debug.LogWarning($"[TurretSpawnerUI] rankPrefabs[{rankIndex}] 프리팹이 비어있음.", this); return; }

        var parent = parentOverride ? parentOverride : spawnPoint.parent;
        if (warnIfParentInactive && parent && !parent.gameObject.activeInHierarchy)
            Debug.LogWarning("[TurretSpawnerUI] parentOverride(또는 spawnPoint.parent)가 비활성 상태입니다. 생성돼도 안 보일 수 있어요.", parent);

        var pos = spawnPoint.position;
        var rot = spawnPoint.rotation;

        // 생성
        var go = Instantiate(prefab, pos, rot, parent);
        if (debugLogs) Debug.Log($"[TurretSpawnerUI] Spawned '{go.name}' @ {pos}", go);

        // 랭크 체인 연결(라운드 업그레이드용)
        var up = go.GetComponent<TurretRankUpgrader>();
        if (!up) up = go.AddComponent<TurretRankUpgrader>();
        up.Initialize(rankPrefabs, (TurretRankUpgrader.Rank)rankIndex, parent);

        // 필수 구성 점검(있으면 바로 전투 가능)
        var turret = go.GetComponentInChildren<Turret>();
        if (!turret) Debug.LogWarning("[TurretSpawnerUI] Turret 컴포넌트를 찾지 못했습니다. 프리팹 구성을 확인하세요.", go);

        var stats = go.GetComponentInChildren<ITurretStats>();
        if (stats == null) Debug.LogWarning("[TurretSpawnerUI] ITurretStats(TurretStatsProvider/Scriptable) 누락.", go);

        var weapon = go.GetComponentInChildren<IWeapon>();
        if (weapon == null) Debug.LogWarning("[TurretSpawnerUI] IWeapon(Projectile/Hitscan) 누락.", go);
    }

#if UNITY_EDITOR
    [ContextMenu("Spawn Normal (Editor Test)")]
    private void SpawnNormal_EditorTest() => SpawnByRank(0);
#endif

    private void OnValidate()
    {
        // 에디터에서 바로 오류 발견
        if (rankPrefabs != null && rankPrefabs.Length > 0 && rankPrefabs[0] == null)
            Debug.LogWarning("[TurretSpawnerUI] rankPrefabs[0](Normal)이 비어있습니다.", this);
        if (!spawnPoint)
            Debug.LogWarning("[TurretSpawnerUI] spawnPoint가 비어있습니다.", this);
    }
}
