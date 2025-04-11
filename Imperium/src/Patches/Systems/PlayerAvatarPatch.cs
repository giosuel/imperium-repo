using HarmonyLib;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(PlayerAvatar))]
internal static class PlayerAvatarPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void StartPostfixPatch(PlayerAvatar __instance)
    {
        if (RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu && RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu)
        {
            Imperium.Launch();
        }
    }
}