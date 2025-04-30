using HarmonyLib;
using TMPro;

namespace Imperium.Patches;

internal static class PreInitPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildName), "Start")]
    private static void StartPatch(BuildName __instance)
    {
        __instance.GetComponent<TextMeshProUGUI>().text += $" (Imperium v{LCMPluginInfo.PLUGIN_VERSION})";
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAvatar), "Start")]
    private static void StartPatch(PlayerAvatar __instance)
    {
        // Ignore if avatar isn't local avatar
        if (!PlayerAvatar.instance || !Imperium.IsImperiumInitialized) return;

        if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
        {
            Imperium.Unload();
        }
        else if (RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu && !Imperium.IsImperiumLaunched)
        {
            Imperium.Networking.RequestImperiumAccess();
        }
    }
}