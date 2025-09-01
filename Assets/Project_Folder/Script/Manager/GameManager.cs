using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUI �� ��

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider playTimeSlider;   // �÷��� �ð� (��� �ð�)
    [SerializeField] private Slider hpSlider;         // ���� ü��
    [SerializeField] private TextMeshProUGUI roundText; // ���� ���� ǥ�� (����)

    [Header("Round Manager (���� ����)")]
    [SerializeField] private RoundManager roundManager;

    [Header("Config")]
    [SerializeField, Min(1)] private int maxHP = 20;          // �䱸����: 20HP
    [SerializeField, Min(1f)] private float timeSliderMax = 300f; // �ʱ� ǥ�� ����(�ڵ� Ȯ��)

    public int CurrentRound => roundManager ? roundManager.CurrentRound : _currentRound;

    // ���� ����
    float _elapsed;
    int _hp;
    int _currentRound; // roundManager ���� �� ���

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        _hp = maxHP;

        // �����̴� �ʱ�ȭ
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

        // ���� �̺�Ʈ ����
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
            // �ð��� �����̴� �ִ븦 ������ �ڵ� Ȯ��
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

    // ��ư �׽�Ʈ�� ��
    public void Btn_DamageBase(int a) => TakeDamage(a);

    // ========== Round hooks ==========
    void OnRoundStarted(int round) => UpdateRoundUI(round);
    void OnRoundEnded(int round) { /* �ʿ��ϸ� ��� ó�� */ }

    void UpdateRoundUI(int round)
    {
        if (roundText) roundText.text = $"Round {round}";
        _currentRound = round; // RoundManager�� ���� ���� ���
    }
}
