namespace Imperium.Types;

public record ExtendedEnemySetup
{
    public EnemySetup Enemy;
    public string EnemyName;
    public EnemyParent.Difficulty Difficulty;
    public EnemyChecklist Checklist;

    public float SpawnedTimeMin;
    public float SpawnedTimeMax;
    public float DespawnedTimeMin;
    public float DespawnedTimeMax;
}