using UnityEngine;

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour, IEnemy, IEnemySetup
{
    [Header("HP")]
    [SerializeField, Min(1)] private int maxHp = 50;
    [SerializeField] private bool resetHpOnAwake = true;

    [Header("Move (선택)")]
    [SerializeField] private EnemyWalker walker;   // 이동 담당(있으면 속도 세팅)
    [SerializeField, Min(0f)] private float fallbackMoveSpeed = 0f; // Walker 없을 때 참조용

    // 런타임
    private int currentHp;

    public int MaxHP => maxHp;
    public int CurrentHP => currentHp;
    public float HP01 => maxHp > 0 ? (float)currentHp / maxHp : 0f;
    public System.Action<int, int> onHPChanged; // (cur,max)
    // IEnemy
    public bool IsDead => currentHp <= 0;
    public Transform Transform => transform;

    private void Awake()
    {
        if (!walker) walker = GetComponent<EnemyWalker>();
        if (resetHpOnAwake) currentHp = maxHp;
        onHPChanged?.Invoke(currentHp, maxHp);
    }

    // 라운드 스폰 직후 Spawner가 호출
    public void SetupStats(int maxHP, float moveSpeed)
    {
        // HP 세팅
        maxHp = Mathf.Max(1, maxHP);
        currentHp = maxHp;

        // 이동 속도 세팅
        if (walker) walker.SetSpeed(moveSpeed);
        else fallbackMoveSpeed = moveSpeed;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHp -= Mathf.Max(0, damage);
        onHPChanged?.Invoke(currentHp, maxHp);
        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHp < 1) maxHp = 1;
    }
#endif
}
