using UnityEngine;

public interface IEnemy
{
    bool IsDead { get; }         // 이미 죽었는지 여부
    Transform Transform { get; } // 위치/추적용 Transform
    void TakeDamage(int damage); // 데미지 처리

    /*
        IsDead: 포탑이 죽은 적을 타겟팅하지 않도록 확인할 수 있음.
        Transform: 위치 추적에 사용. (transform.position 등)
        TakeDamage(int damage): 데미지 입히는 공용 함수.
     */
}
