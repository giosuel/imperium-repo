using HarmonyLib;
using UnityEngine;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyStateInvestigate))]
internal static class EnemyStateInvestigatePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetRPC")]
    private static void SetRPCPatch(EnemyStateInvestigate __instance, Vector3 position)
    {
        Imperium.Visualization.EnemyGizmos.NoiseVisualizerUpdate(__instance, position);
        Imperium.EventLog.EntityEvents.Investigating(__instance, position);
    }
}