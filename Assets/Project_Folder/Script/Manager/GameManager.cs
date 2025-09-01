// C# 기본: Unity Engine 및 UI, TextMeshPro 라이브러리를 사용하기 위한 네임스페이스 선언
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 객체 지향 프로그래밍(OOP): MonoBehaviour를 상속받아 Unity 컴포넌트 시스템에 통합
public class GameManager : MonoBehaviour
{
    // 디자인 패턴 - 싱글톤(Singleton): 'I'라는 static 변수를 통해 어디서든 이 클래스의 단일 인스턴스에 접근할 수 있도록 함
    public static GameManager I { get; private set; }

    // Unity 에디터 기술: [Header] 어트리뷰트로 인스펙터 창 UI를 그룹화
    [Header("UI References")]
    // Unity 에디터 기술: [SerializeField] 어트리뷰트로 private 변수를 인스펙터에 노출시켜 UI 컴포넌트와 연동 (직렬화)
    [SerializeField] private Slider playTimeSlider;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Round Manager (선택 연결)")]
    [SerializeField] private RoundManager roundManager;

    [Header("Config")]
    // Unity 에디터 기술: [Min] 어트리뷰트로 인스펙터에서 설정 가능한 최솟값을 제한
    [SerializeField, Min(1)] private int maxHP = 20;
    [SerializeField, Min(1f)] private float timeSliderMax = 300f;

    // C# 식 본문 멤버 및 삼항 연산자: roundManager 존재 여부에 따라 CurrentRound 값을 조건부로 반환하는 읽기 전용 속성
    public int CurrentRound => roundManager ? roundManager.CurrentRound : _currentRound;

    // 객체 지향 프로그래밍(OOP) - 캡슐화(Encapsulation): 게임의 주요 상태 변수들을 private으로 선언하여 외부의 직접적인 접근을 막음
    float _elapsed;
    int _hp;
    int _currentRound;

    // Unity 생명주기 메서드: 스크립트 인스턴스가 로드될 때 가장 먼저 호출됨 (싱글톤 초기화에 적합)
    void Awake()
    {
        // 싱글톤 패턴 구현: 이미 인스턴스(I)가 존재하고, 그것이 자기 자신이 아닐 경우 중복 생성을 막기 위해 스스로를 파괴
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        _hp = maxHP;

        if (playTimeSlider)
        {
            playTimeSlider.minValue = 0;
            playTimeSlider.maxValue = timeSliderMax;
            playTimeSlider.value = 0;
        }

        if (hpSlider)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHP;
            hpSlider.value = _hp;
        }
    }

    // Unity 생명주기 메서드: 첫 프레임 업데이트 전에 한 번 호출됨 (다른 객체와의 참조 연결에 적합)
    void Start()
    {
        // 의존성 관리 및 방어적 코딩: roundManager가 할당되지 않았다면 씬에서 직접 찾아 연결을 시도
        if (!roundManager) roundManager = Object.FindFirstObjectByType<RoundManager>();

        // 이벤트 기반 프로그래밍 (옵저버 패턴): roundManager의 이벤트(onRoundStarted 등)에 리스너(OnRoundStarted 등)를 등록
        if (roundManager)
        {
            roundManager.onRoundStarted.AddListener(OnRoundStarted);
            roundManager.onRoundEnded.AddListener(OnRoundEnded);
            UpdateRoundUI(roundManager.CurrentRound);
        }
        else
        {
            UpdateRoundUI(_currentRound);
        }
    }

    // Unity 생명주기 메서드: 매 프레임마다 호출되어 실시간 로직을 처리
    void Update()
    {
        _elapsed += Time.deltaTime;
        if (playTimeSlider)
        {
            // 동적 UI 조절: 플레이 시간이 슬라이더의 최대값을 초과하면 최대값을 자동으로 확장
            if (_elapsed > playTimeSlider.maxValue) playTimeSlider.maxValue = _elapsed;
            playTimeSlider.value = _elapsed;
        }
    }

    // 상태 관리 API: 외부에서 게임 상태(HP)를 안전하게 변경할 수 있도록 public 메서드 제공
    public void TakeDamage(int amount)
    {
        // 데이터 무결성 보장: Mathf.Max를 사용하여 체력이 0 미만으로 내려가거나, 데미지 양이 음수가 되는 것을 방지
        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
    }

    public void Heal(int amount)
    {
        // 데이터 무결성 보장: Mathf.Min/Max를 사용하여 체력이 최대 체력을 초과하거나, 회복량이 음수가 되는 것을 방지
        _hp = Mathf.Min(maxHP, _hp + Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
    }

    public void SetMaxHP(int newMax, bool keepRatio = false)
    {
        newMax = Mathf.Max(1, newMax);
        // 형 변환(Type Casting): 정수 나눗셈의 오차를 막기 위해 (float)로 명시적 형 변환 후 비율 계산
        if (keepRatio && maxHP > 0) _hp = Mathf.RoundToInt((float)_hp / maxHP * newMax);
        maxHP = newMax;
        _hp = Mathf.Min(_hp, maxHP);
        if (hpSlider) { hpSlider.maxValue = maxHP; hpSlider.value = _hp; }
    }

    // C# 식 본문 멤버(Expression-Bodied Member): 한 줄짜리 간단한 메서드를 '=>'를 사용해 간결하게 표현
    public void Btn_DamageBase(int a) => TakeDamage(a);

    // 이벤트 핸들러: RoundManager의 onRoundStarted 이벤트가 발생했을 때 호출되도록 등록된 메서드
    void OnRoundStarted(int round) => UpdateRoundUI(round);
    void OnRoundEnded(int round) { }

    // 코드 구조화(헬퍼 메서드): UI 업데이트 로직을 별도의 메서드로 분리하여 재사용성 및 가독성 향상
    void UpdateRoundUI(int round)
    {
        if (roundText) roundText.text = $"Round {round}"; // C# 문자열 보간(String Interpolation) 사용
        _currentRound = round;
    }
}