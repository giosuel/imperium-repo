using HarmonyLib;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("ChangeLevel")]
    private static bool ChangeLevelPatch(RunManager __instance, bool _levelFailed)
    {
        if (_levelFailed) return !Imperium.ArenaManager.DisableGameOver.Value;

        return true;
    }
}