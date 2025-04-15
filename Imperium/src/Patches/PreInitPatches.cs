using HarmonyLib;

namespace Imperium.Patches;

internal static class PreInitPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildManager), "Awake")]
    private static void AwakePatch(BuildManager __instance)
    {
        __instance.version.title += $" ({Imperium.PLUGIN_NAME} v{Imperium.PLUGIN_VERSION})";
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAvatar), "Start")]
    private static void StartPatch(PlayerAvatar __instance)
    {
        Imperium.IO.LogInfo("Start player avatar patch");

        // Ignore if avatar isn't local avatar
        if (!PlayerAvatar.instance) return;

        if (!Imperium.IsImperiumInitialized || Imperium.IsImperiumLaunched) return;

        // if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
        // {
        //     Imperium.Launch();
        // }
        // else if (RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
        // {
        //     Imperium.Networking.RequestImperiumAccess();
        // }

        if (RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu &&
            RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu)
        {
            Imperium.IO.LogInfo("Requesting imperium access");
            Imperium.Networking.RequestImperiumAccess();
        }
    }

    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
    // private static void ChangeLevelPatch(RunManager __instance, RunManager.ChangeLevelType _changeLevelType)
    // {
    //     Imperium.IO.LogInfo($"[LVL] Level switch to {_changeLevelType}");
    //
    //     // Unload Imperium when the game switches from the lobby to the run level
    //     // if (_changeLevelType == RunManager.ChangeLevelType.RunLevel) Imperium.Unload();
    // }
}