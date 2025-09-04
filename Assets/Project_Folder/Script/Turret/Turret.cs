using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform[] muzzles;
    [SerializeField] private LayerMask enemyMask;

    [Header("타겟팅")]
    [SerializeField] private float retargetInterval = 0.2f;

    [Header("디버그")]
    [SerializeField] private bool debugLogs = false;

    // 조준 정확도 (Dot 값). 0.94는 약 20도 이내
    private const float AIM_DOT_THRESHOLD = 0.94f;

    private ITurretStats stats;
    private IWeapon weapon;
    private Transform target;

    private float fireCooldown;
    private float retargetTimer;

    private void Awake()
    {
        stats = this.FindComponent<ITurretStats>();
        weapon = this.FindComponent<IWeapon>();
        if (!yawPivot) yawPivot = transform;

        if (stats == null) Debug.LogWarning("[Turret] ITurretStats 없음", this);
        if (weapon == null) Debug.LogWarning("[Turret] IWeapon 없음", this);

        if (retargetInterval < 0.02f) retargetInterval = 0.02f;
    }

    private void Update()
    {
        if (stats == null || weapon == null) return;

        RetargetTick();
        TrackTarget();
        AttackTick();
    }

    private void RetargetTick()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer > 0f) return;
        retargetTimer = retargetInterval;

        target = FindClosestTargetInRange(stats.Range);
        if (debugLogs && target) Debug.Log($"[Turret] Target: {target.name}", this);
    }

    private Transform FindClosestTargetInRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyMask);
        float bestDistSqr = float.MaxValue;
        Transform bestTf = null;

        foreach (var h in hits)
        {
            if (!h || !h.gameObject.activeInHierarchy) continue;

            // 확장 메서드 사용
            IEnemy enemy = h.FindComponent<IEnemy>();
            if (enemy == null || enemy.IsDead) continue;

            float d2 = (enemy.Transform.position - transform.position).sqrMagnitude;
            if (d2 < bestDistSqr)
            {
                bestDistSqr = d2;
                bestTf = enemy.Transform;
            }
        }
        return bestTf;
    }

    private void TrackTarget()
    {
        if (!target || !yawPivot) return;

        Vector3 dir = target.position - yawPivot.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 1e-4f) return;

        Quaternion to = Quaternion.LookRotation(dir);
        yawPivot.rotation = Quaternion.Slerp(yawPivot.rotation, to, stats.TurnSpeed * Time.deltaTime);
    }

    private void AttackTick()
    {
        if (!target) return;
        if (muzzles == null || muzzles.Length == 0) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f) return;

        Transform refMuzzle = FirstMuzzle();
        if (!refMuzzle) return;

        Vector3 toTarget = (target.position - refMuzzle.position).normalized;
        float aimDot = Vector3.Dot(refMuzzle.forward, toTarget);

        // 상수를 사용하여 의미 명확화
        if (aimDot < AIM_DOT_THRESHOLD) return;

        fireCooldown = Mathf.Max(0.01f, stats.CooldownSeconds);

        foreach (var m in muzzles)
        {
            if (!m) continue;
            weapon.Fire(m, target, stats.Damage, enemyMask, stats.Range);
        }

        if (debugLogs)
            Debug.Log($"[Turret] Fire x{muzzles.Length}! dmg={stats.Damage}, dot={aimDot:0.00}", this);
    }

    private Transform FirstMuzzle()
    {
        if (muzzles == null) return null;
        foreach (var m in muzzles)
            if (m) return m;
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        ITurretStats s = stats ?? this.FindComponent<ITurretStats>();
        float r = (s != null) ? s.Range : 0f;
        if (r <= 0f) return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, r);
    }
}