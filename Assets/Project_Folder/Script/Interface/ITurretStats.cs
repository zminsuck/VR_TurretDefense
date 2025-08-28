using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

// ���������� ��ž�� ���� ����� ǥ��
// Hitscan : ����ĳ��Ʈ �ѹ����� ��� ���� (�����Ѱ� ����)
// Projectile : �߻�ü�� �����ؼ� �������� ��� ( �Ѿ� / �̻��� ���� )
public enum AttackType {  Hitscan }
// ��ž�� �˾ƾ� �� ���� ��༭
// ������ �̸� / Ÿ���� �Ӽ��� ������ �۵�
public interface ITurretStats
{
    // get : �б⸸ �ϰڴٴ� �� (��ž������ �б⸸ ���� �ϰڴ�)
    /*
    get�� ����� ����
    - ĸ��ȭ & ������ -> ��ž �ܺο��� ������ ���Ƿ� �ٲ��� ���ϰ� �� [ �б⸸ ���� ]
    - ���� ������ ���� -> ��� �� / ���� ���̺� / ���� �ջ� �� � ������ε� �� ����
    */
    float Range { get; } // ��Ÿ� / �� Ÿ��, ����� �� �� �ִ� �ִ� �Ÿ�
    float TurnSpeed { get; } // ȸ���ӵ�
    float CooldownSeconds { get; } // �߻� ��Ÿ��
    int Damage { get; } // �ѹ��� ���ط�
}
