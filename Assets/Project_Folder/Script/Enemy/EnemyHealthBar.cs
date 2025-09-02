using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Enemy enemy;     // 자동으로 부모에서 찾음

    [Header("UI")]
    [SerializeField] private Canvas canvas;   // World Space
    [SerializeField] private Image fill;      // Filled Horizontal

    [Header("Options")]
    [SerializeField] private bool billboard = true;   // 카메라 바라보기
    [SerializeField] private Camera cam;              // XR 카메라 지정 가능
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField, Min(0f)] private float showOnHitSeconds = 2f;
    [SerializeField] private Gradient colorByHP;      // 선택: 체력에 따라 색 변화

    float _timer;

    void Awake()
    {
        if (!enemy) enemy = GetComponentInParent<Enemy>();
        if (!canvas) canvas = GetComponent<Canvas>();
        if (!cam) cam = Camera.main;
    }

    void OnEnable()
    {
        if (enemy != null) enemy.onHPChanged += OnHPChanged;
        Refresh();
    }
    void OnDisable()
    {
        if (enemy != null) enemy.onHPChanged -= OnHPChanged;
    }

    void LateUpdate()
    {
        if (billboard && cam)
        {
            // 카메라를 바라보게(빌보드)
            Vector3 fwd = transform.position - cam.transform.position;
            if (fwd.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }

        if (hideWhenFull && canvas)
        {
            _timer -= Time.deltaTime;
            bool visible = (enemy && enemy.CurrentHP < enemy.MaxHP) || _timer > 0f;
            canvas.enabled = visible;
        }
    }

    void OnHPChanged(int cur, int max)
    {
        float t = max > 0 ? (float)cur / max : 0f;
        if (fill)
        {
            fill.fillAmount = t;
            if (colorByHP != null) fill.color = colorByHP.Evaluate(t);
        }
        _timer = showOnHitSeconds;
    }

    public void Refresh()
    {
        if (enemy) OnHPChanged(enemy.CurrentHP, enemy.MaxHP);
    }
}
