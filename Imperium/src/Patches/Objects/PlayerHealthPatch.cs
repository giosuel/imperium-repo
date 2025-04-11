using HarmonyLib;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(PlayerHealth))]
internal static class PlayerHealthPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Hurt")]
    private static bool HurtPatch(PlayerHealth __instance)
    {
        return !Imperium.Settings.Player.GodMode.Value;
    }
}