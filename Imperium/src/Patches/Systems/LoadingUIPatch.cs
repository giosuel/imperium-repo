#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(LoadingUI))]
internal static class LoadingUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("StartLoading")]
    private static void StartLoadingPatch(LoadingUI __instance)
    {
        Imperium.IsLevelLoaded.SetFalse();
        Imperium.GameManager.IsGameLoading = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LoadingUI), "StopLoading")]
    private static void StopLoadingPatch(LoadingUI __instance)
    {
        Imperium.IsLevelLoaded.SetTrue();
        Imperium.GameManager.IsGameLoading = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LoadingUI), "LevelAnimationStart")]
    private static void LevelAnimationStartPatch(LoadingUI __instance)
    {
        if (Imperium.Settings.Preferences.SkipLoading.Value) __instance.debugDisableLevelAnimation = true;
    }
}