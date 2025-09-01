// C# 기본: UnityEngine 라이브러리의 클래스와 함수를 사용하기 위한 네임스페이스 선언
using UnityEngine;

// 객체 지향 프로그래밍(OOP): MonoBehaviour를 상속받아 Unity의 컴포넌트 시스템에 통합
public class Turret : MonoBehaviour
{
    // Unity 에디터 기술: [Header] 어트리뷰트로 인스펙터 창 UI를 그룹화
    [Header("참조")]
    // Unity 에디터 기술: [SerializeField] 어트리뷰트로 private 변수를 인스펙터에 노출시켜 데이터와 로직을 분리 (직렬화)
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform[] muzzles;
    [SerializeField] private LayerMask enemyMask;

    [Header("타겟팅")]
    [SerializeField] private float retargetInterval = 0.2f;

    [Header("디버그")]
    [SerializeField] private bool debugLogs = false;

    // 인터페이스 기반 프로그래밍: 구체적인 클래스가 아닌 인터페이스(ITurretStats, IWeapon)에 의존하여 유연하고 확장 가능한 구조 설계
    private ITurretStats stats;
    private IWeapon weapon;
    private Transform target;

    private float fireCooldown;
    private float retargetTimer;

    // Unity 생명주기 메서드: 스크립트 인스턴스가 로드될 때 'Start' 이전, 한 번만 호출됨 (주로 초기화에 사용)
    private void Awake()
    {
        // C# null 병합 연산자(??): GetComponent<T>()가 null일 경우, GetComponentInChildren<T>()를 실행하여 컴포넌트를 유연하게 탐색
        stats = GetComponent<ITurretStats>() ?? GetComponentInChildren<ITurretStats>();
        weapon = GetComponent<IWeapon>() ?? GetComponentInChildren<IWeapon>();
        // 방어적 프로그래밍: yawPivot이 할당되지 않았을 경우 기본값(자기 자신)을 할당하여 오류 방지
        if (!yawPivot) yawPivot = transform;

        // 디버깅 기술: 필수 컴포넌트가 없을 경우 경고 메시지를 출력하여 개발자에게 피드백
        if (stats == null) Debug.LogWarning("[Turret] ITurretStats 없음 — Stats Provider/Component 붙이세요.", this);
        if (weapon == null) Debug.LogWarning("[Turret] IWeapon 없음 — HitscanWeapon 붙이세요.", this);

        if (retargetInterval < 0.02f) retargetInterval = 0.02f;
    }

    // Unity 생명주기 메서드: 매 프레임마다 호출되어 게임의 주요 로직을 처리
    private void Update()
    {
        if (stats == null || weapon == null) return;

        // 코드 구조화: Update 로직을 기능별(타겟 탐색, 추적, 공격)로 분리된 헬퍼 메서드로 구성하여 가독성 향상
        RetargetTick();
        TrackTarget();
        AttackTick();
    }

    // 성능 최적화: 매 프레임이 아닌 'retargetInterval'마다 타겟 탐색을 실행하여 불필요한 연산을 줄임
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
        // Unity 물리 시스템: 'Physics.OverlapSphere'를 사용하여 지정된 반경 내의 모든 콜라이더를 감지
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyMask);
        float best = float.MaxValue;
        Transform bestTf = null;

        foreach (var h in hits)
        {
            if (!h || !h.gameObject.activeInHierarchy) continue;

            IEnemy enemy = h.GetComponent<IEnemy>() ?? h.GetComponentInParent<IEnemy>();
            if (enemy == null || enemy.IsDead) continue;

            // 성능 최적화: 제곱근 계산(magnitude)보다 연산 비용이 저렴한 'sqrMagnitude'를 사용하여 거리를 비교
            float d2 = (enemy.Transform.position - transform.position).sqrMagnitude;
            if (d2 < best) { best = d2; bestTf = enemy.Transform; }
        }
        return bestTf;
    }

    private void TrackTarget()
    {
        if (!target || !yawPivot) return;

        Vector3 dir = target.position - yawPivot.position;
        // 3D 벡터 수학: 방향 벡터의 y값을 0으로 만들어 수평(XZ 평면) 회전만 하도록 제한
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        // 3D 회전 수학(쿼터니언): 'Quaternion.LookRotation'으로 목표 방향을 바라보는 회전값 생성
        Quaternion to = Quaternion.LookRotation(dir);
        // 3D 회전 수학(쿼터니언): 'Quaternion.Slerp'를 사용하여 현재 각도에서 목표 각도까지 부드럽게 회전 (보간)
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
        // 3D 벡터 수학: '내적(Dot Product)'을 사용하여 포신 방향과 타겟 방향 사이의 각도를 계산 (조준 정확도 체크)
        float aimDot = Vector3.Dot(refMuzzle.forward, toTarget);
        // 내적 결과값이 0.5 (cos 60도) 미만이면 발사하지 않음
        if (aimDot < 0.5f) return;

        fireCooldown = Mathf.Max(0.0001f, stats.CooldownSeconds);

        for (int i = 0; i < muzzles.Length; i++)
        {
            var m = muzzles[i];
            if (!m) continue;
            // 객체 지향 프로그래밍(OOP) - 위임(Delegation): 실제 발사 로직은 'IWeapon' 컴포넌트에 맡김
            weapon.Fire(m, target, stats.Damage, enemyMask, stats.Range);
        }

        if (debugLogs)
            Debug.Log($"[Turret] Fire x{muzzles.Length}! dmg={stats.Damage}, range={stats.Range}, dot={aimDot:0.00}", this);
    }

    // 코드 재사용: 여러 곳에서 사용될 수 있는 첫 번째 유효한 머즐을 찾는 로직을 헬퍼 메서드로 분리
    private Transform FirstMuzzle()
    {
        if (muzzles == null) return null;
        for (int i = 0; i < muzzles.Length; i++)
            if (muzzles[i]) return muzzles[i];
        return null;
    }

    // Unity 에디터 확장: 'OnDrawGizmosSelected'는 씬 뷰에서만 실행되는 특수 메서드로, 시각적 디버깅에 사용
    private void OnDrawGizmosSelected()
    {
        ITurretStats s = stats ?? GetComponentInChildren<ITurretStats>() ?? GetComponent<ITurretStats>();
        float r = (s != null) ? s.Range : 0f;
        if (r <= 0f) return;

        // 기즈모(Gizmos) API: 'Gizmos.DrawWireSphere'를 사용해 터렛의 사정거리를 씬 뷰에 시각적으로 표시
        Gizmos.color = new Color(0f, 1f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, r);
    }
}