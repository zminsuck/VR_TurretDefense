using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Enemy enemy;     // �ڵ����� �θ𿡼� ã��

    [Header("UI")]
    [SerializeField] private Canvas canvas;   // World Space
    [SerializeField] private Image fill;      // Filled Horizontal

    [Header("Options")]
    [SerializeField] private bool billboard = true;   // ī�޶� �ٶ󺸱�
    [SerializeField] private Camera cam;              // XR ī�޶� ���� ����
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField, Min(0f)] private float showOnHitSeconds = 2f;
    [SerializeField] private Gradient colorByHP;      // ����: ü�¿� ���� �� ��ȭ

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
            // ī�޶� �ٶ󺸰�(������)
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
