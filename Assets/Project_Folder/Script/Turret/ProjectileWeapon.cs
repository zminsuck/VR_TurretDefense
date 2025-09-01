// C# 기본: UnityEngine 라이브러리의 클래스와 함수를 사용하기 위한 네임스페이스 선언
using UnityEngine;

// 객체 지향 프로그래밍(OOP): MonoBehaviour 상속으로 Unity 컴포넌트화, IWeapon 인터페이스 구현으로 규격화된 무기 기능 정의
public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    // Unity 에디터 기술: [Header] 어트리뷰트로 인스펙터 창 UI를 그룹화
    [Header("Projectile")]
    // Unity 에디터 기술: [SerializeField] 어트리뷰트로 private 변수를 인스펙터에 노출시켜 데이터와 로직을 분리 (직렬화)
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;
    // Unity 에디터 기술: [Range] 어트리뷰트로 인스펙터에 슬라이더 UI를 제공
    [SerializeField, Range(0f, 10f)] private float spreadDegrees = 0f;

    [Header("VFX (optional)")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;

    // 인터페이스 구현: IWeapon 인터페이스의 Fire 메서드를 구체적으로 정의
    public void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance)
    {
        // 방어적 프로그래밍: 필수 참조 변수가 null일 경우 실행을 중단하여 NullReferenceException을 방지 (가드 클로즈)
        if (!muzzle || !bulletPrefab) return;

        // 코드 구조화: 이펙트 재생 로직을 별도의 '헬퍼 메서드(Helper Method)'로 분리하여 가독성 및 유지보수성 향상
        PlayMuzzle(muzzle);

        // 3D 벡터 수학 및 삼항 연산자: target 유무에 따라 조건부로 발사 방향 벡터(Vector3)를 계산하고, '.normalized'로 정규화
        Vector3 dir = (target ? (target.position - muzzle.position) : muzzle.forward).normalized;

        if (spreadDegrees > 0f)
        {
            // 3D 벡터 수학: '외적(Cross Product)'을 사용해 발사 방향(dir)과 수직인 회전축(axis) 계산
            Vector3 axis = Vector3.Cross(dir, Vector3.up);
            // 예외 처리: 발사 방향이 위/아래 방향일 때 외적 결과가 0이 되는 경우를 방지
            if (axis.sqrMagnitude < 1e-6f) axis = Vector3.right;
            float angle = Random.Range(-spreadDegrees, spreadDegrees);
            // 3D 회전 수학(쿼터니언): 'Quaternion.AngleAxis'로 회전값을 생성하고 벡터에 곱하여 '탄 퍼짐' 효과 적용
            dir = (Quaternion.AngleAxis(angle, axis) * dir).normalized;
        }

        // 3D 회전 수학(쿼터니언): 'Quaternion.LookRotation'을 사용해 특정 방향을 바라보는 회전값 생성
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        // Unity 객체 생성 기술: 'Instantiate'를 사용하여 프리팹으로부터 게임 오브젝트(투사체)를 동적으로 생성
        var go = Instantiate(bulletPrefab, muzzle.position, rot);

        // 컴포넌트 통신: 'TryGetComponent'를 사용하여 생성된 오브젝트에서 Bullet 스크립트를 안전하게 찾아옴
        if (go.TryGetComponent(out Bullet b))
        {
            // 객체 지향 프로그래밍(OOP): 찾은 Bullet 컴포넌트의 Launch 메서드를 호출하여 데이터(속도, 데미지 등)를 전달하고 동작을 위임
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
            // Unity 객체 생명주기 관리: 'Destroy' 메서드에 지연 시간을 주어 파티클 재생이 끝난 후 자동으로 오브젝트를 파괴
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        if (audioSource && fireClip)
        {
            // Unity 오디오 기술: AudioSource 컴포넌트를 통해 사운드를 재생 (기존 오디오 설정을 활용)
            audioSource.PlayOneShot(fireClip);
        }
        // 정적 메서드 활용: AudioSource 컴포넌트 없이 특정 위치에서 사운드를 재생하는 간편한 방식
        else if (fireClip) AudioSource.PlayClipAtPoint(fireClip, muzzle.position);
    }
}