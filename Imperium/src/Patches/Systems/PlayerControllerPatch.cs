#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(PlayerController))]
internal static class PlayerControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static void StartPostfixPatch(PlayerController __instance)
    {
    }
}