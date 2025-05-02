#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(LevelGenerator))]
internal static class LevelGeneratorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("EnemySetup")]
    private static void EnemySetupPatch(LevelGenerator __instance)
    {
        Imperium.ObjectManager.InvokeObjectsChanged();
    }
}