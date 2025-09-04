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
    Vector3 _dir = Vector3.forward;

    public void Launch(float speed, int damage, LayerMask collisionMask, float maxDistance, Vector3 direction, GameObject impactVfxPrefab = null)
    {
        this.speed = speed;
        _damage = damage;
        _collisionMask = collisionMask | extraHitMask;
        _maxDistance = maxDistance;
        if (impactVfxPrefab) this.impactVfxPrefab = impactVfxPrefab;

        _dir = direction.normalized;
        transform.rotation = Quaternion.LookRotation(_dir);

        _traveled = 0f;
        _life = 0f;
        gameObject.SetActive(true);
    }

    void OnEnable() { _life = 0f; _traveled = 0f; }

    void Update()
    {
        _life += Time.deltaTime;
        if (_life >= lifeTime) { Destroy(gameObject); return; }

        float step = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, _dir, out RaycastHit hit, step, _collisionMask, QueryTriggerInteraction.Ignore))
        {
            OnHit(hit);
            return;
        }

        transform.position += _dir * step;
        _traveled += step;

        if (_maxDistance > 0f && _traveled >= _maxDistance) Destroy(gameObject);
    }

    void OnHit(RaycastHit hit)
    {
        // 확장 메서드 사용
        var enemy = hit.collider.FindComponent<IEnemy>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(_damage);
        }

        // FXManager 사용
        FXManager.PlayEffect(impactVfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));

        Destroy(gameObject);
    }

    public void SetSpeed(float s) => speed = s;
    public void SetExtraHitMask(LayerMask m) => extraHitMask = m;
}