#region

using System.IO;
using HarmonyLib;
using Imperium.Core;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

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
            if (!Imperium.IsImperiumLaunched)
            {
                if (!ImpUtils.RunSafe(Imperium.Launch, "Imperium startup failed")) Imperium.DisableImperium();
            }
            else
            {
                Imperium.EnableImperium();
            }
        }
        else if (RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
        {
            Imperium.Networking.RequestImperiumAccess();
        }
    }

    private static bool hasAutoLoaded;

    [HarmonyPatch(typeof(RunManager), "Awake")]
    [HarmonyPrefix]
    private static void AwakeRunManagerPatch(RunManager __instance)
    {
        var autoLaunch = Imperium.Settings.Preferences.QuickloadAutoLaunch.Value;
        
        if (!autoLaunch || hasAutoLoaded) return;
        
        __instance.levelCurrent = __instance.levelMainMenu;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuOpen), "Awake")]
    private static void AwakePlayerAvatarPatch(MainMenuOpen __instance)
    {
        if (hasAutoLoaded) return;
        hasAutoLoaded = true;

        var autoLaunch = Imperium.Settings.Preferences.QuickloadAutoLaunch.Value;
        var mode = Imperium.Settings.Preferences.QuickloadLaunchMode.Value;

        if (!autoLaunch) return;

        string saveFilePath = $"{Application.persistentDataPath}/saves/{ImpConstants.ImperiumSaveFile}/{ImpConstants.ImperiumSaveFile}.es3";
        if (!File.Exists(saveFilePath))
            StatsManager.instance.SaveGame(ImpConstants.ImperiumSaveFile);
        
        if (mode == LaunchMode.Singleplayer)
        {
            __instance.MainMenuSetState((int)MainMenuOpen.MainMenuGameModeState.SinglePlayer);
            SemiFunc.MenuActionSingleplayerGame(ImpConstants.ImperiumSaveFile);
        }

        if (mode == LaunchMode.Multiplayer)
        {
            __instance.MainMenuSetState((int)MainMenuOpen.MainMenuGameModeState.MultiPlayer);
            SemiFunc.MenuActionHostGame(ImpConstants.ImperiumSaveFile);
        }
    }
}
