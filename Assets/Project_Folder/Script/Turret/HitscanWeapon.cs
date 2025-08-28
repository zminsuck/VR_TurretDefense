using UnityEngine;
using System.Collections;

public class HitscanWeapon : MonoBehaviour, IWeapon
{
    [Header("Hitscan")]
    [SerializeField, Min(0f)] private float maxDistanceOverride = 0f;
    [SerializeField] private LayerMask hitMask; // ���� ����ũ ���� ���� ��

    [Header("Visual (Optional)")]
    [SerializeField] private LineRenderer lineRenderer;     // ������ ����
    [SerializeField, Min(0f)] private float laserShowTime = 0.05f;
    [SerializeField] private bool drawLaser = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlash;        // ���� ������ ��ƼŬ (Play ���)
    [SerializeField] private ParticleSystem muzzleFlashPrefab;  // ������ ���������� ����
    [SerializeField] private GameObject impactVfxPrefab;        // ��Ʈ ���� VFX
    [SerializeField] private Light muzzleLight;                 // ��� ��½ ȿ��
    [SerializeField, Min(0f)] private float lightFlashTime = 0.03f;

    [Header("SFX (Optional)")]
    [SerializeField] private AudioSource audioSource;           // ��ž�� �޸� ����� �ҽ�
    [SerializeField] private AudioClip fireClip;

    // ���� ����(����)
    private static readonly RaycastHit[] _hits = new RaycastHit[8];

    public void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance)
    {
        if (!muzzle) return;

        // === �߻� �ð�/�Ҹ� ��� ��� ===
        PlayMuzzleVFX(muzzle);
        PlayFireSfx(muzzle.position);

        Vector3 origin = muzzle.position;
        Vector3 dir = muzzle.forward;
        float dist = (maxDistanceOverride > 0f) ? maxDistanceOverride : Mathf.Max(0f, maxDistance);

        int cnt = Physics.RaycastNonAlloc(origin, dir, _hits, dist, enemyMask, QueryTriggerInteraction.Ignore);

        Vector3 endPoint = origin + dir * dist;

        if (cnt > 0)
        {
            // ���� ����� �� ���� ���� �⺻��
            var hit = Closest(_hits, cnt);
            endPoint = hit.point;

            // ������ ó�� (�ڽ� �ݶ��̴� ����)
            IEnemy enemy = hit.collider.GetComponent<IEnemy>() ?? hit.collider.GetComponentInParent<IEnemy>();
            if (enemy != null && !enemy.IsDead)
                enemy.TakeDamage(damage);

            // ����Ʈ VFX
            if (impactVfxPrefab)
            {
                var rot = Quaternion.LookRotation(hit.normal);
                var go = Instantiate(impactVfxPrefab, hit.point, rot);
                var ps = go.GetComponent<ParticleSystem>();
                Destroy(go, ps ? ps.main.duration + ps.main.startLifetime.constantMax : 2f);
            }
        }

        // ������ ����
        if (drawLaser)
        {
            // ���� ���η������� �޷������� �װ� �켱 ���
            var lr = lineRenderer ? lineRenderer : muzzle.GetComponent<LineRenderer>();
            if (lr)
            {
                lr.enabled = true;
                lr.positionCount = 2;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, endPoint);
                // ���񸶴� ���� ����
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
        // ������ ��ƼŬ�� �켱 ���
        if (muzzleFlash)
        {
            muzzleFlash.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            muzzleFlash.Play(true);
        }
        else if (muzzleFlashPrefab)
        {
            // ������ �ν��Ͻ� ���� (�θ� ����� �θ� ��ž�� �Բ� ������)
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
