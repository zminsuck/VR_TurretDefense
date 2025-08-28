using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

// 열거형으로 포탑의 공격 방식을 표현
// Hitscan : 레이캐스트 한번으로 즉시 판정 (저격총과 유사)
// Projectile : 발사체를 생성해서 날려보는 방식 ( 총알 / 미사일 유사 )
public enum AttackType {  Hitscan }
// 포탑이 알아야 할 스탯 계약서
// 동일한 이름 / 타입의 속성만 있으면 작동
public interface ITurretStats
{
    // get : 읽기만 하겠다는 것 (포탑에서는 읽기만 실행 하겠다)
    /*
    get을 사용한 이유
    - 캡슐화 & 안전성 -> 포탑 외부에서 스탯을 임의로 바꾸지 못하게 함 [ 읽기만 가능 ]
    - 구현 자유도 보장 -> 계산 값 / 레벨 테이블 / 버프 합상 등 어떤 방식으로든 값 제공
    */
    float Range { get; } // 사거리 / 적 타임, 사격을 할 수 있는 최대 거리
    float TurnSpeed { get; } // 회전속도
    float CooldownSeconds { get; } // 발사 쿨타임
    int Damage { get; } // 한번의 피해량
}
