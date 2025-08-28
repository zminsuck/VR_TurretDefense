using UnityEngine;

public interface IWeapon
{
    // 무기 구현체가 필요로 하는 사양은 파라미터로 전달
    void Fire(Transform muzzle, Transform target, int damage, LayerMask enemyMask, float maxDistance);

    /*
     포탑의 발사 로직을 분리
    무기 구현 -> 발사체, 레이캐스트, 레이저, 스플래시 등을 담당
    장점 -> 교체, 업그레이드가 굉장히 쉬움

    - 구현 방식 파라미터 -
    
    Transform muzzle 
    - 발사 총구의 위치 & 방향 (position & forward)를 제공
    - 발사체 생성 위치, 레이캐스트 시작점 등에 사용

    Transform target
    - 현재 조준 중인 적을 추적
    - 발사체 유도 or 목표 중심점 보정 등에 사용
    - 타겟이 없을 수도 있으니 구현체에는 null 체크하여 안전성 보장

    int damage
    - 한발 & 한 번의 공격이 입히는 기본 피해량
    - 무기는 이 값을 전달, 배율 & 크리티컬 등을 내부에서 계산 가능

    LayerMask enemyMase
    - Physics 쿼리에서 맞춰야 할 레이어로 아군 / 지형 등을 필터링하고 적만 명중

    float MaxDistance
    - 공격의 최대 유효 거리
    - 히트스캔 레이를 이 거리까지만 쏘고, 발사체라면
      초기 속도 / 수명 Raycast를 이 값을 제한
     */
}



