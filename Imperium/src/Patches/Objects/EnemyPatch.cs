using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
using UnityEngine;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(Enemy))]
internal static class EnemyPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("EnemyTeleportedRPC")]
    private static void StartPatch(Enemy __instance, Vector3 teleportPosition)
    {
        Imperium.EventLog.EntityEvents.Teleport(__instance.EnemyParent, teleportPosition);
    }
}