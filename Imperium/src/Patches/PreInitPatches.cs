using HarmonyLib;
using TMPro;

namespace Imperium.Patches;

internal static class PreInitPatches
{
    private static bool hasAddedVersion;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildName), "Start")]
    private static void StartPatch(BuildName __instance)
    {
        if (hasAddedVersion) return;

        __instance.GetComponent<TextMeshProUGUI>().text += $" ({LCMPluginInfo.PLUGIN_NAME} v{LCMPluginInfo.PLUGIN_VERSION})";

        hasAddedVersion = true;
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