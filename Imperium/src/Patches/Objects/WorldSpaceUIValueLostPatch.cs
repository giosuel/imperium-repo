#region

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyHealth))]
internal static class EnemyHealthPatch
{
    internal static readonly HashSet<int> valueLostInstances = [];

    [HarmonyPostfix]
    [HarmonyPatch("HurtRPC")]
    private static void HurtRPCPatch(EnemyHealth __instance, int _damage)
    {
        WorldSpaceUIParent.instance.ValueLostCreate(__instance.transform.position, _damage);
        valueLostInstances.Add(WorldSpaceUIParent.instance.valueLostList.Last().GetInstanceID());
    }
}