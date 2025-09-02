// IEnemySetup.cs
public interface IEnemySetup
{
    // 스폰 직후 라운드 난이도 반영
    void SetupStats(int maxHP, float moveSpeed);
}