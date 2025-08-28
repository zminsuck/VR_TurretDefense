using UnityEngine;

public interface IEnemy
{
    bool IsDead { get; }         // �̹� �׾����� ����
    Transform Transform { get; } // ��ġ/������ Transform
    void TakeDamage(int damage); // ������ ó��

    /*
        IsDead: ��ž�� ���� ���� Ÿ�������� �ʵ��� Ȯ���� �� ����.
        Transform: ��ġ ������ ���. (transform.position ��)
        TakeDamage(int damage): ������ ������ ���� �Լ�.
     */
}
