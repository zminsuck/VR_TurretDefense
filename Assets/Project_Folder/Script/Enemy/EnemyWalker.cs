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

    [Header("안전장치")]
    [SerializeField, Min(1f)] private float maxLifeTime = 120f; // 혹시 못 도달 시 자동 제거

    private float life;

    // 스포너에서 호출할 수 있게 공개 메서드도 제공
    public void SetTarget(Transform t) => target = t;

    private void Update()
    {
        if (!target) { enabled = false; return; }

        Vector3 pos = transform.position;
        Vector3 dest = target.position;
        if (lockY) dest.y = pos.y;

        Vector3 to = dest - pos;
        float dist = to.magnitude;

        // 도착하면 제거
        if (dist <= arriveDistance) { Destroy(gameObject); return; }

        // 이동
        Vector3 dir = to / dist;
        float step = speed * Time.deltaTime;
        if (step > dist) step = dist;
        transform.position = pos + dir * step;

        // 진행 방향으로 회전
        if (lookForward)
        {
            Vector3 face = dir; if (lockY) face.y = 0f;
            if (face.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face), 10f * Time.deltaTime);
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
