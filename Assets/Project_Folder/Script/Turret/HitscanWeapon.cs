using UnityEngine;
using System.Collections;

public class HitscanWeapon : MonoBehaviour, IWeapon
{
    [Header("Hitscan")]
    [SerializeField, Min(0f)] private float maxDistanceOverride = 0f;
    [SerializeField] private LayerMask hitMask; // 별도 마스크 쓰고 싶을 때

    [Header("Visual (Optional)")]
    [SerializeField] private LineRenderer lineRenderer;     // 레이저 궤적
    [SerializeField, Min(0f)] private float laserShowTime = 0.05f;
    [SerializeField] private bool drawLaser = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlash;        // 머즐에 고정된 파티클 (Play 방식)
    [SerializeField] private ParticleSystem muzzleFlashPrefab;  // 없으면 프리팹으로 생성
    [SerializeField] private GameObject impactVfxPrefab;        // 히트 지점 VFX
    [SerializeField] private Light muzzleLight;                 // 잠깐 번쩍 효과
    [SerializeField, Min(0f)] private float lightFlashTime = 0.03f;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioSource audioSource;           // 포탑에 달린 오디오 소스
    [SerializeField] private AudioClip fireClip;

    // 내부 버퍼(성능)
    private static readonly RaycastHit[] _hits = new RaycastHit[8];

    public void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance)
    {
        if (!muzzle) return;

        // === 발사 시각/소리 즉시 재생 ===
        PlayMuzzleVFX(muzzle);
        PlayFireSfx(muzzle.position);

        Vector3 origin = muzzle.position;
        Vector3 dir = muzzle.forward;
        float dist = (maxDistanceOverride > 0f) ? maxDistanceOverride : Mathf.Max(0f, maxDistance);

        int cnt = Physics.RaycastNonAlloc(origin, dir, _hits, dist, enemyMask, QueryTriggerInteraction.Ignore);

        Vector3 endPoint = origin + dir * dist;

        if (cnt > 0)
        {
            // 가장 가까운 한 개만 쓰는 기본형
            var hit = Closest(_hits, cnt);
            endPoint = hit.point;

            // 데미지 처리 (자식 콜라이더 대응)
            IEnemy enemy = hit.collider.GetComponent<IEnemy>() ?? hit.collider.GetComponentInParent<IEnemy>();
            if (enemy != null && !enemy.IsDead)
                enemy.TakeDamage(damage);

            // 임팩트 VFX
            if (impactVfxPrefab)
            {
                var rot = Quaternion.LookRotation(hit.normal);
                var go = Instantiate(impactVfxPrefab, hit.point, rot);
                var ps = go.GetComponent<ParticleSystem>();
                Destroy(go, ps ? ps.main.duration + ps.main.startLifetime.constantMax : 2f);
            }
        }

        // 레이저 궤적
        if (drawLaser)
        {
            // 머즐에 라인렌더러가 달려있으면 그걸 우선 사용
            var lr = lineRenderer ? lineRenderer : muzzle.GetComponent<LineRenderer>();
            if (lr)
            {
                lr.enabled = true;
                lr.positionCount = 2;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, endPoint);
                // 머즐마다 개별 끄기
                muzzle.GetComponent<MonoBehaviour>()?.StartCoroutine(DisableLineAfter(lr, laserShowTime));
            }
        }
    }

    private static RaycastHit Closest(RaycastHit[] arr, int count)
    {
        int idx = 0;
        float md = arr[0].distance;
        for (int i = 1; i < count; i++)
        {
            if (arr[i].distance < md) { md = arr[i].distance; idx = i; }
        }
        return arr[idx];
    }

    private void PlayMuzzleVFX(Transform muzzle)
    {
        // 고정형 파티클을 우선 사용
        if (muzzleFlash)
        {
            muzzleFlash.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            muzzleFlash.Play(true);
        }
        else if (muzzleFlashPrefab)
        {
            // 프리팹 인스턴스 생성 (부모를 머즐로 두면 포탑과 함께 움직임)
            var ps = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);
            ps.Play(true);
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        if (muzzleLight) StartCoroutine(FlashLight());
    }

    private IEnumerator FlashLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(lightFlashTime);
        if (muzzleLight) muzzleLight.enabled = false;
    }

    private IEnumerator DisableLineAfter(LineRenderer lr, float t)
    {
        yield return new WaitForSeconds(t);
        if (lr) lr.enabled = false;
    }

    private void PlayFireSfx(Vector3 pos)
    {
        if (audioSource && fireClip) audioSource.PlayOneShot(fireClip);
        else if (fireClip) AudioSource.PlayClipAtPoint(fireClip, pos);
    }
}
