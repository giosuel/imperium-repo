#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("ChangeLevel")]
    private static bool ChangeLevelPatch(RunManager __instance, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType)
    {
        return !_levelFailed || !Imperium.ArenaManager.DisableGameOver.Value;
    }
}