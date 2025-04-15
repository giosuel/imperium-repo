using HarmonyLib;
using UnityEngine;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(EnemyDirector))]
internal static class EnemyDirectorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("SetInvestigate")]
    private static void EnemySetupPatch(EnemyDirector __instance, Vector3 position, float radius)
    {
        Imperium.Visualization.NoiseIndicators.AddNoise(position, radius);
    }
}