using System.Collections.Generic;

namespace Imperium.Extensions;

public static class EnemyDirectorExtensions
{
    public static List<EnemySetup> GetEnemies(this EnemyDirector enemyDirector)
    {
        List<EnemySetup> enemiesDifficulty1 = enemyDirector.enemiesDifficulty1;
        List<EnemySetup> enemiesDifficulty2 = enemyDirector.enemiesDifficulty2;
        List<EnemySetup> enemiesDifficulty3 = enemyDirector.enemiesDifficulty3;
        List<EnemySetup> enemies = new List<EnemySetup>(enemiesDifficulty1.Count + enemiesDifficulty2.Count + enemiesDifficulty3.Count);
        enemies.AddRange((IEnumerable<EnemySetup>) enemiesDifficulty1);
        enemies.AddRange((IEnumerable<EnemySetup>) enemiesDifficulty2);
        enemies.AddRange((IEnumerable<EnemySetup>) enemiesDifficulty3);
        return enemies;
    }
}