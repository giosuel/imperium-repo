#region

using System.Collections.Generic;

#endregion

namespace Imperium.Extensions;

public static class EnemyDirectorExtensions
{
    public static List<EnemySetup> GetEnemies(this EnemyDirector enemyDirector)
    {
        var enemiesDifficulty1 = enemyDirector.enemiesDifficulty1;
        var enemiesDifficulty2 = enemyDirector.enemiesDifficulty2;
        var enemiesDifficulty3 = enemyDirector.enemiesDifficulty3;
        var enemies = new List<EnemySetup>(enemiesDifficulty1.Count + enemiesDifficulty2.Count + enemiesDifficulty3.Count);
        enemies.AddRange(enemiesDifficulty1);
        enemies.AddRange(enemiesDifficulty2);
        enemies.AddRange(enemiesDifficulty3);
        return enemies;
    }
}