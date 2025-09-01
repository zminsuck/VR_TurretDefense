// C# �⺻: UnityEngine ���̺귯���� Ŭ������ �Լ��� ����ϱ� ���� ���ӽ����̽� ����
using UnityEngine;

// ��ü ���� ���α׷���(OOP): MonoBehaviour ������� Unity ������Ʈȭ, IWeapon �������̽� �������� �԰�ȭ�� ���� ��� ����
public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    // Unity ������ ���: [Header] ��Ʈ����Ʈ�� �ν����� â UI�� �׷�ȭ
    [Header("Projectile")]
    // Unity ������ ���: [SerializeField] ��Ʈ����Ʈ�� private ������ �ν����Ϳ� ������� �����Ϳ� ������ �и� (����ȭ)
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;
    // Unity ������ ���: [Range] ��Ʈ����Ʈ�� �ν����Ϳ� �����̴� UI�� ����
    [SerializeField, Range(0f, 10f)] private float spreadDegrees = 0f;

    [Header("VFX (optional)")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    // �������̽� ����: IWeapon �������̽��� Fire �޼��带 ��ü������ ����
    public void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance)
    {
        // ����� ���α׷���: �ʼ� ���� ������ null�� ��� ������ �ߴ��Ͽ� NullReferenceException�� ���� (���� Ŭ����)
        if (!muzzle || !bulletPrefab) return;

        // �ڵ� ����ȭ: ����Ʈ ��� ������ ������ '���� �޼���(Helper Method)'�� �и��Ͽ� ������ �� ���������� ���
        PlayMuzzle(muzzle);

        // 3D ���� ���� �� ���� ������: target ������ ���� ���Ǻη� �߻� ���� ����(Vector3)�� ����ϰ�, '.normalized'�� ����ȭ
        Vector3 dir = (target ? (target.position - muzzle.position) : muzzle.forward).normalized;

        if (spreadDegrees > 0f)
        {
            // 3D ���� ����: '����(Cross Product)'�� ����� �߻� ����(dir)�� ������ ȸ����(axis) ���
            Vector3 axis = Vector3.Cross(dir, Vector3.up);
            // ���� ó��: �߻� ������ ��/�Ʒ� ������ �� ���� ����� 0�� �Ǵ� ��츦 ����
            if (axis.sqrMagnitude < 1e-6f) axis = Vector3.right;
            float angle = Random.Range(-spreadDegrees, spreadDegrees);
            // 3D ȸ�� ����(���ʹϾ�): 'Quaternion.AngleAxis'�� ȸ������ �����ϰ� ���Ϳ� ���Ͽ� 'ź ����' ȿ�� ����
            dir = (Quaternion.AngleAxis(angle, axis) * dir).normalized;
        }

        // 3D ȸ�� ����(���ʹϾ�): 'Quaternion.LookRotation'�� ����� Ư�� ������ �ٶ󺸴� ȸ���� ����
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        // Unity ��ü ���� ���: 'Instantiate'�� ����Ͽ� ���������κ��� ���� ������Ʈ(����ü)�� �������� ����
        var go = Instantiate(bulletPrefab, muzzle.position, rot);

        // ������Ʈ ���: 'TryGetComponent'�� ����Ͽ� ������ ������Ʈ���� Bullet ��ũ��Ʈ�� �����ϰ� ã�ƿ�
        if (go.TryGetComponent(out Bullet b))
        {
            // ��ü ���� ���α׷���(OOP): ã�� Bullet ������Ʈ�� Launch �޼��带 ȣ���Ͽ� ������(�ӵ�, ������ ��)�� �����ϰ� ������ ����
            b.Launch(
                speed: projectileSpeed,
                damage: damage,
                collisionMask: enemyMask,
                maxDistance: maxDistance,
                direction: dir,
                impactVfxPrefab: null
            );
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
            // Unity ��ü �����ֱ� ����: 'Destroy' �޼��忡 ���� �ð��� �־� ��ƼŬ ����� ���� �� �ڵ����� ������Ʈ�� �ı�
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        if (audioSource && fireClip)
        {
            // Unity ����� ���: AudioSource ������Ʈ�� ���� ���带 ��� (���� ����� ������ Ȱ��)
            audioSource.PlayOneShot(fireClip);
        }
        // ���� �޼��� Ȱ��: AudioSource ������Ʈ ���� Ư�� ��ġ���� ���带 ����ϴ� ������ ���
        else if (fireClip) AudioSource.PlayClipAtPoint(fireClip, muzzle.position);
    }
}