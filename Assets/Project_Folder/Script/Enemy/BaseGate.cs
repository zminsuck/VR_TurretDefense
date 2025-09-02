using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BaseGate : MonoBehaviour
{
    [SerializeField] private int damagePerEnemy = 1;
    [Header("Fallback Checks (�ɼ�)")]
    [SerializeField] private bool useTagCheck = true;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private LayerMask enemyLayerMask = ~0;

    void Reset()
    {
        // Ʈ����/�����ٵ� �ڵ� ����
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // �������̽��� �� ã��(�ڽ� �ݶ��̴� ���)
        var enemy = other.GetComponentInParent<IEnemy>();

        //�� ã���� �±�/���̾�� ���� ����
        bool passed = enemy != null;
        if (!passed && useTagCheck) passed |= other.CompareTag(enemyTag) || other.transform.root.CompareTag(enemyTag);
        if (!passed) passed |= ((1 << other.gameObject.layer) & enemyLayerMask.value) != 0;

        if (!passed) return;

        GameManager.I?.TakeDamage(damagePerEnemy);

        // �ı� ���(��Ʈ �������� ����)
        GameObject toDestroy =
            enemy != null ? enemy.Transform.gameObject :
            other.attachedRigidbody ? other.attachedRigidbody.gameObject :
            other.transform.root.gameObject;

        Destroy(toDestroy);
    }
}
