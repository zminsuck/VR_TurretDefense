using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider playTimeSlider;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI roundText;
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTitle;
    [SerializeField] private TextMeshProUGUI gameOverDesc;
    [SerializeField] private bool pauseOnGameOver = true;

    [Header("Round Manager (선택 연결)")]
    [SerializeField] private RoundManager roundManager;
    public int CurrentRound => roundManager ? roundManager.CurrentRound : _currentRound;

    [Header("Config")]
    [SerializeField, Min(1)] private int maxHP = 20;
    [SerializeField, Min(1f)] private float timeSliderMax = 300f;

    // State
    float _elapsed;
    int _hp;
    int _currentRound;
    int _score;
    bool _isGameOver;

    public int Score => _score;
    public int AliveEnemies { get; private set; } // 실시간 적 카운트

    // 적 카운트 관리 API
    public void OnEnemySpawned() => AliveEnemies++;
    public void OnEnemyKilled() => AliveEnemies = Mathf.Max(0, AliveEnemies - 1);

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        Time.timeScale = 1f;
        _hp = maxHP;

        if (playTimeSlider) { playTimeSlider.minValue = 0; playTimeSlider.maxValue = timeSliderMax; playTimeSlider.value = 0; }
        if (hpSlider) { hpSlider.minValue = 0; hpSlider.maxValue = maxHP; hpSlider.value = _hp; }
        if (scoreText) { scoreText.text = "0"; }
        if (gameOverPanel) { gameOverPanel.SetActive(false); }
    }

    void Start()
    {
        if (!roundManager) roundManager = FindFirstObjectByType<RoundManager>();
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

    // 나머지 Heal, SetMaxHP, GameOver, 버튼 메서드 등은 변경 없음
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

    public void Btn_DamageBase(int a) => TakeDamage(a);

    void OnRoundStarted(int round) => UpdateRoundUI(round);
    void OnRoundEnded(int round) {}

    void UpdateRoundUI(int round)
    {
        if (roundText) roundText.text = $"Round {round}";
        _currentRound = round;
    }

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