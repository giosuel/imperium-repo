using HarmonyLib;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(LoadingUI))]
internal static class LoadingUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("StartLoading")]
    private static void StartLoadingPatch(LoadingUI __instance)
    {
        // Imperium.IO.LogInfo("Set arena loaded to false");
        // Imperium.IsArenaLoaded.SetFalse();
        // Imperium.GameManager.IsGameLoading = true;
        // Imperium.IO.LogInfo("Set arena loaded to false; done");
    }

    [HarmonyPostfix]
    [HarmonyPatch("LevelAnimationComplete")]
    private static void LevelAnimationCompletePatch(LoadingUI __instance)
    {
        Imperium.IO.LogInfo("Set arena loaded to true");
        Imperium.IsArenaLoaded.SetTrue();
        Imperium.GameManager.IsGameLoading = false;
        Imperium.IO.LogInfo("Set arena loaded to true; done");
    }
}