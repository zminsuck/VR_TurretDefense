using UnityEngine;

public class EnemyWalker : MonoBehaviour
{
    [Header("��ǥ ����(����)")]
    [SerializeField] private Transform target;          // ������ Transform

    [Header("�̵�")]
    [SerializeField, Min(0.1f)] private float speed = 2f;
    [SerializeField, Min(0.01f)] private float arriveDistance = 0.1f;
    [SerializeField] private bool lockY = true;         // ���� �����ؼ� ���� �̵�
    [SerializeField] private bool lookForward = true;   // ���� ���� �ٶ󺸱�

    [Header("������ġ")]
    [SerializeField, Min(1f)] private float maxLifeTime = 120f; // Ȥ�� �� ���� �� �ڵ� ����

    private float life;

    // �����ʿ��� ȣ���� �� �ְ� ���� �޼��嵵 ����
    public void SetTarget(Transform t) => target = t;

    private void Update()
    {
        if (!target) { enabled = false; return; }

        Vector3 pos = transform.position;
        Vector3 dest = target.position;
        if (lockY) dest.y = pos.y;

        Vector3 to = dest - pos;
        float dist = to.magnitude;

        // �����ϸ� ����
        if (dist <= arriveDistance) { Destroy(gameObject); return; }

        // �̵�
        Vector3 dir = to / dist;
        float step = speed * Time.deltaTime;
        if (step > dist) step = dist;
        transform.position = pos + dir * step;

        // ���� �������� ȸ��
        if (lookForward)
        {
            Vector3 face = dir; if (lockY) face.y = 0f;
            if (face.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face), 10f * Time.deltaTime);
        }

        // ���� ����
        life += Time.deltaTime;
        if (life >= maxLifeTime) Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (!target) return;
        Gizmos.color = Color.yellow;
        Vector3 a = transform.position;
        Vector3 b = target.position;
        if (lockY) b.y = a.y;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(b, 0.12f);
    }
}
