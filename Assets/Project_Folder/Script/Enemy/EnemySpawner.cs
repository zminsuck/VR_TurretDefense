using System.Collections;
using UnityEngine;

// 스탯 계산 로직을 별도 클래스로 분리하여 가독성 향상
public static class StatCalculator
{
    public static int GetScaledHP(int round, int baseHp, int addPerRound, float multPerRound)
    {
        if (round <= 1) return baseHp;
        int hp = Mathf.RoundToInt(
            baseHp * Mathf.Pow(multPerRound, round - 1) +
            addPerRound * (round - 1)
        );
        return Mathf.Max(1, hp);
    }

    public static float GetScaledSpeed(int round, float baseSpeed, float addPerRound, float multPerRound, float maxSpeed)
    {
        if (round <= 1) return baseSpeed;
        float spd = baseSpeed * Mathf.Pow(multPerRound, round - 1) + addPerRound * (round - 1);
        return (maxSpeed > 0f) ? Mathf.Min(spd, maxSpeed) : spd;
    }
}


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

    [Header("Round Scaling")]
    [SerializeField, Min(1)] private int baseHP = 10;
    [SerializeField, Min(0)] private int hpAddPerRound = 3;
    [SerializeField, Min(1f)] private float hpMultPerRound = 1.08f;
    [SerializeField, Min(0f)] private float baseSpeed = 2.0f;
    [SerializeField, Min(0f)] private float speedAddPerRound = 0.1f;
    [SerializeField, Min(1f)] private float speedMultPerRound = 1.02f;
    [SerializeField, Min(0f)] private float maxSpeedCap = 6f;

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

        var walker = go.GetComponent<EnemyWalker>();
        if (walker && goal) walker.SetTarget(goal);

        if (!string.IsNullOrEmpty(enemyTag)) go.tag = enemyTag;
        if (enemyLayer >= 0) SetLayerRecursively(go, enemyLayer);

        ApplyRoundScaling(go);
    }

    void ApplyRoundScaling(GameObject enemy)
    {
        int round = (GameManager.I != null) ? Mathf.Max(1, GameManager.I.CurrentRound) : 1;

        // 분리된 계산기 사용
        int hp = StatCalculator.GetScaledHP(round, baseHP, hpAddPerRound, hpMultPerRound);
        float spd = StatCalculator.GetScaledSpeed(round, baseSpeed, speedAddPerRound, speedMultPerRound, maxSpeedCap);

        if (enemy.TryGetComponent<IEnemySetup>(out var setup))
        {
            setup.SetupStats(hp, spd);
            return;
        }

        if (enemy.TryGetComponent<EnemyWalker>(out var walker))
        {
            walker.SetSpeed(spd);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}