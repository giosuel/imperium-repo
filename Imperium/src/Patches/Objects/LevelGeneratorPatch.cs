#region

using System.Linq;
using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(LevelGenerator))]
internal static class LevelGeneratorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Start")]
    private static void StartPatch(LevelGenerator __instance)
    {
        if (Imperium.GameManager.ModuleOverride.Value == "")
        {
            __instance.DebugModule = null;
        }
        else
        {
            var customModule = Imperium.ObjectManager.LoadedModules.Value
                .First(module => module.name == Imperium.GameManager.ModuleOverride.Value);
            __instance.DebugModule = customModule.gameObject;

            switch ((Module.Type)Imperium.GameManager.OverrideModuleType.Value)
            {
                case Module.Type.Normal:
                    LevelGenerator.Instance.DebugNormal = true;
                    break;
                case Module.Type.Passage:
                    LevelGenerator.Instance.DebugPassage = true;
                    break;
                case Module.Type.DeadEnd:
                case Module.Type.Extraction:
                    LevelGenerator.Instance.DebugDeadEnd = true;
                    break;
            }
        }

        if (Imperium.GameManager.NextLevelOverride)
        {
            // The user manually switched to a level so we ignore the override here
            RunManager.instance.levelCurrent = Imperium.GameManager.NextLevelOverride;
            Imperium.GameManager.NextLevelOverride = null;
        }

        LevelGenerator.Instance.DebugLevelSize = Imperium.GameManager.CustomLevelSize.Value > 0
            ? Imperium.GameManager.CustomLevelSize.Value
            : 1;

        LevelGenerator.Instance.DebugAmount = Imperium.GameManager.CustomModuleAmount.Value;

        ValuableDirector.instance.valuableDebug =
            (ValuableDirector.ValuableDebug)Imperium.GameManager.CustomValuableSpawns.Value;
    }
}