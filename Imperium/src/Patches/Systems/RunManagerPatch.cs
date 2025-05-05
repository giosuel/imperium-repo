#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("ChangeLevel")]
    private static bool ChangeLevelPrefixPatch(RunManager __instance, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType)
    {
        if (Imperium.GameManager.CustomLevelNumber.Value > 0)
        {

        }

        return !_levelFailed || !Imperium.ArenaManager.DisableGameOver.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevel")]
    private static void ChangeLevelPostfixPatch(RunManager __instance, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType)
    {
        if (_levelFailed || Imperium.GameManager.CustomLevelNumber.Value <= 0) return;

        __instance.levelsCompleted = Imperium.GameManager.CustomLevelNumber.Value - 1;
        SemiFunc.StatSetRunLevel(__instance.levelsCompleted);
    }
}