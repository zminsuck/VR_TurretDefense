using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Transform yawPivot;       // 수평 회전부
    [SerializeField] private Transform[] muzzles;      // 여러 머즐
    [SerializeField] private LayerMask enemyMask;

    [Header("타겟팅")]
    [SerializeField] private float retargetInterval = 0.2f;

    [Header("디버그")]
    [SerializeField] private bool debugLogs = false;

    private ITurretStats stats;
    private IWeapon weapon;
    private Transform target;

    private float fireCooldown;
    private float retargetTimer;

    private void Awake()
    {
        stats = GetComponent<ITurretStats>() ?? GetComponentInChildren<ITurretStats>();
        weapon = GetComponent<IWeapon>() ?? GetComponentInChildren<IWeapon>();
        if (!yawPivot) yawPivot = transform;

        if (stats == null) Debug.LogWarning("[Turret] ITurretStats 없음 — Stats Provider/Component 붙이세요.", this);
        if (weapon == null) Debug.LogWarning("[Turret] IWeapon 없음 — HitscanWeapon 붙이세요.", this);

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
        float best = float.MaxValue;
        Transform bestTf = null;

        foreach (var h in hits)
        {
            if (!h || !h.gameObject.activeInHierarchy) continue;

            IEnemy enemy = h.GetComponent<IEnemy>() ?? h.GetComponentInParent<IEnemy>();
            if (enemy == null || enemy.IsDead) continue;

            float d2 = (enemy.Transform.position - transform.position).sqrMagnitude;
            if (d2 < best) { best = d2; bestTf = enemy.Transform; }
        }
        return bestTf;
    }

    private void TrackTarget()
    {
        if (!target || !yawPivot) return;

        Vector3 dir = target.position - yawPivot.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion to = Quaternion.LookRotation(dir);
        yawPivot.rotation = Quaternion.Slerp(yawPivot.rotation, to, stats.TurnSpeed * Time.deltaTime);
    }

    private void AttackTick()
    {
        if (!target) return;
        if (muzzles == null || muzzles.Length == 0) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f) return;

        // 참조용으로 첫 머즐을 사용해 조준각 체크
        Transform refMuzzle = FirstMuzzle();
        if (!refMuzzle) return;

        Vector3 toTarget = (target.position - refMuzzle.position).normalized;
        float aimDot = Vector3.Dot(refMuzzle.forward, toTarget);
        if (aimDot < 0.80f) return;

        fireCooldown = Mathf.Max(0.0001f, stats.CooldownSeconds);

        // 동시에 전 머즐 발사
        for (int i = 0; i < muzzles.Length; i++)
        {
            var m = muzzles[i];
            if (!m) continue;
            weapon.Fire(m, target, stats.Damage, enemyMask, stats.Range);
        }

        if (debugLogs)
            Debug.Log($"[Turret] Fire x{muzzles.Length}! dmg={stats.Damage}, range={stats.Range}, dot={aimDot:0.00}", this);
    }

    private Transform FirstMuzzle()
    {
        if (muzzles == null) return null;
        for (int i = 0; i < muzzles.Length; i++)
            if (muzzles[i]) return muzzles[i];
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        ITurretStats s = stats ?? GetComponentInChildren<ITurretStats>() ?? GetComponent<ITurretStats>();
        float r = (s != null) ? s.Range : 0f;
        if (r <= 0f) return;

        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, r);
    }
}
