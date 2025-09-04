// Enemy.cs
using UnityEngine;

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour, IEnemy, IEnemySetup
{
    [Header("HP")]
    [SerializeField, Min(1)] private int maxHp = 50;
    [SerializeField] private bool resetHpOnAwake = true;

    [Header("Move (선택)")]
    [SerializeField] private EnemyWalker walker;
    [SerializeField, Min(0f)] private float fallbackMoveSpeed = 0f;

    private int currentHp;

    public int MaxHP => maxHp;
    public int CurrentHP => currentHp;
    public float HP01 => maxHp > 0 ? (float)currentHp / maxHp : 0f;
    public System.Action<int, int> onHPChanged;

    public bool IsDead => currentHp <= 0;
    public Transform Transform => transform;

    // 적 생성 시 GameManager에 알림
    private void OnEnable()
    {
        GameManager.I?.OnEnemySpawned();
    }

    private void Awake()
    {
        if (!walker) walker = GetComponent<EnemyWalker>();
        if (resetHpOnAwake) currentHp = maxHp;
        onHPChanged?.Invoke(currentHp, maxHp);
    }

    public void SetupStats(int maxHP, float moveSpeed)
    {
        maxHp = Mathf.Max(1, maxHP);
        currentHp = maxHp;

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
        // 적 사망 시 GameManager에 알림
        GameManager.I?.OnEnemyKilled();
        // 점수 추가 로직
        // GameManager.I?.AddScore(10); 
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHp < 1) maxHp = 1;
    }
#endif
}