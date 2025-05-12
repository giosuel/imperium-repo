#region

using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;

#endregion

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

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void StartPatch(EnemyParent __instance)
    {
        Imperium.EventLog.EntityEvents.Awake(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DespawnRPC")]
    private static void DespawnRPCPatch(EnemyParent __instance)
    {
        Imperium.EventLog.EntityEvents.Despawn(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnRPC")]
    private static void SpawnRPCPatch(EnemyParent __instance)
    {
        Imperium.EventLog.EntityEvents.Spawn(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("Logic", MethodType.Enumerator)]
    private static bool LogicPrefixPatch(object __instance, out bool __result)
    {
        var instance = logicEnemyParentField.GetValue(__instance) as EnemyParent;

        var uniqueId = SemiFunc.IsMultiplayer()
            ? instance!.photonView.ViewID
            : instance!.gameObject.GetInstanceID();

        if (Imperium.ObjectManager.DisabledObjects.Value.Contains(uniqueId))
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

        var uniqueId = SemiFunc.IsMultiplayer()
            ? instance!.photonView.ViewID
            : instance!.gameObject.GetInstanceID();

        if (Imperium.ObjectManager.DisabledObjects.Value.Contains(uniqueId))
        {
            __result = true;
            return false;
        }

        __result = false;
        return true;
    }
}