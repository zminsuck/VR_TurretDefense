using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    [Header("Spawners")]
    [SerializeField] private EnemySpawner[] spawners;

    [Header("Round Flow")]
    [SerializeField] private bool autoStart = false;
    [SerializeField, Min(0f)] private float intermission = 5f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseCount = 6;
    [SerializeField] private int addPerRound = 3;
    [SerializeField] private float baseInterval = 1.4f;
    [SerializeField] private float intervalMult = 0.95f;

    [Header("Turret Rank Up")]
    [SerializeField] private bool rankUpAtRoundStart = true;

    [Header("UI Start 옵션")]
    [SerializeField] private bool skipFirstIntermission = true;

    [System.Serializable] public class IntEvent : UnityEvent<int> { }
    public IntEvent onRoundStarted;
    public IntEvent onRoundEnded;

    public int CurrentRound { get; private set; }
    private Coroutine loopRoutine;
    public bool IsRunning => loopRoutine != null;

    void Start()
    {
        if (spawners == null || spawners.Length == 0)
            spawners = FindObjectsByType<EnemySpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (autoStart) StartGame();
    }

    public void StartGame()
    {
        if (IsRunning) return;
        loopRoutine = StartCoroutine(skipFirstIntermission ? RoundLoopImmediate() : RoundLoop());
    }

    // 성능 개선된 메인 루프
    private IEnumerator RoundLoopInternal(bool immediate)
    {
        CurrentRound = 0;
        if (!immediate)
        {
            yield return new WaitForSeconds(intermission);
        }
        else
        {
            yield return null;
        }

        while (true)
        {
            StartNextRound();

            // GameManager의 카운트를 직접 사용하여 훨씬 효율적으로 대기
            yield return new WaitUntil(() => !AnySpawnerSpawning() && GameManager.I.AliveEnemies <= 0);

            onRoundEnded?.Invoke(CurrentRound);
            yield return new WaitForSeconds(intermission);
        }
    }

    public IEnumerator RoundLoop() => RoundLoopInternal(false);
    private IEnumerator RoundLoopImmediate() => RoundLoopInternal(true);

    public void StartNextRound()
    {
        CurrentRound++;

        if (rankUpAtRoundStart) RankUpAllTurrets();

        int total = baseCount + addPerRound * (CurrentRound - 1);
        float interval = baseInterval * Mathf.Pow(intervalMult, CurrentRound - 1);

        int n = Mathf.Max(1, spawners.Length);
        int each = total / n;
        int rem = total % n;

        for (int i = 0; i < spawners.Length; i++)
        {
            var s = spawners[i];
            if (!s) continue;
            int count = each + (i < rem ? 1 : 0);
            if (count > 0) StartCoroutine(s.SpawnWave(count, interval));
        }

        onRoundStarted?.Invoke(CurrentRound);
    }

    public void StopGame(bool cancelSpawns = true)
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }

        if (cancelSpawns && spawners != null)
        {
            foreach (var s in spawners)
                if (s) s.CancelSpawns();
        }
    }

    bool AnySpawnerSpawning()
    {
        foreach (var s in spawners)
            if (s && s.IsSpawning) return true;
        return false;
    }

    void RankUpAllTurrets()
    {
        var ups = FindObjectsByType<TurretRankUpgrader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var up in ups)
            if (up) up.RankUp();
    }
}