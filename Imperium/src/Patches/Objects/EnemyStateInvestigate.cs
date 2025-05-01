using HarmonyLib;
using UnityEngine;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyStateInvestigate))]
internal static class EnemyStateInvestigatePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Set")]
    private static void SetPatch(EnemyStateInvestigate __instance, Vector3 position)
    {
        Imperium.Visualization.EnemyGizmos.NoiseVisualizerUpdate(__instance, position);
    }
}