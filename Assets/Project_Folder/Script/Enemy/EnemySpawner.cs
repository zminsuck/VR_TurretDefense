using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs (여러 개)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Options")]
    [SerializeField] private Transform goal;        // EnemyWalker가 있다면 넘겨줌
    [SerializeField] private float spawnRadius = 0f;
    [SerializeField] private bool randomYRotation = true;

    [Header("Tag/Layer 강제(옵션)")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string enemyLayerName = "Enemy";

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
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform) SetLayerRecursively(t.gameObject, layer);
    }
}
