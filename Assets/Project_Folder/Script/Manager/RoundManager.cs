// C# �⺻: Unity Engine �� �̺�Ʈ �ý����� ����ϱ� ���� ���ӽ����̽� ����
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// ��ü ���� ���α׷���(OOP): MonoBehaviour�� ��ӹ޾� Unity ������Ʈ �ý��ۿ� ����
public class RoundManager : MonoBehaviour
{
    // Unity ������ ���: [Header] ��Ʈ����Ʈ�� �ν����� â UI�� �׷�ȭ
    [Header("Spawners")]
    // Unity ������ ���: [SerializeField] ��Ʈ����Ʈ�� private ������ �ν����Ϳ� ���� (����ȭ)
    [SerializeField] private EnemySpawner[] spawners;

    [Header("Round Flow")]
    [SerializeField] private bool autoStart = false;
    // Unity ������ ���: [Min] ��Ʈ����Ʈ�� �ν����Ϳ��� ���� ������ �ּڰ��� ����
    [SerializeField, Min(0f)] private float intermission = 5f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseCount = 6;
    [SerializeField] private int addPerRound = 3;
    [SerializeField] private float baseInterval = 1.4f;
    [SerializeField] private float intervalMult = 0.95f;

    [Header("Turret Rank Up")]
    [SerializeField] private bool rankUpAtRoundStart = true;

    [Header("Detect Enemies")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("UI Start �ɼ�")]
    [SerializeField] private bool skipFirstIntermission = true;

    // Unity �̺�Ʈ �ý���: [System.Serializable]�� UnityEvent<T>�� �����Ͽ� �ν����Ϳ��� �̺�Ʈ�� �����ϰ� �ٸ� ��ü�� ������ ����(Loose Coupling)�� ����
    [System.Serializable] public class IntEvent : UnityEvent<int> { }
    public IntEvent onRoundStarted;
    public IntEvent onRoundEnded;

    // ��ü ���� ���α׷���(OOP) - ĸ��ȭ(Encapsulation): get�� public, set�� private���� �����Ͽ� �ܺο����� �б⸸ �����ϰ� ������ Ŭ���� ���ο����� �����ϵ��� ����
    public int CurrentRound { get; private set; }

    private Coroutine loopRoutine;
    // C# �� ���� ���(Expression-Bodied Member): 'IsRunning' �Ӽ��� �����ϰ� ����. �ڷ�ƾ�� ���� ������ ���θ� ��ȯ
    public bool IsRunning => loopRoutine != null;

    // Unity �����ֱ� �޼���: ���� ���� �� �� �� ȣ��� (�ַ� �ʱ�ȭ�� ���)
    void Start()
    {
        // ����� ���α׷���: spawner�� �ν����Ϳ��� �Ҵ���� �ʾ��� ���, ������ �ڵ����� ã�� �Ҵ�
        if (spawners == null || spawners.Length == 0)
            spawners = Object.FindObjectsByType<EnemySpawner>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (autoStart) StartGame();
    }

    // UI ����: UI ��ư ��� ȣ���� �� �ֵ��� public �޼���� API ����
    public void StartGame()
    {
        if (IsRunning) return;
        // �񵿱� ���α׷���(�ڷ�ƾ): 'StartCoroutine'�� ����Ͽ� ������ ���� �����带 �������� �ʰ� �ð��� �帧�� ���� ������ ����
        // C# ���� ������: 'skipFirstIntermission' ���� ���� �ٸ� �ڷ�ƾ�� ���Ǻη� ����
        loopRoutine = StartCoroutine(skipFirstIntermission ? RoundLoopImmediate() : RoundLoop());
    }

    // �񵿱� ���α׷���(�ڷ�ƾ): 'IEnumerator'�� ��ȯ Ÿ������ �����Ͽ� �ڷ�ƾ���� ����
    public IEnumerator RoundLoop()
    {
        CurrentRound = 0;
        // �ڷ�ƾ ����: 'yield return'�� ����Ͽ� �ڵ� ������ ��� ���߰� ������ �ð�(intermission) �Ŀ� �ٽ� ����
        yield return new WaitForSeconds(intermission);

        // ���� ����: 'while(true)'�� 'yield'�� �����Ͽ� ������ ���� ������ ���带 ������ �ݺ��ϴ� ���� ���� ����
        while (true)
        {
            StartNextRound();

            // �ڷ�ƾ ����: Ư�� ����(���� ���� �� �� ����)�� ������ ������ �� ������ ���
            while (AnySpawnerSpawning() || EnemyAliveCount() > 0)
                yield return null; // 'yield return null'�� ���� �����ӱ��� ������ ���

            // C# null ���Ǻ� ������(?.): �̺�Ʈ �����ʰ� ���� ���(null)���� ���� ���� �����ϰ� �̺�Ʈ�� ȣ��
            onRoundEnded?.Invoke(CurrentRound);
            yield return new WaitForSeconds(intermission);
        }
    }

    private IEnumerator RoundLoopImmediate()
    {
        CurrentRound = 0;
        yield return null; // �� ������ ����Ͽ� �ٸ� ��ũ��Ʈ���� �ʱ�ȭ�� �ð��� ��
        while (true)
        {
            StartNextRound();

            while (AnySpawnerSpawning() || EnemyAliveCount() > 0)
                yield return null;

            onRoundEnded?.Invoke(CurrentRound);
            yield return new WaitForSeconds(intermission);
        }
    }

    public void StartNextRound()
    {
        CurrentRound++;

        if (rankUpAtRoundStart) RankUpAllTurrets();

        // ���� �뷱�� ����: ���尡 ����ʿ� ���� ���� ���� ���� ������ �������� ���
        int total = baseCount + addPerRound * (CurrentRound - 1);
        float interval = baseInterval * Mathf.Pow(intervalMult, CurrentRound - 1);

        int n = Mathf.Max(1, spawners.Length);
        // �й� �˰���: ������ �� ���� ���� ��� �����ʿ��� �ִ��� �յ��ϰ� �й�
        int each = total / n;
        int rem = total - each * n; // ������

        for (int i = 0; i < spawners.Length; i++)
        {
            var s = spawners[i];
            if (!s) continue;
            // ������(rem)�� �� ������ �����ʺ��� �ϳ��� ������
            int count = each + (i < rem ? 1 : 0);
            if (count > 0) StartCoroutine(s.SpawnWave(count, interval));
        }

        onRoundStarted?.Invoke(CurrentRound);
    }

    // ���� �޼���: ������ �� �ϳ��� ���� ������ ���θ� Ȯ���ϴ� ���� �Լ�
    bool AnySpawnerSpawning()
    {
        foreach (var s in spawners) if (s && s.IsSpawning) return true;
        return false;
    }

    // ���� ����ȭ ���: ���� ã�� ���� ��� �� ������ �� ���� �±�(Tag) �˻��� �켱������ ���
    int EnemyAliveCount()
    {
        if (!string.IsNullOrEmpty(enemyTag))
            return GameObject.FindGameObjectsWithTag(enemyTag).Length;

        // ��ü ����: �±װ� ���� ���, ���� ��� ���ӿ�����Ʈ�� ��ȸ�ϸ� ���̾�(Layer)�� �� (���� ���ϰ� �� ŭ)
        int layer = LayerMask.NameToLayer(enemyLayerName);
        var gos = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        int count = 0;
        foreach (var go in gos) if (go.layer == layer) count++;
        return count;
    }

    // �� ��ü ��ü�� ��ȣ�ۿ�: 'FindObjectsByType'�� ����� ���� �ִ� ��� 'TurretRankUpgrader'�� ã�� �� ���� ����
    void RankUpAllTurrets()
    {
        var ups = Object.FindObjectsByType<TurretRankUpgrader>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var up in ups) if (up) up.RankUp();
    }
}