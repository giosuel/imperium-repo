using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(DebugComputerCheck))]
internal static class DebugComputerCheckPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch("Start")]
    private static IEnumerable<CodeInstruction> StartTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(
                true,
                new CodeMatch(
                    OpCodes.Call, AccessTools.PropertyGetter(typeof(Application), nameof(Application.isEditor))
                )
            )
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(Transpilers.EmitDelegate(ShouldDisableComputer))
            .Instructions();
    }

    private static bool ShouldDisableComputer(DebugComputerCheck instance)
    {
        if (!Imperium.IsImperiumLaunched) return true;

        Imperium.IO.LogInfo($"Should Disable Computer: {instance?.name}: {Imperium.PlayerManager.ActiveDebugComputer.Value != instance}, active: {Imperium.PlayerManager.ActiveDebugComputer.Value?.name}");

        return Imperium.PlayerManager.ActiveDebugComputer.Value != instance;
    }
}