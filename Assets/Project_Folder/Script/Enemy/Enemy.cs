using UnityEngine;

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour, IEnemy, IEnemySetup
{
    [Header("HP")]
    [SerializeField, Min(1)] private int maxHp = 50;
    [SerializeField] private bool resetHpOnAwake = true;

    [Header("Move (����)")]
    [SerializeField] private EnemyWalker walker;   // �̵� ���(������ �ӵ� ����)
    [SerializeField, Min(0f)] private float fallbackMoveSpeed = 0f; // Walker ���� �� ������

    // ��Ÿ��
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

    // ���� ���� ���� Spawner�� ȣ��
    public void SetupStats(int maxHP, float moveSpeed)
    {
        // HP ����
        maxHp = Mathf.Max(1, maxHP);
        currentHp = maxHp;

        // �̵� �ӵ� ����
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
