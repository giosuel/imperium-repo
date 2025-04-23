using HarmonyLib;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(LoadingUI))]
internal static class LoadingUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("StartLoading")]
    private static void StartLoadingPatch(LoadingUI __instance)
    {
        Imperium.IsArenaLoaded.SetFalse();
        Imperium.GameManager.IsGameLoading = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("LevelAnimationComplete")]
    private static void LevelAnimationCompletePatch(LoadingUI __instance)
    {
        Imperium.IsArenaLoaded.SetTrue();
        Imperium.GameManager.IsGameLoading = false;
    }
}