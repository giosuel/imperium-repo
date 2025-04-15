using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Imperium.Util;
using MonoMod.Utils;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyVision))]
internal static class EnemyVisionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Vision")]
    private static IEnumerator VisionPatch(IEnumerator result, EnemyVision __instance)
    {
        while (result.MoveNext())
        {
            yield return result.Current;
            Imperium.Visualization.EntityGizmos.VisionUpdate(__instance);
        }
    }
}


