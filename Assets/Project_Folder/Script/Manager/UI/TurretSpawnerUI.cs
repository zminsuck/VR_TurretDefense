// C# 기본: Unity Engine 라이브러리의 클래스와 함수를 사용하기 위한 네임스페이스 선언
using UnityEngine;

// 객체 지향 프로그래밍(OOP): MonoBehaviour를 상속받아 Unity 컴포넌트 시스템에 통합
public class TurretSpawnerUI : MonoBehaviour
{
    // Unity 에디터 기술: [Header] 어트리뷰트로 인스펙터 창 UI를 그룹화
    [Header("등급별 프리팹 (0:Normal, 1:Rare, 2:Elite, 3:Epic)")]
    // 데이터 기반 설계: 등급별 터렛 정보를 코드에 하드코딩하지 않고, 프리팹 배열 데이터로 관리하여 유연성을 높임
    [SerializeField] private GameObject[] rankPrefabs = new GameObject[0];

    [Header("스폰 위치/부모")]
    // Unity 에디터 기술: [SerializeField] 어트리뷰트로 private 변수를 인스펙터에 노출시켜 데이터와 로직을 분리 (직렬화)
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform parentOverride;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool warnIfParentInactive = true;

    // UI 이벤트 핸들러: Unity UI의 Button 컴포넌트 OnClick() 이벤트에 직접 연결하기 위해 public 메서드로 작성
    public void SpawnNormal() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnNormal"); SpawnByRank(0); }
    public void SpawnRare() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnRare"); SpawnByRank(1); }
    public void SpawnElite() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnElite"); SpawnByRank(2); }
    public void SpawnEpic() { if (debugLogs) Debug.Log("[TurretSpawnerUI] Click: SpawnEpic"); SpawnByRank(3); }

    // 코드 구조화: 중복되는 스폰 로직을 하나의 메서드로 통합하여 코드의 재사용성과 유지보수성을 높임
    public void SpawnByRank(int rankIndex)
    {
        // 방어적 프로그래밍 (가드 클로즈): 필수 참조 변수가 할당되지 않았을 경우, 경고를 출력하고 함수를 즉시 종료하여 오류를 방지
        if (rankPrefabs == null || rankPrefabs.Length == 0)
        { Debug.LogWarning("[TurretSpawnerUI] rankPrefabs가 비어있습니다.", this); return; }

        if (!spawnPoint)
        { Debug.LogWarning("[TurretSpawnerUI] spawnPoint가 없습니다.", this); return; }

        // 데이터 유효성 검사 (클램핑): 'Mathf.Clamp'를 사용하여 rankIndex가 배열의 유효한 범위 내에 있도록 강제하여 'IndexOutOfRangeException'을 예방
        rankIndex = Mathf.Clamp(rankIndex, 0, rankPrefabs.Length - 1);
        var prefab = rankPrefabs[rankIndex];
        if (!prefab)
        { Debug.LogWarning($"[TurretSpawnerUI] rankPrefabs[{rankIndex}] 프리팹이 비어있음.", this); return; }

        // C# 삼항 연산자: 'parentOverride'가 할당되어 있으면 그것을 사용하고, 그렇지 않으면 'spawnPoint.parent'를 사용하는 조건부 할당
        var parent = parentOverride ? parentOverride : spawnPoint.parent;
        if (warnIfParentInactive && parent && !parent.gameObject.activeInHierarchy)
            Debug.LogWarning("[TurretSpawnerUI] parentOverride(또는 spawnPoint.parent)가 비활성 상태입니다. 생성돼도 안 보일 수 있어요.", parent);

        var pos = spawnPoint.position;
        var rot = spawnPoint.rotation;

        // Unity 객체 생성 기술: 'Instantiate'를 사용하여 프리팹으로부터 게임 오브젝트를 동적으로 생성하고 씬 계층 구조에 배치
        var go = Instantiate(prefab, pos, rot, parent);
        if (debugLogs) Debug.Log($"[TurretSpawnerUI] Spawned '{go.name}' @ {pos}", go);

        var up = go.GetComponent<TurretRankUpgrader>();
        // 동적 컴포넌트 추가: 필요한 컴포넌트가 없을 경우 'AddComponent'를 사용하여 런타임에 동적으로 추가
        if (!up) up = go.AddComponent<TurretRankUpgrader>();
        // 객체 초기화: 생성된 객체의 컴포넌트에 필요한 데이터를 전달하여 초기 상태를 설정
        up.Initialize(rankPrefabs, (TurretRankUpgrader.Rank)rankIndex, parent);

        // 생성 후 검증: 생성된 프리팹에 필수 컴포넌트들이 제대로 포함되어 있는지 확인하고, 누락 시 경고를 출력하여 디버깅을 도움
        var turret = go.GetComponentInChildren<Turret>();
        if (!turret) Debug.LogWarning("[TurretSpawnerUI] Turret 컴포넌트를 찾지 못했습니다. 프리팹 구성을 확인하세요.", go);

        var stats = go.GetComponentInChildren<ITurretStats>();
        if (stats == null) Debug.LogWarning("[TurretSpawnerUI] ITurretStats(TurretStatsProvider/Scriptable) 누락.", go);

        var weapon = go.GetComponentInChildren<IWeapon>();
        if (weapon == null) Debug.LogWarning("[TurretSpawnerUI] IWeapon(Projectile/Hitscan) 누락.", go);
    }

    // C# 전처리기 지시문: '#if UNITY_EDITOR'와 '#endif' 사이의 코드는 Unity 에디터에서만 컴파일되며, 실제 빌드에서는 제외됨
#if UNITY_EDITOR
    // Unity 에디터 확장 (컨텍스트 메뉴): '[ContextMenu]' 어트리뷰트를 사용하여 인스펙터 창의 컨텍스트 메뉴에 테스트용 함수를 추가
    [ContextMenu("Spawn Normal (Editor Test)")]
    private void SpawnNormal_EditorTest() => SpawnByRank(0);
#endif

    // Unity 에디터 콜백: 'OnValidate'는 에디터에서 스크립트가 로드되거나 인스펙터 값이 변경될 때마다 호출됨
    private void OnValidate()
    {
        // 실시간 유효성 검사: 게임을 실행하지 않고도 인스펙터에서 값을 변경하는 즉시 필수 항목이 누락되었는지 검사하여 경고를 표시
        if (rankPrefabs != null && rankPrefabs.Length > 0 && rankPrefabs[0] == null)
            Debug.LogWarning("[TurretSpawnerUI] rankPrefabs[0](Normal)이 비어있습니다.", this);
        if (!spawnPoint)
            Debug.LogWarning("[TurretSpawnerUI] spawnPoint가 비어있습니다.", this);
    }
}