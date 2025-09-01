using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField, Min(0.1f)] private float speed = 20f;
    [SerializeField, Min(0.1f)] private float lifeTime = 3f;
    [SerializeField] private LayerMask extraHitMask;
    [SerializeField] private GameObject impactVfxPrefab;

    int _damage;
    LayerMask _collisionMask;
    float _maxDistance, _traveled, _life;

    Vector3 _dir = Vector3.forward;   // 발사 순간 고정되는 방향(월드)

    /// <summary>무기에서 호출: 총알 파라미터 세팅 + 발사</summary>
    // direction은 반드시 넘겨받도록(non-nullable)
    public void Launch(
        float speed,
        int damage,
        LayerMask collisionMask,
        float maxDistance,
        Vector3 direction,
        GameObject impactVfxPrefab = null)   // ← 선택 파라미터는 끝으로
    {
        this.speed = speed;
        _damage = damage;
        _collisionMask = collisionMask | extraHitMask;
        _maxDistance = maxDistance;
        if (impactVfxPrefab) this.impactVfxPrefab = impactVfxPrefab;

        _dir = direction.normalized;  // ← .Value 제거
        transform.rotation = Quaternion.LookRotation(_dir, Vector3.up);

        _traveled = 0f;
        _life = 0f;
    }

    void OnEnable() { _life = 0f; _traveled = 0f; }

    void Update()
    {
        float dt = Time.deltaTime;
        _life += dt;
        if (_life >= lifeTime) { Destroy(gameObject); return; }

        float step = speed * dt;

        if (Physics.Raycast(transform.position, _dir, out RaycastHit hit,
                            step, _collisionMask, QueryTriggerInteraction.Collide))
        { OnHit(hit); return; }

        transform.position += _dir * step;
        _traveled += step;

        if (_maxDistance > 0f && _traveled >= _maxDistance) Destroy(gameObject);
    }

    void OnHit(RaycastHit hit)
    {
        var enemy = hit.collider.GetComponent<IEnemy>() ?? hit.collider.GetComponentInParent<IEnemy>();
        if (enemy != null && !enemy.IsDead) enemy.TakeDamage(_damage);

        if (impactVfxPrefab)
        {
            var rot = Quaternion.LookRotation(hit.normal);
            var go = Instantiate(impactVfxPrefab, hit.point, rot);
            var ps = go.GetComponent<ParticleSystem>();
            Destroy(go, ps ? ps.main.duration + ps.main.startLifetime.constantMax : 2f);
        }
        Destroy(gameObject);
    }

    public void SetSpeed(float s) => speed = s;
    public void SetExtraHitMask(LayerMask m) => extraHitMask = m;
}
