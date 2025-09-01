using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    [Header("Spawners")]
    [SerializeField] private EnemySpawner[] spawners;

    [Header("Round Flow")]
    [SerializeField] private bool autoStart = false;         // 버튼으로 시작할 거면 기본 false 권장
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
    [SerializeField] private bool skipFirstIntermission = true; // 첫 대기 생략하고 바로 시작할지

    [System.Serializable] public class IntEvent : UnityEvent<int> { }
    public IntEvent onRoundStarted;
    public IntEvent onRoundEnded;

    public int CurrentRound { get; private set; }

    private Coroutine loopRoutine;
    public bool IsRunning => loopRoutine != null;

    void Start()
    {
        if (spawners == null || spawners.Length == 0)
            spawners = Object.FindObjectsByType<EnemySpawner>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (autoStart) StartGame();
    }

    // === UI 버튼에서 호출 ===
    public void StartGame()
    {
        if (IsRunning) return;
        loopRoutine = StartCoroutine(skipFirstIntermission ? RoundLoopImmediate() : RoundLoop());
    }

    public IEnumerator RoundLoop()
    {
        CurrentRound = 0;
        yield return new WaitForSeconds(intermission);

        while (true)
        {
            StartNextRound();

            // 모든 스포너가 끝났고, 살아있는 적이 0이 될 때까지 대기
            while (AnySpawnerSpawning() || EnemyAliveCount() > 0)
                yield return null;

            onRoundEnded?.Invoke(CurrentRound);
            yield return new WaitForSeconds(intermission);
        }
    }

    // 첫 대기 없이 바로 시작하는 버전
    private IEnumerator RoundLoopImmediate()
    {
        CurrentRound = 0;
        yield return null; // 한 프레임 양보
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

        int total = baseCount + addPerRound * (CurrentRound - 1);
        float interval = baseInterval * Mathf.Pow(intervalMult, CurrentRound - 1);

        int n = Mathf.Max(1, spawners.Length);
        int each = total / n;
        int rem = total - each * n;

        for (int i = 0; i < spawners.Length; i++)
        {
            var s = spawners[i];
            if (!s) continue;
            int count = each + (i < rem ? 1 : 0);
            if (count > 0) StartCoroutine(s.SpawnWave(count, interval));
        }

        onRoundStarted?.Invoke(CurrentRound);
    }

    bool AnySpawnerSpawning()
    {
        foreach (var s in spawners) if (s && s.IsSpawning) return true;
        return false;
    }

    int EnemyAliveCount()
    {
        // Tag가 지정돼 있으면 가장 빠름
        if (!string.IsNullOrEmpty(enemyTag))
            return GameObject.FindGameObjectsWithTag(enemyTag).Length;

        // 레이어 기준으로 카운트
        int layer = LayerMask.NameToLayer(enemyLayerName);
        var gos = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        int count = 0;
        foreach (var go in gos) if (go.layer == layer) count++;
        return count;
    }

    void RankUpAllTurrets()
    {
        var ups = Object.FindObjectsByType<TurretRankUpgrader>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var up in ups) if (up) up.RankUp();
    }
}
