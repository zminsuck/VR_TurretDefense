// EnemySpawner.cs (기존 파일에 "Round Scaling" 블록과 Apply 추가)

using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs (여러 개)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Options")]
    [SerializeField] private Transform goal;
    [SerializeField] private float spawnRadius = 0f;
    [SerializeField] private bool randomYRotation = true;

    [Header("Tag/Layer 강제(옵션)")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string enemyLayerName = "Enemy";

    // ✅ 라운드 스케일링 옵션
    [Header("Round Scaling")]
    [SerializeField, Min(1)] private int baseHP = 10;   // 1라운드 HP
    [SerializeField, Min(0)] private int hpAddPerRound = 3;    // 라운드당 HP 가산
    [SerializeField, Min(1f)] private float hpMultPerRound = 1.08f;// 라운드당 HP 배율
    [SerializeField, Min(0f)] private float baseSpeed = 2.0f; // 1라운드 속도
    [SerializeField, Min(0f)] private float speedAddPerRound = 0.1f; // 라운드당 속도 가산
    [SerializeField, Min(1f)] private float speedMultPerRound = 1.02f;// 라운드당 속도 배율
    [SerializeField, Min(0f)] private float maxSpeedCap = 6f;   // 속도 상한(0=무제한)

    public bool IsSpawning { get; private set; }

    int enemyLayer = -1;
    void Awake()
    {
        if (!string.IsNullOrEmpty(enemyLayerName))
            enemyLayer = LayerMask.NameToLayer(enemyLayerName);
    }

    public IEnumerator SpawnWave(int count, float interval)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) yield break;

        IsSpawning = true;
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
            if (i < count - 1) yield return new WaitForSeconds(interval);
        }
        IsSpawning = false;
    }

    public void CancelSpawns()
    {
        StopAllCoroutines();
        IsSpawning = false;
    }

    public void SpawnOne()
    {
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (!prefab) return;

        Vector3 pos = transform.position;
        if (spawnRadius > 0f)
        {
            Vector2 c = Random.insideUnitCircle * spawnRadius;
            pos += new Vector3(c.x, 0f, c.y);
        }
        Quaternion rot = randomYRotation ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : transform.rotation;

        var go = Instantiate(prefab, pos, rot);

        // 목표 지정(직진형 EnemyWalker 사용 시)
        var walker = go.GetComponent<EnemyWalker>();
        if (walker && goal) walker.SetTarget(goal);

        // 태그/레이어 통일
        if (!string.IsNullOrEmpty(enemyTag)) go.tag = enemyTag;
        if (enemyLayer >= 0) SetLayerRecursively(go, enemyLayer);

        // ✅ 라운드 스케일링 적용
        ApplyRoundScaling(go);
    }

    void ApplyRoundScaling(GameObject enemy)
    {
        // 현재 라운드(없으면 1로 간주)
        int round = 1;
        if (GameManager.I) round = Mathf.Max(1, GameManager.I.CurrentRound);

        // HP 계산: 기본 + 가산 + 배율(복합 성장)
        // ex) round=1 → baseHP
        //     round>=2 → baseHP * hpMult^(round-1) + hpAdd*(round-1)
        int hp = Mathf.RoundToInt(
            baseHP * Mathf.Pow(hpMultPerRound, round - 1) +
            hpAddPerRound * (round - 1)
        );
        hp = Mathf.Max(1, hp);

        // 속도 계산
        float spd = baseSpeed * Mathf.Pow(speedMultPerRound, round - 1)
                  + speedAddPerRound * (round - 1);
        if (maxSpeedCap > 0f) spd = Mathf.Min(spd, maxSpeedCap);

        // 1) 적이 IEnemySetup을 구현하면 그 경로로 세팅(권장)
        if (enemy.TryGetComponent<IEnemySetup>(out var setup))
        {
            setup.SetupStats(hp, spd);
            return;
        }

        // 2) 아니면 이동 속도만이라도 설정(EnemyWalker에 SetSpeed 제공 가정)
        if (enemy.TryGetComponent<EnemyWalker>(out var walker))
        {
            walker.SetSpeed(spd);
        }

        // 3) 체력은 IEnemySetup 구현체가 없으면 자동으로 못 넣으므로,
        //    아래처럼 단순 구현을 붙여 쓰는 걸 권장(아래 #3 참고)
        //    Debug.LogWarning($"[{enemy.name}] IEnemySetup 미구현 → HP 스케일 미적용", enemy);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform) SetLayerRecursively(t.gameObject, layer);
    }
}
