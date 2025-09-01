// C# �⺻: Unity Engine �� UI, TextMeshPro ���̺귯���� ����ϱ� ���� ���ӽ����̽� ����
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ��ü ���� ���α׷���(OOP): MonoBehaviour�� ��ӹ޾� Unity ������Ʈ �ý��ۿ� ����
public class GameManager : MonoBehaviour
{
    // ������ ���� - �̱���(Singleton): 'I'��� static ������ ���� ��𼭵� �� Ŭ������ ���� �ν��Ͻ��� ������ �� �ֵ��� ��
    public static GameManager I { get; private set; }

    // Unity ������ ���: [Header] ��Ʈ����Ʈ�� �ν����� â UI�� �׷�ȭ
    [Header("UI References")]
    // Unity ������ ���: [SerializeField] ��Ʈ����Ʈ�� private ������ �ν����Ϳ� ������� UI ������Ʈ�� ���� (����ȭ)
    [SerializeField] private Slider playTimeSlider;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Round Manager (���� ����)")]
    [SerializeField] private RoundManager roundManager;

    [Header("Config")]
    // Unity ������ ���: [Min] ��Ʈ����Ʈ�� �ν����Ϳ��� ���� ������ �ּڰ��� ����
    [SerializeField, Min(1)] private int maxHP = 20;
    [SerializeField, Min(1f)] private float timeSliderMax = 300f;

    // C# �� ���� ��� �� ���� ������: roundManager ���� ���ο� ���� CurrentRound ���� ���Ǻη� ��ȯ�ϴ� �б� ���� �Ӽ�
    public int CurrentRound => roundManager ? roundManager.CurrentRound : _currentRound;

    // ��ü ���� ���α׷���(OOP) - ĸ��ȭ(Encapsulation): ������ �ֿ� ���� �������� private���� �����Ͽ� �ܺ��� �������� ������ ����
    float _elapsed;
    int _hp;
    int _currentRound;

    // Unity �����ֱ� �޼���: ��ũ��Ʈ �ν��Ͻ��� �ε�� �� ���� ���� ȣ��� (�̱��� �ʱ�ȭ�� ����)
    void Awake()
    {
        // �̱��� ���� ����: �̹� �ν��Ͻ�(I)�� �����ϰ�, �װ��� �ڱ� �ڽ��� �ƴ� ��� �ߺ� ������ ���� ���� �����θ� �ı�
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

    // Unity �����ֱ� �޼���: ù ������ ������Ʈ ���� �� �� ȣ��� (�ٸ� ��ü���� ���� ���ῡ ����)
    void Start()
    {
        // ������ ���� �� ����� �ڵ�: roundManager�� �Ҵ���� �ʾҴٸ� ������ ���� ã�� ������ �õ�
        if (!roundManager) roundManager = Object.FindFirstObjectByType<RoundManager>();

        // �̺�Ʈ ��� ���α׷��� (������ ����): roundManager�� �̺�Ʈ(onRoundStarted ��)�� ������(OnRoundStarted ��)�� ���
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

    // Unity �����ֱ� �޼���: �� �����Ӹ��� ȣ��Ǿ� �ǽð� ������ ó��
    void Update()
    {
        _elapsed += Time.deltaTime;
        if (playTimeSlider)
        {
            // ���� UI ����: �÷��� �ð��� �����̴��� �ִ밪�� �ʰ��ϸ� �ִ밪�� �ڵ����� Ȯ��
            if (_elapsed > playTimeSlider.maxValue) playTimeSlider.maxValue = _elapsed;
            playTimeSlider.value = _elapsed;
        }
    }

    // ���� ���� API: �ܺο��� ���� ����(HP)�� �����ϰ� ������ �� �ֵ��� public �޼��� ����
    public void TakeDamage(int amount)
    {
        // ������ ���Ἲ ����: Mathf.Max�� ����Ͽ� ü���� 0 �̸����� �������ų�, ������ ���� ������ �Ǵ� ���� ����
        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
    }

    public void Heal(int amount)
    {
        // ������ ���Ἲ ����: Mathf.Min/Max�� ����Ͽ� ü���� �ִ� ü���� �ʰ��ϰų�, ȸ������ ������ �Ǵ� ���� ����
        _hp = Mathf.Min(maxHP, _hp + Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
    }

    public void SetMaxHP(int newMax, bool keepRatio = false)
    {
        newMax = Mathf.Max(1, newMax);
        // �� ��ȯ(Type Casting): ���� �������� ������ ���� ���� (float)�� ����� �� ��ȯ �� ���� ���
        if (keepRatio && maxHP > 0) _hp = Mathf.RoundToInt((float)_hp / maxHP * newMax);
        maxHP = newMax;
        _hp = Mathf.Min(_hp, maxHP);
        if (hpSlider) { hpSlider.maxValue = maxHP; hpSlider.value = _hp; }
    }

    // C# �� ���� ���(Expression-Bodied Member): �� ��¥�� ������ �޼��带 '=>'�� ����� �����ϰ� ǥ��
    public void Btn_DamageBase(int a) => TakeDamage(a);

    // �̺�Ʈ �ڵ鷯: RoundManager�� onRoundStarted �̺�Ʈ�� �߻����� �� ȣ��ǵ��� ��ϵ� �޼���
    void OnRoundStarted(int round) => UpdateRoundUI(round);
    void OnRoundEnded(int round) { }

    // �ڵ� ����ȭ(���� �޼���): UI ������Ʈ ������ ������ �޼���� �и��Ͽ� ���뼺 �� ������ ���
    void UpdateRoundUI(int round)
    {
        if (roundText) roundText.text = $"Round {round}"; // C# ���ڿ� ����(String Interpolation) ���
        _currentRound = round;
    }
}