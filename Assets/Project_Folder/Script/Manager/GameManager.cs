using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ───────── Singleton ─────────
    public static GameManager I { get; private set; }

    // ───────── UI Refs ─────────
    [Header("UI References")]
    [SerializeField] private Slider playTimeSlider;      // 경과 시간
    [SerializeField] private Slider hpSlider;            // 베이스 HP
    [SerializeField] private TextMeshProUGUI roundText;  // 라운드 표시
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;  // 점수 텍스트

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;         // 오버레이 패널(비활성 시작)
    [SerializeField] private TextMeshProUGUI gameOverTitle;    // "Game Over"
    [SerializeField] private TextMeshProUGUI gameOverDesc;     // 라운드/시간/이유
    [SerializeField] private bool pauseOnGameOver = true;      // 일시정지 여부

    // ───────── RoundManager ─────────
    [Header("Round Manager (선택 연결)")]
    [SerializeField] private RoundManager roundManager;
    public int CurrentRound => roundManager ? roundManager.CurrentRound : _currentRound;

    // ───────── Config ─────────
    [Header("Config")]
    [SerializeField, Min(1)] private int maxHP = 20;
    [SerializeField, Min(1f)] private float timeSliderMax = 300f;

    // ───────── State ─────────
    float _elapsed;
    int _hp;
    int _currentRound;
    int _score;
    bool _isGameOver;

    public int Score => _score;

    // ───────── Lifecycle ─────────
    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        Time.timeScale = 1f; // 에디터 재생 시 안전
        _hp = maxHP;

        if (playTimeSlider) { playTimeSlider.minValue = 0; playTimeSlider.maxValue = timeSliderMax; playTimeSlider.value = 0; }
        if (hpSlider) { hpSlider.minValue = 0; hpSlider.maxValue = maxHP; hpSlider.value = _hp; }
        if (scoreText) { scoreText.text = "0"; }
        if (gameOverPanel) { gameOverPanel.SetActive(false); }
    }

    void Start()
    {
        if (!roundManager) roundManager = Object.FindFirstObjectByType<RoundManager>();
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
        if (_isGameOver) return;

        _elapsed += Time.deltaTime;
        if (playTimeSlider)
        {
            if (_elapsed > playTimeSlider.maxValue) playTimeSlider.maxValue = _elapsed;
            playTimeSlider.value = _elapsed;
        }
    }

    // ───────── Public API ─────────
    public void AddScore(int amount)
    {
        if (_isGameOver || amount == 0) return;
        _score = Mathf.Max(0, _score + amount);
        if (scoreText) scoreText.text = _score.ToString();
    }

    public void TakeDamage(int amount)
    {
        if (_isGameOver) return;
        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        if (hpSlider) hpSlider.value = _hp;
        if (_hp <= 0) GameOver("베이스 체력이 0이 되었습니다.");
    }

    public void Heal(int amount)
    {
        if (_isGameOver) return;
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

    // 버튼 테스트용
    public void Btn_DamageBase(int a) => TakeDamage(a);

    // ───────── Round Hooks ─────────
    void OnRoundStarted(int round) => UpdateRoundUI(round);
    void OnRoundEnded(int round) { /* 필요 시 */ }

    void UpdateRoundUI(int round)
    {
        if (roundText) roundText.text = $"Round {round}";
        _currentRound = round;
    }

    // ───────── Game Over ─────────
    public void GameOver(string reason = "")
    {
        if (_isGameOver) return;
        _isGameOver = true;

        if (roundManager) roundManager.StopGame();

        if (gameOverTitle) gameOverTitle.text = "YOU DIE";
        if (gameOverDesc)
        {
            string timeStr = System.TimeSpan.FromSeconds(_elapsed).ToString(@"mm\:ss");
            gameOverDesc.text = $"Round {CurrentRound}\n\nTime {timeStr}\n\n{reason}";
        }
        if (gameOverPanel) gameOverPanel.SetActive(true);

        if (pauseOnGameOver) Time.timeScale = 0f;
    }

    // ───────── UI Buttons ─────────
    public void Btn_RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Btn_Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
