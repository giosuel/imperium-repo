using HarmonyLib;
using Imperium.Core;
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

        if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu ||
            RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
        {

            Imperium.Unload();
        }
        else if (!Imperium.IsImperiumLaunched)
        {
            Imperium.Networking.RequestImperiumAccess();
        }
    }

    private static bool hasAutoLoaded;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuOpen), "Awake")]
    private static void AwakePlayerAvatarPatch(MainMenuOpen __instance)
    {
        if (hasAutoLoaded) return;
        hasAutoLoaded = true;

        var autoLaunch = Imperium.Settings.Preferences.QuickloadAutoLaunch.Value;
        var mode = Imperium.Settings.Preferences.QuickloadLaunchMode.Value;

        if (!autoLaunch) return;

        if (mode == LaunchMode.Singleplayer)
        {
            RunManager.instance.skipMainMenu = true;
            if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
            {
                RunManager.instance.SetRunLevel();
                // RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.RunLevel);
            }
        }

        if (mode == LaunchMode.Multiplayer)
        {
            StatsManager.instance.SaveGame(ImpConstants.ImperiumSaveFile);

            __instance.MainMenuSetState((int)MainMenuOpen.MainMenuGameModeState.MultiPlayer);
            SemiFunc.MenuActionHostGame(ImpConstants.ImperiumSaveFile);
        }
    }
}