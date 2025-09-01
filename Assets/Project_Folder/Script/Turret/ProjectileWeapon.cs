using UnityEngine;

public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    [Header("Projectile")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;
    [SerializeField, Range(0f, 10f)] private float spreadDegrees = 0f; // ����(����)

    [Header("VFX (optional)")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    public void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance)
    {
        if (!muzzle || !bulletPrefab) return;

        PlayMuzzle(muzzle);

        // 1) �߻� ������ ���� ����(Ÿ���� ������ Ÿ��, ������ �ѱ� ����)
        Vector3 dir = (target ? (target.position - muzzle.position) : muzzle.forward).normalized;

        // 2) ���� ����(���� ���� �� ���� ȸ��)
        if (spreadDegrees > 0f)
        {
            // dir�� �����ϴ� ���� ������ ȸ��
            Vector3 axis = Vector3.Cross(dir, Vector3.up);
            if (axis.sqrMagnitude < 1e-6f) axis = Vector3.right; // dir�� ��/�Ʒ��� �� ����
            float angle = Random.Range(-spreadDegrees, spreadDegrees);
            dir = (Quaternion.AngleAxis(angle, axis) * dir).normalized;
        }

        
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        var go = Instantiate(bulletPrefab, muzzle.position, rot);

        if (go.TryGetComponent(out Bullet b))
        {
            b.Launch(
                speed: projectileSpeed,
                damage: damage,
                collisionMask: enemyMask,
                maxDistance: maxDistance,
                direction: dir,                
                impactVfxPrefab: null
            );
        }
        else
        {
            Debug.LogWarning("[ProjectileWeapon] Bullet prefab�� Bullet ��ũ��Ʈ�� �ʿ��մϴ�.", this);
        }
    }

    private void PlayMuzzle(Transform muzzle)
    {
        if (muzzleFlash)
        {
            muzzleFlash.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            muzzleFlash.Play(true);
        }
        else if (muzzleFlashPrefab)
        {
            var ps = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);
            ps.Play(true);
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        if (audioSource && fireClip) audioSource.PlayOneShot(fireClip);
        else if (fireClip) AudioSource.PlayClipAtPoint(fireClip, muzzle.position);
    }
}
