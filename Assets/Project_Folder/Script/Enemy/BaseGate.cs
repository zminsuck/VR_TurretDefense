using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BaseGate : MonoBehaviour
{
    [SerializeField] private int damagePerEnemy = 1;
    [Header("Fallback Checks (옵션)")]
    [SerializeField] private bool useTagCheck = true;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private LayerMask enemyLayerMask = ~0;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 확장 메서드 사용
        var enemy = other.FindComponent<IEnemy>();

        bool passed = enemy != null;
        if (!passed && useTagCheck)
        {
            passed |= other.CompareTag(enemyTag);
        }
        if (!passed)
        {
            passed |= (enemyLayerMask.value & (1 << other.gameObject.layer)) > 0;
        }

        if (!passed) return;

        GameManager.I?.TakeDamage(damagePerEnemy);

        GameObject toDestroy = enemy != null ? enemy.Transform.gameObject : other.attachedRigidbody.gameObject;
        Destroy(toDestroy);
    }
}