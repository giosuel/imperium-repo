#region

using System.Linq;
using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyHealth))]
internal static class EnemyHealthPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("HurtRPC")]
    private static void HurtRPCPatch(EnemyHealth __instance, int _damage)
    {
        var enemyName = __instance.enemy.EnemyParent.enemyName;
        if (!Imperium.Visualization.EnemyGizmos.EntityInfoConfigs.TryGetValue(enemyName, out var enemyConfig))
        {
            return;
        }

        if (!enemyConfig.Vitality.Value) return;

        var uisBefore = WorldSpaceUIParent.instance.valueLostList.Count;
        WorldSpaceUIParent.instance.ValueLostCreate(__instance.enemy.transform.position, _damage);

        if (WorldSpaceUIParent.instance.valueLostList.Count > uisBefore)
        {
            Imperium.GameManager.EnemyValueLostInstances.Add(WorldSpaceUIParent.instance.valueLostList.Last());
        }
    }
}