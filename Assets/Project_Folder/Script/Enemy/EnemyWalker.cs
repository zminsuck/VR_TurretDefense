using UnityEngine;

public class EnemyWalker : MonoBehaviour
{
    [Header("목표 지점(끝점)")]
    [SerializeField] private Transform target;          // 종착점 Transform

    [Header("이동")]
    [SerializeField, Min(0.1f)] private float speed = 2f;
    [SerializeField, Min(0.01f)] private float arriveDistance = 0.1f;
    [SerializeField] private bool lockY = true;         // 높이 고정해서 수평 이동
    [SerializeField] private bool lookForward = true;   // 진행 방향 바라보기
    [SerializeField, Range(0f, 20f)] private float lookLerp = 10f;

    [Header("도착 처리")]
    [Tooltip("도착 시 즉시 자기 자신을 파괴합니다. BaseGate 트리거로 HP를 깎을 거면 꺼두세요.")]
    [SerializeField] private bool destroyOnArrive = false;

    [Header("안전장치")]
    [SerializeField, Min(1f)] private float maxLifeTime = 120f; // 혹시 못 도달 시 자동 제거

    private float life;

    // 외부 제어용
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

        // 도착 처리
        if (dist <= arriveDistance)
        {
            if (destroyOnArrive)
            {
                Destroy(gameObject);
            }
            // destroyOnArrive가 false면 여기서 멈춰 서서 Gate 트리거에 맡김
            return;
        }

        // 이동
        if (dist > 0f && speed > 0f)
        {
            Vector3 dir = to / dist;
            float step = speed * Time.deltaTime;
            if (step > dist) step = dist;
            transform.position = pos + dir * step;

            // 진행 방향으로 회전
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

        // 안전 제거
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
