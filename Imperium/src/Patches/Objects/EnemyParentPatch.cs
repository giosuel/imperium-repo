using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyParent))]
internal static class EnemyParentPatch
{
    private static FieldInfo logicEnemyParentField;
    private static FieldInfo playerCloseLogicEnemyParentField;

    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    private static void AwakePatch(EnemyParent __instance)
    {
        logicEnemyParentField = typeof(EnemyParent)
            .GetMethod("Logic", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetStateMachineTarget().DeclaringType!
            .GetField("<>4__this", BindingFlags.Instance | BindingFlags.Public)!;

        playerCloseLogicEnemyParentField = typeof(EnemyParent)
            .GetMethod("PlayerCloseLogic", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetStateMachineTarget().DeclaringType!
            .GetField("<>4__this", BindingFlags.Instance | BindingFlags.Public)!;
    }


    [HarmonyPrefix]
    [HarmonyPatch("Despawn")]
    private static void DespawnPatch(EnemyParent __instance)
    {
        Imperium.IO.LogInfo($"Entity despawn: {__instance.name}");
    }

    [HarmonyPrefix]
    [HarmonyPatch("Spawn")]
    private static void SpawnPatch(EnemyParent __instance)
    {
        Imperium.IO.LogInfo($"Entity spawn: {__instance.name}");
    }

    [HarmonyPrefix]
    [HarmonyPatch("Logic", MethodType.Enumerator)]
    private static bool LogicPrefixPatch(object __instance, out bool __result)
    {
        var instance = logicEnemyParentField.GetValue(__instance) as EnemyParent;

        if (Imperium.ObjectManager.DisabledObjects.Value.Contains(instance!.photonView.ViewID))
        {
            __result = true;
            return false;
        }

        __result = false;
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlayerCloseLogic", MethodType.Enumerator)]
    private static bool PlayerCloseLogicPrefixPatch(object __instance, out bool __result)
    {
        var instance = playerCloseLogicEnemyParentField.GetValue(__instance) as EnemyParent;

        if (Imperium.ObjectManager.DisabledObjects.Value.Contains(instance!.photonView.ViewID))
        {
            __result = true;
            return false;
        }

        __result = false;
        return true;
    }
}