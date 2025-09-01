// C# �⺻: Unity Engine ���̺귯���� Ŭ������ �Լ��� ����ϱ� ���� ���ӽ����̽� ����
using UnityEngine;

// ��ü ���� ���α׷���(OOP): MonoBehaviour�� ��ӹ޾� Unity ������Ʈ �ý��ۿ� ����
public class TurretSpawnerUI : MonoBehaviour
{
    // Unity ������ ���: [Header] ��Ʈ����Ʈ�� �ν����� â UI�� �׷�ȭ
    [Header("��޺� ������ (0:Normal, 1:Rare, 2:Elite, 3:Epic)")]
    // ������ ��� ����: ��޺� �ͷ� ������ �ڵ忡 �ϵ��ڵ����� �ʰ�, ������ �迭 �����ͷ� �����Ͽ� �������� ����
    [SerializeField] private GameObject[] rankPrefabs = new GameObject[0];

    [Header("���� ��ġ/�θ�")]
    // Unity ������ ���: [SerializeField] ��Ʈ����Ʈ�� private ������ �ν����Ϳ� ������� �����Ϳ� ������ �и� (����ȭ)
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform parentOverride;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool warnIfParentInactive = true;

    // UI �̺�Ʈ �ڵ鷯: Unity UI�� Button ������Ʈ OnClick() �̺�Ʈ�� ���� �����ϱ� ���� public �޼���� �ۼ�
    public void SpawnNormal() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnNormal"); SpawnByRank(0); }
    public void SpawnRare() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnRare"); SpawnByRank(1); }
    public void SpawnElite() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnElite"); SpawnByRank(2); }
    public void SpawnEpic() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnEpic"); SpawnByRank(3); }

    // �ڵ� ����ȭ: �ߺ��Ǵ� ���� ������ �ϳ��� �޼���� �����Ͽ� �ڵ��� ���뼺�� ������������ ����
    public void SpawnByRank(int rankIndex)
    {
        // ����� ���α׷��� (���� Ŭ����): �ʼ� ���� ������ �Ҵ���� �ʾ��� ���, ��� ����ϰ� �Լ��� ��� �����Ͽ� ������ ����
        if (rankPrefabs == null || rankPrefabs.Length == 0)
        { Debug.LogWarning("[TurretSpawnerUI] rankPrefabs�� ����ֽ��ϴ�.", this); return; }

        if (!spawnPoint)
        { Debug.LogWarning("[TurretSpawnerUI] spawnPoint�� �����ϴ�.", this); return; }

        // ������ ��ȿ�� �˻� (Ŭ����): 'Mathf.Clamp'�� ����Ͽ� rankIndex�� �迭�� ��ȿ�� ���� ���� �ֵ��� �����Ͽ� 'IndexOutOfRangeException'�� ����
        rankIndex = Mathf.Clamp(rankIndex, 0, rankPrefabs.Length - 1);
        var prefab = rankPrefabs[rankIndex];
        if (!prefab)
        { Debug.LogWarning($"[TurretSpawnerUI] rankPrefabs[{rankIndex}] �������� �������.", this); return; }

        // C# ���� ������: 'parentOverride'�� �Ҵ�Ǿ� ������ �װ��� ����ϰ�, �׷��� ������ 'spawnPoint.parent'�� ����ϴ� ���Ǻ� �Ҵ�
        var parent = parentOverride ? parentOverride : spawnPoint.parent;
        if (warnIfParentInactive && parent && !parent.gameObject.activeInHierarchy)
            Debug.LogWarning("[TurretSpawnerUI] parentOverride(�Ǵ� spawnPoint.parent)�� ��Ȱ�� �����Դϴ�. �����ŵ� �� ���� �� �־��.", parent);

        var pos = spawnPoint.position;
        var rot = spawnPoint.rotation;

        // Unity ��ü ���� ���: 'Instantiate'�� ����Ͽ� ���������κ��� ���� ������Ʈ�� �������� �����ϰ� �� ���� ������ ��ġ
        var go = Instantiate(prefab, pos, rot, parent);
        if (debugLogs) Debug.Log($"[TurretSpawnerUI] Spawned '{go.name}' @ {pos}", go);

        var up = go.GetComponent<TurretRankUpgrader>();
        // ���� ������Ʈ �߰�: �ʿ��� ������Ʈ�� ���� ��� 'AddComponent'�� ����Ͽ� ��Ÿ�ӿ� �������� �߰�
        if (!up) up = go.AddComponent<TurretRankUpgrader>();
        // ��ü �ʱ�ȭ: ������ ��ü�� ������Ʈ�� �ʿ��� �����͸� �����Ͽ� �ʱ� ���¸� ����
        up.Initialize(rankPrefabs, (TurretRankUpgrader.Rank)rankIndex, parent);

        // ���� �� ����: ������ �����տ� �ʼ� ������Ʈ���� ����� ���ԵǾ� �ִ��� Ȯ���ϰ�, ���� �� ��� ����Ͽ� ������� ����
        var turret = go.GetComponentInChildren<Turret>();
        if (!turret) Debug.LogWarning("[TurretSpawnerUI] Turret ������Ʈ�� ã�� ���߽��ϴ�. ������ ������ Ȯ���ϼ���.", go);

        var stats = go.GetComponentInChildren<ITurretStats>();
        if (stats == null) Debug.LogWarning("[TurretSpawnerUI] ITurretStats(TurretStatsProvider/Scriptable) ����.", go);

        var weapon = go.GetComponentInChildren<IWeapon>();
        if (weapon == null) Debug.LogWarning("[TurretSpawnerUI] IWeapon(Projectile/Hitscan) ����.", go);
    }

    // C# ��ó���� ���ù�: '#if UNITY_EDITOR'�� '#endif' ������ �ڵ�� Unity �����Ϳ����� �����ϵǸ�, ���� ���忡���� ���ܵ�
#if UNITY_EDITOR
    // Unity ������ Ȯ�� (���ؽ�Ʈ �޴�): '[ContextMenu]' ��Ʈ����Ʈ�� ����Ͽ� �ν����� â�� ���ؽ�Ʈ �޴��� �׽�Ʈ�� �Լ��� �߰�
    [ContextMenu("Spawn Normal (Editor Test)")]
    private void SpawnNormal_EditorTest() => SpawnByRank(0);
#endif

    // Unity ������ �ݹ�: 'OnValidate'�� �����Ϳ��� ��ũ��Ʈ�� �ε�ǰų� �ν����� ���� ����� ������ ȣ���
    private void OnValidate()
    {
        // �ǽð� ��ȿ�� �˻�: ������ �������� �ʰ� �ν����Ϳ��� ���� �����ϴ� ��� �ʼ� �׸��� �����Ǿ����� �˻��Ͽ� ��� ǥ��
        if (rankPrefabs != null && rankPrefabs.Length > 0 && rankPrefabs[0] == null)
            Debug.LogWarning("[TurretSpawnerUI] rankPrefabs[0](Normal)�� ����ֽ��ϴ�.", this);
        if (!spawnPoint)
            Debug.LogWarning("[TurretSpawnerUI] spawnPoint�� ����ֽ��ϴ�.", this);
    }
}