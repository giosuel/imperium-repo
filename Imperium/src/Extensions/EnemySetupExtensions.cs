using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Imperium.Extensions;

public static class EnemySetupExtensions
{
    public static List<GameObject> GetSortedSpawnObjects(this EnemySetup enemySetup)
    {
        if (enemySetup == null || enemySetup.spawnObjects == null) return [];

        return enemySetup.spawnObjects
            .Where(x => x != null)
            .OrderByDescending(x => x.TryGetComponent(out EnemyParent _))
            .ToList();
    }
}