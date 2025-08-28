using UnityEngine;

public class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField, Min(1)] private int maxHp = 50;
    private int currentHp;

    public bool IsDead => currentHp <= 0;
    public Transform Transform => transform;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // TODO: ���� ���� (����Ʈ, ���� ����, ���� ��)
        Destroy(gameObject);
    }
}
