using UnityEngine;

public interface IWeapon
{
    // ���� ����ü�� �ʿ�� �ϴ� ����� �Ķ���ͷ� ����
    void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance);

    /*
     ��ž�� �߻� ������ �и�
    ���� ���� -> �߻�ü, ����ĳ��Ʈ, ������, ���÷��� ���� ���
    ���� -> ��ü, ���׷��̵尡 ������ ����

    - ���� ��� �Ķ���� -
    
    Transform muzzle 
    - �߻� �ѱ��� ��ġ & ���� (position & forward)�� ����
    - �߻�ü ���� ��ġ, ����ĳ��Ʈ ������ � ���

    Transform target
    - ���� ���� ���� ���� ����
    - �߻�ü ���� or ��ǥ �߽��� ���� � ���
    - Ÿ���� ���� ���� ������ ����ü���� null üũ�Ͽ� ������ ����

    int damage
    - �ѹ� & �� ���� ������ ������ �⺻ ���ط�
    - ����� �� ���� ����, ���� & ũ��Ƽ�� ���� ���ο��� ��� ����

    LayerMask enemyMase
    - Physics �������� ����� �� ���̾�� �Ʊ� / ���� ���� ���͸��ϰ� ���� ����

    float MaxDistance
    - ������ �ִ� ��ȿ �Ÿ�
    - ��Ʈ��ĵ ���̸� �� �Ÿ������� ���, �߻�ü���
      �ʱ� �ӵ� / ���� Raycast�� �� ���� ����
     */
}



