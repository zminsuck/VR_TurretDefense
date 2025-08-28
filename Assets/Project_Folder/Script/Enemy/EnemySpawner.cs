using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.1f)] private float interval = 1.5f; // 생성 간격(초)
    [SerializeField, Min(1)] private int count = 10;   // 생성 개수
    [SerializeField] private float startDelay = 0f;            // 시작 딜레이
    [SerializeField] private float spawnRadius = 0f;           // 0이면 정확히 스포너 위치
    [SerializeField] private bool randomYRotation = true;      // Y축 랜덤 회전

    [Header("Walker Target (선택)")]
    [SerializeField] private Transform goal; // EnemyWalker가 있다면 SetTarget으로 전달

    private Coroutine co;

    private void OnEnable()
    {
        co = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (co != null) StopCoroutine(co);
    }

    private IEnumerator SpawnRoutine()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) yield break;

        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < count; i++)
        {
            SpawnOne();
            if (i < count - 1) yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnOne()
    {
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (!prefab) return;

        // 위치/회전 결정
        Vector3 pos = transform.position;
        if (spawnRadius > 0f)
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;
            pos += new Vector3(circle.x, 0f, circle.y);
        }
        Quaternion rot = randomYRotation
            ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            : transform.rotation;

        // 생성
        var go = Instantiate(prefab, pos, rot);

        // EnemyWalker가 있으면 목표 설정
        var walker = go.GetComponent<EnemyWalker>();
        if (walker && goal) walker.SetTarget(goal);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnRadius <= 0f) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
