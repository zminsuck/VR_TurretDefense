using UnityEngine;

public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    [Header("Projectile")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;
    [SerializeField, Range(0f, 10f)] private float spreadDegrees = 0f;

    [Header("VFX (optional)")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    public void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance)
    {
        if (!muzzle || !bulletPrefab) return;

        PlayMuzzle(muzzle);

        Vector3 dir = (target ? (target.position - muzzle.position) : muzzle.forward).normalized;

        if (spreadDegrees > 0f)
        {
            float angle = Random.Range(-spreadDegrees, spreadDegrees);
            // 수평 탄퍼짐만 구현 (단순화)
            dir = Quaternion.Euler(0, angle, 0) * dir;
        }

        var go = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir));

        if (go.TryGetComponent<Bullet>(out Bullet b))
        {
            b.Launch(projectileSpeed, damage, enemyMask, maxDistance, dir, null);
        }
    }

    // FXManager를 사용하여 코드 간소화
    private void PlayMuzzle(Transform muzzle)
    {
        if (muzzleFlash)
        {
            muzzleFlash.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            muzzleFlash.Play(true);
        }
        else if (muzzleFlashPrefab)
        {
            FXManager.PlayEffect(muzzleFlashPrefab.gameObject, muzzle.position, muzzle.rotation, muzzle);
        }

        FXManager.PlaySound(fireClip, muzzle.position, audioSource);
    }
}