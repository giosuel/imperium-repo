#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("ChangeLevel")]
    private static bool ChangeLevelPrefixPatch(RunManager __instance, bool _levelFailed,
        RunManager.ChangeLevelType _changeLevelType)
    {
        return !_levelFailed || !Imperium.ArenaManager.DisableGameOver.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChangeLevel")]
    private static void ChangeLevelPostfixPatch(
        RunManager __instance,
        bool _completedLevel,
        bool _levelFailed,
        RunManager.ChangeLevelType _changeLevelType
    )
    {
        Imperium.EventLog.GameEvents.ChangeLevel(_completedLevel, _levelFailed, _changeLevelType);

        if (Imperium.GameManager.NextLevelNumberOverride > 0)
        {
            __instance.levelsCompleted = Imperium.GameManager.NextLevelNumberOverride - 1;
            SemiFunc.StatSetRunLevel(__instance.levelsCompleted);

            Imperium.GameManager.NextLevelNumberOverride = 0;
        }
        else if (Imperium.GameManager.CustomLevelNumber.Value > 0)
        {
            __instance.levelsCompleted = Imperium.GameManager.CustomLevelNumber.Value - 1;
            SemiFunc.StatSetRunLevel(__instance.levelsCompleted);
        }
    }
}