#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(WorldSpaceUIValueLost))]
internal static class WorldSpaceUIValueLostPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void StartPatch(WorldSpaceUIValueLost __instance)
    {
        if (Imperium.GameManager.EnemyValueLostInstances.Contains(__instance))
        {
            __instance.text.text = $"-{__instance.value} HP";
            __instance.scale *= 2f;
            __instance.transform.localScale = __instance.scale;
        }
    }
}