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
        // 트리거/리짓바디 자동 세팅
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 인터페이스로 적 찾기(자식 콜라이더 대비)
        var enemy = other.GetComponentInParent<IEnemy>();

        //못 찾으면 태그/레이어로 보조 판정
        bool passed = enemy != null;
        if (!passed && useTagCheck) passed |= other.CompareTag(enemyTag) || other.transform.root.CompareTag(enemyTag);
        if (!passed) passed |= ((1 << other.gameObject.layer) & enemyLayerMask.value) != 0;

        if (!passed) return;

        GameManager.I?.TakeDamage(damagePerEnemy);

        // 파괴 대상(루트 기준으로 정리)
        GameObject toDestroy =
            enemy != null ? enemy.Transform.gameObject :
            other.attachedRigidbody ? other.attachedRigidbody.gameObject :
            other.transform.root.gameObject;

        Destroy(toDestroy);
    }
}
