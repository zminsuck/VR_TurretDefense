// C# 기본: Unity Engine 및 이벤트 시스템을 사용하기 위한 네임스페이스 선언
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// 객체 지향 프로그래밍(OOP): MonoBehaviour를 상속받아 Unity 컴포넌트 시스템에 통합
public class RoundManager : MonoBehaviour
{
    // Unity 에디터 기술: [Header] 어트리뷰트로 인스펙터 창 UI를 그룹화
    [Header("Spawners")]
    // Unity 에디터 기술: [SerializeField] 어트리뷰트로 private 변수를 인스펙터에 노출 (직렬화)
    [SerializeField] private EnemySpawner[] spawners;

    [Header("Round Flow")]
    [SerializeField] private bool autoStart = false;
    // Unity 에디터 기술: [Min] 어트리뷰트로 인스펙터에서 설정 가능한 최솟값을 제한
    [SerializeField, Min(0f)] private float intermission = 5f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseCount = 6;
    [SerializeField] private int addPerRound = 3;
    [SerializeField] private float baseInterval = 1.4f;
    [SerializeField] private float intervalMult = 0.95f;

    [Header("Turret Rank Up")]
    [SerializeField] private bool rankUpAtRoundStart = true;

    [Header("Detect Enemies")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("UI Start 옵션")]
    [SerializeField] private bool skipFirstIntermission = true;

    // Unity 이벤트 시스템: [System.Serializable]과 UnityEvent<T>를 조합하여 인스펙터에서 이벤트를 편집하고 다른 객체와 느슨한 결합(Loose Coupling)을 구현
    [System.Serializable] public class IntEvent : UnityEvent<int> { }
    public IntEvent onRoundStarted;
    public IntEvent onRoundEnded;

    // 객체 지향 프로그래밍(OOP) - 캡슐화(Encapsulation): get은 public, set은 private으로 설정하여 외부에서는 읽기만 가능하고 수정은 클래스 내부에서만 가능하도록 제한
    public int CurrentRound { get; private set; }

    private Coroutine loopRoutine;
    // C# 식 본문 멤버(Expression-Bodied Member): 'IsRunning' 속성을 간결하게 정의. 코루틴이 실행 중인지 여부를 반환
    public bool IsRunning => loopRoutine != null;

    // Unity 생명주기 메서드: 게임 시작 시 한 번 호출됨 (주로 초기화에 사용)
    void Start()
    {
        // 방어적 프로그래밍: spawner가 인스펙터에서 할당되지 않았을 경우, 씬에서 자동으로 찾아 할당
        if (spawners == null || spawners.Length == 0)
            spawners = Object.FindObjectsByType<EnemySpawner>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (autoStart) StartGame();
    }

    // UI 연동: UI 버튼 등에서 호출할 수 있도록 public 메서드로 API 제공
    public void StartGame()
    {
        if (IsRunning) return;
        // 비동기 프로그래밍(코루틴): 'StartCoroutine'을 사용하여 게임의 메인 스레드를 차단하지 않고 시간의 흐름에 따른 로직을 실행
        // C# 삼항 연산자: 'skipFirstIntermission' 값에 따라 다른 코루틴을 조건부로 실행
        loopRoutine = StartCoroutine(skipFirstIntermission ? RoundLoopImmediate() : RoundLoop());
    }

    // 비동기 프로그래밍(코루틴): 'IEnumerator'를 반환 타입으로 지정하여 코루틴으로 동작
    public IEnumerator RoundLoop()
    {
        CurrentRound = 0;
        // 코루틴 제어: 'yield return'을 사용하여 코드 실행을 잠시 멈추고 지정된 시간(intermission) 후에 다시 시작
        yield return new WaitForSeconds(intermission);

        // 게임 루프: 'while(true)'와 'yield'를 결합하여 게임이 끝날 때까지 라운드를 무한히 반복하는 메인 루프 구현
        while (true)
        {
            StartNextRound();

            // 코루틴 제어: 특정 조건(스폰 종료 및 적 전멸)이 충족될 때까지 매 프레임 대기
            while (AnySpawnerSpawning() || EnemyAliveCount() > 0)
                yield return null; // 'yield return null'은 다음 프레임까지 실행을 대기

            // C# null 조건부 연산자(?.): 이벤트 리스너가 없는 경우(null)에도 오류 없이 안전하게 이벤트를 호출
            onRoundEnded?.Invoke(CurrentRound);
            yield return new WaitForSeconds(intermission);
        }
    }

    private IEnumerator RoundLoopImmediate()
    {
        CurrentRound = 0;
        yield return null; // 한 프레임 대기하여 다른 스크립트들이 초기화될 시간을 줌
        while (true)
        {
            StartNextRound();

            while (AnySpawnerSpawning() || EnemyAliveCount() > 0)
                yield return null;

            onRoundEnded?.Invoke(CurrentRound);
            yield return new WaitForSeconds(intermission);
        }
    }

    public void StartNextRound()
    {
        CurrentRound++;

        if (rankUpAtRoundStart) RankUpAllTurrets();

        // 게임 밸런싱 로직: 라운드가 진행됨에 따라 적의 수와 스폰 간격을 동적으로 계산
        int total = baseCount + addPerRound * (CurrentRound - 1);
        float interval = baseInterval * Mathf.Pow(intervalMult, CurrentRound - 1);

        int n = Mathf.Max(1, spawners.Length);
        // 분배 알고리즘: 생성할 총 적의 수를 모든 스포너에게 최대한 균등하게 분배
        int each = total / n;
        int rem = total - each * n; // 나머지

        for (int i = 0; i < spawners.Length; i++)
        {
            var s = spawners[i];
            if (!s) continue;
            // 나머지(rem)를 앞 순서의 스포너부터 하나씩 더해줌
            int count = each + (i < rem ? 1 : 0);
            if (count > 0) StartCoroutine(s.SpawnWave(count, interval));
        }

        onRoundStarted?.Invoke(CurrentRound);
    }

    // 헬퍼 메서드: 스포너 중 하나라도 스폰 중인지 여부를 확인하는 보조 함수
    bool AnySpawnerSpawning()
    {
        foreach (var s in spawners) if (s && s.IsSpawning) return true;
        return false;
    }

    // 성능 최적화 고려: 적을 찾는 여러 방법 중 성능이 더 빠른 태그(Tag) 검색을 우선적으로 사용
    int EnemyAliveCount()
    {
        if (!string.IsNullOrEmpty(enemyTag))
            return GameObject.FindGameObjectsWithTag(enemyTag).Length;

        // 대체 로직: 태그가 없을 경우, 씬의 모든 게임오브젝트를 순회하며 레이어(Layer)를 비교 (성능 부하가 더 큼)
        int layer = LayerMask.NameToLayer(enemyLayerName);
        var gos = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        int count = 0;
        foreach (var go in gos) if (go.layer == layer) count++;
        return count;
    }

    // 씬 전체 객체와 상호작용: 'FindObjectsByType'을 사용해 씬에 있는 모든 'TurretRankUpgrader'를 찾아 한 번에 제어
    void RankUpAllTurrets()
    {
        var ups = Object.FindObjectsByType<TurretRankUpgrader>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var up in ups) if (up) up.RankUp();
    }
}