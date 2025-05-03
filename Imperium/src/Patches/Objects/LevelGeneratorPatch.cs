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
        }

        LevelGenerator.Instance.DebugLevelSize = Imperium.GameManager.CustomLevelSize.Value > 0
            ? Imperium.GameManager.CustomLevelSize.Value
            : 1;
        
        LevelGenerator.Instance.DebugAmount = Imperium.GameManager.CustomModuleAmount.Value;
    }
}