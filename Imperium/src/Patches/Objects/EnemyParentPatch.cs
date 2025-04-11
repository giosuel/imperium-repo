using HarmonyLib;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyParent))]
internal static class EnemyParentPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Despawn")]
    private static void DespawnPatch(EnemyParent __instance)
    {
        Imperium.IO.LogInfo($"Entity despawn: {__instance.name}");
    }

    [HarmonyPrefix]
    [HarmonyPatch("Spawn")]
    private static void SpawnPatch(EnemyParent __instance)
    {
        Imperium.IO.LogInfo($"Entity spawn: {__instance.name}");
    }
}