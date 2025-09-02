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
    [SerializeField, Range(0f, 20f)] private float lookLerp = 10f;

    [Header("���� ó��")]
    [Tooltip("���� �� ��� �ڱ� �ڽ��� �ı��մϴ�. BaseGate Ʈ���ŷ� HP�� ���� �Ÿ� ���μ���.")]
    [SerializeField] private bool destroyOnArrive = false;

    [Header("������ġ")]
    [SerializeField, Min(1f)] private float maxLifeTime = 120f; // Ȥ�� �� ���� �� �ڵ� ����

    private float life;

    // �ܺ� �����
    public void SetTarget(Transform t) => target = t;
    public void SetSpeed(float s) => speed = Mathf.Max(0f, s);
    public void SetArriveDistance(float d) => arriveDistance = Mathf.Max(0.001f, d);
    public float CurrentSpeed => speed;

    private void OnEnable()
    {
        life = 0f;
    }

    private void Update()
    {
        if (!target) { enabled = false; return; }

        Vector3 pos = transform.position;
        Vector3 dest = target.position;
        if (lockY) dest.y = pos.y;

        Vector3 to = dest - pos;
        float dist = to.magnitude;

        // ���� ó��
        if (dist <= arriveDistance)
        {
            if (destroyOnArrive)
            {
                Destroy(gameObject);
            }
            // destroyOnArrive�� false�� ���⼭ ���� ���� Gate Ʈ���ſ� �ñ�
            return;
        }

        // �̵�
        if (dist > 0f && speed > 0f)
        {
            Vector3 dir = to / dist;
            float step = speed * Time.deltaTime;
            if (step > dist) step = dist;
            transform.position = pos + dir * step;

            // ���� �������� ȸ��
            if (lookForward)
            {
                Vector3 face = dir;
                if (lockY) face.y = 0f;
                if (face.sqrMagnitude > 1e-6f)
                {
                    var targetRot = Quaternion.LookRotation(face);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookLerp * Time.deltaTime);
                }
            }
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
