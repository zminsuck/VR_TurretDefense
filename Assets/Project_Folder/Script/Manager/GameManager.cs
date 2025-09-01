using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUI 쓸 때

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider playTimeSlider;   // 플레이 시간 (경과 시간)
    [SerializeField] private Slider hpSlider;         // 남은 체력
    [SerializeField] private TextMeshProUGUI roundText; // 현재 라운드 표시 (선택)

    [Header("Round Manager (선택 연결)")]
    [SerializeField] private RoundManager roundManager;

    [Header("Config")]
    [SerializeField, Min(1)] private int maxHP = 20;          // 요구사항: 20HP
    [SerializeField, Min(1f)] private float timeSliderMax = 300f; // 초기 표시 구간(자동 확장)

    public int CurrentRound => roundManager ? roundManager.CurrentRound : _currentRound;

    // 내부 상태
    float _elapsed;
    int _hp;
    int _currentRound; // roundManager 없을 때 대비

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        _hp = maxHP;

        // 슬라이더 초기화
        if (playTimeSlider) { playTimeSlider.minValue = 0;
            playTimeSlider.maxValue = timeSliderMax;
            playTimeSlider.value = 0; }

        if (hpSlider) { hpSlider.minValue = 0;
            hpSlider.maxValue = maxHP;
            hpSlider.value = _hp; }
    }

    void Start()
    {
        if (!roundManager) roundManager = Object.FindFirstObjectByType<RoundManager>();

        // 라운드 이벤트 연결
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

    void Update()
    {
        _elapsed += Time.deltaTime;
        if (playTimeSlider)
        {
            // 시간이 슬라이더 최대를 넘으면 자동 확장
            if (_elapsed > playTimeSlider.maxValue) playTimeSlider.maxValue = _elapsed;
            playTimeSlider.value = _elapsed;
        }
    }

    public void TakeDamage(int amount)
    {
        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
    }

    public void Heal(int amount)
    {
        _hp = Mathf.Min(maxHP, _hp + Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
    }

    public void SetMaxHP(int newMax, bool keepRatio = false)
    {
        newMax = Mathf.Max(1, newMax);
        if (keepRatio && maxHP > 0) _hp = Mathf.RoundToInt((float)_hp / maxHP * newMax);
        maxHP = newMax;
        _hp = Mathf.Min(_hp, maxHP);
        if (hpSlider) { hpSlider.maxValue = maxHP; hpSlider.value = _hp; }
    }

    // 버튼 테스트용 훅
    public void Btn_DamageBase(int a) => TakeDamage(a);

    // ========== Round hooks ==========
    void OnRoundStarted(int round) => UpdateRoundUI(round);
    void OnRoundEnded(int round) { /* 필요하면 결과 처리 */ }

    void UpdateRoundUI(int round)
    {
        if (roundText) roundText.text = $"Round {round}";
        _currentRound = round; // RoundManager가 없을 때도 기록
    }
}
